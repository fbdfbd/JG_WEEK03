using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerAttackController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerProjectile projectilePrefab;

    [Header("Projectile")]
    [SerializeField] private float projectileMoveSpeed = 12f;
    [SerializeField] private float projectileLifetime = 1.5f;

    [Header("Pool")]
    [SerializeField] private int defaultPoolCapacity = 8;
    [SerializeField] private int maxPoolSize = 24;

    private ObjectPool<PlayerProjectile> projectilePool;
    private float nextBasicAttackTime;

    private void Awake()
    {
        playerStatus = GetComponent<PlayerStatus>();

        if (faceBoard == null)
        {
            faceBoard = FindFirstObjectByType<FaceMapGenerator>();
        }

        CreateProjectilePool();
    }

    private void OnDestroy()
    {
        if (projectilePool == null)
        {
            return;
        }

        projectilePool.Clear();
    }

    public bool TryFireBasicAttack()
    {
        if (!CanFireBasicAttack())
        {
            return false;
        }

        if (!TryBuildLaunchData(out PlayerProjectileLaunchData launchData))
        {
            return false;
        }

        PlayerProjectile projectile = projectilePool.Get();
        projectile.Launch(launchData);
        nextBasicAttackTime = Time.time + playerStatus.GetBasicAttackDelay();
        return true;
    }

    private bool CanFireBasicAttack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("PlayerAttackController needs a projectile prefab.");
            return false;
        }

        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerAttackController needs a PlayerStatus reference.");
            return false;
        }

        if (playerStatus.HasNoHealthRemaining())
        {
            return false;
        }

        if (Time.time < nextBasicAttackTime)
        {
            return false;
        }

        return true;
    }

    private bool TryBuildLaunchData(out PlayerProjectileLaunchData launchData)
    {
        launchData = default;

        if (faceBoard == null)
        {
            faceBoard = FindFirstObjectByType<FaceMapGenerator>();
        }

        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            Debug.LogWarning("PlayerAttackController could not find an initialized FaceMapGenerator.");
            return false;
        }

        Vector3 startPosition = GetFireOrigin();
        Vector3 targetPosition = faceBoard.GetBoardCenterWorldPosition();

        launchData = new PlayerProjectileLaunchData(
            gameObject,
            startPosition,
            targetPosition,
            projectileMoveSpeed,
            projectileLifetime,
            playerStatus.GetBasicAttackDamage());

        return true;
    }

    private Vector3 GetFireOrigin()
    {
        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position;
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
        PlayerProjectile projectileInstance = Instantiate(projectilePrefab);
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
