using System;
using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class LobbySceneController : MonoBehaviour
{
    [Header("Territory References")]
    [SerializeField] private TerritoryMapController territoryMapController;
    [SerializeField] private TerritoryTurnService territoryTurnService;
    [SerializeField] private TerritoryInfoPanelBridge territoryInfoPanelBridge;
    [SerializeField] private UI_TerritoryInfoView territoryInfoView;
    [SerializeField] private UI_TerritoryTurnView territoryTurnView;
    [SerializeField] private TerritoryTurnFxController territoryTurnFxController;
    [SerializeField] private TerritoryDayAdvancePresentationController territoryDayAdvancePresentationController;
    [SerializeField] private StageRewardPresentationController stageRewardPresentationController;
    [SerializeField] private FinalDayTerritoryCountPresentationController finalDayTerritoryCountPresentationController;
    [SerializeField] private CampaignResultPanelController campaignResultPanelController;
    [SerializeField] private UI_InputBlockerView inputBlockerView;
    [SerializeField] private UI_TerritoryEventPresenter territoryEventPresenter;

    [Header("Global References")]
    [SerializeField] private CampaignRunService campaignRunService;
    [SerializeField] private StageFlowService stageFlowService;
    [SerializeField] private PlayerProfileService playerProfileService;

    [Header("Stage Settings")]
    [SerializeField] private TerritoryStageSceneSettings stageSceneSettings = new TerritoryStageSceneSettings();

    private readonly TerritoryStageEntryContextFactory _stageEntryContextFactory = new TerritoryStageEntryContextFactory();
    private readonly TerritoryEventActionService _eventActionService = new TerritoryEventActionService();
    private readonly TerritoryStageRewardApplier _stageRewardApplier = new TerritoryStageRewardApplier();
    private readonly CampaignVictoryResolver _campaignVictoryResolver = new CampaignVictoryResolver();
    private readonly FinalDayTerritoryCollector _finalDayTerritoryCollector = new FinalDayTerritoryCollector();
    private bool _isAdvancingDay;

    public event Action<TerritoryStageRequest> StageEntryRequested;
    public event Action<TerritoryDayAdvanceResult> DayAdvanceCompleted;

    private void Awake()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>();
        territoryTurnService ??= territoryMapController != null ? territoryMapController.TurnService : FindFirstObjectByType<TerritoryTurnService>();
        territoryInfoPanelBridge ??= territoryMapController != null ? territoryMapController.InfoPanelBridge : FindFirstObjectByType<TerritoryInfoPanelBridge>();
        territoryInfoView ??= FindFirstObjectByType<UI_TerritoryInfoView>(FindObjectsInactive.Include);
        territoryTurnView ??= FindFirstObjectByType<UI_TerritoryTurnView>(FindObjectsInactive.Include);
        territoryTurnFxController ??= FindFirstObjectByType<TerritoryTurnFxController>(FindObjectsInactive.Include);
        territoryDayAdvancePresentationController ??= GetComponent<TerritoryDayAdvancePresentationController>();
        stageRewardPresentationController ??= GetOrAddComponent<StageRewardPresentationController>();
        finalDayTerritoryCountPresentationController ??= GetOrAddComponent<FinalDayTerritoryCountPresentationController>();
        campaignResultPanelController ??= GetOrAddComponent<CampaignResultPanelController>();
        inputBlockerView ??= FindFirstObjectByType<UI_InputBlockerView>(FindObjectsInactive.Include);
        territoryEventPresenter ??= FindFirstObjectByType<UI_TerritoryEventPresenter>(FindObjectsInactive.Include);

        EnsureGlobalServices();
        territoryMapController?.SetBuildOnStart(false);
    }

    private void OnEnable()
    {
        SubscribeToViews();
        SubscribeToData();
    }

    private void Start()
    {
        territoryInfoView?.Bind();
        territoryTurnView?.Bind();

        EnsureGlobalServices();
        InitializeTerritoryRun();
        TryApplyPendingStageResult();
        RefreshVisibleState();
    }

    private void OnDisable()
    {
        UnsubscribeFromData();
        UnsubscribeFromViews();
    }

    private void SubscribeToViews()
    {
        if (territoryInfoView != null)
        {
            territoryInfoView.PrimaryActionClicked += HandlePrimaryActionClicked;
            territoryInfoView.CloseClicked += HandleCloseClicked;
        }

        if (territoryTurnView != null)
        {
            territoryTurnView.NextDayClicked += HandleNextDayClicked;
        }

        if (territoryEventPresenter != null)
        {
            territoryEventPresenter.OptionSelected += HandleEventOptionSelected;
            territoryEventPresenter.Closed += HandleEventDialogClosed;
        }
    }

    private void UnsubscribeFromViews()
    {
        if (territoryInfoView != null)
        {
            territoryInfoView.PrimaryActionClicked -= HandlePrimaryActionClicked;
            territoryInfoView.CloseClicked -= HandleCloseClicked;
        }

        if (territoryTurnView != null)
        {
            territoryTurnView.NextDayClicked -= HandleNextDayClicked;
        }

        if (territoryEventPresenter != null)
        {
            territoryEventPresenter.OptionSelected -= HandleEventOptionSelected;
            territoryEventPresenter.Closed -= HandleEventDialogClosed;
        }
    }

    private void SubscribeToData()
    {
        if (territoryInfoPanelBridge != null)
        {
            territoryInfoPanelBridge.ViewModelChanged += HandleInfoViewModelChanged;
            territoryInfoPanelBridge.HideRequested += HandleInfoHideRequested;
        }

        if (territoryTurnService != null)
        {
            territoryTurnService.TurnStateChanged += HandleTurnStateChanged;
        }
    }

    private void UnsubscribeFromData()
    {
        if (territoryInfoPanelBridge != null)
        {
            territoryInfoPanelBridge.ViewModelChanged -= HandleInfoViewModelChanged;
            territoryInfoPanelBridge.HideRequested -= HandleInfoHideRequested;
        }

        if (territoryTurnService != null)
        {
            territoryTurnService.TurnStateChanged -= HandleTurnStateChanged;
        }
    }

    private void EnsureGlobalServices()
    {
        if (GameManager.I != null)
        {
            campaignRunService ??= GameManager.I.CampaignRunService;
            stageFlowService ??= GameManager.I.StageFlowService;

            if (GameManager.I.PlayerProfileService != null)
            {
                playerProfileService = GameManager.I.PlayerProfileService;
            }
        }

        playerProfileService ??= FindFirstObjectByType<PlayerProfileService>(FindObjectsInactive.Include);
    }

    // 로비 진입 시 기존 런이 있으면 복원하고, 없으면 새 런을 시작한다.
    private void InitializeTerritoryRun()
    {
        if (territoryMapController == null)
        {
            return;
        }

        if (campaignRunService != null
            && campaignRunService.TryGetRunState(out TerritoryRunState runState)
            && runState != null)
        {
            territoryMapController.LoadRunState(runState);
            return;
        }

        territoryMapController.BuildMap();
        SyncRunState();
    }

    // 스테이지에서 돌아온 결과가 있으면 바로 영토 상태에 반영한다.
    private void TryApplyPendingStageResult()
    {
        if (campaignRunService == null || territoryMapController == null)
        {
            return;
        }

        if (!campaignRunService.TryGetPendingStageEntry(out StageEntryContext stageEntryContext))
        {
            return;
        }

        if (!campaignRunService.ConsumePendingStageResult(out StageResult stageResult))
        {
            return;
        }

        if (!IsCurrentRun(stageEntryContext.RunId) || !IsCurrentRun(stageResult.RunId))
        {
            campaignRunService.ClearPendingStageEntry();
            return;
        }

        TerritoryStageRequest stageRequest = new TerritoryStageRequest(
            stageEntryContext.TileId,
            stageEntryContext.Day,
            stageEntryContext.StageType,
            stageEntryContext.PrimaryActionType,
            stageEntryContext.TileOwnerType,
            stageEntryContext.TileContentType);

        territoryMapController.ApplyStageResult(stageRequest, new TerritoryStageResult(stageResult.WasSuccess));
        TerritoryStageRewardApplicationResult rewardResult = _stageRewardApplier.ApplyRewards(
            stageEntryContext,
            stageResult,
            playerProfileService);

        SyncRunState();
        campaignRunService.ClearPendingStageEntry();

        StartCoroutine(PlayPostStageReturnRoutine(stageEntryContext, stageResult, rewardResult));
    }

    private TerritoryRunState SyncRunState()
    {
        if (territoryMapController == null || campaignRunService == null)
        {
            return null;
        }

        string runId = GetCurrentRunId();
        TerritoryRunState runState = territoryMapController.ExportRunState(runId);
        if (runState == null)
        {
            return null;
        }

        if (campaignRunService.IsRunActive)
        {
            campaignRunService.UpdateRunState(runState);
        }
        else
        {
            campaignRunService.StartNewRun(runState);
        }

        return runState;
    }

    private string GetCurrentRunId()
    {
        if (campaignRunService != null
            && campaignRunService.TryGetRunState(out TerritoryRunState currentRunState)
            && currentRunState != null
            && !string.IsNullOrWhiteSpace(currentRunState.RunId))
        {
            return currentRunState.RunId;
        }

        return Guid.NewGuid().ToString("N");
    }

    private bool IsCurrentRun(string runId)
    {
        return campaignRunService != null
            && campaignRunService.TryGetRunState(out TerritoryRunState currentRunState)
            && currentRunState != null
            && currentRunState.RunId == runId;
    }

    private void RefreshVisibleState()
    {
        if (territoryInfoPanelBridge != null && territoryInfoPanelBridge.CurrentViewModel != null)
        {
            territoryInfoView?.Show(territoryInfoPanelBridge.CurrentViewModel);
        }
        else
        {
            territoryInfoView?.Hide();
        }

        if (territoryTurnService != null && territoryTurnService.IsInitialized)
        {
            territoryTurnView?.Refresh(territoryTurnService.CurrentState);
        }
    }

    private void HandleInfoViewModelChanged(TerritoryTileInfoViewModel viewModel)
    {
        territoryInfoView?.Show(viewModel);
    }

    private void HandleInfoHideRequested()
    {
        territoryInfoView?.Hide();
    }

    private void HandleTurnStateChanged(TerritoryTurnState turnState)
    {
        territoryTurnView?.Refresh(turnState);
    }

    private void HandlePrimaryActionClicked()
    {
        if (territoryMapController == null)
        {
            return;
        }

        if (!territoryMapController.TryCreateSelectedTilePrimaryAction(out TerritoryPrimaryActionRequest actionRequest))
        {
            return;
        }

        if (actionRequest.IsEventAction)
        {
            ShowEventDialog(actionRequest);
            return;
        }

        TryEnterStage(actionRequest);
    }

    private void ShowEventDialog(TerritoryPrimaryActionRequest actionRequest)
    {
        territoryEventPresenter?.TryShow(actionRequest);
    }

    private void HandleEventOptionSelected(int optionIndex)
    {
        TerritoryEventSelectionSession currentSession = territoryEventPresenter != null
            ? territoryEventPresenter.CurrentSession
            : null;

        if (currentSession == null || territoryMapController == null)
        {
            return;
        }

        if (!territoryMapController.TryConsumePrimaryActionAttempt())
        {
            return;
        }

        TerritoryRunState runState = SyncRunState();
        EnsureGlobalServices();

        TerritoryEventActionResult actionResult = _eventActionService.ExecuteOption(
            currentSession,
            optionIndex,
            territoryMapController.MapService,
            playerProfileService,
            runState,
            stageSceneSettings,
            _stageEntryContextFactory);

        territoryEventPresenter?.Hide();

        if (actionResult == null)
        {
            return;
        }

        territoryMapController.RefreshDerivedState();

        if (actionResult.StartsBattle)
        {
            if (campaignRunService != null && actionResult.StageEntryContext != null)
            {
                campaignRunService.SetPendingStageEntry(actionResult.StageEntryContext);
                stageFlowService?.LoadStageScene(actionResult.StageEntryContext);
            }

            StageEntryRequested?.Invoke(actionResult.StageRequest);
            return;
        }

        SyncRunState();
    }

    private void HandleEventDialogClosed()
    {
    }

    private void TryEnterStage(TerritoryPrimaryActionRequest actionRequest)
    {
        if (territoryMapController == null
            || !territoryMapController.TryCreateStageRequest(actionRequest, out TerritoryStageRequest stageRequest)
            || !territoryMapController.TryConsumePrimaryActionAttempt())
        {
            return;
        }

        TerritoryRunState runState = SyncRunState();
        EnsureGlobalServices();

        if (campaignRunService != null
            && territoryMapController.TryGetSelectedTileState(out TerritoryTileState selectedTileState)
            && runState != null)
        {
            StageEntryContext stageEntryContext = _stageEntryContextFactory.Create(
                stageRequest,
                selectedTileState,
                runState,
                stageSceneSettings);

            campaignRunService.SetPendingStageEntry(stageEntryContext);
            stageFlowService?.LoadStageScene(stageEntryContext);
        }

        StageEntryRequested?.Invoke(stageRequest);
    }

    private void HandleNextDayClicked()
    {
        if (_isAdvancingDay || territoryMapController == null)
        {
            return;
        }

        ClearSelectedTerritory();

        if (!territoryMapController.TryAdvanceDay(playerProfileService, out TerritoryDayAdvanceResult dayAdvanceResult))
        {
            return;
        }

        StartCoroutine(PlayDayAdvanceRoutine(dayAdvanceResult));
    }

    private IEnumerator PlayDayAdvanceRoutine(TerritoryDayAdvanceResult dayAdvanceResult)
    {
        _isAdvancingDay = true;
        ShowInputBlocker();

        if (territoryDayAdvancePresentationController != null && dayAdvanceResult != null && dayAdvanceResult.HasPresentationWork)
        {
            yield return territoryDayAdvancePresentationController.Play(dayAdvanceResult);
        }
        else
        {
            territoryTurnFxController?.Play(dayAdvanceResult);
        }

        SyncRunState();
        DayAdvanceCompleted?.Invoke(dayAdvanceResult);
        RefreshVisibleState();

        CampaignOutcomeResult outcomeResult = _campaignVictoryResolver.EvaluateAfterDayAdvance(
            territoryMapController != null ? territoryMapController.MapService : null,
            territoryTurnService);

        if (outcomeResult.IsFinished)
        {
            FinalDayTerritoryCountResult countResult = _finalDayTerritoryCollector.Collect(territoryMapController);
            if (finalDayTerritoryCountPresentationController != null)
            {
                yield return finalDayTerritoryCountPresentationController.Play(countResult, outcomeResult);
                HideInputBlocker();
                campaignResultPanelController?.Show(outcomeResult);
            }
            else
            {
                HideInputBlocker();
                campaignResultPanelController?.Show(outcomeResult);
            }
        }
        else
        {
            HideInputBlocker();
        }

        _isAdvancingDay = false;
    }

    private IEnumerator PlayPostStageReturnRoutine(
        StageEntryContext stageEntryContext,
        StageResult stageResult,
        TerritoryStageRewardApplicationResult rewardResult)
    {
        ShowInputBlocker();

        if (stageRewardPresentationController != null)
        {
            yield return stageRewardPresentationController.Play(rewardResult);
        }

        CampaignOutcomeResult outcomeResult = _campaignVictoryResolver.EvaluateAfterStage(
            stageEntryContext,
            stageResult,
            territoryMapController != null ? territoryMapController.MapService : null,
            territoryTurnService);

        if (outcomeResult.IsFinished)
        {
            HideInputBlocker();
            campaignResultPanelController?.Show(outcomeResult);
        }
        else
        {
            HideInputBlocker();
        }
    }

    private void HandleCloseClicked()
    {
        ClearSelectedTerritory();
    }

    private void ClearSelectedTerritory()
    {
        territoryMapController?.ClearSelection();
    }

    private void ShowInputBlocker()
    {
        inputBlockerView ??= FindFirstObjectByType<UI_InputBlockerView>(FindObjectsInactive.Include);
        inputBlockerView?.Show();
    }

    private void HideInputBlocker()
    {
        inputBlockerView?.Hide();
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return gameObject.AddComponent<T>();
    }
}
