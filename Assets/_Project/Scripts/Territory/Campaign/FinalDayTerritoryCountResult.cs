using System.Collections.Generic;

public sealed class FinalDayTerritoryCountResult
{
    public IReadOnlyList<int> EnemyTileIds { get; }
    public IReadOnlyList<int> PlayerTileIds { get; }

    public int EnemyTileCount => EnemyTileIds != null ? EnemyTileIds.Count : 0;
    public int PlayerTileCount => PlayerTileIds != null ? PlayerTileIds.Count : 0;

    public FinalDayTerritoryCountResult(IReadOnlyList<int> enemyTileIds, IReadOnlyList<int> playerTileIds)
    {
        EnemyTileIds = enemyTileIds ?? System.Array.Empty<int>();
        PlayerTileIds = playerTileIds ?? System.Array.Empty<int>();
    }
}
