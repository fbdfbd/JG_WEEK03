using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class StageBossBootstrap : MonoBehaviour
{
    [Header("Boss Data")]
    [SerializeField] private SOBossTierCatalog bossTierCatalog;

    [Header("Scene References")]
    [SerializeField] private BossBase existingSceneBoss;
    [SerializeField] private UI_StageBossPresenter bossPresenter;

    [Header("Options")]
    [SerializeField] private bool disableExistingSceneBossWhenUsingPrefab = true;

    private readonly BossEncounterResolver _bossEncounterResolver = new BossEncounterResolver();
    private readonly BossPoolSelector _bossPoolSelector = new BossPoolSelector();

    private void Awake()
    {
        ResolveReferences();
        BootstrapBoss();
    }

    private void ResolveReferences()
    {
        if (existingSceneBoss == null)
        {
            existingSceneBoss = FindFirstObjectByType<BossBase>(FindObjectsInactive.Include);
        }

        if (bossPresenter == null)
        {
            bossPresenter = FindFirstObjectByType<UI_StageBossPresenter>(FindObjectsInactive.Include);
        }
    }

    private void BootstrapBoss()
    {
        BossBase activeBoss = existingSceneBoss;

        if (!TrySelectBossDefinition(out StageEntryContext stageEntryContext, out SOBossDefinition selectedBossDefinition)
            || selectedBossDefinition == null)
        {
            bossPresenter?.SetTarget(activeBoss);
            return;
        }

        activeBoss = ResolveActiveBossInstance(selectedBossDefinition, activeBoss);
        if (activeBoss == null)
        {
            Debug.LogWarning("StageBossBootstrap could not find or create a boss instance.");
            return;
        }

        ApplyBossDefinition(activeBoss, selectedBossDefinition);
        bossPresenter?.SetTarget(activeBoss);

        Debug.Log($"StageBossBootstrap selected '{selectedBossDefinition.BossId}' for tile {stageEntryContext.TileId}.");
    }

    private bool TrySelectBossDefinition(out StageEntryContext stageEntryContext, out SOBossDefinition bossDefinition)
    {
        stageEntryContext = null;
        bossDefinition = null;

        if (bossTierCatalog == null
            || GameManager.I == null
            || GameManager.I.CampaignRunService == null
            || !GameManager.I.CampaignRunService.TryGetPendingStageEntry(out stageEntryContext)
            || stageEntryContext == null)
        {
            return false;
        }

        BossTier bossTier = _bossEncounterResolver.ResolveTier(stageEntryContext);
        SOBossPoolDefinition poolDefinition = bossTierCatalog.GetPool(bossTier);
        if (poolDefinition == null)
        {
            Debug.LogWarning($"StageBossBootstrap could not find a boss pool for tier '{bossTier}'.");
            return false;
        }

        return _bossPoolSelector.TrySelectBoss(poolDefinition, stageEntryContext, out bossDefinition);
    }

    private BossBase ResolveActiveBossInstance(SOBossDefinition bossDefinition, BossBase fallbackBoss)
    {
        if (bossDefinition != null && bossDefinition.HasPrefab)
        {
            Vector3 spawnPosition = fallbackBoss != null ? fallbackBoss.transform.position : Vector3.zero;
            Quaternion spawnRotation = fallbackBoss != null ? fallbackBoss.transform.rotation : Quaternion.identity;

            if (fallbackBoss != null && disableExistingSceneBossWhenUsingPrefab)
            {
                fallbackBoss.gameObject.SetActive(false);
            }

            return Instantiate(bossDefinition.BossPrefab, spawnPosition, spawnRotation);
        }

        return fallbackBoss;
    }

    private void ApplyBossDefinition(BossBase boss, SOBossDefinition bossDefinition)
    {
        if (boss == null || bossDefinition == null)
        {
            return;
        }

        boss.ApplyRuntimeDisplayName(bossDefinition.DisplayName);
        boss.ApplyRuntimeMaxHealth(bossDefinition.MaxHealth);

        List<BossRuntimeSkillSlot> runtimeSkillSlots = BuildRuntimeSkillSlots(boss, bossDefinition);
        if (runtimeSkillSlots.Count == 0)
        {
            Debug.LogWarning($"StageBossBootstrap could not build runtime skills for '{bossDefinition.BossId}'. Existing inspector slots will be used.");
            return;
        }

        boss.ApplyRuntimeSkillSlots(runtimeSkillSlots);
    }

    private List<BossRuntimeSkillSlot> BuildRuntimeSkillSlots(BossBase boss, SOBossDefinition bossDefinition)
    {
        List<BossRuntimeSkillSlot> runtimeSkillSlots = new List<BossRuntimeSkillSlot>();
        if (boss == null || bossDefinition == null)
        {
            return runtimeSkillSlots;
        }

        BossSkillBase[] skillLogics = boss.GetComponentsInChildren<BossSkillBase>(true);
        IReadOnlyList<SOBossPatternDefinition> patternDefinitions = bossDefinition.Patterns;

        for (int i = 0; i < patternDefinitions.Count; i++)
        {
            SOBossPatternDefinition patternDefinition = patternDefinitions[i];
            if (patternDefinition == null || !patternDefinition.IsValid)
            {
                continue;
            }

            if (!TryFindSkillLogic(skillLogics, patternDefinition.LogicId, out BossSkillBase skillLogic))
            {
                Debug.LogWarning($"StageBossBootstrap could not find BossSkillBase with logic id '{patternDefinition.LogicId}' on '{boss.name}'.");
                continue;
            }

            runtimeSkillSlots.Add(new BossRuntimeSkillSlot(
                patternDefinition.SkillData,
                skillLogic,
                patternDefinition.Weight,
                patternDefinition.MinHealthRatio,
                patternDefinition.MaxHealthRatio));
        }

        return runtimeSkillSlots;
    }

    private bool TryFindSkillLogic(BossSkillBase[] skillLogics, string logicId, out BossSkillBase skillLogic)
    {
        skillLogic = null;

        if (skillLogics == null || string.IsNullOrWhiteSpace(logicId))
        {
            return false;
        }

        for (int i = 0; i < skillLogics.Length; i++)
        {
            BossSkillBase currentSkillLogic = skillLogics[i];
            if (currentSkillLogic == null || !currentSkillLogic.MatchesLogicId(logicId))
            {
                continue;
            }

            skillLogic = currentSkillLogic;
            return true;
        }

        return false;
    }
}
