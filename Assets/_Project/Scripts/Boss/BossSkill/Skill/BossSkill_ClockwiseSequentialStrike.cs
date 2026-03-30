using System.Collections;
using UnityEngine;

public class BossSkill_ClockwiseSequentialWeakAndStrike : BossSkillBase
{
    [Header("Weak Sector")]
    [SerializeField] private bool useWeakSector = true;
    [SerializeField] private int weakSectorHalfWidth = 0;

    [Header("Attack Sequence")]
    [SerializeField] private int startFaceIndex;
    [SerializeField] private int strikeCount = 4;
    [SerializeField] private int faceStep = 1;
    [SerializeField] private int attackSectorHalfWidth = 0;
    [SerializeField] private int damage = 1;

    [Header("Timing")]
    [SerializeField] private float weakLeadDuration = 0.15f;
    [SerializeField] private float attackActiveDuration = 0.15f;
    [SerializeField] private float intervalBetweenStrikes = 0.1f;

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

        return faceCount > 0 && strikeCount > 0;
    }

    public override IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillData == null || skillContext.SectorCombatController == null)
        {
            yield break;
        }

        BossSectorCombatController controller = skillContext.SectorCombatController;

        if (!controller.TryGetFaceCount(out int faceCount) || faceCount <= 0)
        {
            yield break;
        }

        PreparePattern(controller);

        int currentCenterFaceIndex = WrapIndex(startFaceIndex, faceCount);

        for (int i = 0; i < strikeCount; i++)
        {
            FaceSectorRange weakSectorRange = new FaceSectorRange(currentCenterFaceIndex, weakSectorHalfWidth);
            FaceSectorRange attackSectorRange = new FaceSectorRange(currentCenterFaceIndex, attackSectorHalfWidth);

            if (useWeakSector)
            {
                controller.ApplyWeakSectorOverride(weakSectorRange);
            }

            if (weakLeadDuration > 0f)
            {
                yield return new WaitForSeconds(weakLeadDuration);
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

            bool isLastStrike = i == strikeCount - 1;
            if (!isLastStrike)
            {
                currentCenterFaceIndex = WrapIndex(currentCenterFaceIndex + faceStep, faceCount);
            }

            if (!isLastStrike && intervalBetweenStrikes > 0f)
            {
                yield return new WaitForSeconds(intervalBetweenStrikes);
            }
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
        if (faceCount <= 0)
        {
            return 0;
        }

        int wrappedIndex = index % faceCount;
        if (wrappedIndex < 0)
        {
            wrappedIndex += faceCount;
        }

        return wrappedIndex;
    }
}
