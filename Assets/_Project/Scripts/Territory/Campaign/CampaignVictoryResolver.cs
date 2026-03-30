public sealed class CampaignVictoryResolver
{
    public CampaignOutcomeResult EvaluateAfterStage(
        StageEntryContext stageEntryContext,
        StageResult stageResult,
        TerritoryMapService mapService,
        TerritoryTurnService turnService)
    {
        if (stageEntryContext == null
            || stageResult == null
            || !stageResult.WasSuccess
            || !stageEntryContext.IsEnemyStartTile
            || mapService == null)
        {
            return default;
        }

        return new CampaignOutcomeResult(
            CampaignOutcomeType.Victory,
            CampaignOutcomeReason.EnemyStartCaptured,
            mapService.CountTilesOwnedBy(TerritoryTileOwnerType.Player),
            mapService.CountTilesOwnedBy(TerritoryTileOwnerType.Enemy));
    }

    public CampaignOutcomeResult EvaluateAfterDayAdvance(TerritoryMapService mapService, TerritoryTurnService turnService)
    {
        if (mapService == null || turnService == null || !turnService.IsInitialized || turnService.CurrentDay < turnService.MaxDay)
        {
            return default;
        }

        int playerTileCount = mapService.CountTilesOwnedBy(TerritoryTileOwnerType.Player);
        int enemyTileCount = mapService.CountTilesOwnedBy(TerritoryTileOwnerType.Enemy);

        CampaignOutcomeType outcomeType = CampaignOutcomeType.Draw;
        if (playerTileCount > enemyTileCount)
        {
            outcomeType = CampaignOutcomeType.Victory;
        }
        else if (enemyTileCount > playerTileCount)
        {
            outcomeType = CampaignOutcomeType.Defeat;
        }

        return new CampaignOutcomeResult(
            outcomeType,
            CampaignOutcomeReason.FinalDayTerritoryCount,
            playerTileCount,
            enemyTileCount);
    }
}
