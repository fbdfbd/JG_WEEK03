using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryActionRuleService : MonoBehaviour
{
    private readonly List<TerritoryTileState> _neighborStates = new List<TerritoryTileState>(6);

    private TerritoryMapService _mapService;
    private TerritoryTurnService _turnService;

    public void Initialize(TerritoryMapService mapService, TerritoryTurnService turnService)
    {
        _mapService = mapService;
        _turnService = turnService;
    }

    // 현재 턴 규칙을 기준으로 각 타일의 대표 액션을 갱신한다.
    public void RefreshAvailableActions()
    {
        if (_mapService == null || _turnService == null || !_mapService.IsInitialized || !_turnService.IsInitialized)
        {
            return;
        }

        for (int index = 0; index < _mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = _mapService.TileStates[index];
            TerritoryPrimaryActionType actionType = ResolvePrimaryActionType(tileState);
            _mapService.SetTilePrimaryActionType(tileState.TileId, actionType);
        }
    }

    public TerritoryPrimaryActionType ResolvePrimaryActionType(TerritoryTileState tileState)
    {
        if (tileState == null || !_turnService.HasRemainingAttempts)
        {
            return TerritoryPrimaryActionType.None;
        }

        if (tileState.Owner == TerritoryTileOwnerType.Player
            && tileState.PendingCaptureState == TerritoryPendingCaptureState.EnemyPlanned)
        {
            return TerritoryPrimaryActionType.Defend;
        }

        if (!IsAdjacentToPlayerTerritory(tileState.TileId))
        {
            return TerritoryPrimaryActionType.None;
        }

        if (tileState.Owner == TerritoryTileOwnerType.Neutral)
        {
            return TerritoryPrimaryActionType.Challenge;
        }

        if (tileState.Owner == TerritoryTileOwnerType.Enemy)
        {
            return TerritoryPrimaryActionType.Attack;
        }

        return TerritoryPrimaryActionType.None;
    }

    private bool IsAdjacentToPlayerTerritory(int tileId)
    {
        if (_mapService == null || !_mapService.TryGetNeighborTileStates(tileId, _neighborStates))
        {
            return false;
        }

        for (int index = 0; index < _neighborStates.Count; index++)
        {
            if (_neighborStates[index].Owner == TerritoryTileOwnerType.Player)
            {
                return true;
            }
        }

        return false;
    }
}
