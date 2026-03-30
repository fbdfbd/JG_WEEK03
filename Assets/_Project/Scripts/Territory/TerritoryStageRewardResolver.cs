using UnityEngine;

public sealed class TerritoryStageRewardResolver
{
    public int ResolveGoldReward(StageEntryContext stageEntryContext, bool wasSuccess, SOStageRewardSettings rewardSettings)
    {
        if (!wasSuccess || stageEntryContext == null)
        {
            return 0;
        }

        if (rewardSettings == null)
        {
            return ResolveDefaultGoldReward(stageEntryContext);
        }

        if (stageEntryContext.IsEnemyStartTile)
        {
            return rewardSettings.EnemyStartVictoryGold;
        }

        switch (stageEntryContext.StageType)
        {
            case TerritoryStageType.Event:
                return rewardSettings.EventBattleGold;

            case TerritoryStageType.Defense:
                return rewardSettings.DefenseBattleGold;

            case TerritoryStageType.EnemyBattle:
                return rewardSettings.EnemyOccupiedBattleGold;

            case TerritoryStageType.Boss:
            default:
                return rewardSettings.DefaultBattleGold;
        }
    }

    private int ResolveDefaultGoldReward(StageEntryContext stageEntryContext)
    {
        if (stageEntryContext.IsEnemyStartTile)
        {
            return 0;
        }

        switch (stageEntryContext.StageType)
        {
            case TerritoryStageType.Event:
                return 25;

            case TerritoryStageType.Defense:
                return 25;

            case TerritoryStageType.EnemyBattle:
                return 60;

            case TerritoryStageType.Boss:
            default:
                return 25;
        }
    }
}
