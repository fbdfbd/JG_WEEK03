using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryTurnService : MonoBehaviour
{
    private TerritoryGameSettings _settings;
    private readonly TerritoryTurnState _turnState = new TerritoryTurnState();

    public bool IsInitialized { get; private set; }
    public int CurrentDay => _turnState.CurrentDay;
    public int RemainingAttempts => _turnState.RemainingAttempts;
    public int MaxDay => _turnState.MaxDay;
    public bool HasRemainingAttempts => _turnState.HasRemainingAttempts;
    public TerritoryTurnState CurrentState => _turnState;

    public event Action<TerritoryTurnState> TurnStateChanged;
    public event Action<int> DayAdvanced;

    public void Initialize(TerritoryGameSettings settings)
    {
        _settings = settings;
        _turnState.Initialize(settings.MaxDay, settings.ChallengeAttemptsPerDay);
        IsInitialized = true;
        RaiseTurnStateChanged();
    }

    // 저장된 런 상태를 다시 읽을 때 날짜와 남은 도전 횟수를 그대로 복원한다.
    public void ApplyRuntimeState(int currentDay, int maxDay, int remainingAttempts, int maxAttemptsPerDay)
    {
        _turnState.ApplyRuntimeState(currentDay, maxDay, remainingAttempts, maxAttemptsPerDay);
        IsInitialized = true;
        RaiseTurnStateChanged();
    }

    // 모든 도전은 같은 자원을 소모하므로 한 곳에서 처리한다.
    public bool TryConsumeAttempt(int amount = 1)
    {
        if (!IsInitialized || !_turnState.TryConsumeAttempt(amount))
        {
            return false;
        }

        RaiseTurnStateChanged();
        return true;
    }

    // 아이템처럼 횟수를 회복하는 규칙도 쉽게 붙일 수 있도록 별도 메서드로 둔다.
    public void RestoreAttempts(int amount)
    {
        if (!IsInitialized)
        {
            return;
        }

        _turnState.RestoreAttempts(amount);
        RaiseTurnStateChanged();
    }

    public bool TryAdvanceDay(out int newDay)
    {
        newDay = _turnState.CurrentDay;

        if (!IsInitialized || !_turnState.TryAdvanceDay())
        {
            return false;
        }

        newDay = _turnState.CurrentDay;
        DayAdvanced?.Invoke(newDay);
        RaiseTurnStateChanged();
        return true;
    }

    private void RaiseTurnStateChanged()
    {
        TurnStateChanged?.Invoke(_turnState);
    }
}
