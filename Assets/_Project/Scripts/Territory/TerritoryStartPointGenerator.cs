using System;
using System.Collections.Generic;

public sealed class TerritoryStartPointGenerator
{
    private readonly List<TerritoryTileLayoutData> _edgeTiles = new List<TerritoryTileLayoutData>();
    private readonly List<TerritoryTileLayoutData> _candidateEnemyTiles = new List<TerritoryTileLayoutData>();

    // 시작지는 맵의 가장자리에서 뽑고, 가능하면 정대칭 위치를 우선 사용한다.
    public bool TryGenerate(
        TerritoryMapLayoutData layoutData,
        int minimumDistance,
        bool preferMirroredStartPoints,
        Random random,
        out TerritoryStartPointPair startPointPair)
    {
        startPointPair = default;

        if (layoutData == null || layoutData.TileCount == 0)
        {
            return false;
        }

        CollectEdgeTiles(layoutData);
        if (_edgeTiles.Count < 2)
        {
            return false;
        }

        if (preferMirroredStartPoints && TryPickMirroredPair(layoutData, minimumDistance, random, out startPointPair))
        {
            return true;
        }

        return TryPickDistancePair(minimumDistance, random, out startPointPair);
    }

    private void CollectEdgeTiles(TerritoryMapLayoutData layoutData)
    {
        _edgeTiles.Clear();

        for (int index = 0; index < layoutData.Tiles.Count; index++)
        {
            TerritoryTileLayoutData tile = layoutData.Tiles[index];
            if (tile.DistanceFromCenter == layoutData.Radius)
            {
                _edgeTiles.Add(tile);
            }
        }
    }

    private bool TryPickMirroredPair(
        TerritoryMapLayoutData layoutData,
        int minimumDistance,
        Random random,
        out TerritoryStartPointPair startPointPair)
    {
        startPointPair = default;

        int startIndex = random.Next(_edgeTiles.Count);
        for (int offset = 0; offset < _edgeTiles.Count; offset++)
        {
            TerritoryTileLayoutData playerStart = _edgeTiles[(startIndex + offset) % _edgeTiles.Count];
            HexCoord mirroredCoord = new HexCoord(-playerStart.Coord.Q, -playerStart.Coord.R);
            if (!layoutData.TryGetTile(mirroredCoord, out TerritoryTileLayoutData enemyStart))
            {
                continue;
            }

            if (enemyStart.DistanceFromCenter != layoutData.Radius)
            {
                continue;
            }

            if (playerStart.Coord.DistanceTo(enemyStart.Coord) < minimumDistance)
            {
                continue;
            }

            startPointPair = new TerritoryStartPointPair(playerStart.TileId, enemyStart.TileId);
            return true;
        }

        return false;
    }

    private bool TryPickDistancePair(int minimumDistance, Random random, out TerritoryStartPointPair startPointPair)
    {
        startPointPair = default;

        int startIndex = random.Next(_edgeTiles.Count);
        for (int playerOffset = 0; playerOffset < _edgeTiles.Count; playerOffset++)
        {
            TerritoryTileLayoutData playerStart = _edgeTiles[(startIndex + playerOffset) % _edgeTiles.Count];

            _candidateEnemyTiles.Clear();
            for (int enemyIndex = 0; enemyIndex < _edgeTiles.Count; enemyIndex++)
            {
                TerritoryTileLayoutData enemyStart = _edgeTiles[enemyIndex];
                if (enemyStart.TileId == playerStart.TileId)
                {
                    continue;
                }

                if (playerStart.Coord.DistanceTo(enemyStart.Coord) < minimumDistance)
                {
                    continue;
                }

                _candidateEnemyTiles.Add(enemyStart);
            }

            if (_candidateEnemyTiles.Count == 0)
            {
                continue;
            }

            TerritoryTileLayoutData selectedEnemyStart = _candidateEnemyTiles[random.Next(_candidateEnemyTiles.Count)];
            startPointPair = new TerritoryStartPointPair(playerStart.TileId, selectedEnemyStart.TileId);
            return true;
        }

        return false;
    }
}
