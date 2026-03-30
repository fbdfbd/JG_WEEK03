using System;

[Serializable]
public sealed class TerritoryTileModifierRunState
{
    public TerritoryTileModifierType ModifierType;
    public int RemainingDays;
    public string SourceId;

    public TerritoryTileModifierRunState()
    {
        SourceId = string.Empty;
    }
}
