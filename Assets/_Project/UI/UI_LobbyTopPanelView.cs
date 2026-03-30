using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_LobbyTopPanelView : MonoBehaviour
{
    [SerializeField] private RectTransform _topPanel;

    public RectTransform TopPanel => _topPanel;
}
