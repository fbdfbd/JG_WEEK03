public sealed class TerritoryTileInfoViewModel
{
    public int TileId { get; }
    public int CurrentDay { get; }
    public int RemainingAttempts { get; }
    public string Title { get; }
    public string OwnerText { get; }
    public string ContentText { get; }
    public string StatusText { get; }
    public string HintText { get; }
    public string PrimaryActionLabel { get; }
    public bool CanRequestStage { get; }

    public TerritoryTileInfoViewModel(
        int tileId,
        int currentDay,
        int remainingAttempts,
        string title,
        string ownerText,
        string contentText,
        string statusText,
        string hintText,
        string primaryActionLabel,
        bool canRequestStage)
    {
        TileId = tileId;
        CurrentDay = currentDay;
        RemainingAttempts = remainingAttempts;
        Title = title;
        OwnerText = ownerText;
        ContentText = contentText;
        StatusText = statusText;
        HintText = hintText;
        PrimaryActionLabel = primaryActionLabel;
        CanRequestStage = canRequestStage;
    }
}
