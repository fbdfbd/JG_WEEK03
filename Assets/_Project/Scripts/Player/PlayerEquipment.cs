using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [SerializeField] private int _maxCost = 3;
    [SerializeField] private List<SOEquipmentData> _equippedItems = new List<SOEquipmentData>();

    public int MaxCost => _maxCost;
    public IReadOnlyList<SOEquipmentData> EquippedItems => _equippedItems;

    public event Action EquippedItemsChanged;

    public void SetCost(int cost)
    {
        _maxCost = cost;
        EquippedItemsChanged?.Invoke();
    }

    public int CurrentCost
    {
        get
        {
            int totalCost = 0;

            for (int i = 0; i < _equippedItems.Count; i++)
            {
                totalCost += _equippedItems[i].Cost;
            }

            return totalCost;
        }
    }

    public bool IsEquipped(SOEquipmentData data)
    {
        return _equippedItems.Contains(data);
    }

    public void AddEquipment(SOEquipmentData data)
    {
        if (data == null || _equippedItems.Contains(data))
        {
            return;
        }

        _equippedItems.Add(data);
        EquippedItemsChanged?.Invoke();
    }

    public void RemoveEquipment(SOEquipmentData data)
    {
        if (data == null)
        {
            return;
        }

        if (_equippedItems.Remove(data))
        {
            EquippedItemsChanged?.Invoke();
        }
    }
}
