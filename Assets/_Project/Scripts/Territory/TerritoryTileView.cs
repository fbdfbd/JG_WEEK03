using DG.Tweening;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(TerritoryTileInput))]
public sealed class TerritoryTileView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private SpriteRenderer tileRenderer;
    [SerializeField] private SpriteRenderer[] extraRenderers;
    [SerializeField] private TerritoryTileInput tileInput;
    [SerializeField] private GameObject selectionHighlight;
    [SerializeField] private TMP_Text tileLabel;
    [SerializeField] private bool showTileIdLabel;

    [Header("Colors")]
    [SerializeField] private Color neutralBossColor = Color.white;
    [SerializeField] private Color neutralEventColor = new Color(0.89f, 0.97f, 1f, 1f);
    [SerializeField] private Color playerOwnedColor = new Color(0.58f, 0.82f, 1f, 1f);
    [SerializeField] private Color enemyOwnedColor = new Color(1f, 0.74f, 0.68f, 1f);
    [SerializeField] private Color playerStartColor = new Color(0.42f, 0.76f, 1f, 1f);
    [SerializeField] private Color enemyStartColor = new Color(1f, 0.52f, 0.45f, 1f);
    [SerializeField] private Color pendingCaptureColor = new Color(1f, 0.84f, 0.42f, 1f);
    [SerializeField] private Color selectedTintColor = new Color(1f, 1f, 0.82f, 1f);
    [SerializeField] private Color lockedColor = new Color(0.45f, 0.52f, 0.62f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.65f, 0.65f, 0.65f, 1f);
    [SerializeField] private Color hoverTintColor = new Color(1f, 0.97f, 0.82f, 1f);
    [SerializeField] private Color actionAvailableTintColor = new Color(0.97f, 1f, 0.86f, 1f);

    [Header("Hover Motion")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.12f;
    [SerializeField] private Ease hoverEase = Ease.OutCubic;

    private Tween _hoverTween;
    private Tween _turnFxTween;
    private TerritoryTileState _lastRenderedState;
    private int _tileId = -1;
    private bool _isBound;
    private bool _isHovering;
    private Vector3 _defaultScale = Vector3.one;

    public int TileId => _tileId;
    public TerritoryTileInput Input => tileInput;
    public int SortingOrder => tileRenderer != null ? tileRenderer.sortingOrder : 0;

    private void Awake()
    {
        visualRoot ??= transform;
        tileRenderer ??= GetComponent<SpriteRenderer>();
        tileInput ??= GetComponent<TerritoryTileInput>();
        _defaultScale = visualRoot.localScale;
    }

    private void OnEnable()
    {
        if (tileInput == null)
        {
            return;
        }

        tileInput.HoverEntered += HandleHoverEntered;
        tileInput.HoverExited += HandleHoverExited;
    }

    private void OnDisable()
    {
        if (tileInput != null)
        {
            tileInput.HoverEntered -= HandleHoverEntered;
            tileInput.HoverExited -= HandleHoverExited;
        }

        _isHovering = false;
        StopHoverMotion(resetScale: true);
    }

    private void OnDestroy()
    {
        StopHoverMotion(resetScale: false);
        StopTurnPulseFx(resetScale: false);
    }

    public void Bind(TerritoryTileLayoutData layoutData)
    {
        if (layoutData == null)
        {
            Debug.LogWarning("TerritoryTileView requires layout data when binding.");
            return;
        }

        _tileId = layoutData.TileId;
        _isBound = true;
        transform.localPosition = layoutData.LocalPosition;
        ApplySortingOrder(layoutData.SortingOrder);
        RefreshLabel(layoutData);

        if (tileInput != null)
        {
            tileInput.Bind(layoutData.TileId);
        }
    }

    public void Render(TerritoryTileState tileState)
    {
        if (tileState == null)
        {
            return;
        }

        if (_isBound && tileState.TileId != _tileId)
        {
            Debug.LogWarning($"TerritoryTileView received mismatched state. Bound TileId: {_tileId}, Incoming TileId: {tileState.TileId}");
            return;
        }

        _lastRenderedState = tileState;

        if (tileRenderer != null)
        {
            tileRenderer.color = ResolveTileColor(tileState, _isHovering && tileState.CanInteract);
        }

        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(tileState.IsSelected);
        }

        if (tileInput != null)
        {
            tileInput.SetInteractable(tileState.CanInteract);
        }

        if (!tileState.CanInteract && _isHovering)
        {
            _isHovering = false;
            StopHoverMotion(resetScale: true);
        }
    }

    private void HandleHoverEntered(int tileId)
    {
        if (tileId != _tileId)
        {
            return;
        }

        _isHovering = true;
        PlayHoverMotion(hoverScale);
        RefreshVisualColor();
    }

    private void HandleHoverExited(int tileId)
    {
        if (tileId != _tileId)
        {
            return;
        }

        _isHovering = false;
        PlayHoverMotion(1f);
        RefreshVisualColor();
    }

    private void RefreshLabel(TerritoryTileLayoutData layoutData)
    {
        if (tileLabel == null)
        {
            return;
        }

        tileLabel.text = showTileIdLabel ? layoutData.TileId.ToString() : string.Empty;
    }

    private void RefreshVisualColor()
    {
        if (_lastRenderedState == null || tileRenderer == null)
        {
            return;
        }

        tileRenderer.color = ResolveTileColor(_lastRenderedState, _isHovering && _lastRenderedState.CanInteract);
    }

    private Color ResolveTileColor(TerritoryTileState tileState, bool isHovering)
    {
        if (!tileState.IsUnlocked)
        {
            return lockedColor;
        }

        if (!tileState.IsInteractable)
        {
            return disabledColor;
        }

        Color baseColor = ResolveBaseColor(tileState);

        if (tileState.HasAvailableAction)
        {
            baseColor = Color.Lerp(baseColor, actionAvailableTintColor, 0.28f);
        }

        if (tileState.IsSelected)
        {
            baseColor = Color.Lerp(baseColor, selectedTintColor, 0.35f);
        }

        if (isHovering)
        {
            baseColor = Color.Lerp(baseColor, hoverTintColor, 0.45f);
        }

        return baseColor;
    }

    private Color ResolveBaseColor(TerritoryTileState tileState)
    {
        if (tileState.PendingCaptureState == TerritoryPendingCaptureState.EnemyPlanned)
        {
            return pendingCaptureColor;
        }

        if (tileState.IsPlayerStart)
        {
            return playerStartColor;
        }

        if (tileState.IsEnemyStart)
        {
            return enemyStartColor;
        }

        return tileState.Owner switch
        {
            TerritoryTileOwnerType.Player => playerOwnedColor,
            TerritoryTileOwnerType.Enemy => enemyOwnedColor,
            _ => tileState.ContentType == TerritoryTileContentType.Event ? neutralEventColor : neutralBossColor
        };
    }

    private void ApplySortingOrder(int sortingOrder)
    {
        if (tileRenderer != null)
        {
            tileRenderer.sortingOrder = sortingOrder;
        }

        if (extraRenderers == null)
        {
            return;
        }

        for (int index = 0; index < extraRenderers.Length; index++)
        {
            SpriteRenderer extraRenderer = extraRenderers[index];
            if (extraRenderer == null)
            {
                continue;
            }

            extraRenderer.sortingOrder = sortingOrder + index + 1;
        }
    }

    private void PlayHoverMotion(float targetScaleMultiplier)
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 targetScale = _defaultScale * targetScaleMultiplier;

        StopHoverMotion(resetScale: false);
        _hoverTween = visualRoot.DOScale(targetScale, hoverDuration)
            .SetEase(hoverEase)
            .SetTarget(visualRoot);
    }

    // 턴 전환처럼 일괄 연출이 필요할 때, 타일만 짧게 강조한다.
    public void PlayTurnPulseFx(float targetScaleMultiplier, float duration)
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 baseScale = _isHovering ? _defaultScale * hoverScale : _defaultScale;
        Vector3 targetScale = _defaultScale * targetScaleMultiplier;

        StopTurnPulseFx(resetScale: false);
        _turnFxTween = DOTween.Sequence()
            .Append(visualRoot.DOScale(targetScale, duration))
            .Append(visualRoot.DOScale(baseScale, duration))
            .SetEase(Ease.OutCubic)
            .SetTarget($"{name}_TurnPulse");
    }

    private void StopHoverMotion(bool resetScale)
    {
        if (_hoverTween != null)
        {
            _hoverTween.Kill();
            _hoverTween = null;
        }

        if (resetScale && visualRoot != null)
        {
            visualRoot.localScale = _defaultScale;
        }
    }

    private void StopTurnPulseFx(bool resetScale)
    {
        if (_turnFxTween != null)
        {
            _turnFxTween.Kill();
            _turnFxTween = null;
        }

        if (resetScale && visualRoot != null)
        {
            visualRoot.localScale = _isHovering ? _defaultScale * hoverScale : _defaultScale;
        }
    }
}
