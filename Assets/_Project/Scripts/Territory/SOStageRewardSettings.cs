using UnityEngine;

[CreateAssetMenu(fileName = "StageRewardSettings", menuName = "Scriptable Objects/Territory/Stage Reward Settings")]
public sealed class SOStageRewardSettings : ScriptableObject
{
    [Header("Base Gold Rewards")]
    [Min(0)][SerializeField] private int defaultBattleGold = 25;
    [Min(0)][SerializeField] private int eventBattleGold = 25;
    [Min(0)][SerializeField] private int defenseBattleGold = 25;
    [Min(0)][SerializeField] private int enemyOccupiedBattleGold = 60;

    [Header("Reserved")]
    [Min(0)][SerializeField] private int enemyStartVictoryGold;

    public int DefaultBattleGold => Mathf.Max(0, defaultBattleGold);
    public int EventBattleGold => Mathf.Max(0, eventBattleGold);
    public int DefenseBattleGold => Mathf.Max(0, defenseBattleGold);
    public int EnemyOccupiedBattleGold => Mathf.Max(0, enemyOccupiedBattleGold);
    public int EnemyStartVictoryGold => Mathf.Max(0, enemyStartVictoryGold);
}
