using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossSkill_RandomSafeOppositeWeakStrike : BossSkillBase
{
    [Header("Weak Sector")]
    [SerializeField] private int weakSectorHalfWidth = 0;
    [SerializeField] private float weakSectorLeadDuration = 0.2f;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackActiveDuration = 0.15f;

    [Header("Cleanup")]
    [SerializeField] private bool hideTelegraphAfterAttack = true;
    [SerializeField] private bool clearWeakSectorAfterAttack = true;

    public override bool CanCast(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillData == null || skillContext.SectorCombatController == null)
        {
            return false;
        }

        if (!skillContext.SectorCombatController.TryGetFaceCount(out int faceCount))
        {
            return false;
        }

        return faceCount == 14;
    }

    public override IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillData == null || skillContext.SectorCombatController == null)
        {
            yield break;
        }

        BossSectorCombatController controller = skillContext.SectorCombatController;

        if (!controller.TryGetFaceCount(out int faceCount) || faceCount != 14)
        {
            yield break;
        }

        PreparePattern(controller);

        int safeFaceIndex = Random.Range(0, faceCount);
        int oppositeFaceIndex = WrapIndex(safeFaceIndex + faceCount / 2, faceCount);

        FaceSectorRange weakSectorRange = new FaceSectorRange(oppositeFaceIndex, weakSectorHalfWidth);
        FaceSectorRange attackSectorRange = new FaceSectorRange(oppositeFaceIndex, 6);

        controller.ApplyWeakSectorOverride(weakSectorRange);

        if (weakSectorLeadDuration > 0f)
        {
            yield return new WaitForSeconds(weakSectorLeadDuration);
        }

        controller.ShowTelegraph(attackSectorRange, skillData.TelegraphDuration);

        if (skillData.TelegraphDuration > 0f)
        {
            yield return new WaitForSeconds(skillData.TelegraphDuration);
        }

        if (hideTelegraphAfterAttack)
        {
            controller.HideTelegraph();
        }

        FaceSectorAttackData attackData = new FaceSectorAttackData(
            attackSectorRange,
            damage,
            skillData.TelegraphDuration,
            attackActiveDuration);

        controller.TryApplyDamageToPlayer(attackData);

        if (attackActiveDuration > 0f)
        {
            yield return new WaitForSeconds(attackActiveDuration);
        }

        FinishPattern(controller);
    }

    private void PreparePattern(BossSectorCombatController controller)
    {
        controller.HideTelegraph();
        controller.ClearWeakSectorOverride();
    }

    private void FinishPattern(BossSectorCombatController controller)
    {
        if (hideTelegraphAfterAttack)
        {
            controller.HideTelegraph();
        }

        if (clearWeakSectorAfterAttack)
        {
            controller.ClearWeakSectorOverride();
        }
    }

    private int WrapIndex(int index, int faceCount)
    {
        int wrappedIndex = index % faceCount;
        if (wrappedIndex < 0)
        {
            wrappedIndex += faceCount;
        }

        return wrappedIndex;
    }
}
