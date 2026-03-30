using System.Collections.Generic;
using UnityEngine;

public interface IFaceSectorAttackTarget
{
    bool IsFaceSectorAttackTargetReady { get; }
    bool TryGetCurrentFaceIndex(out int faceIndex);
    bool TryReceiveFaceSectorAttack(FaceSectorAttackData attackData);
}

public readonly struct FaceSectorAttackData
{
    public FaceSectorRange SectorRange { get; }
    public int Damage { get; }
    public float TelegraphDuration { get; }
    public float ActiveDuration { get; }

    public FaceSectorAttackData(
        FaceSectorRange sectorRange,
        int damage,
        float telegraphDuration,
        float activeDuration)
    {
        SectorRange = sectorRange;
        Damage = Mathf.Max(0, damage);
        TelegraphDuration = Mathf.Max(0f, telegraphDuration);
        ActiveDuration = Mathf.Max(0f, activeDuration);
    }
}


public readonly struct FaceSectorMultiAttackData
{
    public IReadOnlyList<int> FaceIndices { get; }
    public int Damage { get; }
    public float TelegraphDuration { get; }
    public float ActiveDuration { get; }

    public FaceSectorMultiAttackData(
        IReadOnlyList<int> faceIndices,
        int damage,
        float telegraphDuration,
        float activeDuration)
    {
        FaceIndices = faceIndices;
        Damage = Mathf.Max(0, damage);
        TelegraphDuration = Mathf.Max(0f, telegraphDuration);
        ActiveDuration = Mathf.Max(0f, activeDuration);
    }
}