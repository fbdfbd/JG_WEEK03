using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public sealed class PlayerSpecialAttackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private Transform specialFirePoint;
    [SerializeField] private PlayerProjectile specialProjectilePrefab;

    [Header("Special Attack")]
    [SerializeField] private float specialAttackGaugeCost = 30f;
    [SerializeField] private int specialAttackDamage = 10;
    [SerializeField] private float specialAttackCooldown = 0f;
    [SerializeField] private float projectileMoveSpeed = 16f;
    [SerializeField] private float projectileLifetime = 1.5f;

    [Header("Pool")]
    [SerializeField] private int defaultPoolCapacity = 4;
    [SerializeField] private int maxPoolSize = 12;

    private ObjectPool<PlayerProjectile> projectilePool;
    private float nextSpecialAttackTime;

    private void Awake()
    {
        playerStatus ??= GetComponent<PlayerStatus>();
        faceBoard ??= FindFirstObjectByType<FaceMapGenerator>();
        CreateProjectilePool();
    }

    private void OnDestroy()
    {
        projectilePool?.Clear();
    }

    public bool TryFireSpecialAttack()
    {
        if (!CanFireSpecialAttack())
        {
            return false;
        }

        if (!playerStatus.TryConsumeScaledSpecialGauge(specialAttackGaugeCost, out float consumedGauge))
        {
            return false;
        }

        if (!TryBuildLaunchData(out PlayerProjectileLaunchData launchData))
        {
            playerStatus.RestoreSpecialGauge(consumedGauge);
            return false;
        }

        PlayerProjectile projectile = projectilePool.Get();
        projectile.Launch(launchData);
        nextSpecialAttackTime = Time.time + specialAttackCooldown;
        return true;
    }

    private bool CanFireSpecialAttack()
    {
        if (specialProjectilePrefab == null)
        {
            Debug.LogWarning("PlayerSpecialAttackController needs a special projectile prefab.");
            return false;
        }

        if (playerStatus == null || playerStatus.HasNoHealthRemaining())
        {
            return false;
        }

        return Time.time >= nextSpecialAttackTime;
    }

    private bool TryBuildLaunchData(out PlayerProjectileLaunchData launchData)
    {
        launchData = default;

        faceBoard ??= FindFirstObjectByType<FaceMapGenerator>();
        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            Debug.LogWarning("PlayerSpecialAttackController could not find an initialized FaceMapGenerator.");
            return false;
        }

        Vector3 startPosition = specialFirePoint != null ? specialFirePoint.position : transform.position;
        Vector3 targetPosition = faceBoard.GetBoardCenterWorldPosition();
        int finalDamage = playerStatus.GetSpecialAttackDamageWithBonus(specialAttackDamage);

        launchData = new PlayerProjectileLaunchData(
            gameObject,
            startPosition,
            targetPosition,
            projectileMoveSpeed,
            projectileLifetime,
            finalDamage);

        return true;
    }

    private void CreateProjectilePool()
    {
        projectilePool = new ObjectPool<PlayerProjectile>(
            CreateProjectile,
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile,
            false,
            defaultPoolCapacity,
            maxPoolSize);
    }

    private PlayerProjectile CreateProjectile()
    {
        PlayerProjectile projectileInstance = Instantiate(specialProjectilePrefab);
        projectileInstance.RegisterPool(projectilePool);
        projectileInstance.gameObject.SetActive(false);
        return projectileInstance;
    }

    private void OnGetProjectile(PlayerProjectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(PlayerProjectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(PlayerProjectile projectile)
    {
        if (projectile == null)
        {
            return;
        }

        Destroy(projectile.gameObject);
    }
}
