using UnityEngine;

[System.Serializable]
public struct EquipmentStatModifiers
{
    [SerializeField] private int maxHealthBonus;
    [SerializeField] private float maxSpecialGaugeBonus;
    [SerializeField] private int basicAttackDamageBonus;
    [SerializeField] private int specialAttackDamageBonus;
    [SerializeField] private float moveDelayScale;
    [SerializeField] private float basicAttackDelayScale;
    [SerializeField] private float specialGaugeCostScale;
    [SerializeField] private int successfulHitCountForRecovery;
    [SerializeField] private int recoveryHealthOnTrigger;
    [SerializeField] private float recoverySpecialGaugeOnTrigger;

    public int MaxHealthBonus => maxHealthBonus;
    public float MaxSpecialGaugeBonus => maxSpecialGaugeBonus;
    public int BasicAttackDamageBonus => basicAttackDamageBonus;
    public int SpecialAttackDamageBonus => specialAttackDamageBonus;
    public float MoveDelayScale => moveDelayScale;
    public float BasicAttackDelayScale => basicAttackDelayScale;
    public float SpecialGaugeCostScale => specialGaugeCostScale;
    public int SuccessfulHitCountForRecovery => successfulHitCountForRecovery;
    public int RecoveryHealthOnTrigger => recoveryHealthOnTrigger;
    public float RecoverySpecialGaugeOnTrigger => recoverySpecialGaugeOnTrigger;

    public EquipmentStatModifiers(
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
        this.maxHealthBonus = maxHealthBonus;
        this.maxSpecialGaugeBonus = maxSpecialGaugeBonus;
        this.basicAttackDamageBonus = basicAttackDamageBonus;
        this.specialAttackDamageBonus = specialAttackDamageBonus;
        this.moveDelayScale = moveDelayScale;
        this.basicAttackDelayScale = basicAttackDelayScale;
        this.specialGaugeCostScale = specialGaugeCostScale;
        this.successfulHitCountForRecovery = successfulHitCountForRecovery;
        this.recoveryHealthOnTrigger = recoveryHealthOnTrigger;
        this.recoverySpecialGaugeOnTrigger = recoverySpecialGaugeOnTrigger;
    }
}

[System.Serializable]
public struct EquipmentCombatEffects
{
    [SerializeField] private int additionalHitFaceCountPerSide;
    [SerializeField] private float additionalHitDamageMultiplier;
    [SerializeField] private bool enableBossWeakPointSpawn;

    public int AdditionalHitFaceCountPerSide => additionalHitFaceCountPerSide;
    public float AdditionalHitDamageMultiplier => additionalHitDamageMultiplier;
    public bool EnableBossWeakPointSpawn => enableBossWeakPointSpawn;

    public EquipmentCombatEffects(
        int additionalHitFaceCountPerSide,
        float additionalHitDamageMultiplier,
        bool enableBossWeakPointSpawn)
    {
        this.additionalHitFaceCountPerSide = additionalHitFaceCountPerSide;
        this.additionalHitDamageMultiplier = additionalHitDamageMultiplier;
        this.enableBossWeakPointSpawn = enableBossWeakPointSpawn;
    }
}

[System.Serializable]
public struct EquipmentInputEffects
{
    [SerializeField] private bool enableHoldToMove;
    [SerializeField] private float holdMoveRepeatIntervalScale;
    [SerializeField] private bool enableInputAdjust;
    [SerializeField] private bool enableHoldToFire;

    public bool EnableHoldToMove => enableHoldToMove;
    public float HoldMoveRepeatIntervalScale => holdMoveRepeatIntervalScale;
    public bool EnableInputAdjust => enableInputAdjust;
    public bool EnableHoldToFire => enableHoldToFire;

    public EquipmentInputEffects(
        bool enableHoldToMove,
        float holdMoveRepeatIntervalScale,
        bool enableInputAdjust,
        bool enableHoldToFire)
    {
        this.enableHoldToMove = enableHoldToMove;
        this.holdMoveRepeatIntervalScale = holdMoveRepeatIntervalScale;
        this.enableInputAdjust = enableInputAdjust;
        this.enableHoldToFire = enableHoldToFire;
    }
}

[System.Serializable]
public struct EquipmentVisualEffects
{
    [SerializeField] private bool usePlayerTint;
    [SerializeField] private Color playerTintColor;

    public bool UsePlayerTint => usePlayerTint;
    public Color PlayerTintColor => playerTintColor;

    public EquipmentVisualEffects(bool usePlayerTint, Color playerTintColor)
    {
        this.usePlayerTint = usePlayerTint;
        this.playerTintColor = playerTintColor;
    }
}

[CreateAssetMenu(fileName = "SOEquipmentData", menuName = "Scriptable Objects/Equipment Data")]
public sealed class SOEquipmentData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _equipmentId;
    [SerializeField] private EquipmentType _equipmentType;
    [SerializeField] private string _displayName;

    [Header("Gameplay")]
    [TextArea(2, 5)]
    [SerializeField] private string _description;
    [SerializeField] private int _cost;
    [SerializeField] private int _price;

    [Header("Visual")]
    [SerializeField] private string _spriteAssetPath;
    [SerializeField] private Sprite _spriteIcon;

    [Header("Effects")]
    [SerializeField] private EquipmentStatModifiers _statModifiers;
    [SerializeField] private EquipmentCombatEffects _combatEffects;
    [SerializeField] private EquipmentInputEffects _inputEffects;
    [SerializeField] private EquipmentVisualEffects _visualEffects;

    public string EquipmentId => _equipmentId;
    public EquipmentType EquipmentType => _equipmentType;
    public string DisplayName => _displayName;
    public string Description => _description;
    public int Cost => _cost;
    public int Price => _price;
    public string SpriteAssetPath => _spriteAssetPath;
    public Sprite SpriteIcon => _spriteIcon;
    public EquipmentStatModifiers StatModifiers => _statModifiers;
    public EquipmentCombatEffects CombatEffects => _combatEffects;
    public EquipmentInputEffects InputEffects => _inputEffects;
    public EquipmentVisualEffects VisualEffects => _visualEffects;

    public void SetData(
        string equipmentId,
        EquipmentType equipmentType,
        string displayName,
        string description,
        int cost,
        int price,
        string spriteAssetPath,
        Sprite spriteIcon)
    {
        _equipmentId = equipmentId;
        _equipmentType = equipmentType;
        _displayName = displayName;
        _description = description;
        _cost = cost;
        _price = price;
        _spriteAssetPath = spriteAssetPath;
        _spriteIcon = spriteIcon;
    }

    public void SetEffects(
        EquipmentStatModifiers statModifiers,
        EquipmentCombatEffects combatEffects,
        EquipmentInputEffects inputEffects,
        EquipmentVisualEffects visualEffects)
    {
        _statModifiers = statModifiers;
        _combatEffects = combatEffects;
        _inputEffects = inputEffects;
        _visualEffects = visualEffects;
    }
}
