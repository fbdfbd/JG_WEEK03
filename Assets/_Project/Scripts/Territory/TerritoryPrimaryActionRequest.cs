public readonly struct TerritoryPrimaryActionRequest
{
    public int TileId { get; }
    public int Day { get; }
    public TerritoryStageType StageType { get; }
    public TerritoryPrimaryActionType PrimaryActionType { get; }
    public TerritoryTileOwnerType TargetOwnerType { get; }
    public TerritoryTileContentType ContentType { get; }

    public bool IsValid => PrimaryActionType != TerritoryPrimaryActionType.None;
    public bool IsEventAction => StageType == TerritoryStageType.Event;

    public TerritoryPrimaryActionRequest(
        int tileId,
        int day,
        TerritoryStageType stageType,
        TerritoryPrimaryActionType primaryActionType,
        TerritoryTileOwnerType targetOwnerType,
        TerritoryTileContentType contentType)
    {
        TileId = tileId;
        Day = day;
        StageType = stageType;
        PrimaryActionType = primaryActionType;
        TargetOwnerType = targetOwnerType;
        ContentType = contentType;
    }
}
