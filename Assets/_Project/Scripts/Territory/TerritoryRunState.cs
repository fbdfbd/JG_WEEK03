using System;
using System.Collections.Generic;

[Serializable]
public sealed class TerritoryRunState
{
    private readonly Dictionary<int, TerritoryTileRunState> _tileStateById = new Dictionary<int, TerritoryTileRunState>();

    public string RunId;
    public int CurrentDay;
    public int MaxDay;
    public int RemainingAttempts;
    public int MaxAttemptsPerDay;
    public int SelectedTileId = -1;
    public List<TerritoryTileRunState> TileStates = new List<TerritoryTileRunState>();

    public bool HasSelectedTile => SelectedTileId >= 0;

    // 런타임에서 타일 상태를 빠르게 찾기 위해 캐시를 유지한다.
    public void RebuildLookup()
    {
        _tileStateById.Clear();

        for (int index = 0; index < TileStates.Count; index++)
        {
            TerritoryTileRunState tileState = TileStates[index];
            if (tileState == null)
            {
                continue;
            }

            _tileStateById[tileState.TileId] = tileState;
        }
    }

    public bool TryGetTileState(int tileId, out TerritoryTileRunState tileState)
    {
        return _tileStateById.TryGetValue(tileId, out tileState);
    }

    public void SetSelectedTile(int tileId)
    {
        SelectedTileId = tileId;
    }

    public void ClearSelectedTile()
    {
        SelectedTileId = -1;
    }
}
