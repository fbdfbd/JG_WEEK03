using System.Collections.Generic;

public sealed class FinalDayTerritoryCollector
{
    private readonly List<TileSortEntry> _enemyEntries = new List<TileSortEntry>();
    private readonly List<TileSortEntry> _playerEntries = new List<TileSortEntry>();
    private readonly List<int> _enemyTileIds = new List<int>();
    private readonly List<int> _playerTileIds = new List<int>();

    public FinalDayTerritoryCountResult Collect(TerritoryMapController territoryMapController)
    {
        _enemyEntries.Clear();
        _playerEntries.Clear();
        _enemyTileIds.Clear();
        _playerTileIds.Clear();

        if (territoryMapController == null || territoryMapController.MapService == null)
        {
            return new FinalDayTerritoryCountResult(_enemyTileIds, _playerTileIds);
        }

        IReadOnlyList<TerritoryTileState> tileStates = territoryMapController.MapService.TileStates;
        for (int i = 0; i < tileStates.Count; i++)
        {
            TerritoryTileState tileState = tileStates[i];
            if (tileState == null)
            {
                continue;
            }

            if (!territoryMapController.TryGetTileView(tileState.TileId, out TerritoryTileView tileView) || tileView == null)
            {
                continue;
            }

            TileSortEntry sortEntry = new TileSortEntry(
                tileState.TileId,
                tileView.transform.position.x,
                tileView.transform.position.y);

            if (tileState.Owner == TerritoryTileOwnerType.Enemy)
            {
                _enemyEntries.Add(sortEntry);
            }
            else if (tileState.Owner == TerritoryTileOwnerType.Player)
            {
                _playerEntries.Add(sortEntry);
            }
        }

        _enemyEntries.Sort(TileSortEntryComparer.Instance);
        _playerEntries.Sort(TileSortEntryComparer.Instance);

        for (int i = 0; i < _enemyEntries.Count; i++)
        {
            _enemyTileIds.Add(_enemyEntries[i].TileId);
        }

        for (int i = 0; i < _playerEntries.Count; i++)
        {
            _playerTileIds.Add(_playerEntries[i].TileId);
        }

        return new FinalDayTerritoryCountResult(
            new List<int>(_enemyTileIds),
            new List<int>(_playerTileIds));
    }

    private readonly struct TileSortEntry
    {
        public int TileId { get; }
        public float X { get; }
        public float Y { get; }

        public TileSortEntry(int tileId, float x, float y)
        {
            TileId = tileId;
            X = x;
            Y = y;
        }
    }

    private sealed class TileSortEntryComparer : IComparer<TileSortEntry>
    {
        public static TileSortEntryComparer Instance { get; } = new TileSortEntryComparer();

        public int Compare(TileSortEntry left, TileSortEntry right)
        {
            int xCompare = left.X.CompareTo(right.X);
            if (xCompare != 0)
            {
                return xCompare;
            }

            return left.Y.CompareTo(right.Y);
        }
    }
}
