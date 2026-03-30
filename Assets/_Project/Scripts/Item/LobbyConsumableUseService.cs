using UnityEngine;

public enum LobbyConsumableUseResult
{
    Success,
    ConsumableNotFound,
    NotOwned,
    UnsupportedConsumableType,
    TurnServiceNotReady,
    StaminaAlreadyFull,
    MapServiceNotReady,
    TargetTileRequired,
    InvalidTargetTile,
    TileIsNotPlayerOwned,
    TileIsNotUnderAttack,
    TileAlreadyProtected,
    SpendFailed
}

[DisallowMultipleComponent]
public sealed class LobbyConsumableUseService : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerProfileService _playerProfileService;
    [SerializeField] private TerritoryTurnService _territoryTurnService;
    [SerializeField] private TerritoryMapService _territoryMapService;

    private const int StaminaRecoverAmount = 1;
    private const int EmergencyDefenseDurationDays = 1;

    private void Awake()
    {
        ResolveReferences();
    }

    public LobbyConsumableUseResult GetUseResult(string consumableId, int? targetTileId = null)
    {
        ResolveReferences();

        if (!TryGetConsumableDefinition(consumableId, out SOConsumableData consumableData))
        {
            return LobbyConsumableUseResult.ConsumableNotFound;
        }

        if (_playerProfileService.GetConsumableCount(consumableId) <= 0)
        {
            return LobbyConsumableUseResult.NotOwned;
        }

        switch (consumableData.ConsumableType)
        {
            case LobbyConsumableType.StaminaRecover:
                return ValidateStaminaRecoverUse();

            case LobbyConsumableType.EmergencyDefense:
                return ValidateEmergencyDefenseUse(targetTileId);

            default:
                return LobbyConsumableUseResult.UnsupportedConsumableType;
        }
    }

    public LobbyConsumableUseResult TryUseSelectedConsumable(string consumableId)
    {
        int? selectedTileId = _territoryMapService != null
            ? _territoryMapService.SelectedTileId
            : null;

        return TryUseConsumable(consumableId, selectedTileId);
    }

    public LobbyConsumableUseResult TryUseConsumable(string consumableId, int? targetTileId = null)
    {
        ResolveReferences();

        LobbyConsumableUseResult validationResult = GetUseResult(consumableId, targetTileId);
        if (validationResult != LobbyConsumableUseResult.Success)
        {
            return validationResult;
        }

        if (!TryGetConsumableDefinition(consumableId, out SOConsumableData consumableData))
        {
            return LobbyConsumableUseResult.ConsumableNotFound;
        }

        if (!_playerProfileService.TrySpendConsumable(consumableId))
        {
            return LobbyConsumableUseResult.SpendFailed;
        }

        switch (consumableData.ConsumableType)
        {
            case LobbyConsumableType.StaminaRecover:
                _territoryTurnService.RestoreAttempts(StaminaRecoverAmount);
                return LobbyConsumableUseResult.Success;

            case LobbyConsumableType.EmergencyDefense:
                int resolvedTileId = ResolveTargetTileId(targetTileId).Value;
                TerritoryTileModifier protectModifier = new TerritoryTileModifier(
                    TerritoryTileModifierType.ProtectFromEnemyCapture,
                    EmergencyDefenseDurationDays,
                    consumableId);
                _territoryMapService.AddOrRefreshTileModifier(resolvedTileId, protectModifier);
                return LobbyConsumableUseResult.Success;

            default:
                return LobbyConsumableUseResult.UnsupportedConsumableType;
        }
    }

    private void ResolveReferences()
    {
        _territoryTurnService ??= FindFirstObjectByType<TerritoryTurnService>(FindObjectsInactive.Include);
        _territoryMapService ??= FindFirstObjectByType<TerritoryMapService>(FindObjectsInactive.Include);

        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            _playerProfileService = GameManager.I.PlayerProfileService;
        }

        _playerProfileService ??= FindFirstObjectByType<PlayerProfileService>(FindObjectsInactive.Include);
    }

    private bool TryGetConsumableDefinition(string consumableId, out SOConsumableData consumableData)
    {
        consumableData = null;

        return _playerProfileService != null
            && _playerProfileService.TryGetConsumable(consumableId, out consumableData);
    }

    private LobbyConsumableUseResult ValidateStaminaRecoverUse()
    {
        if (_territoryTurnService == null || !_territoryTurnService.IsInitialized)
        {
            return LobbyConsumableUseResult.TurnServiceNotReady;
        }

        TerritoryTurnState turnState = _territoryTurnService.CurrentState;
        if (turnState == null || turnState.RemainingAttempts >= turnState.MaxAttemptsPerDay)
        {
            return LobbyConsumableUseResult.StaminaAlreadyFull;
        }

        return LobbyConsumableUseResult.Success;
    }

    private LobbyConsumableUseResult ValidateEmergencyDefenseUse(int? targetTileId)
    {
        if (_territoryMapService == null || !_territoryMapService.IsInitialized)
        {
            return LobbyConsumableUseResult.MapServiceNotReady;
        }

        int? resolvedTileId = ResolveTargetTileId(targetTileId);
        if (!resolvedTileId.HasValue)
        {
            return LobbyConsumableUseResult.TargetTileRequired;
        }

        if (!_territoryMapService.TryGetTileState(resolvedTileId.Value, out TerritoryTileState tileState))
        {
            return LobbyConsumableUseResult.InvalidTargetTile;
        }

        if (tileState.Owner != TerritoryTileOwnerType.Player)
        {
            return LobbyConsumableUseResult.TileIsNotPlayerOwned;
        }

        if (tileState.PendingCaptureState != TerritoryPendingCaptureState.EnemyPlanned)
        {
            return LobbyConsumableUseResult.TileIsNotUnderAttack;
        }

        if (tileState.HasModifier(TerritoryTileModifierType.ProtectFromEnemyCapture))
        {
            return LobbyConsumableUseResult.TileAlreadyProtected;
        }

        return LobbyConsumableUseResult.Success;
    }

    private int? ResolveTargetTileId(int? requestedTileId)
    {
        if (requestedTileId.HasValue)
        {
            return requestedTileId.Value;
        }

        return _territoryMapService != null
            ? _territoryMapService.SelectedTileId
            : null;
    }
}
