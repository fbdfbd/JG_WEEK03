using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TerritoryHexGrid
{
    private readonly struct PendingTileLayout
    {
        public HexCoord Coord { get; }
        public Vector3 LocalPosition { get; }
        public int DistanceFromCenter { get; }

        public PendingTileLayout(HexCoord coord, Vector3 localPosition, int distanceFromCenter)
        {
            Coord = coord;
            LocalPosition = localPosition;
            DistanceFromCenter = distanceFromCenter;
        }
    }

    public int Radius { get; }

    public TerritoryHexGrid(int radius)
    {
        if (radius < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be 0 or greater.");
        }

        Radius = radius;
    }

    public TerritoryMapLayoutData BuildLayout(TerritoryWorldPositionCalculator positionCalculator)
    {
        if (positionCalculator == null)
        {
            throw new ArgumentNullException(nameof(positionCalculator));
        }

        List<PendingTileLayout> pendingTiles = new List<PendingTileLayout>();

        for (int q = -Radius; q <= Radius; q++)
        {
            int minimumR = Mathf.Max(-Radius, -q - Radius);
            int maximumR = Mathf.Min(Radius, -q + Radius);

            for (int r = minimumR; r <= maximumR; r++)
            {
                HexCoord coord = new HexCoord(q, r);
                Vector3 localPosition = positionCalculator.CalculateLocalPosition(coord);
                pendingTiles.Add(new PendingTileLayout(coord, localPosition, coord.DistanceTo(HexCoord.Zero)));
            }
        }

        pendingTiles.Sort(CompareByVisualOrder);

        List<TerritoryTileLayoutData> tiles = new List<TerritoryTileLayoutData>(pendingTiles.Count);
        for (int index = 0; index < pendingTiles.Count; index++)
        {
            PendingTileLayout pendingTile = pendingTiles[index];
            tiles.Add(new TerritoryTileLayoutData(index, pendingTile.Coord, pendingTile.LocalPosition, pendingTile.DistanceFromCenter, index));
        }

        return new TerritoryMapLayoutData(Radius, tiles);
    }

    public int GetTileCount()
    {
        return 1 + (3 * Radius * (Radius + 1));
    }

    private static int CompareByVisualOrder(PendingTileLayout left, PendingTileLayout right)
    {
        int yComparison = right.LocalPosition.y.CompareTo(left.LocalPosition.y);
        if (yComparison != 0)
        {
            return yComparison;
        }

        return left.LocalPosition.x.CompareTo(right.LocalPosition.x);
    }
}
