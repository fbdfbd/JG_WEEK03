using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_ShopItemView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Button _itemButton;

    private string _itemId;

    public event Action<string> Clicked;

    private void Awake()
    {
        if (_itemButton != null)
        {
            _itemButton.onClick.AddListener(HandleItemClicked);
        }
    }

    public void Bind(ShopItemViewModel item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        _itemId = item.ItemId;

        if (_itemNameText != null)
        {
            _itemNameText.text = item.DisplayName;
        }

        if (_priceText != null)
        {
            _priceText.text = item.PriceText;
        }

        if (_stateText != null)
        {
            _stateText.text = item.StatusText;
        }

        if (_itemImage != null)
        {
            _itemImage.sprite = item.Icon;
            _itemImage.enabled = item.Icon != null;
        }

        if (_itemButton != null)
        {
            _itemButton.interactable = true;
        }
    }

    private void Clear()
    {
        _itemId = string.Empty;

        if (_itemNameText != null)
        {
            _itemNameText.text = string.Empty;
        }

        if (_priceText != null)
        {
            _priceText.text = string.Empty;
        }

        if (_stateText != null)
        {
            _stateText.text = string.Empty;
        }

        if (_itemImage != null)
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
        }

        if (_itemButton != null)
        {
            _itemButton.interactable = false;
        }
    }

    private void HandleItemClicked()
    {
        if (string.IsNullOrWhiteSpace(_itemId))
        {
            return;
        }

        Clicked?.Invoke(_itemId);
    }
}
