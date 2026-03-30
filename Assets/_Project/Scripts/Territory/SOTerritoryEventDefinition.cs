using UnityEngine;

[CreateAssetMenu(fileName = "SOTerritoryEventDefinition", menuName = "Scriptable Objects/Territory Event Definition")]
public sealed class SOTerritoryEventDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string eventId = string.Empty;
    [SerializeField] private string title = string.Empty;

    [Header("Description")]
    [SerializeField] [TextArea(3, 6)] private string situationText = string.Empty;

    [Header("Options")]
    [SerializeField] private TerritoryEventOptionDefinition firstOption = new TerritoryEventOptionDefinition();
    [SerializeField] private TerritoryEventOptionDefinition secondOption = new TerritoryEventOptionDefinition();

    public string EventId => eventId;
    public string Title => title;
    public string SituationText => situationText;
    public TerritoryEventOptionDefinition FirstOption => firstOption;
    public TerritoryEventOptionDefinition SecondOption => secondOption;

    public TerritoryEventOptionDefinition GetOption(int optionIndex)
    {
        return optionIndex == 0
            ? firstOption
            : optionIndex == 1
                ? secondOption
                : null;
    }
}
