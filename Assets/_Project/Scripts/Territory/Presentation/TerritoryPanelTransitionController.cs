using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryPanelTransitionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_LobbyTopPanelView topPanelView;
    [SerializeField] private UI_LobbyBottomPanelView bottomPanelView;

    [Header("Motion")]
    [SerializeField] private float hideDuration = 0.35f;
    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hiddenPadding = 40f;
    [SerializeField] private Ease motionEase = Ease.InOutCubic;

    private Vector2 _topShownPosition;
    private Vector2 _bottomShownPosition;
    private bool _isInitialized;

    private void Awake()
    {
        ResolveReferences();
        CacheShownPositions();
    }

    public Tween PlayHide()
    {
        ResolveReferences();
        CacheShownPositions();

        if (topPanelView == null || bottomPanelView == null)
        {
            return null;
        }

        Vector2 topHiddenPosition = _topShownPosition + Vector2.up * GetHiddenDistance(topPanelView.TopPanel);
        Vector2 bottomHiddenPosition = _bottomShownPosition + Vector2.down * GetHiddenDistance(bottomPanelView.BottomPanel);

        return DOTween.Sequence()
            .Join(topPanelView.TopPanel.DOAnchorPos(topHiddenPosition, hideDuration).SetEase(motionEase))
            .Join(bottomPanelView.BottomPanel.DOAnchorPos(bottomHiddenPosition, hideDuration).SetEase(motionEase))
            .SetTarget(gameObject);
    }

    public Tween PlayShow()
    {
        ResolveReferences();
        CacheShownPositions();

        if (topPanelView == null || bottomPanelView == null)
        {
            return null;
        }

        return DOTween.Sequence()
            .Join(topPanelView.TopPanel.DOAnchorPos(_topShownPosition, showDuration).SetEase(motionEase))
            .Join(bottomPanelView.BottomPanel.DOAnchorPos(_bottomShownPosition, showDuration).SetEase(motionEase))
            .SetTarget(gameObject);
    }

    private void ResolveReferences()
    {
        topPanelView ??= FindFirstObjectByType<UI_LobbyTopPanelView>(FindObjectsInactive.Include);
        bottomPanelView ??= FindFirstObjectByType<UI_LobbyBottomPanelView>(FindObjectsInactive.Include);
    }

    private void CacheShownPositions()
    {
        if (_isInitialized)
        {
            return;
        }

        ResolveReferences();
        if (topPanelView == null || bottomPanelView == null)
        {
            return;
        }

        _topShownPosition = topPanelView.TopPanel.anchoredPosition;
        _bottomShownPosition = bottomPanelView.BottomPanel.anchoredPosition;
        _isInitialized = true;
    }

    private float GetHiddenDistance(RectTransform targetPanel)
    {
        if (targetPanel == null)
        {
            return hiddenPadding;
        }

        return targetPanel.rect.height + hiddenPadding;
    }
}
