using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossSkill_DefaultExposeWeakThenStrike : BossSkillBase
{
    [Header("Weak Sector")]
    [SerializeField] private BossSectorTargetingMode weakSectorTargetingMode = BossSectorTargetingMode.OppositePlayerFace;
    [SerializeField] private int weakFixedCenterFaceIndex;
    [SerializeField] private int weakSectorHalfWidth = 1;
    [SerializeField] private float weakSectorLeadDuration = 1f;

    [Header("Attack Sector")]
    [SerializeField] private BossSectorTargetingMode attackSectorTargetingMode = BossSectorTargetingMode.PlayerCurrentFace;
    [SerializeField] private int attackFixedCenterFaceIndex;
    [SerializeField] private int attackSectorHalfWidth = 0;
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

        return TryBuildPattern(skillContext, out _);
    }

    public override IEnumerator Execute(BossSkillContext skillContext, SOBossSkillBase skillData)
    {
        if (skillData == null || skillContext.SectorCombatController == null)
        {
            yield break;
        }

        SectorPatternData patternData;
        if (!TryBuildPattern(skillContext, out patternData))
        {
            yield break;
        }

        PreparePattern(skillContext);
        OpenWeakSector(skillContext, patternData);

        if (weakSectorLeadDuration > 0f)
        {
            yield return new WaitForSeconds(weakSectorLeadDuration);
        }

        ShowAttackTelegraph(skillContext, skillData, patternData);

        if (skillData.TelegraphDuration > 0f)
        {
            yield return new WaitForSeconds(skillData.TelegraphDuration);
        }

        if (hideTelegraphAfterAttack)
        {
            skillContext.SectorCombatController.HideTelegraph();
        }

        ApplyAttack(skillContext, skillData, patternData);

        if (attackActiveDuration > 0f)
        {
            yield return new WaitForSeconds(attackActiveDuration);
        }

        FinishPattern(skillContext);
    }

    private bool TryBuildPattern(BossSkillContext skillContext, out SectorPatternData patternData)
    {
        patternData = default;

        FaceSectorRange weakSectorRange;
        if (!skillContext.SectorCombatController.TryCreateSectorRange(
            weakSectorTargetingMode,
            weakFixedCenterFaceIndex,
            weakSectorHalfWidth,
            out weakSectorRange))
        {
            return false;
        }

        FaceSectorRange attackSectorRange;
        if (!skillContext.SectorCombatController.TryCreateSectorRange(
            attackSectorTargetingMode,
            attackFixedCenterFaceIndex,
            attackSectorHalfWidth,
            out attackSectorRange))
        {
            return false;
        }

        patternData = new SectorPatternData(weakSectorRange, attackSectorRange);
        return true;
    }

    private void PreparePattern(BossSkillContext skillContext)
    {
        skillContext.SectorCombatController.HideTelegraph();
        skillContext.SectorCombatController.ClearWeakSectorOverride();
    }

    private void OpenWeakSector(BossSkillContext skillContext, SectorPatternData patternData)
    {
        skillContext.SectorCombatController.ApplyWeakSectorOverride(patternData.WeakSectorRange);
    }

    private void ShowAttackTelegraph(BossSkillContext skillContext, SOBossSkillBase skillData, SectorPatternData patternData)
    {
        skillContext.SectorCombatController.ShowTelegraph(patternData.AttackSectorRange, skillData.TelegraphDuration);
    }

    private void ApplyAttack(BossSkillContext skillContext, SOBossSkillBase skillData, SectorPatternData patternData)
    {
        FaceSectorAttackData attackData = new FaceSectorAttackData(
            patternData.AttackSectorRange,
            damage,
            skillData.TelegraphDuration,
            attackActiveDuration);

        skillContext.SectorCombatController.TryApplyDamageToPlayer(attackData);
    }

    private void FinishPattern(BossSkillContext skillContext)
    {
        if (hideTelegraphAfterAttack)
        {
            skillContext.SectorCombatController.HideTelegraph();
        }

        if (clearWeakSectorAfterAttack)
        {
            skillContext.SectorCombatController.ClearWeakSectorOverride();
        }
    }

    private readonly struct SectorPatternData
    {
        public FaceSectorRange WeakSectorRange { get; }
        public FaceSectorRange AttackSectorRange { get; }

        public SectorPatternData(FaceSectorRange weakSectorRange, FaceSectorRange attackSectorRange)
        {
            WeakSectorRange = weakSectorRange;
            AttackSectorRange = attackSectorRange;
        }
    }
}
