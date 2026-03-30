using UnityEngine;

public readonly struct FaceSectorRange
{
    public int CenterFaceIndex { get; }
    public int HalfWidth { get; }

    public int SectorLength => HalfWidth * 2 + 1;

    public FaceSectorRange(int centerFaceIndex, int halfWidth)
    {
        CenterFaceIndex = centerFaceIndex;
        HalfWidth = Mathf.Max(0, halfWidth);
    }
}
