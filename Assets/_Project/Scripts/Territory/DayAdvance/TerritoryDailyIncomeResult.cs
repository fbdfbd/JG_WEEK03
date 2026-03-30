using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TerritoryDailyIncomeResult
{
    private static readonly IReadOnlyList<TerritoryDailyIncomeEntry> EmptyEntries = Array.Empty<TerritoryDailyIncomeEntry>();

    public static TerritoryDailyIncomeResult None { get; } = new TerritoryDailyIncomeResult(0, EmptyEntries);

    public int TotalGold { get; }
    public IReadOnlyList<TerritoryDailyIncomeEntry> Entries { get; }
    public bool HasIncome => TotalGold > 0 && Entries.Count > 0;

    public TerritoryDailyIncomeResult(int totalGold, IReadOnlyList<TerritoryDailyIncomeEntry> entries)
    {
        TotalGold = Mathf.Max(0, totalGold);
        Entries = entries ?? EmptyEntries;
    }
}
