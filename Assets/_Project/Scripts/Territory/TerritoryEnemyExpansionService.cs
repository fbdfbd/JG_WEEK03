using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryEnemyExpansionService : MonoBehaviour
{
    private readonly List<int> _neighborTileIds = new List<int>(6);
    private readonly List<TerritoryTileState> _expansionCandidates = new List<TerritoryTileState>();
    private readonly List<TerritoryEnemyExpansionEvent> _expansionEvents = new List<TerritoryEnemyExpansionEvent>();

    private TerritoryMapService _mapService;
    private TerritoryGameSettings _settings;

    public void Initialize(TerritoryMapService mapService, TerritoryGameSettings settings)
    {
        _mapService = mapService;
        _settings = settings;
    }

    public IReadOnlyList<TerritoryEnemyExpansionEvent> ProcessNewDay(int currentDay, TerritoryTileChangeSet tileChangeSet)
    {
        _expansionEvents.Clear();

        if (_mapService == null || _settings == null || !_mapService.IsInitialized)
        {
            return _expansionEvents;
        }

        ResolveExpiredPendingCaptures(currentDay, tileChangeSet);
        MarkNewExpansionTargets(currentDay, tileChangeSet);
        return _expansionEvents;
    }

    private void ResolveExpiredPendingCaptures(int currentDay, TerritoryTileChangeSet tileChangeSet)
    {
        for (int index = 0; index < _mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = _mapService.TileStates[index];
            if (tileState.PendingCaptureState != TerritoryPendingCaptureState.EnemyPlanned)
            {
                continue;
            }

            if (tileState.PendingCaptureCreatedDay >= currentDay)
            {
                continue;
            }

            if (tileState.HasModifier(TerritoryTileModifierType.ProtectFromEnemyCapture))
            {
                _mapService.ClearTilePendingCapture(tileState.TileId);
                tileChangeSet?.AddResolvedPendingCapture(tileState.TileId);
                _expansionEvents.Add(new TerritoryEnemyExpansionEvent(
                    tileState.PendingCaptureSourceTileId >= 0 ? tileState.PendingCaptureSourceTileId : null,
                    tileState.TileId,
                    TerritoryEnemyExpansionEventType.BlockedByProtection));
                continue;
            }

            TerritoryEnemyExpansionEventType eventType = tileState.Owner == TerritoryTileOwnerType.Player
                ? TerritoryEnemyExpansionEventType.OccupiedPlayerTile
                : TerritoryEnemyExpansionEventType.OccupiedNeutralTile;

            _mapService.ClearTilePendingCapture(tileState.TileId);
            _mapService.SetTileOwner(tileState.TileId, TerritoryTileOwnerType.Enemy);
            _mapService.SetTileContentType(tileState.TileId, TerritoryTileContentType.Boss);
            tileChangeSet?.AddResolvedPendingCapture(tileState.TileId);
            tileChangeSet?.AddEnemyOccupied(tileState.TileId);
            _expansionEvents.Add(new TerritoryEnemyExpansionEvent(
                tileState.PendingCaptureSourceTileId >= 0 ? tileState.PendingCaptureSourceTileId : null,
                tileState.TileId,
                eventType));
        }
    }

    private void MarkNewExpansionTargets(int currentDay, TerritoryTileChangeSet tileChangeSet)
    {
        CollectExpansionCandidates();
        if (_expansionCandidates.Count == 0)
        {
            return;
        }

        int expansionCount = Mathf.Min(_settings.GetEnemyExpansionCount(currentDay), _expansionCandidates.Count);
        System.Random random = new System.Random(_settings.GetSeed(currentDay));
        ShuffleCandidates(random);

        for (int index = 0; index < expansionCount; index++)
        {
            TerritoryTileState targetTile = _expansionCandidates[index];
            int? sourceTileId = FindSourceEnemyTileId(targetTile.TileId);

            if (targetTile.Owner == TerritoryTileOwnerType.Player)
            {
                _mapService.SetTilePendingCapture(
                    targetTile.TileId,
                    TerritoryPendingCaptureState.EnemyPlanned,
                    currentDay,
                    sourceTileId ?? -1);
                tileChangeSet?.AddNewPendingCapture(targetTile.TileId);
                _expansionEvents.Add(new TerritoryEnemyExpansionEvent(
                    sourceTileId,
                    targetTile.TileId,
                    TerritoryEnemyExpansionEventType.PlannedOnPlayerTile));
                continue;
            }

            if (targetTile.Owner == TerritoryTileOwnerType.Neutral)
            {
                _mapService.SetTileOwner(targetTile.TileId, TerritoryTileOwnerType.Enemy);
                _mapService.SetTileContentType(targetTile.TileId, TerritoryTileContentType.Boss);
                tileChangeSet?.AddEnemyOccupied(targetTile.TileId);
                _expansionEvents.Add(new TerritoryEnemyExpansionEvent(
                    sourceTileId,
                    targetTile.TileId,
                    TerritoryEnemyExpansionEventType.OccupiedNeutralTile));
            }
        }
    }

    private void CollectExpansionCandidates()
    {
        _expansionCandidates.Clear();
        HashSet<int> candidateIds = new HashSet<int>();

        for (int index = 0; index < _mapService.TileStates.Count; index++)
        {
            TerritoryTileState sourceTile = _mapService.TileStates[index];
            if (sourceTile.Owner != TerritoryTileOwnerType.Enemy)
            {
                continue;
            }

            if (!_mapService.LayoutData.TryGetNeighborTileIds(sourceTile.TileId, _neighborTileIds))
            {
                continue;
            }

            for (int neighborIndex = 0; neighborIndex < _neighborTileIds.Count; neighborIndex++)
            {
                int neighborTileId = _neighborTileIds[neighborIndex];
                if (!candidateIds.Add(neighborTileId))
                {
                    continue;
                }

                if (!_mapService.TryGetTileState(neighborTileId, out TerritoryTileState candidateTile))
                {
                    continue;
                }

                if (candidateTile.Owner == TerritoryTileOwnerType.Enemy)
                {
                    continue;
                }

                if (candidateTile.PendingCaptureState == TerritoryPendingCaptureState.EnemyPlanned)
                {
                    continue;
                }

                if (candidateTile.HasModifier(TerritoryTileModifierType.ProtectFromEnemyCapture))
                {
                    continue;
                }

                _expansionCandidates.Add(candidateTile);
            }
        }
    }

    private int? FindSourceEnemyTileId(int targetTileId)
    {
        if (_mapService == null || !_mapService.LayoutData.TryGetNeighborTileIds(targetTileId, _neighborTileIds))
        {
            return null;
        }

        for (int index = 0; index < _neighborTileIds.Count; index++)
        {
            int neighborTileId = _neighborTileIds[index];
            if (_mapService.TryGetTileState(neighborTileId, out TerritoryTileState sourceTile)
                && sourceTile.Owner == TerritoryTileOwnerType.Enemy)
            {
                return sourceTile.TileId;
            }
        }

        return null;
    }

    private void ShuffleCandidates(System.Random random)
    {
        for (int index = _expansionCandidates.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            TerritoryTileState temp = _expansionCandidates[index];
            _expansionCandidates[index] = _expansionCandidates[swapIndex];
            _expansionCandidates[swapIndex] = temp;
        }
    }
}
