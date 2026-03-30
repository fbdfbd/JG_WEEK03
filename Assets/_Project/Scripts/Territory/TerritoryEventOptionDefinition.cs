using System;
using UnityEngine;

[Serializable]
public sealed class TerritoryEventOptionDefinition
{
    [SerializeField] private string label = string.Empty;
    [SerializeField] [TextArea(2, 4)] private string description = string.Empty;
    [SerializeField] [Min(0)] private int gainGold;
    [SerializeField] private string rewardConsumableId = string.Empty;
    [SerializeField] [Min(0)] private int rewardConsumableCount;
    [SerializeField] private bool captureTileImmediately;
    [SerializeField] private bool startBattle;
    [SerializeField] [Min(1)] private int battleRewardGoldMultiplier = 1;
    [SerializeField] private string battleRewardConsumableId = string.Empty;
    [SerializeField] [Min(0)] private int battleRewardConsumableCount;
    [SerializeField] private bool overrideBattleStageType;
    [SerializeField] private TerritoryStageType battleStageTypeOverride = TerritoryStageType.Event;

    public string Label => label;
    public string Description => description;
    public int GainGold => gainGold;
    public string RewardConsumableId => rewardConsumableId;
    public int RewardConsumableCount => rewardConsumableCount;
    public bool CaptureTileImmediately => captureTileImmediately;
    public bool StartBattle => startBattle;
    public int BattleRewardGoldMultiplier => battleRewardGoldMultiplier;
    public string BattleRewardConsumableId => battleRewardConsumableId;
    public int BattleRewardConsumableCount => battleRewardConsumableCount;
    public bool OverrideBattleStageType => overrideBattleStageType;
    public TerritoryStageType BattleStageTypeOverride => battleStageTypeOverride;

    public TerritoryEventOptionDefinition()
    {
    }

    public TerritoryEventOptionDefinition(
        string optionLabel,
        string optionDescription,
        int goldReward = 0,
        string consumableId = "",
        int consumableCount = 0,
        bool shouldCaptureTileImmediately = false,
        bool shouldStartBattle = false,
        int rewardGoldMultiplier = 1,
        string battleConsumableId = "",
        int battleConsumableCount = 0,
        bool shouldOverrideBattleStageType = false,
        TerritoryStageType overriddenBattleStageType = TerritoryStageType.Event)
    {
        label = optionLabel ?? string.Empty;
        description = optionDescription ?? string.Empty;
        gainGold = Mathf.Max(0, goldReward);
        rewardConsumableId = consumableId ?? string.Empty;
        rewardConsumableCount = Mathf.Max(0, consumableCount);
        captureTileImmediately = shouldCaptureTileImmediately;
        startBattle = shouldStartBattle;
        battleRewardGoldMultiplier = Mathf.Max(1, rewardGoldMultiplier);
        battleRewardConsumableId = battleConsumableId ?? string.Empty;
        battleRewardConsumableCount = Mathf.Max(0, battleConsumableCount);
        overrideBattleStageType = shouldOverrideBattleStageType;
        battleStageTypeOverride = overriddenBattleStageType;
    }
}
