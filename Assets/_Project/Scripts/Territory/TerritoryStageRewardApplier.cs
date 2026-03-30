using UnityEngine;

public sealed class TerritoryStageRewardApplier
{
    public TerritoryStageRewardApplicationResult ApplyRewards(
        StageEntryContext stageEntryContext,
        StageResult stageResult,
        PlayerProfileService playerProfileService)
    {
        int sourceTileId = stageEntryContext != null
            ? stageEntryContext.TileId
            : stageResult != null
                ? stageResult.TileId
                : -1;

        if (stageResult == null || playerProfileService == null || !stageResult.WasSuccess)
        {
            return new TerritoryStageRewardApplicationResult(sourceTileId, 0);
        }

        int goldMultiplier = stageEntryContext != null
            ? Mathf.Max(1, stageEntryContext.RewardGoldMultiplier)
            : 1;

        int appliedGold = 0;

        if (stageResult.RewardGold > 0)
        {
            appliedGold = stageResult.RewardGold * goldMultiplier;
            playerProfileService.AddGold(appliedGold);
        }

        ApplyConsumableReward(
            playerProfileService,
            stageResult.RewardItemId,
            stageResult.RewardItemCount);

        if (stageEntryContext != null)
        {
            ApplyConsumableReward(
                playerProfileService,
                stageEntryContext.BonusRewardItemId,
                stageEntryContext.BonusRewardItemCount);
        }

        return new TerritoryStageRewardApplicationResult(sourceTileId, appliedGold);
    }

    private static void ApplyConsumableReward(
        PlayerProfileService playerProfileService,
        string consumableId,
        int consumableCount)
    {
        if (playerProfileService == null || string.IsNullOrWhiteSpace(consumableId))
        {
            return;
        }

        playerProfileService.AddConsumable(consumableId, Mathf.Max(1, consumableCount));
    }
}
