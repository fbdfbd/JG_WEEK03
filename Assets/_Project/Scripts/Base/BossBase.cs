using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossState
{
    Idle,
    WaitingForSkill,
    CastingSkill,
    Recovering,
    Dead
}

public readonly struct BossRuntimeSkillSlot
{
    public SOBossSkillBase SkillData { get; }
    public BossSkillBase SkillLogic { get; }
    public int Weight { get; }
    public float MinHealthRatio { get; }
    public float MaxHealthRatio { get; }

    public BossRuntimeSkillSlot(
        SOBossSkillBase skillData,
        BossSkillBase skillLogic,
        int weight,
        float minHealthRatio,
        float maxHealthRatio)
    {
        SkillData = skillData;
        SkillLogic = skillLogic;
        Weight = Mathf.Max(1, weight);
        MinHealthRatio = Mathf.Clamp01(Mathf.Min(minHealthRatio, maxHealthRatio));
        MaxHealthRatio = Mathf.Clamp01(Mathf.Max(minHealthRatio, maxHealthRatio));
    }

    public bool IsValid => SkillData != null && SkillLogic != null;
}

public class BossBase : ObjectBase
{
    [System.Serializable]
    protected class BossSkillSlot
    {
        [SerializeField] private SOBossSkillBase skillData;
        [SerializeField] private BossSkillBase skillLogic;
        [Min(1)][SerializeField] private int weight = 1;
        [Range(0f, 1f)][SerializeField] private float minHealthRatio = 0f;
        [Range(0f, 1f)][SerializeField] private float maxHealthRatio = 1f;

        public SOBossSkillBase SkillData => skillData;
        public BossSkillBase SkillLogic => skillLogic;
        public int Weight => weight;
        public bool IsValid => skillData != null && skillLogic != null;

        public BossSkillSlot(
            SOBossSkillBase skillData,
            BossSkillBase skillLogic,
            int weight,
            float minHealthRatio,
            float maxHealthRatio)
        {
            this.skillData = skillData;
            this.skillLogic = skillLogic;
            this.weight = Mathf.Max(1, weight);
            this.minHealthRatio = Mathf.Clamp01(Mathf.Min(minHealthRatio, maxHealthRatio));
            this.maxHealthRatio = Mathf.Clamp01(Mathf.Max(minHealthRatio, maxHealthRatio));
        }

        public bool IsAvailableAtHealthRatio(float currentHealthRatio)
        {
            float clampedHealthRatio = Mathf.Clamp01(currentHealthRatio);
            return clampedHealthRatio >= minHealthRatio && clampedHealthRatio <= maxHealthRatio;
        }
    }

    [Header("Boss Runtime")]
    [SerializeField] private bool stopMovementWhileCasting = true;
    [SerializeField] private float emptySelectionDelay = 0.1f;
    [SerializeField] private float loopStartDelay = 0.25f;

    [Header("References")]
    [SerializeField] private PlayerController targetPlayer;
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private StageManager stageManager;
    [SerializeField] private BossSectorCombatController sectorCombatController;

    [Header("Skill Slots")]
    [SerializeField] private List<BossSkillSlot> skillSlots = new List<BossSkillSlot>();

    private readonly List<float> nextSkillReadyTimes = new List<float>();
    private readonly List<int> readySkillIndexes = new List<int>();

    private Coroutine bossLoopRoutine;
    private BossState currentState;
    private string runtimeDisplayName = string.Empty;

    public event System.Action StatusChanged;

    public BossState CurrentState => currentState;
    public bool IsCastingSkill => currentState == BossState.CastingSkill;
    public bool StopMovementWhileCasting => stopMovementWhileCasting;
    public BossSectorCombatController SectorCombatController => sectorCombatController;
    public string DisplayName => string.IsNullOrWhiteSpace(runtimeDisplayName) ? gameObject.name.Replace("(Clone)", string.Empty).Trim() : runtimeDisplayName;
    protected int SkillSlotCount => skillSlots.Count;

    protected override void Awake()
    {
        base.Awake();

        currentState = BossState.Idle;
        _hp = _maxHp;
        _isDead = false;

        CacheSceneReferences();
        InitializeSkillCooldowns();
    }

    protected virtual void Start()
    {
        StartBossLoop();
    }

    protected virtual void OnDisable()
    {
        StopBossLoop();
    }

