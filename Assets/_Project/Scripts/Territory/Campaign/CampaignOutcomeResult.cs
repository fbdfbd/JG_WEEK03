public enum CampaignOutcomeType
{
    None,
    Victory,
    Defeat,
    Draw
}

public enum CampaignOutcomeReason
{
    None,
    EnemyStartCaptured,
    FinalDayTerritoryCount
}

public readonly struct CampaignOutcomeResult
{
    public CampaignOutcomeType OutcomeType { get; }
    public CampaignOutcomeReason Reason { get; }
    public int PlayerTileCount { get; }
    public int EnemyTileCount { get; }

    public bool IsFinished => OutcomeType != CampaignOutcomeType.None;

    public CampaignOutcomeResult(
        CampaignOutcomeType outcomeType,
        CampaignOutcomeReason reason,
        int playerTileCount,
        int enemyTileCount)
    {
        OutcomeType = outcomeType;
        Reason = reason;
        PlayerTileCount = playerTileCount < 0 ? 0 : playerTileCount;
        EnemyTileCount = enemyTileCount < 0 ? 0 : enemyTileCount;
    }
}
