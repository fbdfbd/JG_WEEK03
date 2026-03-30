using System;
using UnityEngine;

public sealed class TerritoryWorldPositionCalculator
{
    public Vector2 TileWorldSize { get; }
    public Vector2 TileSpacing { get; }
    public Vector2 CenterOffset { get; }

    public TerritoryWorldPositionCalculator(Vector2 tileWorldSize, Vector2 tileSpacing, Vector2 centerOffset)
    {
        if (tileWorldSize.x <= 0f || tileWorldSize.y <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(tileWorldSize), "Tile world size must be greater than 0.");
        }

        TileWorldSize = tileWorldSize;
        TileSpacing = tileSpacing;
        CenterOffset = centerOffset;
    }

    public Vector3 CalculateLocalPosition(HexCoord coord)
    {
        float horizontalStep = (TileWorldSize.x + TileSpacing.x) * 0.75f;
        float verticalStep = TileWorldSize.y + TileSpacing.y;

        float x = coord.Q * horizontalStep;
        float y = -(coord.R + (coord.Q * 0.5f)) * verticalStep;
        return new Vector3(x + CenterOffset.x, y + CenterOffset.y, 0f);
    }
}
