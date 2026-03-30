using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOConsumableCatalog", menuName = "Scriptable Objects/Consumable Catalog")]
public sealed class SOConsumableCatalog : ScriptableObject
{
    [SerializeField] private List<SOConsumableData> _consumableItems = new List<SOConsumableData>();

    public IReadOnlyList<SOConsumableData> ConsumableItems => _consumableItems;

    public bool TryGetConsumableById(string consumableId, out SOConsumableData consumableData)
    {
        consumableData = null;

        if (string.IsNullOrWhiteSpace(consumableId))
        {
            return false;
        }

        for (int index = 0; index < _consumableItems.Count; index++)
        {
            SOConsumableData currentConsumable = _consumableItems[index];
            if (currentConsumable == null)
            {
                continue;
            }

            if (!string.Equals(currentConsumable.ConsumableId, consumableId, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            consumableData = currentConsumable;
            return true;
        }

        return false;
    }

    public SOConsumableData GetConsumableByIdOrNull(string consumableId)
    {
        TryGetConsumableById(consumableId, out SOConsumableData consumableData);
        return consumableData;
    }

    public void ReplaceAll(IReadOnlyList<SOConsumableData> consumableItems)
    {
        _consumableItems.Clear();

        if (consumableItems == null)
        {
            return;
        }

        for (int index = 0; index < consumableItems.Count; index++)
        {
            _consumableItems.Add(consumableItems[index]);
        }
    }
}
