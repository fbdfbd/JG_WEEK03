using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class LobbyConsumableCatalog
{
    [SerializeField] private List<LobbyConsumableData> _items = CreateDefaultItems();

    public IReadOnlyList<LobbyConsumableData> Items => _items;

    public bool TryGetConsumable(string consumableId, out LobbyConsumableData consumableData)
    {
        consumableData = null;

        if (string.IsNullOrWhiteSpace(consumableId) || _items == null)
        {
            return false;
        }

        for (int index = 0; index < _items.Count; index++)
        {
            LobbyConsumableData currentItem = _items[index];
            if (currentItem == null)
            {
                continue;
            }

            if (!string.Equals(currentItem.ConsumableId, consumableId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            consumableData = currentItem;
            return true;
        }

        return false;
    }

    public static LobbyConsumableCatalog CreateDefault()
    {
        return new LobbyConsumableCatalog
        {
            _items = CreateDefaultItems()
        };
    }

    private static List<LobbyConsumableData> CreateDefaultItems()
    {
        return new List<LobbyConsumableData>
    {
        LobbyConsumableData.Create(
            "consumable_stamina_recover",
            "스태미나 드링크",
            "로비 스태미나를 즉시 1 회복합니다.",
            40,
            LobbyConsumableType.StaminaRecover),
        LobbyConsumableData.Create(
            "consumable_emergency_defense",
            "긴급 방어",
            "계획한 점령 타일 1개를 1일 동안 보호합니다.",
            80,
            LobbyConsumableType.EmergencyDefense)
    };
    }
}
