using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class UI_PlayerEquipButtonView : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private Button _equipSelectButton;
    [SerializeField] private Button _unEquipButton;

    public event Action<string> Selected;
    public event Action<string> UnequipClicked;

    public void Bind(EquippedInventoryItemViewModel itemViewModel)
    {
        if (itemViewModel == null)
        {
            Clear();
            return;
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

        if (_itemNameText != null)
        {
            _itemNameText.text = itemViewModel.DisplayName;
        }

        if (_equipSelectButton != null)
        {
            _equipSelectButton.onClick.RemoveAllListeners();
            _equipSelectButton.onClick.AddListener(() => Selected?.Invoke(itemViewModel.EquipmentId));
            _equipSelectButton.interactable = true;
        }

        if (_unEquipButton != null)
        {
            _unEquipButton.onClick.RemoveAllListeners();
            _unEquipButton.onClick.AddListener(() => UnequipClicked?.Invoke(itemViewModel.EquipmentId));
            _unEquipButton.interactable = true;
        }
    }

    private void Clear()
    {
        if (_itemImage != null)
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
        }

        if (_itemNameText != null)
        {
            _itemNameText.text = string.Empty;
        }

        if (_equipSelectButton != null)
        {
            _equipSelectButton.onClick.RemoveAllListeners();
            _equipSelectButton.interactable = false;
        }

        if (_unEquipButton != null)
        {
            _unEquipButton.onClick.RemoveAllListeners();
            _unEquipButton.interactable = false;
        }
    }
}
