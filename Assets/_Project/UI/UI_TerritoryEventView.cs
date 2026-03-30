using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_TerritoryEventView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject rootObject;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text situationText;
    [SerializeField] private Button firstOptionButton;
    [SerializeField] private TMP_Text firstOptionTitleText;
    [SerializeField] private TMP_Text firstOptionDescriptionText;
    [SerializeField] private Button secondOptionButton;
    [SerializeField] private TMP_Text secondOptionTitleText;
    [SerializeField] private TMP_Text secondOptionDescriptionText;
    [SerializeField] private Button closeButton;

    private bool _isBound;

    public bool IsVisible => rootObject != null && rootObject.activeSelf;

    public event Action FirstOptionClicked;
    public event Action SecondOptionClicked;
    public event Action CloseClicked;

    private void Awake()
    {
        rootObject ??= gameObject;
    }

    private void Reset()
    {
        AssignMissingReferences();
    }

    private void OnValidate()
    {
        AssignMissingReferences();
    }

    public void Bind()
    {
        if (_isBound)
        {
            return;
        }

        if (firstOptionButton != null)
        {
            firstOptionButton.onClick.AddListener(HandleFirstOptionClicked);
        }

        if (secondOptionButton != null)
        {
            secondOptionButton.onClick.AddListener(HandleSecondOptionClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        _isBound = true;
    }

    public void Show(TerritoryEventDialogViewModel viewModel)
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

        if (titleText != null)
        {
            titleText.text = viewModel.Title;
        }

        if (situationText != null)
        {
            situationText.text = viewModel.SituationText;
        }

        if (firstOptionTitleText != null)
        {
            firstOptionTitleText.text = viewModel.FirstOption.Title;
        }

        if (firstOptionDescriptionText != null)
        {
            firstOptionDescriptionText.text = viewModel.FirstOption.Description;
        }

        if (secondOptionTitleText != null)
        {
            secondOptionTitleText.text = viewModel.SecondOption.Title;
        }

        if (secondOptionDescriptionText != null)
        {
            secondOptionDescriptionText.text = viewModel.SecondOption.Description;
        }

        if (firstOptionButton != null)
        {
            firstOptionButton.interactable = viewModel.FirstOption.IsSelectable;
        }

        if (secondOptionButton != null)
        {
            secondOptionButton.interactable = viewModel.SecondOption.IsSelectable;
        }
    }

    public void Hide()
    {
        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }

    private void HandleFirstOptionClicked()
    {
        FirstOptionClicked?.Invoke();
    }

    private void HandleSecondOptionClicked()
    {
        SecondOptionClicked?.Invoke();
    }

    private void HandleCloseClicked()
    {
        CloseClicked?.Invoke();
    }

    private void AssignMissingReferences()
    {
        rootObject ??= gameObject;

        titleText ??= FindChildComponent<TMP_Text>("TitleText");
        situationText ??= FindChildComponent<TMP_Text>("SituationText");
        firstOptionButton ??= FindChildComponent<Button>("FirstOptionButton");
        firstOptionTitleText ??= FindChildComponent<TMP_Text>("FirstOptionTitleText");
        firstOptionDescriptionText ??= FindChildComponent<TMP_Text>("FirstOptionDescriptionText");
        secondOptionButton ??= FindChildComponent<Button>("SecondOptionButton");
        secondOptionTitleText ??= FindChildComponent<TMP_Text>("SecondOptionTitleText");
        secondOptionDescriptionText ??= FindChildComponent<TMP_Text>("SecondOptionDescriptionText");
        closeButton ??= FindChildComponent<Button>("CloseButton");
    }

    private T FindChildComponent<T>(string objectName) where T : Component
    {
        Transform[] childTransforms = GetComponentsInChildren<Transform>(true);
        for (int index = 0; index < childTransforms.Length; index++)
        {
            Transform childTransform = childTransforms[index];
            if (!string.Equals(childTransform.name, objectName, StringComparison.Ordinal))
            {
                continue;
            }

            if (childTransform.TryGetComponent(out T component))
            {
                return component;
            }
        }

        return null;
    }
}
