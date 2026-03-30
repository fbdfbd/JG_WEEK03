using System;
using UnityEngine;

public readonly struct HexCoord : IEquatable<HexCoord>
{
    private static readonly HexCoord[] NeighborDirections =
    {
        new HexCoord(1, 0),
        new HexCoord(1, -1),
        new HexCoord(0, -1),
        new HexCoord(-1, 0),
        new HexCoord(-1, 1),
        new HexCoord(0, 1)
    };

    public static HexCoord Zero => new HexCoord(0, 0);

    public int Q { get; }
    public int R { get; }
    public int S => -Q - R;

    public HexCoord(int q, int r)
    {
        Q = q;
        R = r;
    }

    public HexCoord GetNeighbor(int directionIndex)
    {
        int wrappedIndex = ((directionIndex % NeighborDirections.Length) + NeighborDirections.Length) % NeighborDirections.Length;
        HexCoord offset = NeighborDirections[wrappedIndex];
        return new HexCoord(Q + offset.Q, R + offset.R);
    }

    public int DistanceTo(HexCoord other)
    {
        int dq = Mathf.Abs(Q - other.Q);
        int dr = Mathf.Abs(R - other.R);
        int ds = Mathf.Abs(S - other.S);
        return (dq + dr + ds) / 2;
    }

    public bool Equals(HexCoord other)
    {
        return Q == other.Q && R == other.R;
    }

    public override bool Equals(object obj)
    {
        return obj is HexCoord other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Q * 397) ^ R;
        }
    }

    public override string ToString()
    {
        return $"({Q}, {R})";
    }
}
