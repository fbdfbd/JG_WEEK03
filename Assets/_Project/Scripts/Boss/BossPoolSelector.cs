using System;

public sealed class BossPoolSelector
{
    public bool TrySelectBoss(
        SOBossPoolDefinition poolDefinition,
        StageEntryContext stageEntryContext,
        out SOBossDefinition bossDefinition)
    {
        bossDefinition = null;

        if (poolDefinition == null || poolDefinition.Entries == null || poolDefinition.Entries.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;

        for (int i = 0; i < poolDefinition.Entries.Count; i++)
        {
            SOBossPoolDefinition.BossPoolEntry entry = poolDefinition.Entries[i];
            if (entry == null || !entry.IsValid)
            {
                continue;
            }

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        Random random = new Random(CreateDeterministicSeed(stageEntryContext, poolDefinition.PoolId));
        int roll = random.Next(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < poolDefinition.Entries.Count; i++)
        {
            SOBossPoolDefinition.BossPoolEntry entry = poolDefinition.Entries[i];
            if (entry == null || !entry.IsValid)
            {
                continue;
            }

            currentWeight += entry.Weight;

            if (roll < currentWeight)
            {
                bossDefinition = entry.BossDefinition;
                return bossDefinition != null;
            }
        }

        return false;
    }

    private int CreateDeterministicSeed(StageEntryContext stageEntryContext, string poolId)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + GetStableStringHash(poolId);

            if (stageEntryContext == null)
            {
                return hash;
            }

            hash = hash * 31 + GetStableStringHash(stageEntryContext.RunId);
            hash = hash * 31 + stageEntryContext.TileId;
            hash = hash * 31 + stageEntryContext.Day;
            hash = hash * 31 + (int)stageEntryContext.StageType;
            hash = hash * 31 + (int)stageEntryContext.PrimaryActionType;
            hash = hash * 31 + (int)stageEntryContext.TileOwnerType;
            hash = hash * 31 + (int)stageEntryContext.TileContentType;
            return hash;
        }
    }

    private int GetStableStringHash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        unchecked
        {
            int hash = 23;

            for (int i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }

            return hash;
        }
    }
}
