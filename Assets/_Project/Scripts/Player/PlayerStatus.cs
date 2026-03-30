using System;
using UnityEngine;

public readonly struct RecoveryTriggerResult
{
    public static RecoveryTriggerResult None { get; } = new RecoveryTriggerResult(false, 0, 0f);

    public bool Triggered { get; }
    public int RestoredHealth { get; }
    public float RestoredSpecialGauge { get; }

    public RecoveryTriggerResult(bool triggered, int restoredHealth, float restoredSpecialGauge)
    {
        Triggered = triggered;
        RestoredHealth = restoredHealth;
        RestoredSpecialGauge = restoredSpecialGauge;
    }
}

public sealed class PlayerStatus : MonoBehaviour
{
    [Header("Base Health")]
    [SerializeField] private int baseMaxHealth = 10;

    [Header("Base Resource")]
    [SerializeField] private float baseMaxSpecialGauge = 100f;
    [SerializeField] private float baseSpecialGaugeCost = 25f;
    [SerializeField] private float baseAcrossMoveGaugeCost = 15f;
    [SerializeField] private float baseGaugeRecoveryPerConfirmedHit = 1f;

    [Header("Base Damage")]
    [SerializeField] private int baseBasicAttackDamage = 1;
    [SerializeField] private int baseSpecialAttackDamage = 2;

    [Header("Base Timing")]
    [SerializeField] private float baseAdjacentMoveDelay = 0.2f;
    [SerializeField] private float baseAcrossMoveDelay = 0.42f;
    [SerializeField] private float baseBasicAttackDelay = 0.4f;

    [Header("Base Recovery Rule")]
    [SerializeField] private int baseSuccessfulHitCountForRecovery = 0;
    [SerializeField] private int baseRecoveryHealthOnTrigger = 0;

    [Header("Runtime Health")]
    [SerializeField] private int currentHealth;
    [SerializeField] private float currentSpecialGauge;
    [SerializeField] private int currentSuccessfulHitCount;

    [Header("Runtime Modifiers")]
    [SerializeField] private int maxHealthBonus;
    [SerializeField] private float maxSpecialGaugeBonus;
    [SerializeField] private int basicAttackDamageBonus;
    [SerializeField] private int specialAttackDamageBonus;
    [SerializeField] private float moveDelayScale = 1f;
    [SerializeField] private float basicAttackDelayScale = 1f;
    [SerializeField] private float specialGaugeCostScale = 1f;

    [Header("Runtime Recovery Rule")]
    [SerializeField] private int successfulHitCountForRecovery;
    [SerializeField] private int recoveryHealthOnTrigger;
    [SerializeField] private float recoverySpecialGaugeOnTrigger;

    public event Action StatusChanged;

    public void InitializeStatus()
    {
        InitializeFromLoadout(PlayerStatLoadout.Empty);
    }

    public void InitializeFromLoadout(PlayerStatLoadout statLoadout)
    {
        ApplyStatLoadoutInternal(statLoadout);
        currentHealth = GetMaxHealth();
        currentSpecialGauge = GetMaxSpecialGauge();
        currentSuccessfulHitCount = 0;
        NotifyStatusChanged();
    }

    public void FillRuntimeResourcesToMax()
    {
        currentHealth = GetMaxHealth();
        currentSpecialGauge = GetMaxSpecialGauge();
        currentSuccessfulHitCount = 0;
        NotifyStatusChanged();
    }

    public void ResetTemporaryModifiers()
    {
        ApplyStatLoadout(PlayerStatLoadout.Empty);
    }

    public void ApplyStatLoadout(PlayerStatLoadout statLoadout)
    {
        ApplyStatLoadoutInternal(statLoadout);
        ClampRuntimeValues();
        NotifyStatusChanged();
    }

    public void SetMaxHealthBonus(int bonus)
    {
        maxHealthBonus = bonus;
        ClampRuntimeValues();
        NotifyStatusChanged();
    }

