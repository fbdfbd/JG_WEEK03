using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOEquipmentCatalog", menuName = "Scriptable Objects/Equipment Catalog")]
public sealed class SOEquipmentCatalog : ScriptableObject
{
    [SerializeField] private List<SOEquipmentData> _equipmentItems = new List<SOEquipmentData>();

    public IReadOnlyList<SOEquipmentData> EquipmentItems => _equipmentItems;

    public bool TryGetEquipmentById(string equipmentId, out SOEquipmentData equipmentData)
    {
        equipmentData = null;

        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return false;
        }

        for (int index = 0; index < _equipmentItems.Count; index++)
        {
            SOEquipmentData currentEquipment = _equipmentItems[index];
            if (currentEquipment == null)
            {
                continue;
            }

            if (string.Equals(currentEquipment.EquipmentId, equipmentId, System.StringComparison.OrdinalIgnoreCase))
            {
                equipmentData = currentEquipment;
                return true;
            }
        }

        return false;
    }

    public SOEquipmentData GetEquipmentByIdOrNull(string equipmentId)
    {
        TryGetEquipmentById(equipmentId, out SOEquipmentData equipmentData);
        return equipmentData;
    }

    public void ReplaceAll(IReadOnlyList<SOEquipmentData> equipmentItems)
    {
        _equipmentItems.Clear();

        if (equipmentItems == null)
        {
            return;
        }

        for (int index = 0; index < equipmentItems.Count; index++)
        {
            _equipmentItems.Add(equipmentItems[index]);
        }
    }
}
