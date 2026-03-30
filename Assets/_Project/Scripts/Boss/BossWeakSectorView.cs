using UnityEngine;

[DisallowMultipleComponent]
public class BossWeakSectorView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossWeakSectorController weakSectorController;
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private CircleCollider2D bossBodyCollider;
    [SerializeField] private LineRenderer weakSectorLineRenderer;

    [Header("View")]
    [SerializeField] private Color weakSectorColor = new Color(1f, 0.9f, 0.2f, 1f);
    [SerializeField] private bool useAutomaticRadius = true;
    [SerializeField] private float manualRadius = 3.2f;
    [SerializeField] private float radiusPadding = 0.25f;
    [SerializeField] private int segmentCount = 24;
    [SerializeField] private float lineWidth = 0.18f;

    private FaceSectorRange currentWeakSector;
    private bool isShowingWeakSector;

    private void Awake()
    {
        weakSectorController ??= GetComponentInParent<BossWeakSectorController>();
        faceBoard ??= FindFirstObjectByType<FaceMapGenerator>();
        bossBodyCollider ??= GetComponent<CircleCollider2D>();

        if (bossBodyCollider == null)
        {
            bossBodyCollider = GetComponentInParent<CircleCollider2D>();
        }

        if (weakSectorLineRenderer == null)
        {
            weakSectorLineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        ConfigureLineRenderer();
    }

    private void OnEnable()
    {
        if (weakSectorController != null)
        {
            weakSectorController.WeakSectorChanged += HandleWeakSectorChanged;
        }

        RefreshView();
    }

    private void OnDisable()
    {
        if (weakSectorController != null)
        {
            weakSectorController.WeakSectorChanged -= HandleWeakSectorChanged;
        }
    }

    private void LateUpdate()
    {
        if (!isShowingWeakSector)
        {
            return;
        }

        if (!transform.hasChanged)
        {
            return;
        }

        RefreshVisibleWeakSector();
        transform.hasChanged = false;
    }

    public void RefreshView()
    {
        if (weakSectorController == null)
        {
            HideWeakSector();
            return;
        }

        if (!weakSectorController.TryGetCurrentWeakSector(out FaceSectorRange weakSector))
        {
            HideWeakSector();
            return;
        }

        currentWeakSector = weakSector;
        isShowingWeakSector = true;
        RefreshVisibleWeakSector();
    }

    private void RefreshVisibleWeakSector()
    {
        if (weakSectorLineRenderer == null || faceBoard == null || !faceBoard.IsInitialized)
        {
            HideWeakSector();
            return;
        }

        int faceCount = faceBoard.GetFaceCount();
        if (faceCount <= 0)
        {
            HideWeakSector();
            return;
        }

        if (!TryGetSectorCenterAngle(currentWeakSector.CenterFaceIndex, out float centerAngleDegrees))
        {
            HideWeakSector();
            return;
        }

        float faceAngleSize = 360f / faceCount;
        float sectorAngleSize = faceAngleSize * currentWeakSector.SectorLength;
        float startAngleDegrees = centerAngleDegrees - sectorAngleSize * 0.5f;
        float endAngleDegrees = centerAngleDegrees + sectorAngleSize * 0.5f;
        float ringRadius = GetRingRadius();

        int safeSegmentCount = Mathf.Max(2, segmentCount);
        weakSectorLineRenderer.positionCount = safeSegmentCount + 1;

        for (int i = 0; i <= safeSegmentCount; i++)
        {
            float t = i / (float)safeSegmentCount;
            float currentAngleDegrees = Mathf.Lerp(startAngleDegrees, endAngleDegrees, t);
            Vector3 worldPosition = GetWorldPointOnRing(currentAngleDegrees, ringRadius);
            weakSectorLineRenderer.SetPosition(i, worldPosition);
        }
    }

    private void HideWeakSector()
    {
        isShowingWeakSector = false;

        if (weakSectorLineRenderer == null)
        {
            return;
        }

        weakSectorLineRenderer.positionCount = 0;
    }

    private bool TryGetSectorCenterAngle(int centerFaceIndex, out float angleDegrees)
    {
        angleDegrees = 0f;

        if (!faceBoard.TryGetFaceWorldPose(centerFaceIndex, out Vector3 faceWorldPosition, out _))
        {
            return false;
        }

        Vector2 direction = (faceWorldPosition - transform.position).normalized;
        angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return true;
    }

    private Vector3 GetWorldPointOnRing(float angleDegrees, float radius)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float x = Mathf.Cos(angleRadians) * radius;
        float y = Mathf.Sin(angleRadians) * radius;
        return transform.position + new Vector3(x, y, 0f);
    }

    private float GetRingRadius()
    {
        if (!useAutomaticRadius)
        {
            return Mathf.Max(0.01f, manualRadius);
        }

        if (bossBodyCollider == null)
        {
            return Mathf.Max(0.01f, manualRadius);
        }

        float colliderRadius = bossBodyCollider.bounds.extents.x;
        return Mathf.Max(0.01f, colliderRadius + radiusPadding);
    }

    private void ConfigureLineRenderer()
    {
        weakSectorLineRenderer.useWorldSpace = true;
        weakSectorLineRenderer.loop = false;
        weakSectorLineRenderer.positionCount = 0;
        weakSectorLineRenderer.widthMultiplier = lineWidth;
        weakSectorLineRenderer.startColor = weakSectorColor;
        weakSectorLineRenderer.endColor = weakSectorColor;
    }

    private void HandleWeakSectorChanged()
    {
        RefreshView();
    }
}
