using System.Collections.Generic;
using UnityEngine;

public sealed class TerritoryDailyIncomeService
{
    public TerritoryDailyIncomeResult ApplyDailyIncome(
        TerritoryMapService mapService,
        PlayerProfileService playerProfileService,
        TerritoryGameSettings gameSettings)
    {
        if (mapService == null || playerProfileService == null || gameSettings == null || !mapService.IsInitialized)
        {
            return TerritoryDailyIncomeResult.None;
        }

        int goldPerTile = Mathf.Max(0, gameSettings.GoldPerOwnedTilePerDay);
        if (goldPerTile <= 0)
        {
            return TerritoryDailyIncomeResult.None;
        }

        List<TerritoryDailyIncomeEntry> entries = new List<TerritoryDailyIncomeEntry>();
        int totalGold = 0;

        for (int index = 0; index < mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = mapService.TileStates[index];
            if (tileState == null || tileState.Owner != TerritoryTileOwnerType.Player)
            {
                continue;
            }

            entries.Add(new TerritoryDailyIncomeEntry(tileState.TileId, goldPerTile));
            totalGold += goldPerTile;
        }

        if (totalGold <= 0)
        {
            return TerritoryDailyIncomeResult.None;
        }

        playerProfileService.AddGold(totalGold);
        return new TerritoryDailyIncomeResult(totalGold, entries);
    }
}
