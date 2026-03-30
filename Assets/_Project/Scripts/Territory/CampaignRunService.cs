using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CampaignRunService : MonoBehaviour
{
    private TerritoryRunState _currentRunState;
    private StageEntryContext _pendingStageEntry;
    private StageResult _pendingStageResult;

    public bool IsRunActive => _currentRunState != null;
    public TerritoryRunState CurrentRunState => _currentRunState;
    public StageEntryContext PendingStageEntry => _pendingStageEntry;
    public StageResult PendingStageResult => _pendingStageResult;

    public event Action<TerritoryRunState> RunStarted;
    public event Action<TerritoryRunState> RunStateChanged;
    public event Action<StageEntryContext> PendingStageEntryChanged;
    public event Action<StageResult> PendingStageResultChanged;
    public event Action RunEnded;

    // 점령전 한 판을 새로 시작할 때 호출한다.
    public void StartNewRun(TerritoryRunState runState)
    {
        if (runState == null)
        {
            Debug.LogWarning("CampaignRunService requires a valid run state.");
            return;
        }

        runState.RebuildLookup();
        _currentRunState = runState;
        _pendingStageEntry = null;
        _pendingStageResult = null;

        RunStarted?.Invoke(_currentRunState);
        RunStateChanged?.Invoke(_currentRunState);
    }

    public void UpdateRunState(TerritoryRunState runState)
    {
        if (runState == null)
        {
            return;
        }

        runState.RebuildLookup();
        _currentRunState = runState;
        RunStateChanged?.Invoke(_currentRunState);
    }

    public bool TryGetRunState(out TerritoryRunState runState)
    {
        runState = _currentRunState;
        return runState != null;
    }

    public void SetPendingStageEntry(StageEntryContext stageEntryContext)
    {
        if (stageEntryContext == null || !stageEntryContext.IsValid)
        {
            Debug.LogWarning("CampaignRunService received an invalid StageEntryContext.");
            return;
        }

        _pendingStageEntry = stageEntryContext;
        PendingStageEntryChanged?.Invoke(_pendingStageEntry);
    }

    public bool TryGetPendingStageEntry(out StageEntryContext stageEntryContext)
    {
        stageEntryContext = _pendingStageEntry;
        return stageEntryContext != null;
    }

    public void ClearPendingStageEntry()
    {
        _pendingStageEntry = null;
        PendingStageEntryChanged?.Invoke(null);
    }

    public void SetPendingStageResult(StageResult stageResult)
    {
        if (stageResult == null)
        {
            return;
        }

        _pendingStageResult = stageResult;
        PendingStageResultChanged?.Invoke(_pendingStageResult);
    }

    public bool TryGetPendingStageResult(out StageResult stageResult)
    {
        stageResult = _pendingStageResult;
        return stageResult != null;
    }

    public bool ConsumePendingStageResult(out StageResult stageResult)
    {
        stageResult = _pendingStageResult;
        if (stageResult == null)
        {
            return false;
        }

        _pendingStageResult = null;
        PendingStageResultChanged?.Invoke(null);
        return true;
    }

    public void EndRun()
    {
        _currentRunState = null;
        _pendingStageEntry = null;
        _pendingStageResult = null;
        RunEnded?.Invoke();
    }
}
