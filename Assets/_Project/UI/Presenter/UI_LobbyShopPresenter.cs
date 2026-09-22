using System;
using System.Collections.Generic;
using UnityEngine;

public enum ShopItemCategory
{
    Equipment,
    Consumable
}

public sealed class ShopItemViewModel
{
    public string ItemId { get; }
    public string ProductId { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Sprite Icon { get; }
    public int Price { get; }
    public string PriceText { get; }
    public string CostText { get; }
    public string StatusText { get; }
    public string BuyButtonText { get; }
    public bool IsOwned { get; }
    public bool CanBuy { get; }
    public bool ShowCost { get; }
    public ShopItemCategory Category { get; }

    public ShopItemViewModel(
        string itemId,
        string productId,
        string displayName,
        string description,
        Sprite icon,
        int price,
        string priceText,
        string costText,
        string statusText,
        string buyButtonText,
        bool isOwned,
        bool canBuy,
        bool showCost,
        ShopItemCategory category)
    {
        ItemId = itemId;
        ProductId = productId;
        DisplayName = displayName;
        Description = description;
        Icon = icon;
        Price = price;
        PriceText = priceText;
        CostText = costText;
        StatusText = statusText;
        BuyButtonText = buyButtonText;
        IsOwned = isOwned;
        CanBuy = canBuy;
        ShowCost = showCost;
        Category = category;
    }
}

[DisallowMultipleComponent]
public sealed class UI_LobbyShopPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_ShopView _shopView;
    [SerializeField] private PlayerProfileService _playerProfileService;
    [SerializeField] private SOEquipmentCatalog _equipmentCatalog;
    [SerializeField] private SOConsumableCatalog _consumableCatalog;

    private readonly List<ShopItemViewModel> _shopItems = new List<ShopItemViewModel>();
    private string _selectedItemId;
    private bool _hasRequiredReferences;

    private void Awake()
    {
        ResolveDependencies();
        _hasRequiredReferences = ValidateReferences();

        if (_hasRequiredReferences)
        {
            _shopView.Bind();
        }
    }

    private void OnEnable()
    {
        if (!_hasRequiredReferences)
        {
            return;
        }

        SubscribeToView();
        SubscribeToData();
        RefreshView();
    }

    private void Start()
    {
        if (_hasRequiredReferences)
        {
            RefreshView();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromData();
        UnsubscribeFromView();
    }

    private void ResolveDependencies()
    {
        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            _playerProfileService = GameManager.I.PlayerProfileService;
        }

        if (_equipmentCatalog == null && _playerProfileService != null)
        {
            _equipmentCatalog = _playerProfileService.EquipmentCatalog;
        }

        if (_consumableCatalog == null && _playerProfileService != null)
        {
            _consumableCatalog = _playerProfileService.ConsumableCatalog;
        }

        if (_playerProfileService != null && _playerProfileService.EquipmentCatalog == null && _equipmentCatalog != null)
        {
            _playerProfileService.SetEquipmentCatalog(_equipmentCatalog);
        }

        if (_playerProfileService != null && _playerProfileService.ConsumableCatalog == null && _consumableCatalog != null)
        {
            _playerProfileService.SetConsumableCatalog(_consumableCatalog);
        }

        if (_equipmentCatalog == null && _playerProfileService != null)
        {
            _equipmentCatalog = _playerProfileService.EquipmentCatalog;
        }

        if (_consumableCatalog == null && _playerProfileService != null)
        {
            _consumableCatalog = _playerProfileService.ConsumableCatalog;
        }
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (_shopView == null)
        {
            Debug.LogError($"{nameof(UI_LobbyShopPresenter)} requires a {nameof(UI_ShopView)} reference.", this);
            isValid = false;
        }

        if (_playerProfileService == null)
        {
            Debug.LogError($"{nameof(UI_LobbyShopPresenter)} requires a {nameof(PlayerProfileService)} reference.", this);
            isValid = false;
        }

        if (_equipmentCatalog == null)
        {
            Debug.LogError($"{nameof(UI_LobbyShopPresenter)} requires an {nameof(SOEquipmentCatalog)} reference.", this);
            isValid = false;
        }

        if (_consumableCatalog == null)
        {
            Debug.LogError($"{nameof(UI_LobbyShopPresenter)} requires a {nameof(SOConsumableCatalog)} reference.", this);
            isValid = false;
        }

        return isValid;
    }

