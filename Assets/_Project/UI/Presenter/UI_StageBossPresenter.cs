using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_StageBossPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_StageBossStatusView _stageBossStatusView;
    [SerializeField] private BossBase _boss;

    [Header("Options")]
    [SerializeField] private string _bossNameOverride;
    [SerializeField] private bool _hideViewWhenTargetIsMissing = true;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToStatus();
        RefreshView();
    }

    public void SetTarget(BossBase boss)
    {
        UnsubscribeFromStatus();
        _boss = boss;
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

    private void ResolveReferences()
    {
        _stageBossStatusView ??= FindFirstObjectByType<UI_StageBossStatusView>(FindObjectsInactive.Include);

        if (_boss == null)
        {
            _boss = FindFirstObjectByType<BossBase>(FindObjectsInactive.Include);
        }
    }

    private void SubscribeToStatus()
    {
        if (_boss == null)
        {
            return;
        }

        _boss.StatusChanged -= HandleStatusChanged;
        _boss.StatusChanged += HandleStatusChanged;
    }

    private void UnsubscribeFromStatus()
    {
        if (_boss == null)
        {
            return;
        }

        _boss.StatusChanged -= HandleStatusChanged;
    }

    private void RefreshView()
    {
        if (_stageBossStatusView == null || _boss == null)
        {
            ResolveReferences();
        }

        if (_stageBossStatusView == null)
        {
            return;
        }

        if (_boss == null)
        {
            if (_hideViewWhenTargetIsMissing)
            {
                _stageBossStatusView.Hide();
            }

            return;
        }

        string bossName = GetBossName();
        int currentHealth = _boss.Hp;
        int maxHealth = _boss.MaxHp;

        _stageBossStatusView.Show();
        _stageBossStatusView.SetBossName(bossName);
        _stageBossStatusView.RenderHealth(currentHealth, maxHealth);
    }

    private string GetBossName()
    {
        if (!string.IsNullOrWhiteSpace(_bossNameOverride))
        {
            return _bossNameOverride;
        }

        if (_boss == null)
        {
            return string.Empty;
        }

        return _boss.DisplayName;
    }

    private void HandleStatusChanged()
    {
        RefreshView();
    }
}
