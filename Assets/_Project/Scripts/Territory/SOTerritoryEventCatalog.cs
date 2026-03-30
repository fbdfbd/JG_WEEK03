using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOTerritoryEventCatalog", menuName = "Scriptable Objects/Territory Event Catalog")]
public sealed class SOTerritoryEventCatalog : ScriptableObject
{
    [SerializeField] private List<SOTerritoryEventDefinition> eventDefinitions = new List<SOTerritoryEventDefinition>();

    public IReadOnlyList<SOTerritoryEventDefinition> EventDefinitions => eventDefinitions;

    public bool HasDefinitions => eventDefinitions != null && eventDefinitions.Count > 0;

    public SOTerritoryEventDefinition ResolveDefinition(int tileId, int day)
    {
        if (!HasDefinitions)
        {
            return null;
        }

        int eventIndex = Mathf.Abs(tileId + day) % eventDefinitions.Count;
        return eventDefinitions[eventIndex];
    }
}
