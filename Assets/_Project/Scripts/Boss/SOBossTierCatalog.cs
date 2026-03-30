using UnityEngine;

[CreateAssetMenu(fileName = "BossTierCatalog", menuName = "Scriptable Objects/Boss/Boss Tier Catalog")]
public sealed class SOBossTierCatalog : ScriptableObject
{
    [SerializeField] private SOBossPoolDefinition easyPool;
    [SerializeField] private SOBossPoolDefinition normalPool;
    [SerializeField] private SOBossPoolDefinition hardPool;
    [SerializeField] private SOBossPoolDefinition veryHardPool;

    public SOBossPoolDefinition GetPool(BossTier tier)
    {
        switch (tier)
        {
            case BossTier.Normal:
                return normalPool;

            case BossTier.Hard:
                return hardPool;

            case BossTier.VeryHard:
                return veryHardPool;

            case BossTier.Easy:
            default:
                return easyPool;
        }
    }
}
