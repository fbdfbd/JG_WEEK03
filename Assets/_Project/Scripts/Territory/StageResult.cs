using System;

[Serializable]
public sealed class StageResult
{
    public string RunId;
    public int TileId;
    public bool WasSuccess;
    public TerritoryStageType StageType;
    public TerritoryPrimaryActionType PrimaryActionType;
    public int RewardGold;
    public string RewardItemId;
    public int RewardItemCount;
    public int ClearedDay;

    public StageResult()
    {
        RunId = string.Empty;
        RewardItemId = string.Empty;
        RewardItemCount = 0;
    }
}
