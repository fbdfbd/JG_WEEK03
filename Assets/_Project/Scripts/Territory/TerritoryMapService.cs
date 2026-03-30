using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryMapService : MonoBehaviour
{
    [SerializeField] private bool unlockAllTilesOnInitialize = true;
    [SerializeField] private bool enableAllTilesOnInitialize = true;

    private readonly List<TerritoryTileState> _tileStates = new List<TerritoryTileState>();
    private readonly Dictionary<int, TerritoryTileState> _tileStateById = new Dictionary<int, TerritoryTileState>();
    private TerritoryMapLayoutData _layoutData;
    private int? _selectedTileId;

    public bool IsInitialized => _layoutData != null;
    public TerritoryMapLayoutData LayoutData => _layoutData;
    public IReadOnlyList<TerritoryTileState> TileStates => _tileStates;
    public int? SelectedTileId => _selectedTileId;

    public event Action Initialized;
    public event Action<TerritoryTileState> TileStateChanged;
    public event Action<TerritorySelectionResult> SelectionChanged;

    public void Initialize(TerritoryMapLayoutData layoutData)
    {
        if (layoutData == null)
        {
            Debug.LogWarning("TerritoryMapService requires a valid layout.");
            return;
        }

        _layoutData = layoutData;
        _selectedTileId = null;
        _tileStates.Clear();
        _tileStateById.Clear();

        for (int index = 0; index < layoutData.Tiles.Count; index++)
        {
            TerritoryTileLayoutData tileLayout = layoutData.Tiles[index];
            TerritoryTileState state = new TerritoryTileState(tileLayout.TileId, tileLayout.Coord, tileLayout.DistanceFromCenter);
            state.SetUnlocked(unlockAllTilesOnInitialize);
            state.SetInteractable(enableAllTilesOnInitialize);

            _tileStates.Add(state);
            _tileStateById[state.TileId] = state;
        }

        Initialized?.Invoke();
    }

    public bool TryGetTileState(int tileId, out TerritoryTileState tileState)
    {
        return _tileStateById.TryGetValue(tileId, out tileState);
    }

    public bool TryGetSelectedTileState(out TerritoryTileState tileState)
    {
        tileState = null;

        return _selectedTileId.HasValue
            && TryGetTileState(_selectedTileId.Value, out tileState);
    }

    public TerritorySelectionResult ToggleSelection(int tileId)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState) || !tileState.CanInteract)
        {
            return TerritorySelectionResult.None;
        }

        if (_selectedTileId == tileId)
        {
            return ClearSelection();
        }

        int? previousTileId = _selectedTileId;
        if (previousTileId.HasValue && TryGetTileState(previousTileId.Value, out TerritoryTileState previousSelectedState))
        {
            previousSelectedState.SetSelected(false);
            RaiseTileStateChanged(previousSelectedState);
        }

        _selectedTileId = tileId;
        tileState.SetSelected(true);
        RaiseTileStateChanged(tileState);

        TerritorySelectionChangeType changeType = previousTileId.HasValue
            ? TerritorySelectionChangeType.Changed
            : TerritorySelectionChangeType.Selected;

        TerritorySelectionResult selectionResult = new TerritorySelectionResult(
            changeType,
            previousTileId,
            _selectedTileId);

        SelectionChanged?.Invoke(selectionResult);
        return selectionResult;
    }

    public TerritorySelectionResult ClearSelection()
    {
        if (!_selectedTileId.HasValue)
        {
            return TerritorySelectionResult.None;
        }

        int previousTileId = _selectedTileId.Value;

        if (TryGetTileState(_selectedTileId.Value, out TerritoryTileState selectedState))
        {
            selectedState.SetSelected(false);
            RaiseTileStateChanged(selectedState);
        }

        _selectedTileId = null;

        TerritorySelectionResult selectionResult = new TerritorySelectionResult(
            TerritorySelectionChangeType.Deselected,
            previousTileId,
            null);

        SelectionChanged?.Invoke(selectionResult);
        return selectionResult;
    }

    // 타일의 소유/콘텐츠/시작지 정보를 한 번에 갱신할 때 사용한다.
    public bool SetTileStateProfile(
        int tileId,
        TerritoryTileOwnerType owner,
        TerritoryTileContentType contentType,
        TerritoryTileFlags flags)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        tileState.SetStateProfile(owner, contentType, flags);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTileUnlocked(int tileId, bool isUnlocked)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.IsUnlocked == isUnlocked)
        {
            return true;
        }

        tileState.SetUnlocked(isUnlocked);

        if (!tileState.CanInteract && tileState.IsSelected)
        {
            ClearSelection();
        }

        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTileInteractable(int tileId, bool isInteractable)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.IsInteractable == isInteractable)
        {
            return true;
        }

        tileState.SetInteractable(isInteractable);

        if (!tileState.CanInteract && tileState.IsSelected)
        {
            ClearSelection();
        }

        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTileOwner(int tileId, TerritoryTileOwnerType owner)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.Owner == owner)
        {
            return true;
        }

        tileState.SetOwner(owner);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTileContentType(int tileId, TerritoryTileContentType contentType)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.ContentType == contentType)
        {
            return true;
        }

        tileState.SetContentType(contentType);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTileFlags(int tileId, TerritoryTileFlags flags)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.Flags == flags)
        {
            return true;
        }

        tileState.SetFlags(flags);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTilePendingCapture(
        int tileId,
        TerritoryPendingCaptureState pendingCaptureState,
        int createdDay,
        int sourceTileId = -1)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        bool hasSameState = tileState.PendingCaptureState == pendingCaptureState
            && tileState.PendingCaptureCreatedDay == createdDay
            && tileState.PendingCaptureSourceTileId == sourceTileId;

        if (hasSameState)
        {
            return true;
        }

        tileState.SetPendingCapture(pendingCaptureState, createdDay, sourceTileId);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool ClearTilePendingCapture(int tileId)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.PendingCaptureState == TerritoryPendingCaptureState.None)
        {
            return true;
        }

        tileState.ClearPendingCapture();
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool SetTilePrimaryActionType(int tileId, TerritoryPrimaryActionType primaryActionType)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (tileState.PrimaryActionType == primaryActionType)
        {
            return true;
        }

        tileState.SetPrimaryActionType(primaryActionType);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool AddOrRefreshTileModifier(int tileId, TerritoryTileModifier modifier)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        tileState.AddOrRefreshModifier(modifier);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public bool ReplaceTileModifiers(int tileId, IReadOnlyList<TerritoryTileModifier> modifiers)
    {
        if (!TryGetTileState(tileId, out TerritoryTileState tileState))
        {
            return false;
        }

        tileState.ReplaceModifiers(modifiers);
        RaiseTileStateChanged(tileState);
        return true;
    }

    public int CountTilesOwnedBy(TerritoryTileOwnerType owner)
    {
        int count = 0;

        for (int index = 0; index < _tileStates.Count; index++)
        {
            if (_tileStates[index].Owner == owner)
            {
                count++;
            }
        }

        return count;
    }

    public bool AdvanceDayForRuntimeState()
    {
        bool hasAnyChange = false;

        for (int index = 0; index < _tileStates.Count; index++)
        {
            TerritoryTileState tileState = _tileStates[index];
            if (!tileState.AdvanceDay())
            {
                continue;
            }

            hasAnyChange = true;
            RaiseTileStateChanged(tileState);
        }

        return hasAnyChange;
    }

    public bool TryGetNeighborTileStates(int tileId, List<TerritoryTileState> resultStates)
    {
        resultStates?.Clear();

        if (resultStates == null || _layoutData == null)
        {
            return false;
        }

        List<int> neighborTileIds = new List<int>(6);
        if (!_layoutData.TryGetNeighborTileIds(tileId, neighborTileIds))
        {
            return false;
        }

        for (int index = 0; index < neighborTileIds.Count; index++)
        {
            int neighborTileId = neighborTileIds[index];
            if (_tileStateById.TryGetValue(neighborTileId, out TerritoryTileState neighborState))
            {
                resultStates.Add(neighborState);
            }
        }

        return true;
    }

    private void RaiseTileStateChanged(TerritoryTileState tileState)
    {
        TileStateChanged?.Invoke(tileState);
    }
}
