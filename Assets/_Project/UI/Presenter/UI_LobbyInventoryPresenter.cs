using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_LobbyInventoryPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_InventoryView _inventoryView;
    [SerializeField] private PlayerProfileService _playerProfileService;
    [SerializeField] private LobbyConsumableUseService _consumableUseService;
    [SerializeField] private LobbyConsumableTargetingCoordinator _targetingCoordinator;

    private readonly List<InventoryEntryViewModel> _equipmentEntries = new List<InventoryEntryViewModel>();
    private readonly List<InventoryEntryViewModel> _consumableEntries = new List<InventoryEntryViewModel>();
    private readonly List<InventoryEntryViewModel> _allEntries = new List<InventoryEntryViewModel>();
    private readonly List<EquippedInventoryItemViewModel> _equippedEntries = new List<EquippedInventoryItemViewModel>();

    private string _selectedItemId;

    private void Awake()
    {
        ResolveReferences();
        _inventoryView?.Bind();
    }

    private void OnEnable()
    {
        ResolveReferences();
        _inventoryView?.Bind();
        SubscribeToView();
        SubscribeToData();
        RefreshView();
    }

    private void Start()
    {
        RefreshView();
    }

    private void OnDisable()
    {
        UnsubscribeFromData();
        UnsubscribeFromView();
    }

    private void ResolveReferences()
    {
        _inventoryView ??= FindFirstObjectByType<UI_InventoryView>(FindObjectsInactive.Include);
        _consumableUseService ??= FindFirstObjectByType<LobbyConsumableUseService>(FindObjectsInactive.Include);
        _targetingCoordinator ??= FindFirstObjectByType<LobbyConsumableTargetingCoordinator>(FindObjectsInactive.Include);

        if (GameManager.I != null && GameManager.I.PlayerProfileService != null)
        {
            _playerProfileService = GameManager.I.PlayerProfileService;
        }

        _playerProfileService ??= FindFirstObjectByType<PlayerProfileService>(FindObjectsInactive.Include);
    }

    private void SubscribeToView()
    {
        if (_inventoryView == null)
        {
            return;
        }

        _inventoryView.InventoryEntrySelected += HandleInventoryEntrySelected;
        _inventoryView.EquippedItemSelected += HandleEquippedItemSelected;
        _inventoryView.EquippedItemUnequipClicked += HandleEquippedItemUnequipClicked;
        _inventoryView.EquipClicked += HandleEquipClicked;
        _inventoryView.UseClicked += HandleUseClicked;
    }

    private void UnsubscribeFromView()
    {
        if (_inventoryView == null)
        {
            return;
        }

        _inventoryView.InventoryEntrySelected -= HandleInventoryEntrySelected;
        _inventoryView.EquippedItemSelected -= HandleEquippedItemSelected;
        _inventoryView.EquippedItemUnequipClicked -= HandleEquippedItemUnequipClicked;
        _inventoryView.EquipClicked -= HandleEquipClicked;
        _inventoryView.UseClicked -= HandleUseClicked;
    }

    private void SubscribeToData()
    {
        if (_playerProfileService != null)
        {
            _playerProfileService.ProfileChanged += HandleProfileChanged;
        }

        if (_targetingCoordinator != null)
        {
            _targetingCoordinator.TargetingStateChanged += HandleTargetingStateChanged;
            _targetingCoordinator.ConsumableUseFinished += HandleConsumableUseFinished;
        }
    }

    private void UnsubscribeFromData()
    {
        if (_playerProfileService != null)
        {
            _playerProfileService.ProfileChanged -= HandleProfileChanged;
        }

        if (_targetingCoordinator != null)
        {
            _targetingCoordinator.TargetingStateChanged -= HandleTargetingStateChanged;
            _targetingCoordinator.ConsumableUseFinished -= HandleConsumableUseFinished;
        }
    }

    private void RefreshView()
    {
        if (_inventoryView == null || _playerProfileService == null)
        {
            return;
        }

        BuildInventoryEntries();

        _inventoryView.RenderOwnedEquipments(_equipmentEntries);
        _inventoryView.RenderOwnedConsumables(_consumableEntries);
        _inventoryView.RenderEquippedItems(_equippedEntries);
        int currentCostLimit = _playerProfileService.ProfileData != null
            ? _playerProfileService.ProfileData.EquipmentCostLimit
            : PlayerProfileData.MaxEquipmentCostLimit;
        _inventoryView.RefreshEquipmentCost(
            _playerProfileService.GetCurrentEquippedCost(),
            currentCostLimit);

        InventoryEntryViewModel selectedEntry = GetSelectedEntryOrDefault();
        if (selectedEntry == null)
        {
            _selectedItemId = string.Empty;
            _inventoryView.ClearSelection();
            _inventoryView.SetEquipButtonState(false, LobbyUiText.SelectEquipment);
            _inventoryView.SetUseButtonState(false, LobbyUiText.SelectItem);
            return;
        }

        _selectedItemId = selectedEntry.ItemId;
        _inventoryView.ShowSelectedItem(selectedEntry);
        RefreshActionButtons(selectedEntry);
    }

    private void BuildInventoryEntries()
    {
        _equipmentEntries.Clear();
        _consumableEntries.Clear();
        _allEntries.Clear();
        _equippedEntries.Clear();

        BuildEquipmentEntries();
        BuildConsumableEntries();
        BuildEquippedEntries();
    }

    private void BuildEquipmentEntries()
    {
        IReadOnlyList<SOEquipmentData> ownedEquipments = _playerProfileService.GetOwnedEquipments();
        for (int index = 0; index < ownedEquipments.Count; index++)
        {
            SOEquipmentData equipmentData = ownedEquipments[index];
            if (equipmentData == null)
            {
                continue;
            }

            InventoryEntryViewModel entry = CreateEquipmentEntry(equipmentData);
            _equipmentEntries.Add(entry);
            _allEntries.Add(entry);
        }
    }

    private void BuildConsumableEntries()
    {
        IReadOnlyList<SOConsumableData> consumableDefinitions = _playerProfileService.GetConsumableDefinitions();
        for (int index = 0; index < consumableDefinitions.Count; index++)
        {
            SOConsumableData consumableData = consumableDefinitions[index];
            if (consumableData == null)
            {
                continue;
            }

            int count = _playerProfileService.GetConsumableCount(consumableData.ConsumableId);
            if (count <= 0)
            {
                continue;
            }

            InventoryEntryViewModel entry = CreateConsumableEntry(consumableData, count);
            _consumableEntries.Add(entry);
            _allEntries.Add(entry);
        }
    }

    private void BuildEquippedEntries()
    {
        IReadOnlyList<SOEquipmentData> equippedEquipments = _playerProfileService.GetEquippedEquipments();
        for (int index = 0; index < equippedEquipments.Count; index++)
        {
            SOEquipmentData equipmentData = equippedEquipments[index];
            if (equipmentData == null)
            {
                continue;
            }

            _equippedEntries.Add(new EquippedInventoryItemViewModel(
                equipmentData.EquipmentId,
                equipmentData.DisplayName,
                equipmentData.SpriteIcon));
        }
    }

    private InventoryEntryViewModel CreateEquipmentEntry(SOEquipmentData equipmentData)
    {
        bool isEquipped = _playerProfileService.IsEquipped(equipmentData.EquipmentId);

        string statusText = isEquipped
            ? LobbyUiText.Equipped
            : LobbyUiText.Owned;

        return new InventoryEntryViewModel(
            CreateItemId(InventoryEntryCategory.Equipment, equipmentData.EquipmentId),
            equipmentData.EquipmentId,
            InventoryEntryCategory.Equipment,
            LobbyConsumableTargetType.None,
            equipmentData.DisplayName,
            equipmentData.Description,
            equipmentData.SpriteIcon,
            equipmentData.Cost,
            0,
            LobbyUiText.Cost(equipmentData.Cost),
            string.Empty,
            statusText,
            true,
            false);
    }

    private InventoryEntryViewModel CreateConsumableEntry(SOConsumableData consumableData, int count)
    {
        return new InventoryEntryViewModel(
            CreateItemId(InventoryEntryCategory.Consumable, consumableData.ConsumableId),
            consumableData.ConsumableId,
            InventoryEntryCategory.Consumable,
            consumableData.TargetType,
            consumableData.DisplayName,
            consumableData.Description,
            consumableData.Icon,
            0,
            count,
            string.Empty,
            LobbyUiText.Count(count),
            LobbyUiText.OwnedCount(count),
            false,
            true);
    }

    private string CreateItemId(InventoryEntryCategory category, string productId)
    {
        return $"{category}:{productId}";
    }

    private InventoryEntryViewModel GetSelectedEntryOrDefault()
    {
        InventoryEntryViewModel selectedEntry = FindEntryById(_selectedItemId);
        if (selectedEntry != null)
        {
            return selectedEntry;
        }

        if (_allEntries.Count == 0)
        {
            return null;
        }

        return _allEntries[0];
    }

    private InventoryEntryViewModel FindEntryById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            return null;
        }

        for (int index = 0; index < _allEntries.Count; index++)
        {
            InventoryEntryViewModel entry = _allEntries[index];
            if (entry == null || !string.Equals(entry.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return entry;
        }

        return null;
    }

    private void RefreshActionButtons(InventoryEntryViewModel selectedEntry)
    {
        if (selectedEntry.Category == InventoryEntryCategory.Equipment)
        {
            RefreshEquipmentButtons(selectedEntry);
            return;
        }

        RefreshConsumableButtons(selectedEntry);
    }

    private void RefreshEquipmentButtons(InventoryEntryViewModel selectedEntry)
    {
        EquipmentEquipResult equipResult = _playerProfileService.EvaluateEquipEquipment(selectedEntry.ProductId);
        bool canEquip = equipResult == EquipmentEquipResult.Success;

        _inventoryView.SetEquipButtonState(canEquip, BuildEquipmentButtonText(equipResult));
        _inventoryView.SetUseButtonState(false, LobbyUiText.Use);
    }

    private void RefreshConsumableButtons(InventoryEntryViewModel selectedEntry)
    {
        _inventoryView.SetEquipButtonState(false, LobbyUiText.Equip);

        if (selectedEntry.TargetType == LobbyConsumableTargetType.SelectedTile)
        {
            bool isCurrentTarget = _targetingCoordinator != null
                && _targetingCoordinator.IsTargeting
                && string.Equals(_targetingCoordinator.PendingConsumableId, selectedEntry.ProductId, StringComparison.OrdinalIgnoreCase);

            string targetedUseText = isCurrentTarget
                ? LobbyUiText.CancelTargeting
                : LobbyUiText.SelectTile;

            _inventoryView.SetUseButtonState(true, targetedUseText);
            return;
        }

        LobbyConsumableUseResult useResult = _consumableUseService != null
            ? _consumableUseService.GetUseResult(selectedEntry.ProductId)
            : LobbyConsumableUseResult.ConsumableNotFound;

        bool canUse = useResult == LobbyConsumableUseResult.Success;
        string useText = canUse
            ? LobbyUiText.Use
            : BuildConsumableDisabledText(useResult);

        _inventoryView.SetUseButtonState(canUse, useText);
    }

    private string BuildConsumableDisabledText(LobbyConsumableUseResult useResult)
    {
        switch (useResult)
        {
            case LobbyConsumableUseResult.StaminaAlreadyFull:
                return LobbyUiText.StaminaFull;

            case LobbyConsumableUseResult.NotOwned:
                return LobbyUiText.NoItem;

            default:
                return LobbyUiText.Use;
        }
    }

    private string BuildEquipmentButtonText(EquipmentEquipResult equipResult)
    {
        switch (equipResult)
        {
            case EquipmentEquipResult.Success:
                return LobbyUiText.Equip;

            case EquipmentEquipResult.AlreadyEquipped:
                return LobbyUiText.Equipped;

            case EquipmentEquipResult.CostLimitExceeded:
                return LobbyUiText.NeedMoreCost;

            case EquipmentEquipResult.ExclusiveTypeAlreadyEquipped:
                return "색깔 장비는 1개만 장착 가능";

            default:
                return LobbyUiText.Equip;
        }
    }

    private void HandleInventoryEntrySelected(string itemId)
    {
        _selectedItemId = itemId;
        RefreshView();
    }

    private void HandleEquippedItemSelected(string equipmentId)
    {
        _selectedItemId = CreateItemId(InventoryEntryCategory.Equipment, equipmentId);
        RefreshView();
    }

    private void HandleEquippedItemUnequipClicked(string equipmentId)
    {
        if (_playerProfileService == null)
        {
            return;
        }

        _playerProfileService.TryUnequipEquipment(equipmentId);
        RefreshView();
    }

    private void HandleEquipClicked()
    {
        InventoryEntryViewModel selectedEntry = FindEntryById(_selectedItemId);
        if (selectedEntry == null || selectedEntry.Category != InventoryEntryCategory.Equipment)
        {
            return;
        }

        EquipmentEquipResult equipResult = _playerProfileService.TryEquipEquipment(selectedEntry.ProductId);
        if (equipResult != EquipmentEquipResult.Success)
        {
            Debug.LogWarning($"Inventory equip failed: {selectedEntry.ProductId} / {equipResult}");
        }

        RefreshView();
    }

    private void HandleUseClicked()
    {
        InventoryEntryViewModel selectedEntry = FindEntryById(_selectedItemId);
        if (selectedEntry == null || selectedEntry.Category != InventoryEntryCategory.Consumable)
        {
            return;
        }

        if (selectedEntry.TargetType == LobbyConsumableTargetType.SelectedTile)
        {
            HandleTargetedConsumableUse(selectedEntry);
            return;
        }

        if (_consumableUseService == null)
        {
            return;
        }

        LobbyConsumableUseResult useResult = _consumableUseService.TryUseConsumable(selectedEntry.ProductId);
        if (useResult != LobbyConsumableUseResult.Success)
        {
            Debug.LogWarning($"Inventory use failed: {selectedEntry.ProductId} / {useResult}");
        }

        RefreshView();
    }

    private void HandleTargetedConsumableUse(InventoryEntryViewModel selectedEntry)
    {
        if (_targetingCoordinator == null)
        {
            return;
        }

        bool isCurrentTarget = _targetingCoordinator.IsTargeting
            && string.Equals(_targetingCoordinator.PendingConsumableId, selectedEntry.ProductId, StringComparison.OrdinalIgnoreCase);

        if (isCurrentTarget)
        {
            _targetingCoordinator.CancelTargeting();
        }
        else
        {
            _targetingCoordinator.StartTargeting(selectedEntry.ProductId);
        }

        RefreshView();
    }

    private void HandleProfileChanged()
    {
        RefreshView();
    }

    private void HandleTargetingStateChanged(LobbyConsumableTargetingState targetingState)
    {
        RefreshView();
    }

    private void HandleConsumableUseFinished(LobbyConsumableUseFeedback feedback)
    {
        if (!feedback.IsSuccess)
        {
            Debug.LogWarning($"Consumable use failed: {feedback.ConsumableId} / {feedback.Result}");
        }

        RefreshView();
    }
}
