public sealed class TerritoryTurnState
{
    public int CurrentDay { get; private set; }
    public int MaxDay { get; private set; }
    public int RemainingAttempts { get; private set; }
    public int MaxAttemptsPerDay { get; private set; }

    public bool HasRemainingAttempts => RemainingAttempts > 0;
    public bool IsFinalDay => CurrentDay >= MaxDay;
    public bool CanAdvanceDay => RemainingAttempts <= 0 && !IsFinalDay;

    public void Initialize(int maxDay, int maxAttemptsPerDay)
    {
        MaxDay = maxDay;
        MaxAttemptsPerDay = maxAttemptsPerDay;
        CurrentDay = 1;
        RemainingAttempts = maxAttemptsPerDay;
    }

    public void ApplyRuntimeState(int currentDay, int maxDay, int remainingAttempts, int maxAttemptsPerDay)
    {
        MaxDay = maxDay;
        MaxAttemptsPerDay = maxAttemptsPerDay;
        CurrentDay = currentDay;
        RemainingAttempts = UnityEngine.Mathf.Clamp(remainingAttempts, 0, MaxAttemptsPerDay);
    }

    public bool TryConsumeAttempt(int amount)
    {
        if (amount <= 0 || RemainingAttempts < amount)
        {
            return false;
        }

        RemainingAttempts -= amount;
        return true;
    }

    public void RestoreAttempts(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        RemainingAttempts = UnityEngine.Mathf.Clamp(RemainingAttempts + amount, 0, MaxAttemptsPerDay);
    }

    public bool TryAdvanceDay()
    {
        if (!CanAdvanceDay)
        {
            return false;
        }

        CurrentDay++;
        RemainingAttempts = MaxAttemptsPerDay;
        return true;
    }
}
