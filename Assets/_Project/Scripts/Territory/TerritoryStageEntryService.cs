using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryStageEntryService : MonoBehaviour
{
    private TerritoryMapService _mapService;
    private TerritoryTurnService _turnService;
    private TerritoryActionRuleService _actionRuleService;

    public event Action<TerritoryStageRequest> StageEntryRequested;

    public void Initialize(
        TerritoryMapService mapService,
        TerritoryTurnService turnService,
        TerritoryActionRuleService actionRuleService)
    {
        _mapService = mapService;
        _turnService = turnService;
        _actionRuleService = actionRuleService;
    }

    // 실제 씬 전환은 나중에 붙이고, 지금은 "진입 요청"만 발행한다.
    public bool TryRequestStageEntry(int tileId, out TerritoryStageRequest stageRequest)
    {
        stageRequest = default;

        if (!TryCreateStageRequest(tileId, out stageRequest))
        {
            return false;
        }

        StageEntryRequested?.Invoke(stageRequest);
        return true;
    }

    public bool TryCreateStageRequest(int tileId, out TerritoryStageRequest stageRequest)
    {
        stageRequest = default;

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

        stageRequest = new TerritoryStageRequest(
            tileState.TileId,
            _turnService.CurrentDay,
            ResolveStageType(tileState, actionType),
            actionType,
            tileState.Owner,
            tileState.ContentType);

        return true;
    }

    public bool TryCreateStageRequest(
        TerritoryPrimaryActionRequest actionRequest,
        out TerritoryStageRequest stageRequest)
    {
        stageRequest = default;

        if (!actionRequest.IsValid)
        {
            return false;
        }

        stageRequest = new TerritoryStageRequest(
            actionRequest.TileId,
            actionRequest.Day,
            actionRequest.StageType,
            actionRequest.PrimaryActionType,
            actionRequest.TargetOwnerType,
            actionRequest.ContentType);

        return true;
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
