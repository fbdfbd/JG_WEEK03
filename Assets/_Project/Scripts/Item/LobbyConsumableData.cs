using System;
using UnityEngine;

[Serializable]
public sealed class LobbyConsumableData
{
    [SerializeField] private string _consumableId;
    [SerializeField] private string _displayName;
    [TextArea(2, 5)]
    [SerializeField] private string _description;
    [SerializeField] private int _price;
    [SerializeField] private Sprite _icon;
    [SerializeField] private LobbyConsumableType _consumableType;

    public string ConsumableId => _consumableId;
    public string DisplayName => _displayName;
    public string Description => _description;
    public int Price => _price;
    public Sprite Icon => _icon;
    public LobbyConsumableType ConsumableType => _consumableType;

    public static LobbyConsumableData Create(
        string consumableId,
        string displayName,
        string description,
        int price,
        LobbyConsumableType consumableType)
    {
        return new LobbyConsumableData
        {
            _consumableId = consumableId,
            _displayName = displayName,
            _description = description,
            _price = Mathf.Max(0, price),
            _consumableType = consumableType
        };
    }
}