    public void SetMaxSpecialGaugeBonus(float bonus)
    {
        maxSpecialGaugeBonus = bonus;
        ClampRuntimeValues();
        NotifyStatusChanged();
    }

    public void SetBasicAttackDamageBonus(int bonus)
    {
        basicAttackDamageBonus = bonus;
    }

    public void SetSpecialAttackDamageBonus(int bonus)
    {
        specialAttackDamageBonus = bonus;
    }

    public void SetMoveDelayScale(float scale)
    {
        moveDelayScale = SanitizeScale(scale);
    }

    public void SetBasicAttackDelayScale(float scale)
    {
        basicAttackDelayScale = SanitizeScale(scale);
    }

    public void SetSpecialGaugeCostScale(float scale)
    {
        specialGaugeCostScale = Mathf.Max(0f, scale);
    }

    public void SetSuccessfulHitCountForRecovery(int hitCount)
    {
        successfulHitCountForRecovery = Mathf.Max(0, hitCount);
        currentSuccessfulHitCount = Mathf.Min(currentSuccessfulHitCount, successfulHitCountForRecovery);
    }

    public void SetRecoveryHealthOnTrigger(int amount)
    {
        recoveryHealthOnTrigger = Mathf.Max(0, amount);
    }

    public void SetRecoverySpecialGaugeOnTrigger(float amount)
    {
        recoverySpecialGaugeOnTrigger = Mathf.Max(0f, amount);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return Mathf.Max(1, baseMaxHealth + maxHealthBonus);
    }

    public float GetCurrentSpecialGauge()
    {
        return currentSpecialGauge;
    }

    public float GetMaxSpecialGauge()
    {
        return Mathf.Max(0f, baseMaxSpecialGauge + maxSpecialGaugeBonus);
    }

    public int GetBasicAttackDamage()
    {
        return Mathf.Max(0, baseBasicAttackDamage + basicAttackDamageBonus);
    }

    public int GetSpecialAttackDamage()
    {
        return Mathf.Max(0, baseSpecialAttackDamage + specialAttackDamageBonus);
    }

    public float GetAdjacentMoveDelay()
    {
        return Mathf.Max(0.01f, baseAdjacentMoveDelay * moveDelayScale);
    }

    public float GetAcrossMoveDelay()
    {
        return Mathf.Max(0.01f, baseAcrossMoveDelay * moveDelayScale);
    }

    public float GetBasicAttackDelay()
    {
        return Mathf.Max(0.01f, baseBasicAttackDelay * basicAttackDelayScale);
    }

    public float GetSpecialGaugeCost()
    {
        return Mathf.Max(0f, baseSpecialGaugeCost * specialGaugeCostScale);
    }

    public float GetAcrossMoveGaugeCost()
    {
        return GetScaledSpecialGaugeCost(baseAcrossMoveGaugeCost);
    }

    public float GetScaledSpecialGaugeCost(float baseCost)
    {
        return Mathf.Max(0f, baseCost * specialGaugeCostScale);
    }

    public int GetSuccessfulHitCountForRecovery()
    {
        return successfulHitCountForRecovery;
    }

    public int GetRecoveryHealthOnTrigger()
    {
        return recoveryHealthOnTrigger;
    }

    public float GetRecoverySpecialGaugeOnTrigger()
    {
        return recoverySpecialGaugeOnTrigger;
    }

    public bool HasNoHealthRemaining()
    {
        return currentHealth <= 0;
    }

    public void ApplyDamage(int damage)
    {
        if (damage <= 0 || HasNoHealthRemaining())
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        ResetSuccessfulHitCounter();
        NotifyStatusChanged();
    }

    public void RestoreHealth(int amount)
    {
        if (amount <= 0 || HasNoHealthRemaining())
        {
            return;
        }

        currentHealth = Mathf.Min(GetMaxHealth(), currentHealth + amount);
        NotifyStatusChanged();
    }

