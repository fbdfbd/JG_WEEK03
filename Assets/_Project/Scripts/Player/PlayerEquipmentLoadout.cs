using System.Collections.Generic;
using UnityEngine;

public readonly struct PlayerStatLoadout
{
    public static PlayerStatLoadout Empty { get; } = new PlayerStatLoadout(0, 0f, 0, 0, 1f, 1f, 1f, 0, 0, 0f);

    public int MaxHealthBonus { get; }
    public float MaxSpecialGaugeBonus { get; }
    public int BasicAttackDamageBonus { get; }
    public int SpecialAttackDamageBonus { get; }
    public float MoveDelayScale { get; }
    public float BasicAttackDelayScale { get; }
    public float SpecialGaugeCostScale { get; }
    public int SuccessfulHitCountForRecovery { get; }
    public int RecoveryHealthOnTrigger { get; }
    public float RecoverySpecialGaugeOnTrigger { get; }

    public PlayerStatLoadout(
        int maxHealthBonus,
        float maxSpecialGaugeBonus,
        int basicAttackDamageBonus,
        int specialAttackDamageBonus,
        float moveDelayScale,
        float basicAttackDelayScale,
        float specialGaugeCostScale,
        int successfulHitCountForRecovery,
        int recoveryHealthOnTrigger,
        float recoverySpecialGaugeOnTrigger)
    {
        MaxHealthBonus = maxHealthBonus;
        MaxSpecialGaugeBonus = maxSpecialGaugeBonus;
        BasicAttackDamageBonus = basicAttackDamageBonus;
        SpecialAttackDamageBonus = specialAttackDamageBonus;
        MoveDelayScale = moveDelayScale;
        BasicAttackDelayScale = basicAttackDelayScale;
        SpecialGaugeCostScale = specialGaugeCostScale;
        SuccessfulHitCountForRecovery = successfulHitCountForRecovery;
        RecoveryHealthOnTrigger = recoveryHealthOnTrigger;
        RecoverySpecialGaugeOnTrigger = recoverySpecialGaugeOnTrigger;
    }
}

public readonly struct PlayerFeatureLoadout
{
    public static PlayerFeatureLoadout Empty { get; } = new PlayerFeatureLoadout(
        0,
        1f,
        false,
        Color.white,
        false);

    public int AdditionalHitFaceCountPerSide { get; }
    public float AdditionalHitDamageMultiplier { get; }
    public bool EnableBossWeakPointSpawn { get; }
    public Color PlayerTintColor { get; }
    public bool UsePlayerTint { get; }

    public PlayerFeatureLoadout(
        int additionalHitFaceCountPerSide,
        float additionalHitDamageMultiplier,
        bool enableBossWeakPointSpawn,
        Color playerTintColor,
        bool usePlayerTint)
    {
        AdditionalHitFaceCountPerSide = additionalHitFaceCountPerSide;
        AdditionalHitDamageMultiplier = additionalHitDamageMultiplier;
        EnableBossWeakPointSpawn = enableBossWeakPointSpawn;
        PlayerTintColor = playerTintColor;
        UsePlayerTint = usePlayerTint;
    }
}

public readonly struct PlayerInputLoadout
{
    public static PlayerInputLoadout Empty { get; } = new PlayerInputLoadout(false, 1f, false, false);

    public bool EnableHoldToMove { get; }
    public float HoldMoveRepeatIntervalScale { get; }
    public bool EnableInputAdjust { get; }
    public bool EnableHoldToFire { get; }

    public PlayerInputLoadout(
        bool enableHoldToMove,
        float holdMoveRepeatIntervalScale,
        bool enableInputAdjust,
        bool enableHoldToFire)
    {
        EnableHoldToMove = enableHoldToMove;
        HoldMoveRepeatIntervalScale = holdMoveRepeatIntervalScale;
        EnableInputAdjust = enableInputAdjust;
        EnableHoldToFire = enableHoldToFire;
    }
}

public readonly struct PlayerEquipmentLoadout
{
    public static PlayerEquipmentLoadout Empty { get; } = new PlayerEquipmentLoadout(
        PlayerStatLoadout.Empty,
        PlayerInputLoadout.Empty,
        PlayerFeatureLoadout.Empty);

    public PlayerStatLoadout Stats { get; }
    public PlayerInputLoadout Input { get; }
    public PlayerFeatureLoadout Features { get; }

    public PlayerEquipmentLoadout(
        PlayerStatLoadout stats,
        PlayerInputLoadout input,
        PlayerFeatureLoadout features)
    {
        Stats = stats;
        Input = input;
        Features = features;
    }
}

