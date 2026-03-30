using UnityEngine;

[DisallowMultipleComponent]
public class FaceSectorAttackResolver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FaceSectorResolver sectorResolver;
    [SerializeField] private PlayerController playerController;

    private IFaceSectorAttackTarget playerAttackTarget;

    public bool IsPlayerInsideSector(FaceSectorRange sectorRange)
    {
        return IsPrimaryTargetInsideSector(sectorRange);
    }

    public bool TryApplyDamageToPlayer(FaceSectorAttackData attackData)
    {
        if (!IsPrimaryTargetInsideSector(attackData.SectorRange))
        {
            return false;
        }

        IFaceSectorAttackTarget target;
        if (!TryGetPrimaryTarget(out target))
        {
            return false;
        }

        return target.TryReceiveFaceSectorAttack(attackData);
    }

    private bool IsPrimaryTargetInsideSector(FaceSectorRange sectorRange)
    {
        if (sectorResolver == null)
        {
            return false;
        }

        IFaceSectorAttackTarget target;
        if (!TryGetPrimaryTarget(out target))
        {
            return false;
        }

        int targetFaceIndex;
        if (!target.TryGetCurrentFaceIndex(out targetFaceIndex))
        {
            return false;
        }

        return sectorResolver.ContainsFace(sectorRange, targetFaceIndex);
    }

    private void Awake()
    {
        if (sectorResolver == null)
        {
            sectorResolver = GetComponentInParent<FaceSectorResolver>();
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        playerAttackTarget = playerController as IFaceSectorAttackTarget;
    }

    private bool TryGetPrimaryTarget(out IFaceSectorAttackTarget target)
    {
        target = playerAttackTarget;

        if (target == null)
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            target = playerController as IFaceSectorAttackTarget;
            playerAttackTarget = target;
        }

        if (target == null)
        {
            return false;
        }

        return target.IsFaceSectorAttackTargetReady;
    }
}
