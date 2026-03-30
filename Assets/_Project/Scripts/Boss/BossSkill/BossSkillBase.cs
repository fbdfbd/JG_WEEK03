using UnityEngine;
using System.Collections;

public readonly struct BossSkillContext
{
    public BossBase Boss { get; }
    public PlayerController Player { get; }
    public FaceMapGenerator FaceBoard { get; }
    public StageManager StageManager { get; }
    public BossSectorCombatController SectorCombatController { get; }

    public BossSkillContext(
        BossBase boss,
        PlayerController player,
        FaceMapGenerator faceBoard,
        StageManager stageManager,
        BossSectorCombatController sectorCombatController)
    {
        Boss = boss;
        Player = player;
        FaceBoard = faceBoard;
        StageManager = stageManager;
        SectorCombatController = sectorCombatController;
    }
}

public abstract class BossSkillBase : MonoBehaviour
{
    [Header("Boss Skill Identity")]
    [SerializeField] private string logicId = string.Empty;

    public string LogicId
    {
        get
        {
            return string.IsNullOrWhiteSpace(logicId)
                ? GetType().Name
                : logicId;
        }
    }

    public bool MatchesLogicId(string candidateLogicId)
    {
        if (string.IsNullOrWhiteSpace(candidateLogicId))
        {
            return false;
        }

        return string.Equals(LogicId, candidateLogicId, System.StringComparison.OrdinalIgnoreCase);
    }

    public abstract bool CanCast(BossSkillContext skillContext, SOBossSkillBase skillData);
    public abstract IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData);
}
