using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_TerritoryInfoView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private TMP_Text tileNameText;
    [SerializeField] private TMP_Text tileOwnerText;
    [SerializeField] private TMP_Text tileStateText;
    [SerializeField] private TMP_Text tileHintText;
    [SerializeField] private Button enterStageButton;
    [SerializeField] private TMP_Text enterStageButtonText;
    [SerializeField] private Button closeButton;

    private bool _isBound;

    public event Action PrimaryActionClicked;
    public event Action CloseClicked;

    private void Awake()
    {
        rootObject ??= gameObject;

        if (enterStageButtonText == null && enterStageButton != null)
        {
            enterStageButtonText = enterStageButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Bind()
    {
        if (_isBound)
        {
            return;
        }

        if (enterStageButton != null)
        {
            enterStageButton.onClick.AddListener(HandlePrimaryActionClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        _isBound = true;
    }

    // 패널은 전달받은 표시용 모델만 그리고, 규칙 판단은 하지 않는다.
    public void Show(TerritoryTileInfoViewModel viewModel)
    {
        if (viewModel == null)
        {
            Hide();
            return;
        }

        if (rootObject != null)
        {
            rootObject.SetActive(true);
        }

        if (tileNameText != null)
        {
            tileNameText.text = viewModel.Title;
        }

        if (tileOwnerText != null)
        {
            tileOwnerText.text = viewModel.OwnerText;
        }

        if (tileStateText != null)
        {
            tileStateText.text = $"{viewModel.ContentText} / {viewModel.StatusText}";
        }

        if (tileHintText != null)
        {
            tileHintText.text = viewModel.HintText;
        }

        if (enterStageButtonText != null)
        {
            enterStageButtonText.text = viewModel.PrimaryActionLabel;
        }

        if (enterStageButton != null)
        {
            enterStageButton.gameObject.SetActive(viewModel.CanRequestStage);
            enterStageButton.interactable = viewModel.CanRequestStage;
        }
    }

    public void Hide()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }

    private void HandlePrimaryActionClicked()
    {
        PrimaryActionClicked?.Invoke();
    }

    private void HandleCloseClicked()
    {
        Hide();
        CloseClicked?.Invoke();
    }
}
