using System;
using UnityEngine;

public sealed class FaceGrid
{
    public int FaceCount { get; }
    public Vector2 Center { get; }
    public float Radius { get; }

    private readonly FaceData[] _faces;
    private readonly Vector2[] _vertices;

    public FaceGrid(int faceCount, Vector2 center, float radius)
    {
        if (faceCount < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(faceCount), "Face count must be at least 3.");
        }

        if (radius <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be greater than 0.");
        }

        FaceCount = faceCount;
        Center = center;
        Radius = radius;

        _vertices = new Vector2[faceCount];
        _faces = new FaceData[faceCount];

        BuildVertices();
        BuildFaces();
    }

    public FaceData GetFace(int index)
    {
        return _faces[Wrap(index)];
    }

    public int GetAdjacentIndex(int index, int offset)
    {
        return Wrap(index + offset);
    }

    public int GetOppositeIndex(int index)
    {
        if (FaceCount % 2 != 0)
        {
            throw new InvalidOperationException("Opposite index is only valid when the face count is even.");
        }

        return Wrap(index + FaceCount / 2);
    }

    public Vector2 GetFaceMidPoint(int index)
    {
        return GetFace(index).MidPoint;
    }

    public float GetFaceAngleDegrees(int index)
    {
        return GetFace(index).AngleRad * Mathf.Rad2Deg;
    }

    public int Wrap(int index)
    {
        return ((index % FaceCount) + FaceCount) % FaceCount;
    }

    private void BuildVertices()
    {
        for (int i = 0; i < FaceCount; i++)
        {
            float theta = i * Mathf.PI * 2f / FaceCount - Mathf.PI / 2f;
            _vertices[i] = Center + new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * Radius;
        }
    }

    private void BuildFaces()
    {
        for (int i = 0; i < FaceCount; i++)
        {
            Vector2 start = _vertices[i];
            Vector2 end = _vertices[(i + 1) % FaceCount];
            Vector2 midPoint = (start + end) * 0.5f;
            float angleRad = Mathf.Atan2(end.y - start.y, end.x - start.x);

            _faces[i] = new FaceData(i, start, end, midPoint, angleRad);
        }
    }
}
