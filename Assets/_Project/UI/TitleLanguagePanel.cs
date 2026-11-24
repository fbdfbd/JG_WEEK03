using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Created under the existing title Canvas, using the title button's font and colors.
public sealed class TitleLanguagePanel
{
    private readonly GameObject root;
    private readonly Button koreanButton;
    private readonly Button englishButton;
    private readonly TMP_Text closeLabel;
    private readonly Button opener;
    private readonly UnityAction languageChanged;
    private readonly Color buttonColor;

    public TitleLanguagePanel(Button template, UnityAction onLanguageChanged)
    {
        opener = template;
        languageChanged = onLanguageChanged;
        buttonColor = template.targetGraphic.color;
        Canvas canvas = template.GetComponentInParent<Canvas>().rootCanvas;
        TMP_Text source = template.GetComponentInChildren<TMP_Text>(true);

        root = new GameObject("LanguageOptions", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        RectTransform backdrop = (RectTransform)root.transform;
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

        RectTransform panel = Rect("Panel", root.transform, new Vector2(540f, 380f), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.11f, 0.14f, 0.18f, 1f);
        Label("Language", panel, source, "언어 / Language", new Vector2(460f, 60f), new Vector2(0f, 135f));
        koreanButton = MakeButton("Korean", panel, source, template, "한국어", 50f, () => Select(false));
        englishButton = MakeButton("English", panel, source, template, "English", -35f, () => Select(true));
        Button close = MakeButton("Close", panel, source, template, "", -130f, Hide);
        closeLabel = close.GetComponentInChildren<TMP_Text>();
        LinkNavigation(koreanButton, close, englishButton);
        LinkNavigation(englishButton, koreanButton, close);
        LinkNavigation(close, englishButton, koreanButton);
        root.SetActive(false);
    }

    public void Show()
    {
        Refresh();
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        (GameLanguage.IsEnglish ? englishButton : koreanButton).Select();
    }

    public void Hide()
    {
        if (root == null) return;
        root.SetActive(false);
        if (opener != null && opener.isActiveAndEnabled) opener.Select();
    }

    private void Select(bool useEnglish)
    {
        GameLanguage.SetEnglish(useEnglish);
        languageChanged?.Invoke();
        Refresh();
    }

    private void Refresh()
    {
        closeLabel.text = GameLanguage.Text("닫기");
        Color selectedColor = new Color(0.2f, 0.35f, 0.5f);
        koreanButton.GetComponent<Image>().color = GameLanguage.IsEnglish ? buttonColor : selectedColor;
        englishButton.GetComponent<Image>().color = GameLanguage.IsEnglish ? selectedColor : buttonColor;
    }

    private static Button MakeButton(string name, Transform parent, TMP_Text source, Button template,
        string text, float y, UnityAction action)
    {
        RectTransform rect = Rect(name, parent, new Vector2(380f, 66f), new Vector2(0f, y));
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = template.targetGraphic.color;
        if (template.targetGraphic is Image original)
        {
            image.sprite = original.sprite;
            image.type = original.type;
        }
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = template.colors;
        button.onClick.AddListener(action);
        Label("Label", rect, source, text, new Vector2(350f, 56f), Vector2.zero);
        return button;
    }

    private static void LinkNavigation(Button button, Button up, Button down)
    {
        button.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up,
            selectOnDown = down
        };
    }

    private static void Label(string name, Transform parent, TMP_Text source, string text, Vector2 size, Vector2 position)
    {
        RectTransform rect = Rect(name, parent, size, position);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = source.font;
        label.fontSharedMaterial = source.fontSharedMaterial;
        label.fontSize = 32f;
        label.color = source.color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        label.text = text;
        GameLanguage.Fit(label);
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }
}
