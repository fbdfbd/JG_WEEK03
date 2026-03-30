public sealed class BossEncounterResolver
{
    public BossTier ResolveTier(StageEntryContext stageEntryContext)
    {
        if (stageEntryContext == null)
        {
            return BossTier.Easy;
        }

        if (stageEntryContext.IsEnemyStartTile)
        {
            return BossTier.VeryHard;
        }

        if (stageEntryContext.TileOwnerType == TerritoryTileOwnerType.Enemy
            && stageEntryContext.TileContentType == TerritoryTileContentType.Boss)
        {
            return BossTier.Hard;
        }

        if (stageEntryContext.StageType == TerritoryStageType.Defense
            || stageEntryContext.StageType == TerritoryStageType.Event)
        {
            return BossTier.Normal;
        }

        return BossTier.Easy;
    }
}
