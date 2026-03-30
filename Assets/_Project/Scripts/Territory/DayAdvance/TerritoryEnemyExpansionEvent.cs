public readonly struct TerritoryEnemyExpansionEvent
{
    public int? SourceTileId { get; }
    public int TargetTileId { get; }
    public TerritoryEnemyExpansionEventType EventType { get; }

    public TerritoryEnemyExpansionEvent(int? sourceTileId, int targetTileId, TerritoryEnemyExpansionEventType eventType)
    {
        SourceTileId = sourceTileId;
        TargetTileId = targetTileId;
        EventType = eventType;
    }
}
