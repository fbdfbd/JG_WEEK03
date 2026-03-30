public readonly struct TerritoryStageRewardApplicationResult
{
    public int SourceTileId { get; }
    public int AppliedGold { get; }

    public bool HasGoldReward => SourceTileId >= 0 && AppliedGold > 0;

    public TerritoryStageRewardApplicationResult(int sourceTileId, int appliedGold)
    {
        SourceTileId = sourceTileId;
        AppliedGold = appliedGold < 0 ? 0 : appliedGold;
    }
}
