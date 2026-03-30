public readonly struct TerritoryDailyIncomeEntry
{
    public int TileId { get; }
    public int GoldAmount { get; }

    public TerritoryDailyIncomeEntry(int tileId, int goldAmount)
    {
        TileId = tileId;
        GoldAmount = goldAmount < 0 ? 0 : goldAmount;
    }
}