public sealed class EquipmentEffectCalculator
{
    public PlayerEquipmentLoadout Build(IReadOnlyList<SOEquipmentData> equippedItems)
    {
        if (equippedItems == null || equippedItems.Count == 0)
        {
            return PlayerEquipmentLoadout.Empty;
        }

        int maxHealthBonus = 0;
        float maxSpecialGaugeBonus = 0f;
        int basicAttackDamageBonus = 0;
        int specialAttackDamageBonus = 0;
        float moveDelayScale = 1f;
        float basicAttackDelayScale = 1f;
        float specialGaugeCostScale = 1f;
        int recoveryHitCount = 0;
        int recoveryHealthAmount = 0;
        float recoverySpecialGaugeAmount = 0f;

        int additionalHitFaceCountPerSide = 0;
        float additionalHitDamageMultiplier = 1f;
        bool enableBossWeakPointSpawn = false;
        bool enableHoldToMove = false;
        float holdMoveRepeatIntervalScale = 1f;
        bool enableInputAdjust = false;
        bool enableHoldToFire = false;
        bool usePlayerTint = false;
        Color playerTintColor = Color.white;

        for (int index = 0; index < equippedItems.Count; index++)
        {
            SOEquipmentData equipmentData = equippedItems[index];
            if (equipmentData == null)
            {
                continue;
            }

            AccumulateStatEffects(
                equipmentData.StatModifiers,
                ref maxHealthBonus,
                ref maxSpecialGaugeBonus,
                ref basicAttackDamageBonus,
                ref specialAttackDamageBonus,
                ref moveDelayScale,
                ref basicAttackDelayScale,
                ref specialGaugeCostScale,
                ref recoveryHitCount,
                ref recoveryHealthAmount,
                ref recoverySpecialGaugeAmount);

            AccumulateCombatEffects(
                equipmentData.CombatEffects,
                ref additionalHitFaceCountPerSide,
                ref additionalHitDamageMultiplier,
                ref enableBossWeakPointSpawn);

            AccumulateInputEffects(
                equipmentData.InputEffects,
                ref enableHoldToMove,
                ref holdMoveRepeatIntervalScale,
                ref enableInputAdjust,
                ref enableHoldToFire);

            AccumulateVisualEffects(
                equipmentData.VisualEffects,
                ref usePlayerTint,
                ref playerTintColor);
        }

        return new PlayerEquipmentLoadout(
            new PlayerStatLoadout(
                maxHealthBonus,
                maxSpecialGaugeBonus,
                basicAttackDamageBonus,
                specialAttackDamageBonus,
                moveDelayScale,
                basicAttackDelayScale,
                specialGaugeCostScale,
                recoveryHitCount,
                recoveryHealthAmount,
                recoverySpecialGaugeAmount),
            new PlayerInputLoadout(
                enableHoldToMove,
                holdMoveRepeatIntervalScale,
                enableInputAdjust,
                enableHoldToFire),
            new PlayerFeatureLoadout(
                additionalHitFaceCountPerSide,
                additionalHitDamageMultiplier,
                enableBossWeakPointSpawn,
                playerTintColor,
                usePlayerTint));
    }

