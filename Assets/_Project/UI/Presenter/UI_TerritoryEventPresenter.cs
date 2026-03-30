using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_TerritoryEventPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_TerritoryEventView eventView;
    [SerializeField] private SOTerritoryEventCatalog eventCatalog;

    private readonly TerritoryEventActionService _eventActionService = new TerritoryEventActionService();

    public TerritoryEventSelectionSession CurrentSession { get; private set; }
    public bool IsShowing => eventView != null && eventView.IsVisible;

    public event Action<int> OptionSelected;
    public event Action Closed;

    private void Awake()
    {
        ResolveReferences();
        eventView?.Bind();
        eventView?.Hide();
    }

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        eventView?.Bind();
        SubscribeToView();
    }

    private void OnDisable()
    {
        UnsubscribeFromView();
    }

    public bool TryShow(TerritoryPrimaryActionRequest actionRequest)
    {
        if (eventView == null || eventCatalog == null)
        {
            return false;
        }

        TerritoryEventSelectionSession session = _eventActionService.CreateSession(actionRequest, eventCatalog);
        if (session == null)
        {
            return false;
        }

        CurrentSession = session;
        eventView.Show(BuildViewModel(session));
        return true;
    }

    public void Hide()
    {
        CurrentSession = null;
        eventView?.Hide();
    }

    private void ResolveReferences()
    {
        eventView ??= FindFirstObjectByType<UI_TerritoryEventView>(FindObjectsInactive.Include);
    }

    private void SubscribeToView()
    {
        if (eventView == null)
        {
            return;
        }

        eventView.FirstOptionClicked += HandleFirstOptionClicked;
        eventView.SecondOptionClicked += HandleSecondOptionClicked;
        eventView.CloseClicked += HandleCloseClicked;
    }

    private void UnsubscribeFromView()
    {
        if (eventView == null)
        {
            return;
        }

        eventView.FirstOptionClicked -= HandleFirstOptionClicked;
        eventView.SecondOptionClicked -= HandleSecondOptionClicked;
        eventView.CloseClicked -= HandleCloseClicked;
    }

    private TerritoryEventDialogViewModel BuildViewModel(TerritoryEventSelectionSession session)
    {
        return new TerritoryEventDialogViewModel(
            session.EventDefinition.Title,
            session.EventDefinition.SituationText,
            BuildOptionViewModel(session.EventDefinition.FirstOption),
            BuildOptionViewModel(session.EventDefinition.SecondOption));
    }

    private TerritoryEventOptionViewModel BuildOptionViewModel(TerritoryEventOptionDefinition option)
    {
        if (option == null)
        {
            return TerritoryEventOptionViewModel.Empty;
        }

        return new TerritoryEventOptionViewModel(
            option.Label,
            option.Description,
            !string.IsNullOrWhiteSpace(option.Label));
    }

    private void HandleFirstOptionClicked()
    {
        OptionSelected?.Invoke(0);
    }

    private void HandleSecondOptionClicked()
    {
        OptionSelected?.Invoke(1);
    }

    private void HandleCloseClicked()
    {
        Hide();
        Closed?.Invoke();
    }
}
