using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TerritoryMapService))]
[RequireComponent(typeof(TerritoryTurnService))]
[RequireComponent(typeof(TerritoryActionRuleService))]
[RequireComponent(typeof(TerritoryPrimaryActionService))]
[RequireComponent(typeof(TerritoryEnemyExpansionService))]
[RequireComponent(typeof(TerritoryStageEntryService))]
[RequireComponent(typeof(TerritoryStageResultApplier))]
[RequireComponent(typeof(TerritoryMapPointerInput))]
[RequireComponent(typeof(TerritoryMapCameraController))]
[RequireComponent(typeof(TerritoryInfoPanelBridge))]
public sealed class TerritoryMapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapGenerator mapGenerator;
    [SerializeField] private TerritoryMapService mapService;
    [SerializeField] private TerritoryTurnService turnService;
    [SerializeField] private TerritoryActionRuleService actionRuleService;
    [SerializeField] private TerritoryPrimaryActionService primaryActionService;
    [SerializeField] private TerritoryEnemyExpansionService enemyExpansionService;
    [SerializeField] private TerritoryStageEntryService stageEntryService;
    [SerializeField] private TerritoryStageResultApplier stageResultApplier;
    [SerializeField] private TerritoryMapPointerInput mapPointerInput;
    [SerializeField] private TerritoryMapCameraController mapCameraController;
    [SerializeField] private TerritoryInfoPanelBridge infoPanelBridge;

    [Header("Build Settings")]
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] [Min(0)] private int mapRadius = 3;
    [SerializeField] private Vector2 tileWorldSize = new Vector2(1.6f, 1.4f);
    [SerializeField] private Vector2 tileSpacing = new Vector2(0.08f, 0.08f);
    [SerializeField] private Vector2 centerOffset = Vector2.zero;

    [Header("Game Settings")]
    [SerializeField] private TerritoryGameSettings gameSettings = new TerritoryGameSettings();

    private readonly TerritoryMapSetupService _mapSetupService = new TerritoryMapSetupService();
    private readonly TerritoryRunStateFactory _runStateFactory = new TerritoryRunStateFactory();
    private readonly TerritoryRunStateApplier _runStateApplier = new TerritoryRunStateApplier();
    private readonly TerritoryDailyIncomeService _dailyIncomeService = new TerritoryDailyIncomeService();

    private TerritoryMapLayoutData _layoutData;

    public TerritoryMapLayoutData LayoutData => _layoutData;
    public TerritoryMapService MapService => mapService;
    public TerritoryTurnService TurnService => turnService;
    public TerritoryInfoPanelBridge InfoPanelBridge => infoPanelBridge;
    public TerritoryMapPointerInput MapPointerInput => mapPointerInput;

    public event Action<int> TileClicked;
    public event Action<int> TileHoverEntered;
    public event Action<int> TileHoverExited;

    private void Awake()
    {
        mapService ??= GetOrAddComponent<TerritoryMapService>();
        turnService ??= GetOrAddComponent<TerritoryTurnService>();
        actionRuleService ??= GetOrAddComponent<TerritoryActionRuleService>();
        primaryActionService ??= GetOrAddComponent<TerritoryPrimaryActionService>();
        enemyExpansionService ??= GetOrAddComponent<TerritoryEnemyExpansionService>();
        stageEntryService ??= GetOrAddComponent<TerritoryStageEntryService>();
        stageResultApplier ??= GetOrAddComponent<TerritoryStageResultApplier>();
        mapPointerInput ??= GetOrAddComponent<TerritoryMapPointerInput>();
        mapCameraController ??= GetOrAddComponent<TerritoryMapCameraController>();
        infoPanelBridge ??= GetOrAddComponent<TerritoryInfoPanelBridge>();
        mapGenerator ??= GetComponentInChildren<TerritoryMapGenerator>(true);
    }

    private void OnEnable()
    {
        SubscribeToService();
        SubscribeToPointerInput();
        SubscribeToViews();
    }

    private void Start()
    {
        if (!buildOnStart)
        {
            return;
        }

        BuildMap();
    }

    private void OnDisable()
    {
        UnsubscribeFromViews();
        UnsubscribeFromPointerInput();
        UnsubscribeFromService();
    }

    public void SetBuildOnStart(bool shouldBuildOnStart)
    {
        buildOnStart = shouldBuildOnStart;
    }

    public void BuildMap()
    {
        if (!TryPrepareRuntime())
        {
            return;
        }

        if (!_mapSetupService.TrySetupNewGame(mapService, gameSettings))
        {
            Debug.LogWarning("TerritoryMapController could not initialize the territory map with the current game settings.");
        }

        RefreshRuleDrivenState();
        RefreshSelectionPresentation();
    }

    public bool LoadRunState(TerritoryRunState runState)
    {
        if (runState == null)
        {
            return false;
        }

        if (!TryPrepareRuntime())
        {
            return false;
        }

        _runStateApplier.Apply(runState, mapService, turnService);
        RefreshRuleDrivenState();
        RefreshSelectionPresentation();
        return true;
    }

    public TerritoryRunState ExportRunState(string runId)
    {
        if (string.IsNullOrWhiteSpace(runId) || mapService == null || turnService == null)
        {
            return null;
        }

        return _runStateFactory.Create(runId, mapService, turnService);
    }

    public bool TryGetSelectedTileState(out TerritoryTileState tileState)
    {
        tileState = null;
        return mapService != null && mapService.TryGetSelectedTileState(out tileState);
    }

    public TerritorySelectionResult ToggleSelection(int tileId)
    {
        return mapService != null
            ? mapService.ToggleSelection(tileId)
            : TerritorySelectionResult.None;
    }

    public void ClearSelection()
    {
        mapService?.ClearSelection();
    }

    public bool TryRequestSelectedTileStage(out TerritoryStageRequest stageRequest)
    {
        stageRequest = default;

        if (!TryCreateSelectedTilePrimaryAction(out TerritoryPrimaryActionRequest actionRequest)
            || !TryCreateStageRequest(actionRequest, out stageRequest)
            || !TryConsumePrimaryActionAttempt())
        {
            return false;
        }

        return true;
    }

    public bool TryCreateSelectedTilePrimaryAction(out TerritoryPrimaryActionRequest actionRequest)
    {
        actionRequest = default;

        if (mapService == null
            || primaryActionService == null
            || !mapService.SelectedTileId.HasValue)
        {
            return false;
        }

        return primaryActionService.TryCreateActionRequest(mapService.SelectedTileId.Value, out actionRequest);
    }

    public bool TryCreateStageRequest(
        TerritoryPrimaryActionRequest actionRequest,
        out TerritoryStageRequest stageRequest)
    {
        stageRequest = default;
        return stageEntryService != null
            && stageEntryService.TryCreateStageRequest(actionRequest, out stageRequest);
    }

    public bool TryConsumePrimaryActionAttempt()
    {
        if (primaryActionService == null || !primaryActionService.TryConsumeAttempt())
        {
            return false;
        }

        RefreshRuleDrivenState();
        RefreshSelectedInfoPanel();
        return true;
    }

    public bool ApplyStageResult(TerritoryStageRequest stageRequest, TerritoryStageResult stageResult)
    {
        if (stageResultApplier == null || !stageResultApplier.ApplyResult(stageRequest, stageResult))
        {
            return false;
        }

        RefreshRuleDrivenState();
        RefreshSelectedInfoPanel();
        return true;
    }

    public bool TryAdvanceDay()
    {
        return TryAdvanceDay(out _);
    }

    public bool TryAdvanceDay(PlayerProfileService playerProfileService, out TerritoryDayAdvanceResult dayAdvanceResult)
    {
        dayAdvanceResult = TerritoryDayAdvanceResult.Failed(
            turnService != null ? turnService.CurrentDay : 0,
            turnService != null ? turnService.RemainingAttempts : 0);

        if (turnService == null || enemyExpansionService == null || mapService == null)
        {
            return false;
        }

        int previousDay = turnService.CurrentDay;
        if (!turnService.TryAdvanceDay(out int newDay))
        {
            return false;
        }

        TerritoryDailyIncomeResult incomeResult = _dailyIncomeService.ApplyDailyIncome(
            mapService,
            playerProfileService,
            gameSettings);

        TerritoryTileChangeSet tileChangeSet = new TerritoryTileChangeSet();
        IReadOnlyList<TerritoryEnemyExpansionEvent> enemyExpansionEvents =
            enemyExpansionService.ProcessNewDay(newDay, tileChangeSet);

        mapService.AdvanceDayForRuntimeState();
        RefreshRuleDrivenState();
        RefreshSelectedInfoPanel();

        dayAdvanceResult = new TerritoryDayAdvanceResult(
            true,
            previousDay,
            newDay,
            turnService.RemainingAttempts,
            tileChangeSet,
            incomeResult,
            enemyExpansionEvents);

        return true;
    }

    public bool TryAdvanceDay(out TerritoryDayAdvanceResult dayAdvanceResult)
    {
        return TryAdvanceDay(null, out dayAdvanceResult);
    }

    public void RefreshDerivedState()
    {
        RefreshRuleDrivenState();
        RefreshSelectedInfoPanel();
    }

    public bool TryGetTileView(int tileId, out TerritoryTileView tileView)
    {
        tileView = null;
        return mapGenerator != null && mapGenerator.TryGetView(tileId, out tileView);
    }

    private bool TryPrepareRuntime()
    {
        if (mapGenerator == null)
        {
            Debug.LogWarning("TerritoryMapController requires a TerritoryMapGenerator reference.");
            return false;
        }

        if (mapService == null)
        {
            Debug.LogWarning("TerritoryMapController requires a TerritoryMapService reference.");
            return false;
        }

        UnsubscribeFromViews();

        TerritoryHexGrid hexGrid = new TerritoryHexGrid(mapRadius);
        TerritoryWorldPositionCalculator positionCalculator = new TerritoryWorldPositionCalculator(tileWorldSize, tileSpacing, centerOffset);
        _layoutData = hexGrid.BuildLayout(positionCalculator);

        mapService.Initialize(_layoutData);
        turnService.Initialize(gameSettings);
        actionRuleService.Initialize(mapService, turnService);
        primaryActionService.Initialize(mapService, turnService, actionRuleService);
        enemyExpansionService.Initialize(mapService, gameSettings);
        stageEntryService.Initialize(mapService, turnService, actionRuleService);
        stageResultApplier.Initialize(mapService);

        if (!mapGenerator.Build(_layoutData))
        {
            return false;
        }

        SubscribeToViews();
        return true;
    }

    private void SubscribeToService()
    {
        if (mapService != null)
        {
            mapService.TileStateChanged += HandleTileStateChanged;
            mapService.SelectionChanged += HandleSelectionChanged;
        }

        if (turnService != null)
        {
            turnService.TurnStateChanged += HandleTurnStateChanged;
        }
    }

    private void UnsubscribeFromService()
    {
        if (mapService != null)
        {
            mapService.TileStateChanged -= HandleTileStateChanged;
            mapService.SelectionChanged -= HandleSelectionChanged;
        }

        if (turnService != null)
        {
            turnService.TurnStateChanged -= HandleTurnStateChanged;
        }
    }

    private void SubscribeToPointerInput()
    {
        if (mapPointerInput == null)
        {
            return;
        }

        mapPointerInput.BackgroundClicked += HandleBackgroundClicked;
    }

    private void UnsubscribeFromPointerInput()
    {
        if (mapPointerInput == null)
        {
            return;
        }

        mapPointerInput.BackgroundClicked -= HandleBackgroundClicked;
    }

    private void SubscribeToViews()
    {
        if (mapGenerator == null)
        {
            return;
        }

        for (int index = 0; index < mapGenerator.GeneratedViews.Count; index++)
        {
            TerritoryTileView tileView = mapGenerator.GeneratedViews[index];
            if (tileView == null || tileView.Input == null)
            {
                continue;
            }

            tileView.Input.Clicked += HandleTileClicked;
            tileView.Input.HoverEntered += HandleTileHoverEntered;
            tileView.Input.HoverExited += HandleTileHoverExited;
        }
    }

    private void UnsubscribeFromViews()
    {
        if (mapGenerator == null)
        {
            return;
        }

        for (int index = 0; index < mapGenerator.GeneratedViews.Count; index++)
        {
            TerritoryTileView tileView = mapGenerator.GeneratedViews[index];
            if (tileView == null || tileView.Input == null)
            {
                continue;
            }

            tileView.Input.Clicked -= HandleTileClicked;
            tileView.Input.HoverEntered -= HandleTileHoverEntered;
            tileView.Input.HoverExited -= HandleTileHoverExited;
        }
    }

    private void HandleTileClicked(int tileId)
    {
        if (mapService == null)
        {
            return;
        }

        mapService.ToggleSelection(tileId);
        TileClicked?.Invoke(tileId);
    }

    private void HandleTileHoverEntered(int tileId)
    {
        TileHoverEntered?.Invoke(tileId);
    }

    private void HandleTileHoverExited(int tileId)
    {
        TileHoverExited?.Invoke(tileId);
    }

    private void HandleBackgroundClicked()
    {
        if (mapService == null || !mapService.SelectedTileId.HasValue)
        {
            return;
        }

        mapService.ClearSelection();
    }

    private void HandleTileStateChanged(TerritoryTileState tileState)
    {
        if (tileState == null || mapGenerator == null)
        {
            return;
        }

        if (mapGenerator.TryGetView(tileState.TileId, out TerritoryTileView tileView))
        {
            tileView.Render(tileState);
        }
    }

    private void HandleSelectionChanged(TerritorySelectionResult selectionResult)
    {
        if (!selectionResult.HasChanged)
        {
            return;
        }

        RefreshSelectionPresentation();
    }

    private void HandleTurnStateChanged(TerritoryTurnState turnState)
    {
        RefreshDerivedState();
    }

    private void RefreshAllViews()
    {
        if (mapService == null || mapGenerator == null)
        {
            return;
        }

        for (int index = 0; index < mapService.TileStates.Count; index++)
        {
            TerritoryTileState tileState = mapService.TileStates[index];
            if (mapGenerator.TryGetView(tileState.TileId, out TerritoryTileView tileView))
            {
                tileView.Render(tileState);
            }
        }
    }

    private void RefreshRuleDrivenState()
    {
        if (actionRuleService == null)
        {
            return;
        }

        actionRuleService.RefreshAvailableActions();
        RefreshAllViews();
    }

    private void RefreshSelectionPresentation()
    {
        if (mapService == null || mapGenerator == null)
        {
            return;
        }

        if (mapService.SelectedTileId.HasValue
            && mapGenerator.TryGetView(mapService.SelectedTileId.Value, out TerritoryTileView selectedTileView))
        {
            mapCameraController?.FocusOnWorldPosition(selectedTileView.transform.position);
            RefreshSelectedInfoPanel();
            return;
        }

        mapCameraController?.ResetFocus();
        infoPanelBridge?.Hide();
    }

    private void RefreshSelectedInfoPanel()
    {
        if (infoPanelBridge == null || mapService == null || turnService == null)
        {
            return;
        }

        if (!mapService.TryGetSelectedTileState(out TerritoryTileState tileState))
        {
            infoPanelBridge.Hide();
            return;
        }

        TerritoryTileInfoViewModel viewModel = TerritoryTileInfoFactory.Create(tileState, turnService.CurrentState);
        infoPanelBridge.Show(viewModel);
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
