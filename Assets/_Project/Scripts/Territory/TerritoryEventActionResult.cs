public sealed class TerritoryEventActionResult
{
    public TerritoryStageRequest StageRequest { get; }
    public StageEntryContext StageEntryContext { get; }

    public bool StartsBattle => StageEntryContext != null;

    public TerritoryEventActionResult(
        TerritoryStageRequest stageRequest = default,
        StageEntryContext stageEntryContext = null)
    {
        StageRequest = stageRequest;
        StageEntryContext = stageEntryContext;
    }
}
