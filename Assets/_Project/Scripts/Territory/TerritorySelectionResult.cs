public readonly struct TerritorySelectionResult
{
    public static TerritorySelectionResult None => new TerritorySelectionResult(
        TerritorySelectionChangeType.None,
        null,
        null);

    public TerritorySelectionChangeType ChangeType { get; }
    public int? PreviousTileId { get; }
    public int? CurrentTileId { get; }
    public bool HasChanged => ChangeType != TerritorySelectionChangeType.None;

    public TerritorySelectionResult(
        TerritorySelectionChangeType changeType,
        int? previousTileId,
        int? currentTileId)
    {
        ChangeType = changeType;
        PreviousTileId = previousTileId;
        CurrentTileId = currentTileId;
    }
}
