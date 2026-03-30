using System;
using System.Collections.Generic;

public sealed class TerritoryMapLayoutData
{
    private readonly List<TerritoryTileLayoutData> _tiles;
    private readonly Dictionary<int, TerritoryTileLayoutData> _tileById;
    private readonly Dictionary<HexCoord, TerritoryTileLayoutData> _tileByCoord;

    public int Radius { get; }
    public int TileCount => _tiles.Count;
    public IReadOnlyList<TerritoryTileLayoutData> Tiles => _tiles;

    public TerritoryMapLayoutData(int radius, IReadOnlyList<TerritoryTileLayoutData> tiles)
    {
        if (radius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be 0 or greater.");
        }

        if (tiles == null)
        {
            throw new ArgumentNullException(nameof(tiles));
        }

        Radius = radius;
        _tiles = new List<TerritoryTileLayoutData>(tiles.Count);
        _tileById = new Dictionary<int, TerritoryTileLayoutData>(tiles.Count);
        _tileByCoord = new Dictionary<HexCoord, TerritoryTileLayoutData>(tiles.Count);

        for (int index = 0; index < tiles.Count; index++)
        {
            TerritoryTileLayoutData tile = tiles[index];
            if (tile == null)
            {
                continue;
            }

            _tiles.Add(tile);
            _tileById[tile.TileId] = tile;
            _tileByCoord[tile.Coord] = tile;
        }
    }

    public bool TryGetTile(int tileId, out TerritoryTileLayoutData tile)
    {
        return _tileById.TryGetValue(tileId, out tile);
    }

    public bool TryGetTile(HexCoord coord, out TerritoryTileLayoutData tile)
    {
        return _tileByCoord.TryGetValue(coord, out tile);
    }

    public bool TryGetNeighborTileIds(int tileId, List<int> resultIds)
    {
        resultIds?.Clear();

        if (resultIds == null || !TryGetTile(tileId, out TerritoryTileLayoutData tile))
        {
            return false;
        }

        for (int directionIndex = 0; directionIndex < 6; directionIndex++)
        {
            HexCoord neighborCoord = tile.Coord.GetNeighbor(directionIndex);
            if (_tileByCoord.TryGetValue(neighborCoord, out TerritoryTileLayoutData neighborTile))
            {
                resultIds.Add(neighborTile.TileId);
            }
        }

        return true;
    }
}
