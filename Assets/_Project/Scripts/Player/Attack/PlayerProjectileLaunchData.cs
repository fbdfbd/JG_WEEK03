using UnityEngine;

public readonly struct PlayerProjectileLaunchData
{
    public GameObject Instigator { get; }
    public Vector3 StartPosition { get; }
    public Vector3 TargetPosition { get; }
    public float MoveSpeed { get; }
    public float Lifetime { get; }
    public int Damage { get; }

    public PlayerProjectileLaunchData(
        GameObject instigator,
        Vector3 startPosition,
        Vector3 targetPosition,
        float moveSpeed,
        float lifetime,
        int damage)
    {
        Instigator = instigator;
        StartPosition = startPosition;
        TargetPosition = targetPosition;
        MoveSpeed = moveSpeed;
        Lifetime = lifetime;
        Damage = damage;
    }
}
