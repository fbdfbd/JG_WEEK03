using System;
using UnityEngine;

public readonly struct LobbyConsumableTargetingState
{
    public static LobbyConsumableTargetingState None => new LobbyConsumableTargetingState(false, string.Empty);

    public bool IsActive { get; }
    public string ConsumableId { get; }

    public LobbyConsumableTargetingState(bool isActive, string consumableId)
    {
        IsActive = isActive;
        ConsumableId = consumableId ?? string.Empty;
    }
}

public readonly struct LobbyConsumableUseFeedback
{
    public string ConsumableId { get; }
    public int? TargetTileId { get; }
    public LobbyConsumableUseResult Result { get; }

    public bool IsSuccess => Result == LobbyConsumableUseResult.Success;

    public LobbyConsumableUseFeedback(string consumableId, int? targetTileId, LobbyConsumableUseResult result)
    {
        ConsumableId = consumableId ?? string.Empty;
        TargetTileId = targetTileId;
        Result = result;
    }
}

[DisallowMultipleComponent]
public sealed class LobbyConsumableTargetingCoordinator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapService _territoryMapService;
    [SerializeField] private LobbyConsumableUseService _consumableUseService;

    private string _pendingConsumableId;

    public bool IsTargeting => !string.IsNullOrWhiteSpace(_pendingConsumableId);
    public string PendingConsumableId => _pendingConsumableId;

    public event Action<LobbyConsumableTargetingState> TargetingStateChanged;
    public event Action<LobbyConsumableUseFeedback> ConsumableUseFinished;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (_territoryMapService != null)
        {
            _territoryMapService.SelectionChanged += HandleSelectionChanged;
        }
    }

    private void OnDisable()
    {
        if (_territoryMapService != null)
        {
            _territoryMapService.SelectionChanged -= HandleSelectionChanged;
        }
    }

    public void StartTargeting(string consumableId)
    {
        ResolveReferences();

        if (string.IsNullOrWhiteSpace(consumableId))
        {
            return;
        }

        _pendingConsumableId = consumableId;
        RaiseTargetingStateChanged();
    }

    public void CancelTargeting()
    {
        if (!IsTargeting)
        {
            return;
        }

        _pendingConsumableId = string.Empty;
        RaiseTargetingStateChanged();
    }

    private void ResolveReferences()
    {
        _territoryMapService ??= FindFirstObjectByType<TerritoryMapService>(FindObjectsInactive.Include);
        _consumableUseService ??= FindFirstObjectByType<LobbyConsumableUseService>(FindObjectsInactive.Include);
    }

    private void HandleSelectionChanged(TerritorySelectionResult selectionResult)
    {
        if (!IsTargeting || !_territoryMapService.SelectedTileId.HasValue || _consumableUseService == null)
        {
            return;
        }

        int targetTileId = _territoryMapService.SelectedTileId.Value;
        LobbyConsumableUseResult useResult = _consumableUseService.TryUseConsumable(_pendingConsumableId, targetTileId);

        LobbyConsumableUseFeedback feedback = new LobbyConsumableUseFeedback(
            _pendingConsumableId,
            targetTileId,
            useResult);

        if (feedback.IsSuccess)
        {
            _pendingConsumableId = string.Empty;
            RaiseTargetingStateChanged();
        }

        ConsumableUseFinished?.Invoke(feedback);
    }

    private void RaiseTargetingStateChanged()
    {
        TargetingStateChanged?.Invoke(new LobbyConsumableTargetingState(IsTargeting, _pendingConsumableId));
    }
}
