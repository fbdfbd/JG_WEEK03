using UnityEngine;

public sealed class TerritoryTileLayoutData
{
    public int TileId { get; }
    public HexCoord Coord { get; }
    public Vector3 LocalPosition { get; }
    public int DistanceFromCenter { get; }
    public int SortingOrder { get; }

    public TerritoryTileLayoutData(int tileId, HexCoord coord, Vector3 localPosition, int distanceFromCenter, int sortingOrder)
    {
        TileId = tileId;
        Coord = coord;
        LocalPosition = localPosition;
        DistanceFromCenter = distanceFromCenter;
        SortingOrder = sortingOrder;
    }
}
