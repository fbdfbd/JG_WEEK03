using System.Collections.Generic;

public sealed class TerritoryRunStateApplier
{
    // 저장된 런 상태를 현재 로비 맵과 턴 상태에 다시 덮어쓴다.
    public void Apply(
        TerritoryRunState runState,
        TerritoryMapService mapService,
        TerritoryTurnService turnService)
    {
        if (runState == null || mapService == null || turnService == null)
        {
            return;
        }

        turnService.ApplyRuntimeState(
            runState.CurrentDay,
            runState.MaxDay,
            runState.RemainingAttempts,
            runState.MaxAttemptsPerDay);

        for (int index = 0; index < runState.TileStates.Count; index++)
        {
            TerritoryTileRunState tileRunState = runState.TileStates[index];
            mapService.SetTileStateProfile(tileRunState.TileId, tileRunState.Owner, tileRunState.ContentType, tileRunState.Flags);
            mapService.SetTilePendingCapture(
                tileRunState.TileId,
                tileRunState.PendingCaptureState,
                tileRunState.PendingCaptureCreatedDay,
                tileRunState.PendingCaptureSourceTileId);
            mapService.SetTilePrimaryActionType(tileRunState.TileId, tileRunState.PrimaryActionType);
            mapService.ReplaceTileModifiers(tileRunState.TileId, CreateModifiers(tileRunState.Modifiers));
        }

        mapService.ClearSelection();
        if (runState.HasSelectedTile)
        {
            mapService.ToggleSelection(runState.SelectedTileId);
        }
    }

    private List<TerritoryTileModifier> CreateModifiers(IReadOnlyList<TerritoryTileModifierRunState> modifierRunStates)
    {
        List<TerritoryTileModifier> modifiers = new List<TerritoryTileModifier>();
        if (modifierRunStates == null)
        {
            return modifiers;
        }

        for (int index = 0; index < modifierRunStates.Count; index++)
        {
            TerritoryTileModifierRunState modifierRunState = modifierRunStates[index];
            modifiers.Add(new TerritoryTileModifier(
                modifierRunState.ModifierType,
                modifierRunState.RemainingDays,
                modifierRunState.SourceId));
        }

        return modifiers;
    }
}
