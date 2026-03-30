using System;
using UnityEngine;

[Serializable]
public sealed class TerritoryGameSettings
{
    [Header("Start Points")]
    [SerializeField] [Min(1)] private int minimumStartDistance = 5;
    [SerializeField] private bool preferMirroredStartPoints = true;

    [Header("Turn Rules")]
    [SerializeField] [Min(1)] private int maxDay = 10;
    [SerializeField] [Min(1)] private int challengeAttemptsPerDay = 3;

    [Header("Daily Income")]
    [SerializeField] [Min(0)] private int goldPerOwnedTilePerDay = 15;

    [Header("Enemy Expansion")]
    [SerializeField] [Min(1)] private int baseEnemyExpansionCount = 1;
    [SerializeField] [Min(1)] private int expansionIncreaseInterval = 2;
    [SerializeField] [Min(1)] private int maxEnemyExpansionCount = 4;

    [Header("Map Content")]
    [SerializeField] [Range(0f, 1f)] private float neutralEventChance = 0.35f;

    [Header("Random")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int randomSeed = 1001;

    public int MinimumStartDistance => minimumStartDistance;
    public bool PreferMirroredStartPoints => preferMirroredStartPoints;
    public int MaxDay => maxDay;
    public int ChallengeAttemptsPerDay => challengeAttemptsPerDay;
    public int GoldPerOwnedTilePerDay => goldPerOwnedTilePerDay;
    public float NeutralEventChance => neutralEventChance;

    public int GetEnemyExpansionCount(int day)
    {
        int bonus = Mathf.Max(0, (day - 1) / Mathf.Max(1, expansionIncreaseInterval));
        return Mathf.Clamp(baseEnemyExpansionCount + bonus, 1, maxEnemyExpansionCount);
    }

    public int GetSeed(int salt = 0)
    {
        if (useRandomSeed)
        {
            return Environment.TickCount ^ (salt * 486187739);
        }

        return randomSeed + (salt * 486187739);
    }
}
