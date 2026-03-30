using UnityEngine;

[CreateAssetMenu(fileName = "BossPatternDefinition", menuName = "Scriptable Objects/Boss/Boss Pattern Definition")]
public sealed class SOBossPatternDefinition : ScriptableObject
{
    [SerializeField] private string patternId = "boss_pattern";
    [SerializeField] private string logicId = string.Empty;
    [SerializeField] private SOBossSkillBase skillData;
    [Min(1)][SerializeField] private int weight = 1;
    [Range(0f, 100f)][SerializeField] private float minHealthPercent = 0f;
    [Range(0f, 100f)][SerializeField] private float maxHealthPercent = 100f;

    public string PatternId => patternId;
    public string LogicId => logicId;
    public SOBossSkillBase SkillData => skillData;
    public int Weight => Mathf.Max(1, weight);

    public float MinHealthRatio
    {
        get
        {
            float clampedMin = Mathf.Clamp(minHealthPercent, 0f, 100f);
            float clampedMax = Mathf.Clamp(maxHealthPercent, 0f, 100f);
            return Mathf.Min(clampedMin, clampedMax) / 100f;
        }
    }

    public float MaxHealthRatio
    {
        get
        {
            float clampedMin = Mathf.Clamp(minHealthPercent, 0f, 100f);
            float clampedMax = Mathf.Clamp(maxHealthPercent, 0f, 100f);
            return Mathf.Max(clampedMin, clampedMax) / 100f;
        }
    }

    public bool IsValid
    {
        get
        {
            return !string.IsNullOrWhiteSpace(logicId) && skillData != null;
        }
    }
}
