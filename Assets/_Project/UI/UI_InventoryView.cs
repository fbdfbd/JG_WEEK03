using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InventoryView : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _playerEquipContainer;
    [SerializeField] private Transform _equipContainer;
    [SerializeField] private Transform _itemContainer;

    [Header("Prefabs")]
    [SerializeField] private UI_PlayerEquipButtonView _playerEquipButtonPrefab;
    [SerializeField] private UI_InventoryEquipButtonView _inventoryEquipButtonPrefab;
    [SerializeField] private UI_InventoryItemButtonView _inventoryItemButtonPrefab;

    [Header("Action Buttons")]
    [SerializeField] private Button _equipButton;
    [SerializeField] private Button _useButton;
    [SerializeField] private TMP_Text _equipButtonText;
    [SerializeField] private TMP_Text _useButtonText;

    [Header("Selected Item")]
    [SerializeField] private Image _selectItemImage;
    [SerializeField] private TMP_Text _selectItemNameText;
    [SerializeField] private TMP_Text _selectItemStatusText;
    [SerializeField] private TMP_Text _selectItemCostText;
    [SerializeField] private TMP_Text _selectItemCountText;
    [SerializeField] private TMP_Text _selectItemDescriptionText;

    [Header("Cost")]
    [SerializeField] private TMP_Text _currentCostText;
    [SerializeField] private List<UI_InventoryCostImageView> _costImageViews = new List<UI_InventoryCostImageView>();

    public event Action<string> InventoryEntrySelected;
    public event Action EquipClicked;
    public event Action UseClicked;
    public event Action<string> EquippedItemSelected;
    public event Action<string> EquippedItemUnequipClicked;

    public void Bind()
    {
        CacheButtonTexts();
        BindButton(_equipButton, HandleEquipButtonClicked);
        BindButton(_useButton, HandleUseButtonClicked);
    }

    public void RenderOwnedEquipments(IReadOnlyList<InventoryEntryViewModel> equipmentItems)
    {
        RenderEquipmentButtons(equipmentItems);
    }

    public void RenderOwnedConsumables(IReadOnlyList<InventoryEntryViewModel> consumableItems)
    {
        RenderConsumableButtons(consumableItems);
    }

    public void RenderEquippedItems(IReadOnlyList<EquippedInventoryItemViewModel> equippedItems)
    {
        ClearChildren(_playerEquipContainer);

        if (_playerEquipContainer == null || _playerEquipButtonPrefab == null || equippedItems == null)
        {
            return;
        }

        for (int index = 0; index < equippedItems.Count; index++)
        {
            EquippedInventoryItemViewModel equippedItem = equippedItems[index];
            if (equippedItem == null)
            {
                continue;
            }

            UI_PlayerEquipButtonView buttonView = Instantiate(_playerEquipButtonPrefab, _playerEquipContainer);
            buttonView.Bind(equippedItem);
            buttonView.Selected += HandleEquippedItemSelected;
            buttonView.UnequipClicked += HandleEquippedItemUnequipClicked;
        }
    }

    public void ShowSelectedItem(InventoryEntryViewModel selectedItem)
    {
        if (selectedItem == null)
        {
            ClearSelection();
            return;
        }

        if (_selectItemImage != null)
        {
            _selectItemImage.sprite = selectedItem.Icon;
            _selectItemImage.enabled = selectedItem.Icon != null;
        }

        if (_selectItemNameText != null)
        {
            _selectItemNameText.text = selectedItem.DisplayName;
        }

        if (_selectItemStatusText != null)
        {
            _selectItemStatusText.text = selectedItem.StatusText;
        }

        if (_selectItemCostText != null)
        {
            _selectItemCostText.text = selectedItem.ShowCost
                ? selectedItem.CostText
                : string.Empty;
        }

        if (_selectItemCountText != null)
        {
            _selectItemCountText.text = selectedItem.ShowCount
                ? selectedItem.CountText
                : string.Empty;
        }

        if (_selectItemDescriptionText != null)
        {
            _selectItemDescriptionText.text = selectedItem.Description;
        }
    }

    public void ClearSelection()
    {
        if (_selectItemImage != null)
        {
            _selectItemImage.sprite = null;
            _selectItemImage.enabled = false;
        }

        if (_selectItemNameText != null)
        {
            _selectItemNameText.text = string.Empty;
        }

        if (_selectItemStatusText != null)
        {
            _selectItemStatusText.text = string.Empty;
        }

        if (_selectItemCostText != null)
        {
            _selectItemCostText.text = string.Empty;
        }

        if (_selectItemCountText != null)
        {
            _selectItemCountText.text = string.Empty;
        }

        if (_selectItemDescriptionText != null)
        {
            _selectItemDescriptionText.text = string.Empty;
        }
    }

    public void SetEquipButtonState(bool isInteractable, string buttonText)
    {
        if (_equipButton != null)
        {
            _equipButton.interactable = isInteractable;
        }

        if (_equipButtonText != null)
        {
            _equipButtonText.text = buttonText ?? string.Empty;
        }
    }

    public void SetUseButtonState(bool isInteractable, string buttonText)
    {
        if (_useButton != null)
        {
            _useButton.interactable = isInteractable;
        }

        if (_useButtonText != null)
        {
            _useButtonText.text = buttonText ?? string.Empty;
        }
    }

    public void RefreshEquipmentCost(int currentCost, int maxCost)
    {
        int clampedMaxCost = Mathf.Clamp(maxCost, 0, PlayerProfileData.MaxEquipmentCostLimit);
        int clampedCurrentCost = Mathf.Clamp(currentCost, 0, clampedMaxCost);

        if (_currentCostText != null)
        {
            _currentCostText.text = LobbyUiText.CostProgress(clampedCurrentCost, clampedMaxCost);
        }

        for (int index = 0; index < _costImageViews.Count; index++)
        {
            UI_InventoryCostImageView costImageView = _costImageViews[index];
            if (costImageView == null)
            {
                continue;
            }

            costImageView.SetActive(index < clampedCurrentCost);
        }
    }

    public void RefreshSelectItemDesc(string text)
    {
        if (_selectItemDescriptionText != null)
        {
            _selectItemDescriptionText.text = text;
        }
    }

    private void RenderEquipmentButtons(IReadOnlyList<InventoryEntryViewModel> equipmentItems)
    {
        ClearChildren(_equipContainer);

        if (_equipContainer == null || _inventoryEquipButtonPrefab == null || equipmentItems == null)
        {
            return;
        }

        for (int index = 0; index < equipmentItems.Count; index++)
        {
            InventoryEntryViewModel equipmentItem = equipmentItems[index];
            if (equipmentItem == null)
            {
                continue;
            }

            UI_InventoryEquipButtonView buttonView = Instantiate(_inventoryEquipButtonPrefab, _equipContainer);
            buttonView.Bind(equipmentItem);
            buttonView.Clicked += HandleInventoryEntrySelected;
        }
    }

    private void RenderConsumableButtons(IReadOnlyList<InventoryEntryViewModel> consumableItems)
    {
        ClearChildren(_itemContainer);

        if (_itemContainer == null || _inventoryItemButtonPrefab == null || consumableItems == null)
        {
            return;
        }

        for (int index = 0; index < consumableItems.Count; index++)
        {
            InventoryEntryViewModel consumableItem = consumableItems[index];
            if (consumableItem == null)
            {
                continue;
            }

            UI_InventoryItemButtonView buttonView = Instantiate(_inventoryItemButtonPrefab, _itemContainer);
            buttonView.Bind(consumableItem);
            buttonView.Clicked += HandleInventoryEntrySelected;
        }
    }

    private void CacheButtonTexts()
    {
        _equipButtonText ??= _equipButton != null
            ? _equipButton.GetComponentInChildren<TMP_Text>(true)
            : null;

        _useButtonText ??= _useButton != null
            ? _useButton.GetComponentInChildren<TMP_Text>(true)
            : null;
    }

    private void BindButton(Button button, UnityEngine.Events.UnityAction onClick)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    private void HandleInventoryEntrySelected(string itemId)
    {
        InventoryEntrySelected?.Invoke(itemId);
    }

    private void HandleEquippedItemSelected(string equipmentId)
    {
        EquippedItemSelected?.Invoke(equipmentId);
    }

    private void HandleEquippedItemUnequipClicked(string equipmentId)
    {
        EquippedItemUnequipClicked?.Invoke(equipmentId);
    }

    private void HandleEquipButtonClicked()
    {
        EquipClicked?.Invoke();
    }

    private void HandleUseButtonClicked()
    {
        UseClicked?.Invoke();
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int index = parent.childCount - 1; index >= 0; index--)
        {
            Destroy(parent.GetChild(index).gameObject);
        }
    }
}
