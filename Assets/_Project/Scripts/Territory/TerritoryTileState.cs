using System.Collections.Generic;

public sealed class TerritoryTileState
{
    private readonly List<TerritoryTileModifier> _runtimeModifiers = new List<TerritoryTileModifier>();

    public int TileId { get; }
    public HexCoord Coord { get; }
    public int DistanceFromCenter { get; }

    public bool IsUnlocked { get; private set; }
    public bool IsInteractable { get; private set; }
    public bool IsSelected { get; private set; }
    public TerritoryTileOwnerType Owner { get; private set; }
    public TerritoryTileContentType ContentType { get; private set; }
    public TerritoryTileFlags Flags { get; private set; }
    public TerritoryPendingCaptureState PendingCaptureState { get; private set; }
    public int PendingCaptureCreatedDay { get; private set; }
    public int PendingCaptureSourceTileId { get; private set; }
    public TerritoryPrimaryActionType PrimaryActionType { get; private set; }
    public IReadOnlyList<TerritoryTileModifier> RuntimeModifiers => _runtimeModifiers;

    public bool CanInteract => IsUnlocked && IsInteractable;
    public bool IsOccupied => Owner != TerritoryTileOwnerType.Neutral;
    public int OccupationLevel => IsOccupied ? 1 : 0;
    public bool IsPlayerStart => (Flags & TerritoryTileFlags.PlayerStart) != 0;
    public bool IsEnemyStart => (Flags & TerritoryTileFlags.EnemyStart) != 0;
    public bool HasAvailableAction => PrimaryActionType != TerritoryPrimaryActionType.None;

    public TerritoryTileState(int tileId, HexCoord coord, int distanceFromCenter)
    {
        TileId = tileId;
        Coord = coord;
        DistanceFromCenter = distanceFromCenter;
        IsUnlocked = true;
        IsInteractable = true;
        IsSelected = false;
        Owner = TerritoryTileOwnerType.Neutral;
        ContentType = TerritoryTileContentType.Boss;
        Flags = TerritoryTileFlags.None;
        PendingCaptureState = TerritoryPendingCaptureState.None;
        PendingCaptureCreatedDay = 0;
        PendingCaptureSourceTileId = -1;
        PrimaryActionType = TerritoryPrimaryActionType.None;
    }

    public void SetUnlocked(bool isUnlocked)
    {
        IsUnlocked = isUnlocked;
    }

    public void SetInteractable(bool isInteractable)
    {
        IsInteractable = isInteractable;
    }

    public void SetSelected(bool isSelected)
    {
        IsSelected = isSelected;
    }

    public void SetStateProfile(
        TerritoryTileOwnerType owner,
        TerritoryTileContentType contentType,
        TerritoryTileFlags flags)
    {
        Owner = owner;
        ContentType = contentType;
        Flags = flags;
    }

    public void SetOwner(TerritoryTileOwnerType owner)
    {
        Owner = owner;
    }

    public void SetContentType(TerritoryTileContentType contentType)
    {
        ContentType = contentType;
    }

    public void SetFlags(TerritoryTileFlags flags)
    {
        Flags = flags;
    }

    public void SetPendingCapture(TerritoryPendingCaptureState pendingCaptureState, int createdDay, int sourceTileId = -1)
    {
        PendingCaptureState = pendingCaptureState;
        PendingCaptureCreatedDay = createdDay;
        PendingCaptureSourceTileId = sourceTileId;
    }

    public void ClearPendingCapture()
    {
        PendingCaptureState = TerritoryPendingCaptureState.None;
        PendingCaptureCreatedDay = 0;
        PendingCaptureSourceTileId = -1;
    }

    public void SetPrimaryActionType(TerritoryPrimaryActionType primaryActionType)
    {
        PrimaryActionType = primaryActionType;
    }

    public void AddOrRefreshModifier(TerritoryTileModifier modifier)
    {
        if (modifier == null)
        {
            return;
        }

        for (int index = 0; index < _runtimeModifiers.Count; index++)
        {
            TerritoryTileModifier runtimeModifier = _runtimeModifiers[index];
            if (runtimeModifier.ModifierType != modifier.ModifierType)
            {
                continue;
            }

            runtimeModifier.RefreshDuration(modifier.RemainingDays);
            return;
        }

        _runtimeModifiers.Add(modifier);
    }

    public void ReplaceModifiers(IReadOnlyList<TerritoryTileModifier> modifiers)
    {
        _runtimeModifiers.Clear();

        if (modifiers == null)
        {
            return;
        }

        for (int index = 0; index < modifiers.Count; index++)
        {
            TerritoryTileModifier modifier = modifiers[index];
            if (modifier == null)
            {
                continue;
            }

            _runtimeModifiers.Add(new TerritoryTileModifier(
                modifier.ModifierType,
                modifier.RemainingDays,
                modifier.SourceId));
        }
    }

    public bool HasModifier(TerritoryTileModifierType modifierType)
    {
        for (int index = 0; index < _runtimeModifiers.Count; index++)
        {
            if (_runtimeModifiers[index].ModifierType == modifierType)
            {
                return true;
            }
        }

        return false;
    }

    public bool AdvanceDay()
    {
        bool hasChanged = false;

        for (int index = _runtimeModifiers.Count - 1; index >= 0; index--)
        {
            TerritoryTileModifier modifier = _runtimeModifiers[index];
            modifier.ConsumeDay();

            if (modifier.IsExpired)
            {
                _runtimeModifiers.RemoveAt(index);
                hasChanged = true;
            }
        }

        return hasChanged;
    }
}
