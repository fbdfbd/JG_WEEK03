using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossPoolDefinition", menuName = "Scriptable Objects/Boss/Boss Pool Definition")]
public sealed class SOBossPoolDefinition : ScriptableObject
{
    [System.Serializable]
    public sealed class BossPoolEntry
    {
        [SerializeField] private SOBossDefinition bossDefinition;
        [Min(1)][SerializeField] private int weight = 1;

        public SOBossDefinition BossDefinition => bossDefinition;
        public int Weight => Mathf.Max(1, weight);
        public bool IsValid => bossDefinition != null;
    }

    [SerializeField] private string poolId = "boss_pool";
    [SerializeField] private BossTier tier = BossTier.Easy;
    [SerializeField] private List<BossPoolEntry> entries = new List<BossPoolEntry>();

    public string PoolId => poolId;
    public BossTier Tier => tier;
    public IReadOnlyList<BossPoolEntry> Entries => entries;
}
