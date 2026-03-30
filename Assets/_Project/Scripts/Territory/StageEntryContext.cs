using System;

[Serializable]
public sealed class StageEntryContext
{
    public string RunId;
    public int TileId;
    public int Day;
    public string StageDefinitionId;
    public string SceneName;
    public TerritoryStageType StageType;
    public TerritoryPrimaryActionType PrimaryActionType;
    public TerritoryTileOwnerType TileOwnerType;
    public TerritoryTileContentType TileContentType;
    public bool IsDefense;
    public bool IsEnemyStartTile;
    public string SourceEventId;
    public string SourceEventTitle;
    public int RewardGoldMultiplier;
    public string BonusRewardItemId;
    public int BonusRewardItemCount;

    public bool IsValid => !string.IsNullOrWhiteSpace(SceneName);

    public StageEntryContext()
    {
        RunId = string.Empty;
        StageDefinitionId = string.Empty;
        SceneName = string.Empty;
        SourceEventId = string.Empty;
        SourceEventTitle = string.Empty;
        RewardGoldMultiplier = 1;
        BonusRewardItemId = string.Empty;
        BonusRewardItemCount = 0;
    }
}
