using System.Collections.Generic;

public sealed class TerritoryTileChangeSet
{
    private readonly HashSet<int> _allChangedTileIds = new HashSet<int>();
    private readonly List<int> _resolvedPendingCaptureTileIds = new List<int>();
    private readonly List<int> _newPendingCaptureTileIds = new List<int>();
    private readonly List<int> _enemyOccupiedTileIds = new List<int>();
    private readonly List<int> _playerOccupiedTileIds = new List<int>();

    public IReadOnlyCollection<int> AllChangedTileIds => _allChangedTileIds;
    public IReadOnlyList<int> ResolvedPendingCaptureTileIds => _resolvedPendingCaptureTileIds;
    public IReadOnlyList<int> NewPendingCaptureTileIds => _newPendingCaptureTileIds;
    public IReadOnlyList<int> EnemyOccupiedTileIds => _enemyOccupiedTileIds;
    public IReadOnlyList<int> PlayerOccupiedTileIds => _playerOccupiedTileIds;
    public bool HasAnyChange => _allChangedTileIds.Count > 0;

    public void AddResolvedPendingCapture(int tileId)
    {
        AddChanged(tileId);
        AddUnique(_resolvedPendingCaptureTileIds, tileId);
    }

    public void AddNewPendingCapture(int tileId)
    {
        AddChanged(tileId);
        AddUnique(_newPendingCaptureTileIds, tileId);
    }

    public void AddEnemyOccupied(int tileId)
    {
        AddChanged(tileId);
        AddUnique(_enemyOccupiedTileIds, tileId);
    }

    public void AddPlayerOccupied(int tileId)
    {
        AddChanged(tileId);
        AddUnique(_playerOccupiedTileIds, tileId);
    }

    public void AddChanged(int tileId)
    {
        _allChangedTileIds.Add(tileId);
    }

    private void AddUnique(List<int> resultList, int tileId)
    {
        if (!resultList.Contains(tileId))
        {
            resultList.Add(tileId);
        }
    }
}
