using System;

public sealed class TerritoryMapSetupService
{
    private readonly TerritoryStartPointGenerator _startPointGenerator = new TerritoryStartPointGenerator();

    // 새 게임 시작 시 맵 상태를 한 번에 초기화한다.
    public bool TrySetupNewGame(TerritoryMapService mapService, TerritoryGameSettings settings)
    {
        if (mapService == null || settings == null || !mapService.IsInitialized)
        {
            return false;
        }

        Random random = new Random(settings.GetSeed());
        if (!_startPointGenerator.TryGenerate(
                mapService.LayoutData,
                settings.MinimumStartDistance,
                settings.PreferMirroredStartPoints,
                random,
                out TerritoryStartPointPair startPointPair))
        {
            return false;
        }

        InitializeNeutralTiles(mapService, settings, random);
        ApplyStartPoints(mapService, startPointPair);
        return true;
    }

    private void InitializeNeutralTiles(TerritoryMapService mapService, TerritoryGameSettings settings, Random random)
    {
        for (int index = 0; index < mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = mapService.TileStates[index];
            TerritoryTileContentType contentType = random.NextDouble() <= settings.NeutralEventChance
                ? TerritoryTileContentType.Event
                : TerritoryTileContentType.Boss;

            mapService.SetTileStateProfile(
                tileState.TileId,
                TerritoryTileOwnerType.Neutral,
                contentType,
                TerritoryTileFlags.None);

            mapService.ClearTilePendingCapture(tileState.TileId);
            mapService.SetTilePrimaryActionType(tileState.TileId, TerritoryPrimaryActionType.None);
            mapService.SetTileUnlocked(tileState.TileId, true);
            mapService.SetTileInteractable(tileState.TileId, true);
        }
    }

    private void ApplyStartPoints(TerritoryMapService mapService, TerritoryStartPointPair startPointPair)
    {
        mapService.SetTileStateProfile(
            startPointPair.PlayerStartTileId,
            TerritoryTileOwnerType.Player,
            TerritoryTileContentType.Boss,
            TerritoryTileFlags.PlayerStart);

        mapService.SetTileStateProfile(
            startPointPair.EnemyStartTileId,
            TerritoryTileOwnerType.Enemy,
            TerritoryTileContentType.Boss,
            TerritoryTileFlags.EnemyStart);
    }
}
