public readonly struct TerritoryStageResult
{
    public bool IsSuccess { get; }

    public TerritoryStageResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }
}
