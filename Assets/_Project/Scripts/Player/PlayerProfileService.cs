using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipmentPurchaseResult
{
    Success,
    EquipmentNotFound,
    AlreadyOwned,
    NotEnoughGold
}

public enum EquipmentEquipResult
{
    Success,
    EquipmentNotFound,
    NotOwned,
    AlreadyEquipped,
    CostLimitExceeded,
    ExclusiveTypeAlreadyEquipped
}

public enum ConsumablePurchaseResult
{
    Success,
    ConsumableNotFound,
    NotEnoughGold
}

[DisallowMultipleComponent]
public sealed class PlayerProfileService : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private SOEquipmentCatalog _equipmentCatalog;
    [SerializeField] private SOConsumableCatalog _consumableCatalog;

    [Header("Profile")]
    [SerializeField] private PlayerProfileData _profileData = new PlayerProfileData();

    public SOEquipmentCatalog EquipmentCatalog => _equipmentCatalog;
    public SOConsumableCatalog ConsumableCatalog => _consumableCatalog;
    public PlayerProfileData ProfileData => _profileData;

    public event Action ProfileChanged;
    public event Action GoldChanged;
    public event Action InventoryChanged;
    public event Action EquippedItemsChanged;

    private void Awake()
    {
        _profileData ??= new PlayerProfileData();
    }

    private void OnValidate()
    {
        _profileData ??= new PlayerProfileData();
    }

    public void SetEquipmentCatalog(SOEquipmentCatalog equipmentCatalog)
    {
        _equipmentCatalog = equipmentCatalog;
    }

    public void SetConsumableCatalog(SOConsumableCatalog consumableCatalog)
    {
        _consumableCatalog = consumableCatalog;
    }

    public bool HasEquipment(string equipmentId)
    {
        return _profileData != null && _profileData.HasOwnedEquipment(equipmentId);
    }

    public bool IsEquipped(string equipmentId)
    {
        return _profileData != null && _profileData.IsEquipped(equipmentId);
    }

    public int GetCurrentEquippedCost()
    {
        if (_profileData == null || _equipmentCatalog == null)
        {
            return 0;
        }

        int totalCost = 0;
        IReadOnlyList<string> equippedIds = _profileData.EquippedEquipmentIds;

        for (int index = 0; index < equippedIds.Count; index++)
        {
            if (_equipmentCatalog.TryGetEquipmentById(equippedIds[index], out SOEquipmentData equipmentData))
            {
                totalCost += equipmentData.Cost;
            }
        }

        return totalCost;
    }

    public IReadOnlyList<SOEquipmentData> GetOwnedEquipments()
    {
        return BuildEquipmentList(_profileData?.OwnedEquipmentIds);
    }

    public IReadOnlyList<SOEquipmentData> GetEquippedEquipments()
    {
        return BuildEquipmentList(_profileData?.EquippedEquipmentIds);
    }

    public IReadOnlyList<SOConsumableData> GetConsumableDefinitions()
    {
        return _consumableCatalog != null
            ? _consumableCatalog.ConsumableItems
            : Array.Empty<SOConsumableData>();
    }

    public int GetConsumableCount(string consumableId)
    {
        return _profileData != null
            ? _profileData.GetConsumableCount(consumableId)
            : 0;
    }

    public bool TrySpendConsumable(string consumableId, int amount = 1)
    {
        if (_profileData == null || !_profileData.TrySpendConsumable(consumableId, amount))
        {
            return false;
        }

        NotifyInventoryChanged();
        return true;
    }

    public bool TryGetConsumable(string consumableId, out SOConsumableData consumableData)
    {
        consumableData = null;
        return _consumableCatalog != null
            && _consumableCatalog.TryGetConsumableById(consumableId, out consumableData);
    }

    public EquipmentPurchaseResult TryBuyEquipment(string equipmentId)
    {
        if (_equipmentCatalog == null || !_equipmentCatalog.TryGetEquipmentById(equipmentId, out SOEquipmentData equipmentData))
        {
            return EquipmentPurchaseResult.EquipmentNotFound;
        }

        if (_profileData.HasOwnedEquipment(equipmentId))
        {
            return EquipmentPurchaseResult.AlreadyOwned;
        }

        if (!_profileData.TrySpendGold(equipmentData.Price))
        {
            return EquipmentPurchaseResult.NotEnoughGold;
        }

        _profileData.AddOwnedEquipment(equipmentId);
        NotifyGoldChanged();
        NotifyInventoryChanged();
        return EquipmentPurchaseResult.Success;
    }

    public ConsumablePurchaseResult TryBuyConsumable(string consumableId)
    {
        if (_consumableCatalog == null || !_consumableCatalog.TryGetConsumableById(consumableId, out SOConsumableData consumableData))
        {
            return ConsumablePurchaseResult.ConsumableNotFound;
        }

        if (!_profileData.TrySpendGold(consumableData.Price))
        {
            return ConsumablePurchaseResult.NotEnoughGold;
        }

        _profileData.AddConsumable(consumableId);
        NotifyGoldChanged();
        NotifyInventoryChanged();
        return ConsumablePurchaseResult.Success;
    }

    public EquipmentEquipResult TryEquipEquipment(string equipmentId)
    {
        EquipmentEquipResult validationResult = EvaluateEquipEquipment(equipmentId, out SOEquipmentData equipmentData);
        if (validationResult != EquipmentEquipResult.Success)
        {
            return validationResult;
        }

        _profileData.AddEquippedEquipment(equipmentId);
        NotifyEquippedItemsChanged();
        return EquipmentEquipResult.Success;
    }

    public bool TryUnequipEquipment(string equipmentId)
    {
        if (_profileData == null)
        {
            return false;
        }

        if (!_profileData.RemoveEquippedEquipment(equipmentId))
        {
            return false;
        }

        NotifyEquippedItemsChanged();
        return true;
    }

    public void SetGold(int gold)
    {
        _profileData.SetGold(gold);
        NotifyGoldChanged();
    }

    public void AddGold(int amount)
    {
        int previousGold = _profileData.Gold;
        _profileData.AddGold(amount);

        if (_profileData.Gold != previousGold)
        {
            NotifyGoldChanged();
        }
    }

    public void AddConsumable(string consumableId, int amount = 1)
    {
        if (_profileData == null)
        {
            return;
        }

        int previousCount = _profileData.GetConsumableCount(consumableId);
        _profileData.AddConsumable(consumableId, amount);

        if (_profileData.GetConsumableCount(consumableId) != previousCount)
        {
            NotifyInventoryChanged();
        }
    }

    public void SetEquipmentCostLimit(int equipmentCostLimit)
    {
        _profileData.SetEquipmentCostLimit(equipmentCostLimit);
        NotifyProfileChanged();
    }

    public EquipmentEquipResult EvaluateEquipEquipment(string equipmentId)
    {
        return EvaluateEquipEquipment(equipmentId, out _);
    }

    private IReadOnlyList<SOEquipmentData> BuildEquipmentList(IReadOnlyList<string> equipmentIds)
    {
        List<SOEquipmentData> equipments = new List<SOEquipmentData>();

        if (_equipmentCatalog == null || equipmentIds == null)
        {
            return equipments;
        }

        for (int index = 0; index < equipmentIds.Count; index++)
        {
            if (_equipmentCatalog.TryGetEquipmentById(equipmentIds[index], out SOEquipmentData equipmentData))
            {
                equipments.Add(equipmentData);
            }
        }

        return equipments;
    }

    private EquipmentEquipResult EvaluateEquipEquipment(string equipmentId, out SOEquipmentData equipmentData)
    {
        equipmentData = null;

        if (_profileData == null || _equipmentCatalog == null || !_equipmentCatalog.TryGetEquipmentById(equipmentId, out equipmentData))
        {
            return EquipmentEquipResult.EquipmentNotFound;
        }

        if (!_profileData.HasOwnedEquipment(equipmentId))
        {
            return EquipmentEquipResult.NotOwned;
        }

        if (_profileData.IsEquipped(equipmentId))
        {
            return EquipmentEquipResult.AlreadyEquipped;
        }

        if (HasExclusiveTypeConflict(equipmentData))
        {
            return EquipmentEquipResult.ExclusiveTypeAlreadyEquipped;
        }

        int nextCost = GetCurrentEquippedCost() + equipmentData.Cost;
        if (nextCost > _profileData.EquipmentCostLimit)
        {
            return EquipmentEquipResult.CostLimitExceeded;
        }

        return EquipmentEquipResult.Success;
    }

    private bool HasExclusiveTypeConflict(SOEquipmentData targetEquipment)
    {
        if (targetEquipment == null || !IsExclusiveEquipmentType(targetEquipment.EquipmentType))
        {
            return false;
        }

        IReadOnlyList<string> equippedIds = _profileData.EquippedEquipmentIds;
        for (int index = 0; index < equippedIds.Count; index++)
        {
            if (!_equipmentCatalog.TryGetEquipmentById(equippedIds[index], out SOEquipmentData equippedData) || equippedData == null)
            {
                continue;
            }

            if (equippedData.EquipmentType == targetEquipment.EquipmentType)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExclusiveEquipmentType(EquipmentType equipmentType)
    {
        return equipmentType == EquipmentType.Color;
    }

    private void NotifyGoldChanged()
    {
        GoldChanged?.Invoke();
        NotifyProfileChanged();
    }

    private void NotifyInventoryChanged()
    {
        InventoryChanged?.Invoke();
        NotifyProfileChanged();
    }

    private void NotifyEquippedItemsChanged()
    {
        EquippedItemsChanged?.Invoke();
        NotifyProfileChanged();
    }

    private void NotifyProfileChanged()
    {
        ProfileChanged?.Invoke();
    }
}
