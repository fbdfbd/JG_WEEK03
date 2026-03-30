using UnityEngine;
using System;

[DisallowMultipleComponent]
public class BossWeakSectorController : MonoBehaviour
{
    [SerializeField] private FaceSectorResolver sectorResolver;

    private FaceSectorRange baseWeakSector;
    private FaceSectorRange overrideWeakSector;
    private bool hasBaseWeakSector;
    private bool hasWeakSectorOverride;

    public bool HasBaseWeakSector => hasBaseWeakSector;
    public bool HasWeakSectorOverride => hasWeakSectorOverride;
    public bool HasCurrentWeakSector => hasWeakSectorOverride || hasBaseWeakSector;
    public event Action WeakSectorChanged;

    private void Awake()
    {
        if (sectorResolver == null)
        {
            sectorResolver = GetComponentInParent<FaceSectorResolver>();
        }
    }

    public void SetBaseWeakSector(FaceSectorRange sectorRange)
    {
        baseWeakSector = sectorRange;
        hasBaseWeakSector = true;
        NotifyWeakSectorChanged();
    }

    public void ClearBaseWeakSector()
    {
        hasBaseWeakSector = false;
        NotifyWeakSectorChanged();
    }

    public void ApplyWeakSectorOverride(FaceSectorRange sectorRange)
    {
        overrideWeakSector = sectorRange;
        hasWeakSectorOverride = true;
        NotifyWeakSectorChanged();
    }

    public void ClearWeakSectorOverride()
    {
        hasWeakSectorOverride = false;
        NotifyWeakSectorChanged();
    }

    public bool CanReceiveHitFromFace(int faceIndex)
    {
        if (sectorResolver == null)
        {
            return false;
        }

        FaceSectorRange currentWeakSector;
        if (!TryGetCurrentWeakSector(out currentWeakSector))
        {
            return false;
        }

        return sectorResolver.ContainsFace(currentWeakSector, faceIndex);
    }

    public bool CanReceiveHit(in ProjectileHitData hitData)
    {
        if (sectorResolver == null)
        {
            return false;
        }

        int hitFaceIndex;
        if (!sectorResolver.TryGetNearestFaceIndexFromWorldPoint(hitData.HitPoint, out hitFaceIndex))
        {
            return false;
        }

        return CanReceiveHitFromFace(hitFaceIndex);
    }

    public bool TryGetCurrentWeakSector(out FaceSectorRange sectorRange)
    {
        if (hasWeakSectorOverride)
        {
            sectorRange = overrideWeakSector;
            return true;
        }

        if (hasBaseWeakSector)
        {
            sectorRange = baseWeakSector;
            return true;
        }

        sectorRange = default;
        return false;
    }

    private void NotifyWeakSectorChanged()
    {
        if (WeakSectorChanged != null)
        {
            WeakSectorChanged();
        }
    }
}
