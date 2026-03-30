using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossWeakSectorSkill : BossSkillBase
{
    [Header("Targeting")]
    [SerializeField] private BossSectorTargetingMode targetingMode = BossSectorTargetingMode.FixedFaceIndex;
    [SerializeField] private int fixedCenterFaceIndex;
    [SerializeField] private int sectorHalfWidth = 1;

    [Header("Weak Sector")]
    [SerializeField] private float activeDuration = 1f;
    [SerializeField] private bool clearWeakSectorOnFinish = true;

    public override bool CanCast(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillData == null || skillContext.SectorCombatController == null)
        {
            return false;
        }

        FaceSectorRange sectorRange;
        return skillContext.SectorCombatController.TryCreateSectorRange(
            targetingMode,
            fixedCenterFaceIndex,
            sectorHalfWidth,
            out sectorRange);
    }

    public override IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillContext.SectorCombatController == null)
        {
            yield break;
        }

        FaceSectorRange sectorRange;
        if (!skillContext.SectorCombatController.TryCreateSectorRange(
            targetingMode,
            fixedCenterFaceIndex,
            sectorHalfWidth,
            out sectorRange))
        {
            yield break;
        }

        skillContext.SectorCombatController.ApplyWeakSectorOverride(sectorRange);

        if (activeDuration > 0f)
        {
            yield return new WaitForSeconds(activeDuration);
        }

        if (clearWeakSectorOnFinish)
        {
            skillContext.SectorCombatController.ClearWeakSectorOverride();
        }
    }
}