    public override void TakeDamage(int damage)
    {
        if (_isDead || damage <= 0)
        {
            return;
        }

        _hp = Mathf.Max(0, _hp - damage);
        OnDamageTaken(damage);
        NotifyStatusChanged();

        if (_hp > 0)
        {
            return;
        }

        _isDead = true;
        SetBossState(BossState.Dead);
        StopBossLoop();
        OnDeath();
    }

    public void ReceiveHitPointDamage(in ProjectileHitData hitData, float damageMultiplier)
    {
        if (_isDead)
        {
            return;
        }

        int finalDamage = Mathf.RoundToInt(hitData.Damage * Mathf.Max(0f, damageMultiplier));
        if (finalDamage <= 0)
        {
            return;
        }

        TakeDamage(finalDamage);
        NotifyInstigatorAboutConfirmedHit(hitData, finalDamage);
    }

    protected override void OnDeath()
    {
        stageManager?.TryCompleteCurrentStage(true);
    }

    protected virtual void OnDamageTaken(int damage)
    {
    }

    protected virtual void OnSkillCastStarted(BossSkillBase skillLogic, SOBossSkillBase skillData)
    {
    }

    protected virtual void OnSkillCastFinished(BossSkillBase skillLogic, SOBossSkillBase skillData)
    {
    }

    public void ApplyRuntimeDisplayName(string displayName)
    {
        runtimeDisplayName = displayName ?? string.Empty;
        NotifyStatusChanged();
    }

    public void ApplyRuntimeMaxHealth(int maxHealth)
    {
        _maxHp = Mathf.Max(1, maxHealth);
        _hp = _maxHp;
        _isDead = false;
        NotifyStatusChanged();
    }

    public void ApplyRuntimeSkillSlots(IReadOnlyList<BossRuntimeSkillSlot> runtimeSkillSlots)
    {
        if (runtimeSkillSlots == null || runtimeSkillSlots.Count == 0)
        {
            return;
        }

        skillSlots.Clear();

        for (int i = 0; i < runtimeSkillSlots.Count; i++)
        {
            BossRuntimeSkillSlot runtimeSkillSlot = runtimeSkillSlots[i];
            if (!runtimeSkillSlot.IsValid)
            {
                continue;
            }

            skillSlots.Add(new BossSkillSlot(
                runtimeSkillSlot.SkillData,
                runtimeSkillSlot.SkillLogic,
                runtimeSkillSlot.Weight,
                runtimeSkillSlot.MinHealthRatio,
                runtimeSkillSlot.MaxHealthRatio));
        }

        InitializeSkillCooldowns();
        NotifyStatusChanged();
    }

    protected bool TryBuildSkillContext(out BossSkillContext skillContext)
    {
        CacheSceneReferences();

        if (faceBoard == null)
        {
            skillContext = default;
            return false;
        }

        if (sectorCombatController == null)
        {
            sectorCombatController = GetComponentInChildren<BossSectorCombatController>();
        }

        skillContext = new BossSkillContext(this, targetPlayer, faceBoard, stageManager, sectorCombatController);
        return true;
    }

    private void StartBossLoop()
    {
        if (bossLoopRoutine != null || _isDead)
        {
            return;
        }

        bossLoopRoutine = StartCoroutine(RunBossLoop());
    }

    private void StopBossLoop()
    {
        if (bossLoopRoutine == null)
        {
            return;
        }

        StopCoroutine(bossLoopRoutine);
        bossLoopRoutine = null;
    }

    private IEnumerator RunBossLoop()
    {
        if (loopStartDelay > 0f)
        {
            yield return new WaitForSeconds(loopStartDelay);
        }

        while (!_isDead)
        {
            if (!TryBuildSkillContext(out BossSkillContext skillContext))
            {
                yield return null;
                continue;
            }

            if (!TrySelectSkillSlot(skillContext, out int selectedSkillIndex))
            {
                SetBossState(BossState.WaitingForSkill);
                yield return WaitForEmptySelection();
                continue;
            }

            yield return CastSkillRoutine(selectedSkillIndex, skillContext);
        }
    }

