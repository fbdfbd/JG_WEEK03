using System;
using System.Collections.Generic;

public sealed class TerritoryDayAdvanceResult
{
    private static readonly IReadOnlyList<TerritoryEnemyExpansionEvent> EmptyEnemyExpansionEvents
        = Array.Empty<TerritoryEnemyExpansionEvent>();

    public static TerritoryDayAdvanceResult Failed(int currentDay, int remainingAttempts)
    {
        return new TerritoryDayAdvanceResult(
            false,
            currentDay,
            currentDay,
            remainingAttempts,
            new TerritoryTileChangeSet(),
            TerritoryDailyIncomeResult.None,
            EmptyEnemyExpansionEvents);
    }

    public bool WasSuccessful { get; }
    public int PreviousDay { get; }
    public int CurrentDay { get; }
    public int RemainingAttempts { get; }
    public TerritoryTileChangeSet TileChangeSet { get; }
    public TerritoryDailyIncomeResult IncomeResult { get; }
    public IReadOnlyList<TerritoryEnemyExpansionEvent> EnemyExpansionEvents { get; }

    public bool HasPresentationWork =>
        WasSuccessful &&
        ((TileChangeSet != null && TileChangeSet.HasAnyChange)
        || (IncomeResult != null && IncomeResult.HasIncome)
        || (EnemyExpansionEvents != null && EnemyExpansionEvents.Count > 0));

    public TerritoryDayAdvanceResult(
        bool wasSuccessful,
        int previousDay,
        int currentDay,
        int remainingAttempts,
        TerritoryTileChangeSet tileChangeSet,
        TerritoryDailyIncomeResult incomeResult,
        IReadOnlyList<TerritoryEnemyExpansionEvent> enemyExpansionEvents)
    {
        WasSuccessful = wasSuccessful;
        PreviousDay = previousDay;
        CurrentDay = currentDay;
        RemainingAttempts = remainingAttempts;
        TileChangeSet = tileChangeSet ?? new TerritoryTileChangeSet();
        IncomeResult = incomeResult ?? TerritoryDailyIncomeResult.None;
        EnemyExpansionEvents = enemyExpansionEvents ?? EmptyEnemyExpansionEvents;
    }
}
