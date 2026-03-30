using UnityEngine;

public readonly struct TerritoryPopupEntry
{
    public int TileId { get; }
    public string Text { get; }
    public Color Color { get; }

    public TerritoryPopupEntry(int tileId, string text, Color color)
    {
        TileId = tileId;
        Text = text ?? string.Empty;
        Color = color;
    }
}
