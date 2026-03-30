using UnityEngine;

public enum InventoryEntryCategory
{
    Equipment,
    Consumable
}

public sealed class InventoryEntryViewModel
{
    public string ItemId { get; }
    public string ProductId { get; }
    public InventoryEntryCategory Category { get; }
    public LobbyConsumableTargetType TargetType { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Sprite Icon { get; }
    public int Cost { get; }
    public int Count { get; }
    public string CostText { get; }
    public string CountText { get; }
    public string StatusText { get; }
    public bool ShowCost { get; }
    public bool ShowCount { get; }

    public InventoryEntryViewModel(
        string itemId,
        string productId,
        InventoryEntryCategory category,
        LobbyConsumableTargetType targetType,
        string displayName,
        string description,
        Sprite icon,
        int cost,
        int count,
        string costText,
        string countText,
        string statusText,
        bool showCost,
        bool showCount)
    {
        ItemId = itemId;
        ProductId = productId;
        Category = category;
        TargetType = targetType;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Cost = cost;
        Count = count;
        CostText = costText;
        CountText = countText;
        StatusText = statusText;
        ShowCost = showCost;
        ShowCount = showCount;
    }
}

public sealed class EquippedInventoryItemViewModel
{
    public string EquipmentId { get; }
    public string DisplayName { get; }
    public Sprite Icon { get; }

    public EquippedInventoryItemViewModel(string equipmentId, string displayName, Sprite icon)
    {
        EquipmentId = equipmentId;
        DisplayName = displayName;
        Icon = icon;
    }
}
