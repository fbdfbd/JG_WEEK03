using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(LineRenderer))]
public class FaceSectorTelegraphView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FaceSectorResolver sectorResolver;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private MeshFilter fillMeshFilter;
    [SerializeField] private MeshRenderer fillMeshRenderer;
    [SerializeField] private Material fillMaterial;

    [Header("Color")]
    [SerializeField] private Color telegraphColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float outlineAlpha = 1f;
    [SerializeField] private float fillAlpha = 0.24f;

    [Header("Fill Animation")]
    [SerializeField] private Ease fillEase = Ease.Linear;

    [Header("Sorting")]
    [SerializeField] private int fillSortingOrderOffset = -1;

    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

    private readonly List<Vector3> sectorOutlinePoints = new List<Vector3>();
    private readonly List<Vector3> fillLocalVertices = new List<Vector3>();
    private readonly List<Vector3> animatedFillLocalVertices = new List<Vector3>();
    private MaterialPropertyBlock fillPropertyBlock;

    private FaceSectorRange currentSectorRange;
    private Mesh fillMesh;
    private Material runtimeFillMaterial;
    private Tween fillTween;
    private float currentFillProgress;
    private bool isShowing;

    private void Awake()
    {
        if (sectorResolver == null)
        {
            sectorResolver = GetComponentInParent<FaceSectorResolver>();
        }

        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        EnsureFillRenderer();
        ConfigureLineRenderer();
        ConfigureFillRenderer();
        Hide();
    }

    private void LateUpdate()
    {
        if (!isShowing)
        {
            return;
        }

        if (!transform.hasChanged)
        {
            return;
        }

        RefreshView();
        transform.hasChanged = false;
    }

    private void OnDisable()
    {
        Hide();
    }

    private void OnDestroy()
    {
        StopFillTween();

        if (fillMesh != null)
        {
            Destroy(fillMesh);
        }

        if (runtimeFillMaterial != null)
        {
            Destroy(runtimeFillMaterial);
        }
    }

    public void Show(FaceSectorRange sectorRange, float duration)
    {
        currentSectorRange = sectorRange;
        isShowing = true;
        currentFillProgress = 0f;
        RefreshView();
        PlayFillTween(duration);
    }

    public void Hide()
    {
        isShowing = false;
        currentFillProgress = 0f;
        StopFillTween();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }

        if (fillMesh != null)
        {
            fillMesh.Clear();
        }

        if (fillMeshRenderer != null)
        {
            fillMeshRenderer.enabled = false;
        }
    }

    public void RefreshView()
    {
        if (!isShowing || lineRenderer == null || sectorResolver == null)
        {
            return;
        }

        if (!sectorResolver.TryBuildSectorOutline(currentSectorRange, sectorOutlinePoints))
        {
            Hide();
            return;
        }

        RefreshOutline();
        RefreshFill();
        RefreshColors();
    }

    public void SetColor(Color color)
    {
        telegraphColor = color;
        RefreshColors();
    }

    private void RefreshOutline()
    {
        lineRenderer.positionCount = sectorOutlinePoints.Count;

        for (int i = 0; i < sectorOutlinePoints.Count; i++)
        {
            lineRenderer.SetPosition(i, sectorOutlinePoints[i]);
        }
    }

    private void RefreshFill()
    {
        if (fillMeshFilter == null || fillMeshRenderer == null)
        {
            return;
        }

        EnsureFillMesh();
        fillLocalVertices.Clear();

        int vertexCount = Mathf.Max(0, sectorOutlinePoints.Count - 1);
        if (vertexCount < 3)
        {
            fillMesh.Clear();
            fillMeshRenderer.enabled = false;
            return;
        }

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 localPoint = transform.InverseTransformPoint(sectorOutlinePoints[i]);
            fillLocalVertices.Add(localPoint);
        }

        int triangleCount = vertexCount - 2;
        int[] triangles = new int[triangleCount * 3];

        for (int i = 0; i < triangleCount; i++)
        {
            int triangleStartIndex = i * 3;
            triangles[triangleStartIndex] = 0;
            triangles[triangleStartIndex + 1] = i + 1;
            triangles[triangleStartIndex + 2] = i + 2;
        }

        animatedFillLocalVertices.Clear();

        Vector3 centerPoint = fillLocalVertices[0];
        for (int i = 0; i < fillLocalVertices.Count; i++)
        {
            Vector3 targetPoint = fillLocalVertices[i];
            Vector3 animatedPoint = i == 0
                ? centerPoint
                : Vector3.Lerp(centerPoint, targetPoint, currentFillProgress);

            animatedFillLocalVertices.Add(animatedPoint);
        }

        fillMesh.Clear();
        fillMesh.SetVertices(animatedFillLocalVertices);
        fillMesh.triangles = triangles;
        fillMesh.RecalculateBounds();

        fillMeshRenderer.enabled = true;
    }

    private void RefreshColors()
    {
        if (lineRenderer != null)
        {
            Color outlineColor = telegraphColor;
            outlineColor.a *= outlineAlpha;
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
        }

        if (fillMeshRenderer != null)
        {
            EnsureFillPropertyBlock();

            Color fillColor = telegraphColor;
            fillColor.a = fillAlpha;
            fillPropertyBlock.SetColor(ColorPropertyId, fillColor);
            fillMeshRenderer.SetPropertyBlock(fillPropertyBlock);
        }
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        RefreshColors();
    }

    private void ConfigureFillRenderer()
    {
        if (fillMeshRenderer == null)
        {
            return;
        }

        Renderer sourceRenderer = lineRenderer;
        fillMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        fillMeshRenderer.receiveShadows = false;
        fillMeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        fillMeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        fillMeshRenderer.sortingLayerID = sourceRenderer != null ? sourceRenderer.sortingLayerID : fillMeshRenderer.sortingLayerID;
        fillMeshRenderer.sortingOrder = sourceRenderer != null
            ? sourceRenderer.sortingOrder + fillSortingOrderOffset
            : fillMeshRenderer.sortingOrder;

        if (fillMaterial != null)
        {
            fillMeshRenderer.sharedMaterial = fillMaterial;
        }
        else
        {
            EnsureRuntimeFillMaterial();
            fillMeshRenderer.sharedMaterial = runtimeFillMaterial;
        }

        RefreshColors();
        fillMeshRenderer.enabled = false;
    }

    private void EnsureFillRenderer()
    {
        if (fillMeshFilter != null && fillMeshRenderer != null)
        {
            return;
        }

        Transform fillRoot = transform.Find("TelegraphFill");
        if (fillRoot == null)
        {
            GameObject fillObject = new GameObject("TelegraphFill");
            fillRoot = fillObject.transform;
            fillRoot.SetParent(transform, false);
        }

        if (fillMeshFilter == null)
        {
            fillMeshFilter = fillRoot.GetComponent<MeshFilter>();
            if (fillMeshFilter == null)
            {
                fillMeshFilter = fillRoot.gameObject.AddComponent<MeshFilter>();
            }
        }

        if (fillMeshRenderer == null)
        {
            fillMeshRenderer = fillRoot.GetComponent<MeshRenderer>();
            if (fillMeshRenderer == null)
            {
                fillMeshRenderer = fillRoot.gameObject.AddComponent<MeshRenderer>();
            }
        }
    }

    private void EnsureFillMesh()
    {
        if (fillMeshFilter == null)
        {
            return;
        }

        if (fillMesh == null)
        {
            fillMesh = new Mesh
            {
                name = "FaceSectorTelegraphFillMesh"
            };
        }

        if (fillMeshFilter.sharedMesh != fillMesh)
        {
            fillMeshFilter.sharedMesh = fillMesh;
        }
    }

    private void EnsureRuntimeFillMaterial()
    {
        if (runtimeFillMaterial != null)
        {
            return;
        }

        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader == null)
        {
            return;
        }

        runtimeFillMaterial = new Material(spriteShader)
        {
            name = "FaceSectorTelegraphFillMaterial"
        };
        runtimeFillMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    private void EnsureFillPropertyBlock()
    {
        if (fillPropertyBlock != null)
        {
            return;
        }

        fillPropertyBlock = new MaterialPropertyBlock();
    }

    private void PlayFillTween(float duration)
    {
        StopFillTween();

        if (duration <= 0f)
        {
            SetFillProgress(1f);
            return;
        }

        fillTween = DOVirtual.Float(0f, 1f, duration, SetFillProgress)
            .SetEase(fillEase)
            .SetTarget(this);
    }

    private void StopFillTween()
    {
        fillTween?.Kill();
        fillTween = null;
    }

    private void SetFillProgress(float progress)
    {
        currentFillProgress = Mathf.Clamp01(progress);
        RefreshFill();
    }
}