    private IEnumerator CastSkillRoutine(int slotIndex, BossSkillContext skillContext)
    {
        BossSkillSlot skillSlot = skillSlots[slotIndex];

        SetBossState(BossState.CastingSkill);
        OnSkillCastStarted(skillSlot.SkillLogic, skillSlot.SkillData);

        yield return skillSlot.SkillLogic.Execute(skillContext, skillSlot.SkillData);

        MarkSkillCooldown(slotIndex);
        OnSkillCastFinished(skillSlot.SkillLogic, skillSlot.SkillData);

        float recoveryDuration = skillSlot.SkillData.RecoveryDuration;
        if (recoveryDuration > 0f)
        {
            SetBossState(BossState.Recovering);
            yield return new WaitForSeconds(recoveryDuration);
        }

        SetBossState(BossState.Idle);
    }

    private bool TrySelectSkillSlot(BossSkillContext skillContext, out int selectedSkillIndex)
    {
        selectedSkillIndex = -1;
        CollectReadySkillIndexes(skillContext);

        if (readySkillIndexes.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;

        for (int i = 0; i < readySkillIndexes.Count; i++)
        {
            int readySlotIndex = readySkillIndexes[i];
            totalWeight += skillSlots[readySlotIndex].Weight;
        }

        if (totalWeight <= 0)
        {
            selectedSkillIndex = readySkillIndexes[0];
            return true;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < readySkillIndexes.Count; i++)
        {
            int readySlotIndex = readySkillIndexes[i];
            currentWeight += skillSlots[readySlotIndex].Weight;

            if (randomWeight < currentWeight)
            {
                selectedSkillIndex = readySlotIndex;
                return true;
            }
        }

        selectedSkillIndex = readySkillIndexes[readySkillIndexes.Count - 1];
        return true;
    }

    private void CollectReadySkillIndexes(BossSkillContext skillContext)
    {
        readySkillIndexes.Clear();
        float currentHealthRatio = _maxHp > 0 ? (float)_hp / _maxHp : 0f;

        for (int i = 0; i < skillSlots.Count; i++)
        {
            BossSkillSlot skillSlot = skillSlots[i];
            if (!skillSlot.IsValid)
            {
                continue;
            }

            if (Time.time < nextSkillReadyTimes[i])
            {
                continue;
            }

            if (!skillSlot.IsAvailableAtHealthRatio(currentHealthRatio))
            {
                continue;
            }

            if (!skillSlot.SkillLogic.CanCast(skillContext, skillSlot.SkillData))
            {
                continue;
            }

            readySkillIndexes.Add(i);
        }
    }

    private void InitializeSkillCooldowns()
    {
        nextSkillReadyTimes.Clear();

        for (int i = 0; i < skillSlots.Count; i++)
        {
            nextSkillReadyTimes.Add(0f);
        }
    }

    private void MarkSkillCooldown(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Count)
        {
            return;
        }

        nextSkillReadyTimes[slotIndex] = Time.time + skillSlots[slotIndex].SkillData.Cooldown;
    }

    private void CacheSceneReferences()
    {
        if (targetPlayer == null)
        {
            targetPlayer = FindFirstObjectByType<PlayerController>();
        }

        if (faceBoard == null)
        {
            faceBoard = FindFirstObjectByType<FaceMapGenerator>();
        }

        if (stageManager == null)
        {
            stageManager = FindFirstObjectByType<StageManager>();
        }

        if (sectorCombatController == null)
        {
            sectorCombatController = GetComponentInChildren<BossSectorCombatController>();
        }
    }

    private YieldInstruction WaitForEmptySelection()
    {
        if (emptySelectionDelay <= 0f)
        {
            return null;
        }

        return new WaitForSeconds(emptySelectionDelay);
    }

    private void SetBossState(BossState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        currentState = nextState;
        NotifyStatusChanged();
    }

    private void NotifyStatusChanged()
    {
        StatusChanged?.Invoke();
    }

    private void NotifyInstigatorAboutConfirmedHit(in ProjectileHitData hitData, int appliedDamage)
    {
        if (appliedDamage <= 0 || hitData.Instigator == null)
        {
            return;
        }

        Transform instigatorRoot = hitData.Instigator.transform.root;
        if (instigatorRoot == null)
        {
            return;
        }

        PlayerCombatRewardService rewardService = instigatorRoot.GetComponent<PlayerCombatRewardService>();
        rewardService?.HandleConfirmedHit(hitData, appliedDamage);
    }

}
