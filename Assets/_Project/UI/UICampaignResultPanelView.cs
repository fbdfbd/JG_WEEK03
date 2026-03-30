using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UICampaignResultPanelView : MonoBehaviour
{
    [SerializeField] private GameObject rootObject;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private Button closeButton;

    public bool IsShowing => rootObject != null && rootObject.activeSelf;

    public event Action CloseClicked;

    private void OnEnable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }
    }

    private void OnDisable()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }
    }

    public void Initialize(GameObject rootObject, TMP_Text titleText, TMP_Text summaryText, Button closeButton)
    {
        this.rootObject = rootObject;
        this.titleText = titleText;
        this.summaryText = summaryText;
        this.closeButton = closeButton;
    }

    public void Show(string title, string summary)
    {
        if (rootObject == null)
        {
            rootObject = gameObject;
        }

        if (titleText != null)
        {
            titleText.text = title ?? string.Empty;
        }

        if (summaryText != null)
        {
            summaryText.text = summary ?? string.Empty;
        }

        rootObject.SetActive(true);
    }

    public void Hide()
    {
        if (rootObject == null)
        {
            rootObject = gameObject;
        }

        rootObject.SetActive(false);
    }

    private void HandleCloseClicked()
    {
        CloseClicked?.Invoke();
    }
}
