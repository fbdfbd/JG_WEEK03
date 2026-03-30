public readonly struct TerritoryStartPointPair
{
    public int PlayerStartTileId { get; }
    public int EnemyStartTileId { get; }

    public TerritoryStartPointPair(int playerStartTileId, int enemyStartTileId)
    {
        PlayerStartTileId = playerStartTileId;
        EnemyStartTileId = enemyStartTileId;
    }
}
