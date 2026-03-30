using DG.Tweening;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(TextMeshProUGUI))]
public sealed class UI_TerritoryFloatingTextView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI textView;

    private void Awake()
    {
        rectTransform ??= GetComponent<RectTransform>();
        canvasGroup ??= GetComponent<CanvasGroup>();
        textView ??= GetComponent<TextMeshProUGUI>();
    }

    public Tween Play(
        string text,
        Color color,
        Vector2 startAnchoredPosition,
        Vector2 moveOffset,
        float duration)
    {
        rectTransform.anchoredPosition = startAnchoredPosition;
        canvasGroup.alpha = 1f;
        textView.text = text;
        textView.color = color;

        return DOTween.Sequence()
            .Append(rectTransform.DOAnchorPos(startAnchoredPosition + moveOffset, duration).SetEase(Ease.OutCubic))
            .Join(canvasGroup.DOFade(0f, duration).SetEase(Ease.OutQuad))
            .SetTarget(gameObject);
    }
}
