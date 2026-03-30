using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InventoryEquipButtonView : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Button _itemButton;

    public event Action<string> Clicked;

    public void Bind(InventoryEntryViewModel itemViewModel)
    {
        if (itemViewModel == null)
        {
            Clear();
            return;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = itemViewModel.DisplayName;
        }

        if (_itemImage != null)
        {
            if (itemViewModel.Icon != null)
            {
                _itemImage.sprite = itemViewModel.Icon;
                _itemImage.color = Color.white;
                _itemImage.enabled = true;
            }
            else
            {
                _itemImage.sprite = null;
                _itemImage.enabled = false;
            }
        }

        if (_itemButton != null)
        {
            _itemButton.onClick.RemoveAllListeners();
            _itemButton.onClick.AddListener(() => Clicked?.Invoke(itemViewModel.ItemId));
            _itemButton.interactable = true;
        }
    }

    private void Clear()
    {
        if (_itemNameText != null)
        {
            _itemNameText.text = string.Empty;
        }

        if (_itemImage != null)
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
        }

        if (_itemButton != null)
        {
            _itemButton.onClick.RemoveAllListeners();
            _itemButton.interactable = false;
        }
    }
}
