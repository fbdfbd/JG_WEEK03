using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossSectorAttackSkill : BossSkillBase
{
    [Header("Targeting")]
    [SerializeField] private BossSectorTargetingMode targetingMode = BossSectorTargetingMode.FixedFaceIndex;
    [SerializeField] private int fixedCenterFaceIndex;
    [SerializeField] private int sectorHalfWidth = 1;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float activeDuration = 0.1f;
    [SerializeField] private bool hideTelegraphAfterAttack = true;

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
        if (skillData == null || skillContext.SectorCombatController == null)
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

        skillContext.SectorCombatController.ShowTelegraph(sectorRange, skillData.TelegraphDuration);

        if (skillData.TelegraphDuration > 0f)
        {
            yield return new WaitForSeconds(skillData.TelegraphDuration);
        }

        if (hideTelegraphAfterAttack)
        {
            skillContext.SectorCombatController.HideTelegraph();
        }

        FaceSectorAttackData attackData = new FaceSectorAttackData(
            sectorRange,
            damage,
            skillData.TelegraphDuration,
            activeDuration);

        skillContext.SectorCombatController.TryApplyDamageToPlayer(attackData);

        if (activeDuration > 0f)
        {
            yield return new WaitForSeconds(activeDuration);
        }
    }
}
