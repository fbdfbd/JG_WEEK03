using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class PlayerProfileData
{
    public const int MaxEquipmentCostLimit = 10;

    [Serializable]
    public sealed class PlayerConsumableEntry
    {
        [SerializeField] private string _consumableId;
        [SerializeField] private int _count;

        public string ConsumableId => _consumableId;
        public int Count => _count;

        public PlayerConsumableEntry(string consumableId, int count)
        {
            _consumableId = consumableId;
            _count = Mathf.Max(0, count);
        }

        public void AddCount(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _count += amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || _count < amount)
            {
                return false;
            }

            _count -= amount;
            return true;
        }
    }

    [SerializeField] private int _gold;
    [SerializeField] private int _equipmentCostLimit = MaxEquipmentCostLimit;
    [SerializeField] private List<string> _ownedEquipmentIds = new List<string>();
    [SerializeField] private List<string> _equippedEquipmentIds = new List<string>();
    [SerializeField] private List<PlayerConsumableEntry> _ownedConsumables = new List<PlayerConsumableEntry>();

    public int Gold => _gold;
    public int EquipmentCostLimit => _equipmentCostLimit;
    public IReadOnlyList<string> OwnedEquipmentIds => _ownedEquipmentIds;
    public IReadOnlyList<string> EquippedEquipmentIds => _equippedEquipmentIds;
    public IReadOnlyList<PlayerConsumableEntry> OwnedConsumables => _ownedConsumables;

    public void SetGold(int gold)
    {
        _gold = Mathf.Max(0, gold);
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _gold += amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0)
        {
            return false;
        }

        if (_gold < amount)
        {
            return false;
        }

        _gold -= amount;
        return true;
    }

    public void SetEquipmentCostLimit(int equipmentCostLimit)
    {
        _equipmentCostLimit = Mathf.Clamp(equipmentCostLimit, 0, MaxEquipmentCostLimit);
    }

    public bool HasOwnedEquipment(string equipmentId)
    {
        return ContainsId(_ownedEquipmentIds, equipmentId);
    }

    public bool IsEquipped(string equipmentId)
    {
        return ContainsId(_equippedEquipmentIds, equipmentId);
    }

    public bool AddOwnedEquipment(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId) || HasOwnedEquipment(equipmentId))
        {
            return false;
        }

        _ownedEquipmentIds.Add(equipmentId);
        return true;
    }

    public bool AddEquippedEquipment(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId) || IsEquipped(equipmentId))
        {
            return false;
        }

        _equippedEquipmentIds.Add(equipmentId);
        return true;
    }

    public bool RemoveEquippedEquipment(string equipmentId)
    {
        if (string.IsNullOrWhiteSpace(equipmentId))
        {
            return false;
        }

        for (int index = 0; index < _equippedEquipmentIds.Count; index++)
        {
            if (!string.Equals(_equippedEquipmentIds[index], equipmentId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            _equippedEquipmentIds.RemoveAt(index);
            return true;
        }

        return false;
    }

    public int GetConsumableCount(string consumableId)
    {
        int entryIndex = FindConsumableEntryIndex(consumableId);
        if (entryIndex < 0)
        {
            return 0;
        }

        return _ownedConsumables[entryIndex].Count;
    }

    public void AddConsumable(string consumableId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(consumableId) || amount <= 0)
        {
            return;
        }

        int entryIndex = FindConsumableEntryIndex(consumableId);
        if (entryIndex >= 0)
        {
            _ownedConsumables[entryIndex].AddCount(amount);
            return;
        }

        _ownedConsumables.Add(new PlayerConsumableEntry(consumableId, amount));
    }

    public bool TrySpendConsumable(string consumableId, int amount = 1)
    {
        int entryIndex = FindConsumableEntryIndex(consumableId);
        if (entryIndex < 0)
        {
            return false;
        }

        PlayerConsumableEntry entry = _ownedConsumables[entryIndex];
        if (!entry.TrySpend(amount))
        {
            return false;
        }

        if (entry.Count <= 0)
        {
            _ownedConsumables.RemoveAt(entryIndex);
        }

        return true;
    }

    private static bool ContainsId(IReadOnlyList<string> ids, string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return false;
        }

        for (int index = 0; index < ids.Count; index++)
        {
            if (string.Equals(ids[index], targetId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private int FindConsumableEntryIndex(string consumableId)
    {
        if (string.IsNullOrWhiteSpace(consumableId))
        {
            return -1;
        }

        for (int index = 0; index < _ownedConsumables.Count; index++)
        {
            if (!string.Equals(_ownedConsumables[index].ConsumableId, consumableId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return index;
        }

        return -1;
    }
}
