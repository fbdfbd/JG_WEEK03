using System;
using UnityEngine;

[Serializable]
public sealed class TerritoryStageSceneSettings
{
    [Header("Scene Names")]
    [SerializeField] private string bossSceneName = "03_Stage";
    [SerializeField] private string eventSceneName = "03_Stage";
    [SerializeField] private string enemyBattleSceneName = "03_Stage";
    [SerializeField] private string defenseSceneName = "03_Stage";

    [Header("Definition Ids")]
    [SerializeField] private string bossStageDefinitionId = "territory_boss_default";
    [SerializeField] private string eventStageDefinitionId = "territory_event_default";
    [SerializeField] private string enemyBattleStageDefinitionId = "territory_enemy_default";
    [SerializeField] private string defenseStageDefinitionId = "territory_defense_default";

    public string GetSceneName(TerritoryStageType stageType)
    {
        switch (stageType)
        {
            case TerritoryStageType.Event:
                return eventSceneName;

            case TerritoryStageType.EnemyBattle:
                return enemyBattleSceneName;

            case TerritoryStageType.Defense:
                return defenseSceneName;

            default:
                return bossSceneName;
        }
    }

    public string GetStageDefinitionId(TerritoryStageType stageType)
    {
        switch (stageType)
        {
            case TerritoryStageType.Event:
                return eventStageDefinitionId;

            case TerritoryStageType.EnemyBattle:
                return enemyBattleStageDefinitionId;

            case TerritoryStageType.Defense:
                return defenseStageDefinitionId;

            default:
                return bossStageDefinitionId;
        }
    }
}
