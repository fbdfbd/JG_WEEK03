using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_ShopView : MonoBehaviour
{
    [Header("Item List")]
    [SerializeField] private Transform _shopEquipContainer;
    [SerializeField] private Transform _shopItemContainer;
    [SerializeField] private UI_ShopItemView _shopItemPrefab;

    [Header("Selected Item")]
    [SerializeField] private Image _selectItemImage;
    [SerializeField] private TMP_Text _selectItemNameText;
    [SerializeField] private TMP_Text _selectItemPriceText;
    [SerializeField] private TMP_Text _selectItemCostText;
    [SerializeField] private TMP_Text _selectItemStatusText;
    [SerializeField] private TMP_Text _selectItemDescriptionText;

    [Header("Buy Button")]
    [SerializeField] private Button _buyButton;
    [SerializeField] private TMP_Text _buyButtonText;

    private readonly List<UI_ShopItemView> _spawnedItemViews = new List<UI_ShopItemView>();
    private bool _isBound;

    public event Action<string> ItemSelected;
    public event Action BuyClicked;

    private void Awake()
    {
        if (_buyButtonText == null && _buyButton != null)
        {
            _buyButtonText = _buyButton.GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void Bind()
    {
        if (_isBound)
        {
            return;
        }

        if (_buyButton != null)
        {
            _buyButton.onClick.AddListener(HandleBuyButtonClicked);
        }

        _isBound = true;
    }

    public void RenderItems(IReadOnlyList<ShopItemViewModel> items)
    {
        ClearItemViews();

        if (items == null || _shopItemPrefab == null)
        {
            return;
        }

        for (int index = 0; index < items.Count; index++)
        {
            ShopItemViewModel item = items[index];
            if (item == null)
            {
                continue;
            }

            Transform parent = GetTargetContainer(item.Category);
            UI_ShopItemView itemView = Instantiate(_shopItemPrefab, parent);
            itemView.Bind(item);
            itemView.Clicked += HandleItemSelected;
            _spawnedItemViews.Add(itemView);
        }
    }

    public void ShowSelectedItem(ShopItemViewModel item)
    {
        if (item == null)
        {
            ClearSelection();
            return;
        }

        if (_selectItemImage != null)
        {
            _selectItemImage.sprite = item.Icon;
            _selectItemImage.enabled = item.Icon != null;
        }

        if (_selectItemNameText != null)
        {
            _selectItemNameText.text = item.DisplayName;
        }

        if (_selectItemPriceText != null)
        {
            _selectItemPriceText.text = item.PriceText;
        }

        if (_selectItemCostText != null)
        {
            _selectItemCostText.text = item.ShowCost
                ? item.CostText
                : string.Empty;
        }

        if (_selectItemStatusText != null)
        {
            _selectItemStatusText.text = item.StatusText;
        }

        if (_selectItemDescriptionText != null)
        {
            _selectItemDescriptionText.text = item.Description;
        }

        SetBuyButtonState(item.CanBuy, item.BuyButtonText);
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

        if (_selectItemPriceText != null)
        {
            _selectItemPriceText.text = string.Empty;
        }

        if (_selectItemCostText != null)
        {
            _selectItemCostText.text = string.Empty;
        }

        if (_selectItemStatusText != null)
        {
            _selectItemStatusText.text = string.Empty;
        }

        if (_selectItemDescriptionText != null)
        {
            _selectItemDescriptionText.text = string.Empty;
        }

        SetBuyButtonState(false, LobbyUiText.Buy);
    }

    public void SetBuyButtonState(bool canBuy, string buttonText)
    {
        if (_buyButton != null)
        {
            _buyButton.interactable = canBuy;
        }

        if (_buyButtonText != null)
        {
            _buyButtonText.text = string.IsNullOrWhiteSpace(buttonText) ? LobbyUiText.Buy : buttonText;
        }
    }

    private Transform GetTargetContainer(ShopItemCategory category)
    {
        if (category == ShopItemCategory.Consumable && _shopItemContainer != null)
        {
            return _shopItemContainer;
        }

        if (_shopEquipContainer != null)
        {
            return _shopEquipContainer;
        }

        if (_shopItemContainer != null)
        {
            return _shopItemContainer;
        }

        return transform;
    }

    private void ClearItemViews()
    {
        for (int index = 0; index < _spawnedItemViews.Count; index++)
        {
            UI_ShopItemView itemView = _spawnedItemViews[index];
            if (itemView == null)
            {
                continue;
            }

            itemView.Clicked -= HandleItemSelected;
            Destroy(itemView.gameObject);
        }

        _spawnedItemViews.Clear();
    }

    private void HandleItemSelected(string itemId)
    {
        ItemSelected?.Invoke(itemId);
    }

    private void HandleBuyButtonClicked()
    {
        BuyClicked?.Invoke();
    }
}
