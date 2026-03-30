using System;
using System.Collections.Generic;

[Serializable]
public sealed class TerritoryTileRunState
{
    public int TileId;
    public TerritoryTileOwnerType Owner;
    public TerritoryTileContentType ContentType;
    public TerritoryTileFlags Flags;
    public TerritoryPendingCaptureState PendingCaptureState;
    public int PendingCaptureCreatedDay;
    public int PendingCaptureSourceTileId;
    public TerritoryPrimaryActionType PrimaryActionType;
    public List<TerritoryTileModifierRunState> Modifiers = new List<TerritoryTileModifierRunState>();
}
