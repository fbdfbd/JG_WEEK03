using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_StagePlayerPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_StagePlayerStatusView _stagePlayerStatusView;
    [SerializeField] private PlayerStatus _playerStatus;

    [Header("Options")]
    [SerializeField] private bool _hideViewWhenTargetIsMissing = true;

    private bool _hasRequiredReferences;

    private void Awake()
    {
        _hasRequiredReferences = ValidateReferences();
    }

    private void OnEnable()
    {
        if (!_hasRequiredReferences)
        {
            return;
        }

        SubscribeToStatus();
        RefreshView();
    }

    public void SetTarget(PlayerStatus playerStatus)
    {
        UnsubscribeFromStatus();
        _playerStatus = playerStatus;
        SubscribeToStatus();
        RefreshView();
    }

    public void RefreshNow()
    {
        RefreshView();
    }

    private void OnDisable()
    {
        UnsubscribeFromStatus();
    }

    private bool ValidateReferences()
    {
        if (_stagePlayerStatusView != null)
        {
            return true;
        }

        Debug.LogError($"{nameof(UI_StagePlayerPresenter)} requires a {nameof(UI_StagePlayerStatusView)} reference.", this);
        return false;
    }

    private void SubscribeToStatus()
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.StatusChanged -= HandleStatusChanged;
        _playerStatus.StatusChanged += HandleStatusChanged;
    }

    private void UnsubscribeFromStatus()
    {
        if (_playerStatus == null)
        {
            return;
        }

        _playerStatus.StatusChanged -= HandleStatusChanged;
    }

    private void RefreshView()
    {
        if (_stagePlayerStatusView == null)
        {
            return;
        }

        if (_playerStatus == null)
        {
            if (_hideViewWhenTargetIsMissing)
            {
                _stagePlayerStatusView.Hide();
            }

            return;
        }

        int currentHealth = _playerStatus.GetCurrentHealth();
        int maxHealth = _playerStatus.GetMaxHealth();
        float currentSpecialGauge = _playerStatus.GetCurrentSpecialGauge();
        float maxSpecialGauge = _playerStatus.GetMaxSpecialGauge();

        _stagePlayerStatusView.Show();
        _stagePlayerStatusView.RenderHealth(currentHealth, maxHealth);
        _stagePlayerStatusView.RenderSpecialGauge(currentSpecialGauge, maxSpecialGauge);
    }

    private void HandleStatusChanged()
    {
        RefreshView();
    }
}
