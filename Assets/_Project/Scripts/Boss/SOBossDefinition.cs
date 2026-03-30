using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossDefinition", menuName = "Scriptable Objects/Boss/Boss Definition")]
public sealed class SOBossDefinition : ScriptableObject
{
    [SerializeField] private string bossId = "boss_default";
    [SerializeField] private string displayName = "Boss";
    [SerializeField] private BossBase bossPrefab;
    [Min(1)][SerializeField] private int maxHealth = 10;
    [SerializeField] private List<SOBossPatternDefinition> patterns = new List<SOBossPatternDefinition>();

    public string BossId => bossId;
    public string DisplayName => displayName;
    public BossBase BossPrefab => bossPrefab;
    public int MaxHealth => Mathf.Max(1, maxHealth);
    public IReadOnlyList<SOBossPatternDefinition> Patterns => patterns;
    public bool HasPrefab => bossPrefab != null;
}
