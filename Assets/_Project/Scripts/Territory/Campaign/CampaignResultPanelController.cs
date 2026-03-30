using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CampaignResultPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UICampaignResultPanelView resultPanelView;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private CampaignRunService campaignRunService;

    [Header("Style")]
    [SerializeField] private Vector2 panelSize = new Vector2(560f, 320f);
    [SerializeField] private Color backdropColor = new Color(0f, 0f, 0f, 0.72f);
    [SerializeField] private Color panelColor = new Color(0.11f, 0.14f, 0.18f, 0.97f);
    [SerializeField] private Color buttonColor = new Color(0.92f, 0.78f, 0.29f, 1f);
    [SerializeField] private Color titleColor = Color.white;
    [SerializeField] private Color summaryColor = new Color(0.92f, 0.95f, 0.98f, 1f);
    [SerializeField] private Color buttonTextColor = new Color(0.12f, 0.12f, 0.12f, 1f);

    public bool IsShowing => resultPanelView != null && resultPanelView.IsShowing;

    private static Sprite sharedWhiteSprite;

    private void Awake()
    {
        ResolveReferences();
        EnsureView();
        resultPanelView?.Hide();
    }

    private void OnEnable()
    {
        EnsureView();

        if (resultPanelView != null)
        {
            resultPanelView.CloseClicked -= HandleCloseClicked;
            resultPanelView.CloseClicked += HandleCloseClicked;
        }
    }

    private void OnDisable()
    {
        if (resultPanelView != null)
        {
            resultPanelView.CloseClicked -= HandleCloseClicked;
        }
    }

    public void Show(CampaignOutcomeResult outcomeResult)
    {
        if (!outcomeResult.IsFinished)
        {
            return;
        }

        ResolveReferences();
        EnsureView();
        if (resultPanelView == null)
        {
            return;
        }

        resultPanelView.Show(
            BuildTitle(outcomeResult),
            BuildSummary(outcomeResult));
    }

    public void Hide()
    {
        resultPanelView?.Hide();
    }

    private void HandleCloseClicked()
    {
        Hide();
        campaignRunService?.EndRun();

        if (GameManager.I != null)
        {
            GameManager.I.QuitGame();
            return;
        }

        Application.Quit();
    }

    private static string BuildTitle(CampaignOutcomeResult outcomeResult)
    {
        switch (outcomeResult.OutcomeType)
        {
            case CampaignOutcomeType.Victory:
                return "승리";
            case CampaignOutcomeType.Defeat:
                return "패배";
            case CampaignOutcomeType.Draw:
                return "무승부";
            default:
                return string.Empty;
        }
    }

    private static string BuildSummary(CampaignOutcomeResult outcomeResult)
    {
        switch (outcomeResult.Reason)
        {
            case CampaignOutcomeReason.EnemyStartCaptured:
                return "적 시작지를 점령했습니다.";
            case CampaignOutcomeReason.FinalDayTerritoryCount:
                return $"10일차 종료\n플레이어 {outcomeResult.PlayerTileCount}칸 / 적 {outcomeResult.EnemyTileCount}칸";
            default:
                return string.Empty;
        }
    }

    private void ResolveReferences()
    {
        if (GameManager.I != null)
        {
            campaignRunService ??= GameManager.I.CampaignRunService;
        }

        targetCanvas ??= FindCanvas();
        resultPanelView ??= FindFirstObjectByType<UICampaignResultPanelView>(FindObjectsInactive.Include);
    }

    private void EnsureView()
    {
        if (resultPanelView != null)
        {
            return;
        }

        if (targetCanvas == null)
        {
            targetCanvas = FindCanvas();
        }

        if (targetCanvas == null)
        {
            return;
        }

        Sprite whiteSprite = GetSharedWhiteSprite();

        GameObject rootObject = new GameObject("CampaignResultPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(UICampaignResultPanelView));
        rootObject.transform.SetParent(targetCanvas.transform, false);

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image backdropImage = rootObject.GetComponent<Image>();
        backdropImage.sprite = whiteSprite;
        backdropImage.type = Image.Type.Simple;
        backdropImage.color = backdropColor;
        backdropImage.raycastTarget = true;

        GameObject panelObject = CreateChild("Panel", rootObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = panelSize;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.sprite = whiteSprite;
        panelImage.type = Image.Type.Simple;
        panelImage.color = panelColor;

        TextMeshProUGUI titleText = CreateText("TitleText", panelObject.transform, 54f, titleColor, FontStyles.Bold);
        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -36f);
        titleRect.sizeDelta = new Vector2(-48f, 72f);

        TextMeshProUGUI summaryText = CreateText("SummaryText", panelObject.transform, 32f, summaryColor, FontStyles.Normal);
        RectTransform summaryRect = summaryText.rectTransform;
        summaryRect.anchorMin = new Vector2(0f, 0.5f);
        summaryRect.anchorMax = new Vector2(1f, 0.5f);
        summaryRect.pivot = new Vector2(0.5f, 0.5f);
        summaryRect.anchoredPosition = new Vector2(0f, 4f);
        summaryRect.sizeDelta = new Vector2(-72f, 120f);

        GameObject closeButtonObject = CreateChild("CloseButton", panelObject.transform, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        RectTransform closeButtonRect = closeButtonObject.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.5f, 0f);
        closeButtonRect.anchorMax = new Vector2(0.5f, 0f);
        closeButtonRect.pivot = new Vector2(0.5f, 0f);
        closeButtonRect.anchoredPosition = new Vector2(0f, 28f);
        closeButtonRect.sizeDelta = new Vector2(180f, 56f);

        Image closeButtonImage = closeButtonObject.GetComponent<Image>();
        closeButtonImage.sprite = whiteSprite;
        closeButtonImage.type = Image.Type.Simple;
        closeButtonImage.color = buttonColor;

        Button closeButton = closeButtonObject.GetComponent<Button>();
        closeButton.targetGraphic = closeButtonImage;

        TextMeshProUGUI closeButtonText = CreateText("ButtonText", closeButtonObject.transform, 28f, buttonTextColor, FontStyles.Bold);
        RectTransform closeTextRect = closeButtonText.rectTransform;
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        closeButtonText.text = "종료";

        resultPanelView = rootObject.GetComponent<UICampaignResultPanelView>();
        resultPanelView.Initialize(rootObject, titleText, summaryText, closeButton);
        resultPanelView.Hide();
    }

    private Canvas FindCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas != null && canvas.name == "Canvas_HUD")
            {
                return canvas;
            }
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }

    private static GameObject CreateChild(string objectName, Transform parent, params System.Type[] componentTypes)
    {
        GameObject gameObject = new GameObject(objectName, componentTypes);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static TextMeshProUGUI CreateText(string objectName, Transform parent, float fontSize, Color color, FontStyles fontStyle)
    {
        GameObject textObject = CreateChild(objectName, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        TextMeshProUGUI textView = textObject.GetComponent<TextMeshProUGUI>();
        textView.alignment = TextAlignmentOptions.Center;
        textView.fontSize = fontSize;
        textView.color = color;
        textView.fontStyle = fontStyle;
        textView.raycastTarget = false;
        return textView;
    }

    private static Sprite GetSharedWhiteSprite()
    {
        if (sharedWhiteSprite != null)
        {
            return sharedWhiteSprite;
        }

        sharedWhiteSprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f));

        return sharedWhiteSprite;
    }
}