    private void SubscribeToView()
    {
        if (_shopView == null)
        {
            return;
        }

        _shopView.ItemSelected += HandleItemSelected;
        _shopView.BuyClicked += HandleBuyClicked;
    }

    private void UnsubscribeFromView()
    {
        if (_shopView == null)
        {
            return;
        }

        _shopView.ItemSelected -= HandleItemSelected;
        _shopView.BuyClicked -= HandleBuyClicked;
    }

    private void SubscribeToData()
    {
        if (_playerProfileService == null)
        {
            return;
        }

        _playerProfileService.ProfileChanged += HandleProfileChanged;
    }

    private void UnsubscribeFromData()
    {
        if (_playerProfileService == null)
        {
            return;
        }

        _playerProfileService.ProfileChanged -= HandleProfileChanged;
    }

    private void RefreshView()
    {
        if (_shopView == null)
        {
            return;
        }

        BuildShopItems();
        _shopView.RenderItems(_shopItems);

        ShopItemViewModel selectedItem = GetSelectedItemOrDefault();
        if (selectedItem == null)
        {
            _selectedItemId = string.Empty;
            _shopView.ClearSelection();
            return;
        }

        _selectedItemId = selectedItem.ItemId;
        _shopView.ShowSelectedItem(selectedItem);
    }

    private void BuildShopItems()
    {
        _shopItems.Clear();
        BuildEquipmentItems();
        BuildConsumableItems();
    }

    private void BuildEquipmentItems()
    {
        if (_equipmentCatalog == null)
        {
            return;
        }

        IReadOnlyList<SOEquipmentData> equipmentItems = _equipmentCatalog.EquipmentItems;
        for (int index = 0; index < equipmentItems.Count; index++)
        {
            SOEquipmentData equipmentData = equipmentItems[index];
            if (equipmentData == null)
            {
                continue;
            }

            _shopItems.Add(CreateEquipmentViewModel(equipmentData));
        }
    }

    private void BuildConsumableItems()
    {
        if (_playerProfileService == null || _consumableCatalog == null)
        {
            return;
        }

        IReadOnlyList<SOConsumableData> consumableItems = _consumableCatalog.ConsumableItems;
        for (int index = 0; index < consumableItems.Count; index++)
        {
            SOConsumableData consumableData = consumableItems[index];
            if (consumableData == null)
            {
                continue;
            }

            _shopItems.Add(CreateConsumableViewModel(consumableData));
        }
    }

    private ShopItemViewModel CreateEquipmentViewModel(SOEquipmentData equipmentData)
    {
        bool hasProfile = _playerProfileService != null && _playerProfileService.ProfileData != null;
        bool isOwned = hasProfile && _playerProfileService.HasEquipment(equipmentData.EquipmentId);
        int currentGold = hasProfile ? _playerProfileService.ProfileData.Gold : 0;
        bool canBuy = hasProfile && !isOwned && currentGold >= equipmentData.Price;

        string priceText = LobbyUiText.Price(equipmentData.Price);
        string costText = LobbyUiText.Cost(equipmentData.Cost);
        string statusText = BuildEquipmentStatusText(hasProfile, isOwned, canBuy);
        string buyButtonText = BuildEquipmentBuyButtonText(hasProfile, isOwned, canBuy, equipmentData.Price);

        return new ShopItemViewModel(
            CreateShopItemId(ShopItemCategory.Equipment, equipmentData.EquipmentId),
            equipmentData.EquipmentId,
            equipmentData.DisplayName,
            equipmentData.Description,
            equipmentData.SpriteIcon,
            equipmentData.Price,
            priceText,
            costText,
            statusText,
            buyButtonText,
            isOwned,
            canBuy,
            true,
            ShopItemCategory.Equipment);
    }

