using UnityEngine;

public class BossHitPoint : MonoBehaviour, IProjectileHitReceiver
{
    [Header("Hit Point")]
    [SerializeField] private BossBase bossOwner;
    [SerializeField] private BossWeakSectorController weakSectorController;
    [SerializeField] private bool canReceiveProjectileHit = true;
    [SerializeField] private float damageMultiplier = 1f;

    private void Awake()
    {
        if (bossOwner == null)
        {
            bossOwner = GetComponentInParent<BossBase>();
        }

        if (weakSectorController == null)
        {
            weakSectorController = GetComponentInParent<BossWeakSectorController>();
        }
    }

    public void ReceiveProjectileHit(in ProjectileHitData hitData)
    {
        if (!canReceiveProjectileHit)
        {
            return;
        }

        if (bossOwner == null)
        {
            return;
        }

        if (weakSectorController != null && !weakSectorController.CanReceiveHit(hitData))
        {
            return;
        }

        bossOwner.ReceiveHitPointDamage(hitData, damageMultiplier);
    }
}
