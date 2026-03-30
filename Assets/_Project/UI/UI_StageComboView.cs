using DG.Tweening;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_StageComboView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject comboRoot;
    [SerializeField] private RectTransform comboPanel;
    [SerializeField] private CanvasGroup comboCanvasGroup;
    [SerializeField] private TMP_Text comboText;

    [Header("Layout")]
    [SerializeField] private Vector2 shownAnchoredPosition = new Vector2(80f, -120f);
    [SerializeField] private Vector2 hiddenAnchoredPosition = new Vector2(-260f, -120f);

    [Header("Animation")]
    [SerializeField] private float slideInDuration = 0.22f;
    [SerializeField] private float slideOutDuration = 0.18f;
    [SerializeField] private Ease slideInEase = Ease.OutCubic;
    [SerializeField] private Ease slideOutEase = Ease.InCubic;

    private Tween moveTween;
    private Tween fadeTween;
    private bool isVisible;

    private void Awake()
    {
        ResolveReferences();
        ForceHiddenState();
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public void ShowCombo(int comboCount)
    {
        ResolveReferences();
        if (comboRoot == null || comboPanel == null || comboCanvasGroup == null || comboText == null)
        {
            return;
        }

        comboText.text = $"{comboCount} COMBO";
        comboRoot.SetActive(true);

        KillTweens();

        if (!isVisible)
        {
            comboPanel.anchoredPosition = hiddenAnchoredPosition;
            comboCanvasGroup.alpha = 0f;
        }

        moveTween = comboPanel.DOAnchorPos(shownAnchoredPosition, slideInDuration).SetEase(slideInEase);
        fadeTween = comboCanvasGroup.DOFade(1f, slideInDuration).SetEase(Ease.OutSine);
        isVisible = true;
    }

    public void HideCombo()
    {
        ResolveReferences();
        if (comboRoot == null || comboPanel == null || comboCanvasGroup == null)
        {
            return;
        }

        if (!comboRoot.activeSelf)
        {
            ForceHiddenState();
            return;
        }

        KillTweens();

        moveTween = comboPanel
            .DOAnchorPos(hiddenAnchoredPosition, slideOutDuration)
            .SetEase(slideOutEase)
            .OnComplete(ForceHiddenState);

        fadeTween = comboCanvasGroup.DOFade(0f, slideOutDuration).SetEase(Ease.InSine);
        isVisible = false;
    }

    private void ResolveReferences()
    {
        comboRoot ??= gameObject;
        comboPanel ??= comboRoot != null ? comboRoot.GetComponent<RectTransform>() : null;
        comboCanvasGroup ??= comboRoot != null ? comboRoot.GetComponent<CanvasGroup>() : null;
        comboText ??= GetComponentInChildren<TMP_Text>(true);
    }

    private void ForceHiddenState()
    {
        ResolveReferences();
        isVisible = false;

        if (comboPanel != null)
        {
            comboPanel.anchoredPosition = hiddenAnchoredPosition;
        }

        if (comboCanvasGroup != null)
        {
            comboCanvasGroup.alpha = 0f;
        }

        if (comboRoot != null)
        {
            comboRoot.SetActive(false);
        }
    }

    private void KillTweens()
    {
        moveTween?.Kill();
        fadeTween?.Kill();
        moveTween = null;
        fadeTween = null;
    }
}
