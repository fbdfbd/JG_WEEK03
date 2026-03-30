using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryPrimaryActionService : MonoBehaviour
{
    private TerritoryMapService _mapService;
    private TerritoryTurnService _turnService;
    private TerritoryActionRuleService _actionRuleService;

    public void Initialize(
        TerritoryMapService mapService,
        TerritoryTurnService turnService,
        TerritoryActionRuleService actionRuleService)
    {
        _mapService = mapService;
        _turnService = turnService;
        _actionRuleService = actionRuleService;
    }

    public bool TryCreateActionRequest(int tileId, out TerritoryPrimaryActionRequest actionRequest)
    {
        actionRequest = default;

        if (_mapService == null
            || _turnService == null
            || _actionRuleService == null
            || !_mapService.TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        TerritoryPrimaryActionType actionType = _actionRuleService.ResolvePrimaryActionType(tileState);
        if (actionType == TerritoryPrimaryActionType.None)
        {
            return false;
        }

        actionRequest = new TerritoryPrimaryActionRequest(
            tileState.TileId,
            _turnService.CurrentDay,
            ResolveStageType(tileState, actionType),
            actionType,
            tileState.Owner,
            tileState.ContentType);

        return true;
    }

    public bool TryConsumeAttempt()
    {
        return _turnService != null && _turnService.TryConsumeAttempt();
    }

    private TerritoryStageType ResolveStageType(TerritoryTileState tileState, TerritoryPrimaryActionType actionType)
    {
        if (actionType == TerritoryPrimaryActionType.Defend)
        {
            return TerritoryStageType.Defense;
        }

        if (tileState.Owner == TerritoryTileOwnerType.Enemy)
        {
            return TerritoryStageType.EnemyBattle;
        }

        return tileState.ContentType == TerritoryTileContentType.Event
            ? TerritoryStageType.Event
            : TerritoryStageType.Boss;
    }
}
