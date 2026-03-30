using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public class PlayerProjectile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private bool rotateTowardTravelDirection = true;

    private ObjectPool<PlayerProjectile> ownerPool;
    private GameObject instigator;
    private Vector3 targetPosition;
    private float moveSpeed;
    private float remainingLifetime;
    private int damage;
    private bool isLaunched;

    public int Damage => damage;

    public void RegisterPool(ObjectPool<PlayerProjectile> pool)
    {
        ownerPool = pool;
    }

    public void Launch(PlayerProjectileLaunchData launchData)
    {
        instigator = launchData.Instigator;
        transform.position = launchData.StartPosition;
        targetPosition = launchData.TargetPosition;
        moveSpeed = Mathf.Max(0.01f, launchData.MoveSpeed);
        remainingLifetime = Mathf.Max(0.01f, launchData.Lifetime);
        damage = Mathf.Max(0, launchData.Damage);
        isLaunched = true;

        ApplyTravelRotation();
    }

    private void Update()
    {
        if (!isLaunched)
        {
            return;
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f)
        {
            ReleaseToPool();
            return;
        }

        float moveStep = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveStep);

        if (HasReachedTarget())
        {
            ReleaseToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched)
        {
            return;
        }

        if (IsInstigatorCollider(other) || IsOtherProjectile(other))
        {
            return;
        }

        TryNotifyHitReceiver(other);
        ReleaseToPool();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLaunched)
        {
            return;
        }

        Collider2D other = collision.collider;
        if (IsInstigatorCollider(other) || IsOtherProjectile(other))
        {
            return;
        }

        TryNotifyHitReceiver(other);
        ReleaseToPool();
    }

    private bool HasReachedTarget()
    {
        return Vector3.SqrMagnitude(transform.position - targetPosition) <= 0.0001f;
    }

    private void ApplyTravelRotation()
    {
        if (!rotateTowardTravelDirection)
        {
            return;
        }

        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Transform targetTransform = visualRoot != null ? visualRoot : transform;
        targetTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ReleaseToPool()
    {
        if (ownerPool == null)
        {
            gameObject.SetActive(false);
            isLaunched = false;
            return;
        }

        ownerPool.Release(this);
    }

    private void OnDisable()
    {
        isLaunched = false;
        instigator = null;
        moveSpeed = 0f;
        remainingLifetime = 0f;
        damage = 0;
    }

    private bool IsInstigatorCollider(Collider2D other)
    {
        if (instigator == null || other == null)
        {
            return false;
        }

        return other.transform.root.gameObject == instigator;
    }

    private bool IsOtherProjectile(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        PlayerProjectile otherProjectile = other.GetComponentInParent<PlayerProjectile>();
        return otherProjectile != null && otherProjectile != this;
    }

    private void TryNotifyHitReceiver(Collider2D other)
    {
        IProjectileHitReceiver hitReceiver = other.GetComponentInParent<IProjectileHitReceiver>();
        if (hitReceiver == null)
        {
            return;
        }

        ProjectileHitData hitData = new ProjectileHitData(
            instigator,
            damage,
            other.ClosestPoint(transform.position),
            GetTravelDirection());

        hitReceiver.ReceiveProjectileHit(hitData);
    }

    private Vector2 GetTravelDirection()
    {
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return direction.normalized;
    }
}