    public bool TryConsumeSpecialGauge(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (currentSpecialGauge < amount)
        {
            return false;
        }

        currentSpecialGauge -= amount;
        NotifyStatusChanged();
        return true;
    }

    public bool TryConsumeScaledSpecialGauge(float baseCost, out float consumedAmount)
    {
        consumedAmount = GetScaledSpecialGaugeCost(baseCost);
        return TryConsumeSpecialGauge(consumedAmount);
    }

    public void RestoreSpecialGauge(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentSpecialGauge = Mathf.Min(GetMaxSpecialGauge(), currentSpecialGauge + amount);
        NotifyStatusChanged();
    }

    public void RestoreGaugeOnConfirmedHit()
    {
        RestoreSpecialGauge(baseGaugeRecoveryPerConfirmedHit);
    }

    public RecoveryTriggerResult RegisterConfirmedHitWithoutTakingDamage()
    {
        bool hasRecoveryReward = recoveryHealthOnTrigger > 0 || recoverySpecialGaugeOnTrigger > 0f;
        if (successfulHitCountForRecovery <= 0 || !hasRecoveryReward)
        {
            return RecoveryTriggerResult.None;
        }

        currentSuccessfulHitCount++;

        if (currentSuccessfulHitCount < successfulHitCountForRecovery)
        {
            return RecoveryTriggerResult.None;
        }

        currentSuccessfulHitCount = 0;
        if (recoveryHealthOnTrigger > 0)
        {
            RestoreHealth(recoveryHealthOnTrigger);
        }

        if (recoverySpecialGaugeOnTrigger > 0f)
        {
            RestoreSpecialGauge(recoverySpecialGaugeOnTrigger);
        }

        return new RecoveryTriggerResult(true, recoveryHealthOnTrigger, recoverySpecialGaugeOnTrigger);
    }

    public int RegisterSuccessfulHitWithoutTakingDamage()
    {
        return RegisterConfirmedHitWithoutTakingDamage().RestoredHealth;
    }

    public int GetSpecialAttackDamageWithBonus(int baseDamage)
    {
        return Mathf.Max(0, baseDamage + specialAttackDamageBonus);
    }

    public void ResetSuccessfulHitCounter()
    {
        currentSuccessfulHitCount = 0;
    }

    private void ClampRuntimeValues()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0, GetMaxHealth());
        currentSpecialGauge = Mathf.Clamp(currentSpecialGauge, 0f, GetMaxSpecialGauge());
    }

    private void ApplyStatLoadoutInternal(PlayerStatLoadout statLoadout)
    {
        maxHealthBonus = statLoadout.MaxHealthBonus;
        maxSpecialGaugeBonus = statLoadout.MaxSpecialGaugeBonus;
        basicAttackDamageBonus = statLoadout.BasicAttackDamageBonus;
        specialAttackDamageBonus = statLoadout.SpecialAttackDamageBonus;
        moveDelayScale = SanitizeScale(statLoadout.MoveDelayScale);
        basicAttackDelayScale = SanitizeScale(statLoadout.BasicAttackDelayScale);
        specialGaugeCostScale = Mathf.Max(0f, statLoadout.SpecialGaugeCostScale);
        successfulHitCountForRecovery = statLoadout.SuccessfulHitCountForRecovery > 0
            ? statLoadout.SuccessfulHitCountForRecovery
            : baseSuccessfulHitCountForRecovery;
        recoveryHealthOnTrigger = statLoadout.RecoveryHealthOnTrigger > 0
            ? statLoadout.RecoveryHealthOnTrigger
            : baseRecoveryHealthOnTrigger;
        recoverySpecialGaugeOnTrigger = Mathf.Max(0f, statLoadout.RecoverySpecialGaugeOnTrigger);
    }

    private float SanitizeScale(float scale)
    {
        return Mathf.Max(0.01f, scale);
    }

    private void NotifyStatusChanged()
    {
        StatusChanged?.Invoke();
    }
}