    private static void AccumulateStatEffects(
        EquipmentStatModifiers statModifiers,
        ref int maxHealthBonus,
        ref float maxSpecialGaugeBonus,
        ref int basicAttackDamageBonus,
        ref int specialAttackDamageBonus,
        ref float moveDelayScale,
        ref float basicAttackDelayScale,
        ref float specialGaugeCostScale,
        ref int recoveryHitCount,
        ref int recoveryHealthAmount,
        ref float recoverySpecialGaugeAmount)
    {
        maxHealthBonus += statModifiers.MaxHealthBonus;
        maxSpecialGaugeBonus += statModifiers.MaxSpecialGaugeBonus;
        basicAttackDamageBonus += statModifiers.BasicAttackDamageBonus;
        specialAttackDamageBonus += statModifiers.SpecialAttackDamageBonus;
        moveDelayScale *= SanitizeScale(statModifiers.MoveDelayScale);
        basicAttackDelayScale *= SanitizeScale(statModifiers.BasicAttackDelayScale);
        specialGaugeCostScale *= Mathf.Max(0f, statModifiers.SpecialGaugeCostScale);

        if (statModifiers.SuccessfulHitCountForRecovery > 0)
        {
            recoveryHitCount = recoveryHitCount == 0
                ? statModifiers.SuccessfulHitCountForRecovery
                : Mathf.Min(recoveryHitCount, statModifiers.SuccessfulHitCountForRecovery);
        }

        if (statModifiers.RecoveryHealthOnTrigger > 0)
        {
            recoveryHealthAmount += statModifiers.RecoveryHealthOnTrigger;
        }

        if (statModifiers.RecoverySpecialGaugeOnTrigger > 0f)
        {
            recoverySpecialGaugeAmount += statModifiers.RecoverySpecialGaugeOnTrigger;
        }
    }

    private static void AccumulateCombatEffects(
        EquipmentCombatEffects combatEffects,
        ref int additionalHitFaceCountPerSide,
        ref float additionalHitDamageMultiplier,
        ref bool enableBossWeakPointSpawn)
    {
        additionalHitFaceCountPerSide += combatEffects.AdditionalHitFaceCountPerSide;

        if (combatEffects.AdditionalHitDamageMultiplier > 0f)
        {
            additionalHitDamageMultiplier = Mathf.Min(additionalHitDamageMultiplier, combatEffects.AdditionalHitDamageMultiplier);
        }

        enableBossWeakPointSpawn |= combatEffects.EnableBossWeakPointSpawn;
    }

    private static void AccumulateInputEffects(
        EquipmentInputEffects inputEffects,
        ref bool enableHoldToMove,
        ref float holdMoveRepeatIntervalScale,
        ref bool enableInputAdjust,
        ref bool enableHoldToFire)
    {
        enableHoldToMove |= inputEffects.EnableHoldToMove;
        holdMoveRepeatIntervalScale *= SanitizeScale(inputEffects.HoldMoveRepeatIntervalScale);
        enableInputAdjust |= inputEffects.EnableInputAdjust;
        enableHoldToFire |= inputEffects.EnableHoldToFire;
    }

    private static void AccumulateVisualEffects(
        EquipmentVisualEffects visualEffects,
        ref bool usePlayerTint,
        ref Color playerTintColor)
    {
        if (!visualEffects.UsePlayerTint)
        {
            return;
        }

        usePlayerTint = true;
        playerTintColor = visualEffects.PlayerTintColor;
    }

    private static float SanitizeScale(float scale)
    {
        return scale <= 0f ? 1f : scale;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerEquipmentService))]
public sealed class StagePlayerLoadoutInstaller : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerEquipmentService playerEquipmentService;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private PlayerProfileService playerProfileService;

    private readonly EquipmentEffectCalculator equipmentEffectCalculator = new EquipmentEffectCalculator();

    private void Awake()
    {
        ResolveReferences();
    }

    public void ApplyLoadout()
    {
        ResolveReferences();

        PlayerEquipmentLoadout loadout = equipmentEffectCalculator.Build(
            playerProfileService != null
                ? playerProfileService.GetEquippedEquipments()
                : System.Array.Empty<SOEquipmentData>());

        if (playerStatus != null)
        {
            playerStatus.InitializeFromLoadout(loadout.Stats);
        }

        if (playerEquipmentService != null)
        {
            playerEquipmentService.ApplyFeatureLoadout(loadout.Features);
        }

        if (playerInputHandler != null)
        {
            playerInputHandler.ApplyRuntimeOptions(loadout.Input);
        }
    }

    public void SetProfileService(PlayerProfileService profileService)
    {
        playerProfileService = profileService;
    }

    private void ResolveReferences()
    {
        playerStatus ??= GetComponent<PlayerStatus>();
        playerEquipmentService ??= GetComponent<PlayerEquipmentService>();
        playerInputHandler ??= GetComponent<PlayerInputHandler>();

        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            playerProfileService = GameManager.I.PlayerProfileService;
        }

        playerProfileService ??= FindFirstObjectByType<PlayerProfileService>(FindObjectsInactive.Include);
    }
}
