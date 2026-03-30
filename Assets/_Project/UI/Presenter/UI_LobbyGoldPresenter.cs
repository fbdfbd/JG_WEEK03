using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_LobbyGoldPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_LobbyGoldView _goldView;
    [SerializeField] private PlayerProfileService _playerProfileService;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (_playerProfileService != null)
        {
            _playerProfileService.GoldChanged += HandleGoldChanged;
        }

        RefreshGold();
    }

    private void Start()
    {
        RefreshGold();
    }

    private void OnDisable()
    {
        if (_playerProfileService != null)
        {
            _playerProfileService.GoldChanged -= HandleGoldChanged;
        }
    }

    private void ResolveReferences()
    {
        _goldView ??= FindFirstObjectByType<UI_LobbyGoldView>(FindObjectsInactive.Include);

        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            _playerProfileService = GameManager.I.PlayerProfileService;
        }

        _playerProfileService ??= FindFirstObjectByType<PlayerProfileService>(FindObjectsInactive.Include);
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