    private ShopItemViewModel CreateConsumableViewModel(SOConsumableData consumableData)
    {
        bool hasProfile = _playerProfileService != null && _playerProfileService.ProfileData != null;
        int currentGold = hasProfile ? _playerProfileService.ProfileData.Gold : 0;
        int ownedCount = hasProfile ? _playerProfileService.GetConsumableCount(consumableData.ConsumableId) : 0;
        bool canBuy = hasProfile && currentGold >= consumableData.Price;

        string priceText = LobbyUiText.Price(consumableData.Price);
        string statusText = BuildConsumableStatusText(hasProfile, ownedCount);
        string buyButtonText = BuildConsumableBuyButtonText(hasProfile, canBuy, consumableData.Price);

        return new ShopItemViewModel(
            CreateShopItemId(ShopItemCategory.Consumable, consumableData.ConsumableId),
            consumableData.ConsumableId,
            consumableData.DisplayName,
            consumableData.Description,
            consumableData.Icon,
            consumableData.Price,
            priceText,
            string.Empty,
            statusText,
            buyButtonText,
            ownedCount > 0,
            canBuy,
            false,
            ShopItemCategory.Consumable);
    }

    private ShopItemViewModel GetSelectedItemOrDefault()
    {
        ShopItemViewModel selectedItem = FindItemById(_selectedItemId);
        if (selectedItem != null)
        {
            return selectedItem;
        }

        if (_shopItems.Count == 0)
        {
            return null;
        }

        return _shopItems[0];
    }

    private ShopItemViewModel FindItemById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        for (int index = 0; index < _shopItems.Count; index++)
        {
            ShopItemViewModel item = _shopItems[index];
            if (item == null || !string.Equals(item.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return item;
        }

        return null;
    }

    private string BuildEquipmentStatusText(bool hasProfile, bool isOwned, bool canBuy)
    {
        if (!hasProfile)
        {
            return LobbyUiText.ProfileRequired;
        }

        if (isOwned)
        {
            return LobbyUiText.Owned;
        }

        if (canBuy)
        {
            return LobbyUiText.CanBuy;
        }

        return LobbyUiText.NeedMoreGold;
    }

    private string BuildEquipmentBuyButtonText(bool hasProfile, bool isOwned, bool canBuy, int price)
    {
        if (!hasProfile)
        {
            return LobbyUiText.ProfileRequired;
        }

        if (isOwned)
        {
            return LobbyUiText.Owned;
        }

        if (canBuy)
        {
            return LobbyUiText.BuyWithGold(price);
        }

        return LobbyUiText.NeedMoreGold;
    }

    private string BuildConsumableStatusText(bool hasProfile, int ownedCount)
    {
        if (!hasProfile)
        {
            return LobbyUiText.ProfileRequired;
        }

        return LobbyUiText.OwnedCount(ownedCount);
    }

    private string BuildConsumableBuyButtonText(bool hasProfile, bool canBuy, int price)
    {
        if (!hasProfile)
        {
            return LobbyUiText.ProfileRequired;
        }

        if (canBuy)
        {
            return LobbyUiText.BuyWithGold(price);
        }

        return LobbyUiText.NeedMoreGold;
    }

    private string CreateShopItemId(ShopItemCategory category, string productId)
    {
        return $"{category}:{productId}";
    }

    private void HandleItemSelected(string itemId)
    {
        _selectedItemId = itemId;
        _shopView.ShowSelectedItem(FindItemById(itemId));
    }

    private void HandleBuyClicked()
    {
        if (_playerProfileService == null || string.IsNullOrWhiteSpace(_selectedItemId))
        {
            return;
        }

        ShopItemViewModel selectedItem = FindItemById(_selectedItemId);
        if (selectedItem == null)
        {
            return;
        }

        switch (selectedItem.Category)
        {
            case ShopItemCategory.Equipment:
                EquipmentPurchaseResult equipmentPurchaseResult = _playerProfileService.TryBuyEquipment(selectedItem.ProductId);
                if (equipmentPurchaseResult != EquipmentPurchaseResult.Success)
                {
                    Debug.LogWarning($"Shop purchase failed: {selectedItem.ProductId} / {equipmentPurchaseResult}");
                }
                break;

            case ShopItemCategory.Consumable:
                ConsumablePurchaseResult consumablePurchaseResult = _playerProfileService.TryBuyConsumable(selectedItem.ProductId);
                if (consumablePurchaseResult != ConsumablePurchaseResult.Success)
                {
                    Debug.LogWarning($"Shop purchase failed: {selectedItem.ProductId} / {consumablePurchaseResult}");
                }
                break;
        }

        RefreshView();
    }

    private void HandleProfileChanged()
    {
        RefreshView();
    }
}
