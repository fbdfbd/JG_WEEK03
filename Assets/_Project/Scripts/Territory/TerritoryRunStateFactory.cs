public sealed class TerritoryRunStateFactory
{
    // 현재 맵/턴 상태를 씬 전환이 가능한 순수 데이터로 복사한다.
    public TerritoryRunState Create(
        string runId,
        TerritoryMapService mapService,
        TerritoryTurnService turnService)
    {
        TerritoryRunState runState = new TerritoryRunState
        {
            RunId = runId,
            CurrentDay = turnService.CurrentDay,
            MaxDay = turnService.MaxDay,
            RemainingAttempts = turnService.RemainingAttempts,
            MaxAttemptsPerDay = turnService.CurrentState.MaxAttemptsPerDay,
        };

        if (mapService.SelectedTileId.HasValue)
        {
            runState.SetSelectedTile(mapService.SelectedTileId.Value);
        }

        for (int index = 0; index < mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = mapService.TileStates[index];
            TerritoryTileRunState tileRunState = new TerritoryTileRunState
            {
                TileId = tileState.TileId,
                Owner = tileState.Owner,
                ContentType = tileState.ContentType,
                Flags = tileState.Flags,
                PendingCaptureState = tileState.PendingCaptureState,
                PendingCaptureCreatedDay = tileState.PendingCaptureCreatedDay,
                PendingCaptureSourceTileId = tileState.PendingCaptureSourceTileId,
                PrimaryActionType = tileState.PrimaryActionType,
            };

            for (int modifierIndex = 0; modifierIndex < tileState.RuntimeModifiers.Count; modifierIndex++)
            {
                TerritoryTileModifier modifier = tileState.RuntimeModifiers[modifierIndex];
                tileRunState.Modifiers.Add(new TerritoryTileModifierRunState
                {
                    ModifierType = modifier.ModifierType,
                    RemainingDays = modifier.RemainingDays,
                    SourceId = modifier.SourceId,
                });
            }

            runState.TileStates.Add(tileRunState);
        }

        runState.RebuildLookup();
        return runState;
    }
}
