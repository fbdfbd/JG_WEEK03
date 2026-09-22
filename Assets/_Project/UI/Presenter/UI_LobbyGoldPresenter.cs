using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_LobbyGoldPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_LobbyGoldView _goldView;
    [SerializeField] private PlayerProfileService _playerProfileService;

    private bool _hasRequiredReferences;

    private void Awake()
    {
        ResolvePlayerProfileService();
        _hasRequiredReferences = ValidateReferences();
    }

    private void OnEnable()
    {
        if (!_hasRequiredReferences)
        {
            return;
        }

        _playerProfileService.GoldChanged += HandleGoldChanged;

        RefreshGold();
    }

    private void Start()
    {
        if (_hasRequiredReferences)
        {
            RefreshGold();
        }
    }

    private void OnDisable()
    {
        if (_playerProfileService != null)
        {
            _playerProfileService.GoldChanged -= HandleGoldChanged;
        }
    }

    private void ResolvePlayerProfileService()
    {
        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            _playerProfileService = GameManager.I.PlayerProfileService;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (_goldView == null)
        {
            Debug.LogError($"{nameof(UI_LobbyGoldPresenter)} requires a {nameof(UI_LobbyGoldView)} reference.", this);
            isValid = false;
        }

        if (_playerProfileService == null)
        {
            Debug.LogError($"{nameof(UI_LobbyGoldPresenter)} requires a {nameof(PlayerProfileService)} reference.", this);
            isValid = false;
        }

        return isValid;
    }

    private void HandleGoldChanged()
    {
        RefreshGold();
    }

    private void RefreshGold()
    {
        if (_goldView == null || _playerProfileService == null || _playerProfileService.ProfileData == null)
        {
            return;
        }

        _goldView.SetGold(_playerProfileService.ProfileData.Gold);
    }
}
