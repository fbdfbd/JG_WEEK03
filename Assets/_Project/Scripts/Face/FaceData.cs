using UnityEngine;

public readonly struct FaceData
{
    public int Index { get; }
    public Vector2 Start { get; }
    public Vector2 End { get; }
    public Vector2 MidPoint { get; }
    public float AngleRad { get; }

    public FaceData(int index, Vector2 start, Vector2 end, Vector2 midPoint, float angleRad)
    {
        Index = index;
        Start = start;
        End = end;
        MidPoint = midPoint;
        AngleRad = angleRad;
    }
}