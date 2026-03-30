using UnityEngine;

public readonly struct ProjectileHitData
{
    public GameObject Instigator { get; }
    public int Damage { get; }
    public Vector2 HitPoint { get; }
    public Vector2 TravelDirection { get; }

    public ProjectileHitData(
        GameObject instigator,
        int damage,
        Vector2 hitPoint,
        Vector2 travelDirection)
    {
        Instigator = instigator;
        Damage = damage;
        HitPoint = hitPoint;
        TravelDirection = travelDirection;
    }
}

public interface IProjectileHitReceiver
{
    void ReceiveProjectileHit(in ProjectileHitData hitData);
}

public abstract class ObjectBase : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int _hp;
    [SerializeField] protected int _maxHp;
    [SerializeField] protected bool _isDead;

    protected Rigidbody2D Rb { get; private set; }

    public int Hp => _hp;
    public int MaxHp => _maxHp;
    public bool IsDead => _isDead;

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        Rb.gravityScale = 0;
    }

    public abstract void TakeDamage(int damage);
    protected abstract void OnDeath();
}
