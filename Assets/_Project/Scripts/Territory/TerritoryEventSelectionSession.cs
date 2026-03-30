public sealed class TerritoryEventSelectionSession
{
    public TerritoryPrimaryActionRequest ActionRequest { get; }
    public SOTerritoryEventDefinition EventDefinition { get; }

    public TerritoryEventSelectionSession(
        TerritoryPrimaryActionRequest actionRequest,
        SOTerritoryEventDefinition eventDefinition)
    {
        ActionRequest = actionRequest;
        EventDefinition = eventDefinition;
    }
}
