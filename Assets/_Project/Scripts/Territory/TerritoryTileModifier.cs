using System;

[Serializable]
public sealed class TerritoryTileModifier
{
    public TerritoryTileModifierType ModifierType { get; }
    public int RemainingDays { get; private set; }
    public string SourceId { get; }

    public bool IsExpired => RemainingDays <= 0;

    public TerritoryTileModifier(TerritoryTileModifierType modifierType, int remainingDays, string sourceId = "")
    {
        ModifierType = modifierType;
        RemainingDays = remainingDays;
        SourceId = sourceId ?? string.Empty;
    }

    public void RefreshDuration(int remainingDays)
    {
        RemainingDays = remainingDays;
    }

    public void ConsumeDay()
    {
        RemainingDays--;
    }
}
