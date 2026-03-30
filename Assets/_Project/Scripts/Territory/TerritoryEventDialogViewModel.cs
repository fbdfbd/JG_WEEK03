public readonly struct TerritoryEventOptionViewModel
{
    public static TerritoryEventOptionViewModel Empty => new TerritoryEventOptionViewModel(string.Empty, string.Empty, false);

    public string Title { get; }
    public string Description { get; }
    public bool IsSelectable { get; }

    public TerritoryEventOptionViewModel(
        string title,
        string description,
        bool isSelectable)
    {
        Title = title ?? string.Empty;
        Description = description ?? string.Empty;
        IsSelectable = isSelectable;
    }
}

public sealed class TerritoryEventDialogViewModel
{
    public string Title { get; }
    public string SituationText { get; }
    public TerritoryEventOptionViewModel FirstOption { get; }
    public TerritoryEventOptionViewModel SecondOption { get; }

    public TerritoryEventDialogViewModel(
        string title,
        string situationText,
        TerritoryEventOptionViewModel firstOption,
        TerritoryEventOptionViewModel secondOption)
    {
        Title = title ?? string.Empty;
        SituationText = situationText ?? string.Empty;
        FirstOption = firstOption;
        SecondOption = secondOption;
    }
}
