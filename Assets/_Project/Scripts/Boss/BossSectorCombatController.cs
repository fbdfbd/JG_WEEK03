using UnityEngine;

public enum BossSectorTargetingMode
{
    FixedFaceIndex,
    PlayerCurrentFace,
    OppositePlayerFace
}

[DisallowMultipleComponent]
public class BossSectorCombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private FaceSectorResolver faceSectorResolver;
    [SerializeField] private FaceSectorAttackResolver faceSectorAttackResolver;
    [SerializeField] private FaceSectorTelegraphView faceSectorTelegraphView;
    [SerializeField] private BossWeakSectorController bossWeakSectorController;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Attack Telegraph")]
    [SerializeField] private Color attackTelegraphColor = new Color(1f, 0.2f, 0.15f, 1f);

    private void Awake()
    {
        if (faceBoard == null)
        {
            faceBoard = FindFirstObjectByType<FaceMapGenerator>();
        }

        if (faceSectorResolver == null)
        {
            faceSectorResolver = GetComponentInChildren<FaceSectorResolver>();
        }

        if (faceSectorAttackResolver == null)
        {
            faceSectorAttackResolver = GetComponentInChildren<FaceSectorAttackResolver>();
        }

        if (faceSectorTelegraphView == null)
        {
            faceSectorTelegraphView = GetComponentInChildren<FaceSectorTelegraphView>();
        }

        if (bossWeakSectorController == null)
        {
            bossWeakSectorController = GetComponentInChildren<BossWeakSectorController>();
        }

        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (faceSectorTelegraphView != null)
        {
            faceSectorTelegraphView.SetColor(attackTelegraphColor);
        }
    }

    public bool TryCreateSectorRange(
        BossSectorTargetingMode targetingMode,
        int fixedCenterFaceIndex,
        int halfWidth,
        out FaceSectorRange sectorRange)
    {
        sectorRange = default;

        if (faceSectorResolver == null || faceBoard == null || !faceBoard.IsInitialized)
        {
            return false;
        }

        int centerFaceIndex;
        if (!TryResolveCenterFaceIndex(targetingMode, fixedCenterFaceIndex, out centerFaceIndex))
        {
            return false;
        }

        sectorRange = faceSectorResolver.CreateSectorFromCenter(centerFaceIndex, halfWidth);
        return true;
    }

    public void ShowTelegraph(FaceSectorRange sectorRange, float duration)
    {
        if (faceSectorTelegraphView == null)
        {
            return;
        }

        faceSectorTelegraphView.Show(sectorRange, duration);
    }

    public void HideTelegraph()
    {
        if (faceSectorTelegraphView == null)
        {
            return;
        }

        faceSectorTelegraphView.Hide();
    }

    public bool TryApplyDamageToPlayer(FaceSectorAttackData attackData)
    {
        if (faceSectorAttackResolver == null)
        {
            return false;
        }

        return faceSectorAttackResolver.TryApplyDamageToPlayer(attackData);
    }

    public void ApplyWeakSectorOverride(FaceSectorRange sectorRange)
    {
        if (bossWeakSectorController == null)
        {
            return;
        }

        bossWeakSectorController.ApplyWeakSectorOverride(sectorRange);
    }

    public void ClearWeakSectorOverride()
    {
        if (bossWeakSectorController == null)
        {
            return;
        }

        bossWeakSectorController.ClearWeakSectorOverride();
    }

    public void SetBaseWeakSector(FaceSectorRange sectorRange)
    {
        if (bossWeakSectorController == null)
        {
            return;
        }

        bossWeakSectorController.SetBaseWeakSector(sectorRange);
    }

    public void ClearBaseWeakSector()
    {
        if (bossWeakSectorController == null)
        {
            return;
        }

        bossWeakSectorController.ClearBaseWeakSector();
    }

    public bool TryGetCurrentWeakSector(out FaceSectorRange sectorRange)
    {
        sectorRange = default;

        if (bossWeakSectorController == null)
        {
            return false;
        }

        return bossWeakSectorController.TryGetCurrentWeakSector(out sectorRange);
    }

    public bool TryGetFaceCount(out int faceCount)
    {
        faceCount = 0;

        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            return false;
        }

        faceCount = faceBoard.GetFaceCount();
        return faceCount > 0;
    }

    private bool TryResolveCenterFaceIndex(
        BossSectorTargetingMode targetingMode,
        int fixedCenterFaceIndex,
        out int centerFaceIndex)
    {
        centerFaceIndex = 0;

        switch (targetingMode)
        {
            case BossSectorTargetingMode.FixedFaceIndex:
                centerFaceIndex = faceBoard.GetWrappedFaceIndex(fixedCenterFaceIndex);
                return true;

            case BossSectorTargetingMode.PlayerCurrentFace:
                if (playerMovement == null || !playerMovement.IsInitialized)
                {
                    return false;
                }

                centerFaceIndex = playerMovement.CurrentFaceIndex;
                return true;

            case BossSectorTargetingMode.OppositePlayerFace:
                if (playerMovement == null || !playerMovement.IsInitialized)
                {
                    return false;
                }

                centerFaceIndex = faceBoard.GetOppositeFaceIndex(playerMovement.CurrentFaceIndex);
                return true;

            default:
                return false;
        }
    }
}
