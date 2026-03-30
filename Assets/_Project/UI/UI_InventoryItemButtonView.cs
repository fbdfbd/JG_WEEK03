using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_InventoryItemButtonView : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemCountText;
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

        if (_itemCountText != null)
        {
            _itemCountText.text = itemViewModel.ShowCount
                ? itemViewModel.CountText
                : string.Empty;
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

        if (_itemCountText != null)
        {
            _itemCountText.text = string.Empty;
        }

        if (_itemButton != null)
        {
            _itemButton.onClick.RemoveAllListeners();
            _itemButton.interactable = false;
        }
    }
}
