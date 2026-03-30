using UnityEngine;

[CreateAssetMenu(fileName = "SOConsumableData", menuName = "Scriptable Objects/Consumable Data")]
public sealed class SOConsumableData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string _consumableId;
    [SerializeField] private LobbyConsumableType _consumableType;
    [SerializeField] private LobbyConsumableTargetType _targetType;
    [SerializeField] private string _displayName;

    [Header("Shop")]
    [TextArea(2, 5)]
    [SerializeField] private string _description;
    [SerializeField] private int _price;

    [Header("Visual")]
    [SerializeField] private Sprite _icon;

    public string ConsumableId => _consumableId;
    public LobbyConsumableType ConsumableType => _consumableType;
    public LobbyConsumableTargetType TargetType => _targetType;
    public string DisplayName => _displayName;
    public string Description => _description;
    public int Price => _price;
    public Sprite Icon => _icon;

    public void SetData(
        string consumableId,
        LobbyConsumableType consumableType,
        LobbyConsumableTargetType targetType,
        string displayName,
        string description,
        int price,
        Sprite icon)
    {
        _consumableId = consumableId;
        _consumableType = consumableType;
        _targetType = targetType;
        _displayName = displayName;
        _description = description;
        _price = Mathf.Max(0, price);
        _icon = icon;
    }
}
