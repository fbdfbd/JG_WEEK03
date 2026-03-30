using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossSkill_DeployMine : BossSkillBase
{
    [Header("Mine")]
    [SerializeField] private BossMineSpec mineSpec = BossMineSpec.CreateDefault();

    [Header("Deployment")]
    [Min(1)][SerializeField] private int spawnCount = 1;
    [Min(0f)][SerializeField] private float intervalBetweenSpawns = 0.12f;

    private void Reset()
    {
        mineSpec = BossMineSpec.CreateDefault();
        spawnCount = 1;
        intervalBetweenSpawns = 0.12f;
    }

    public override bool CanCast(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (!TryGetHazardController(skillContext, out BossFaceHazardController hazardController))
        {
            return false;
        }

        return hazardController.HasAvailableFace(mineSpec.AllowSpawnOnPlayerFace);
    }

    public override IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (!TryGetHazardController(skillContext, out BossFaceHazardController hazardController))
        {
            yield break;
        }

        Vector3 throwStartWorldPosition = skillContext.Boss != null
            ? skillContext.Boss.transform.position
            : transform.position;

        int safeSpawnCount = Mathf.Max(1, spawnCount);

        for (int i = 0; i < safeSpawnCount; i++)
        {
            if (hazardController.TryGetRandomAvailableFaceIndex(mineSpec.AllowSpawnOnPlayerFace, out int faceIndex))
            {
                hazardController.TrySpawnMine(faceIndex, mineSpec, throwStartWorldPosition);
            }

            bool isLastSpawn = i >= safeSpawnCount - 1;
            if (!isLastSpawn && intervalBetweenSpawns > 0f)
            {
                yield return new WaitForSeconds(intervalBetweenSpawns);
            }
        }
    }

    private bool TryGetHazardController(BossSkillContext skillContext, out BossFaceHazardController hazardController)
    {
        hazardController = null;

        if (skillContext.Boss == null)
        {
            return false;
        }

        hazardController = skillContext.Boss.GetComponentInChildren<BossFaceHazardController>(true);
        return hazardController != null;
    }
}
