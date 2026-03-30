using UnityEngine;

[CreateAssetMenu(fileName = "SOBossSkillBase", menuName = "Scriptable Objects/SOBossSkillBase")]
public class SOBossSkillBase : ScriptableObject
{
    [SerializeField] private string skillId = "boss_skill";
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private float telegraphDuration = 0.5f;
    [SerializeField] private float recoveryDuration = 0.5f;

    public string SkillId => skillId;
    public float Cooldown => Mathf.Max(0f, cooldown);
    public float TelegraphDuration => Mathf.Max(0f, telegraphDuration);
    public float RecoveryDuration => Mathf.Max(0f, recoveryDuration);
}
