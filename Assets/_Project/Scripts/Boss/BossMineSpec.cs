using UnityEngine;

[System.Serializable]
public struct BossMineSpec
{
    [Header("Gameplay")]
    [Min(1)][SerializeField] private int damage;
    [Min(0f)][SerializeField] private float fuseDuration;
    [Min(0)][SerializeField] private int blastHalfWidth;
    [SerializeField] private bool detonateOnTimer;
    [SerializeField] private bool detonateOnStep;
    [SerializeField] private bool allowSpawnOnPlayerFace;

    [Header("View")]
    [SerializeField] private GameObject mineViewPrefab;
    [Min(0.01f)][SerializeField] private float throwDuration;
    [SerializeField] private float throwArcHeight;
    [Min(0.01f)][SerializeField] private float detonationDuration;
    [Min(1f)][SerializeField] private float detonationScaleMultiplier;

    public int Damage => Mathf.Max(0, damage);
    public float FuseDuration => Mathf.Max(0f, fuseDuration);
    public int BlastHalfWidth => Mathf.Max(0, blastHalfWidth);
    public bool DetonateOnTimer => detonateOnTimer;
    public bool DetonateOnStep => detonateOnStep;
    public bool AllowSpawnOnPlayerFace => allowSpawnOnPlayerFace;
    public GameObject MineViewPrefab => mineViewPrefab;
    public float ThrowDuration => Mathf.Max(0f, throwDuration);
    public float ThrowArcHeight => throwArcHeight;
    public float DetonationDuration => Mathf.Max(0.01f, detonationDuration);
    public float DetonationScaleMultiplier => Mathf.Max(1f, detonationScaleMultiplier);

    public static BossMineSpec CreateDefault()
    {
        return new BossMineSpec
        {
            damage = 1,
            fuseDuration = 2f,
            blastHalfWidth = 0,
            detonateOnTimer = true,
            detonateOnStep = true,
            allowSpawnOnPlayerFace = false,
            throwDuration = 0.35f,
            throwArcHeight = 1.5f,
            detonationDuration = 0.18f,
            detonationScaleMultiplier = 1.8f
        };
    }
}
