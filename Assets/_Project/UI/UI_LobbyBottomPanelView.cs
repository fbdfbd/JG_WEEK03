using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UI_LobbyBottomPanelView : MonoBehaviour
{
    [SerializeField] private RectTransform _bottomPanel;

    public RectTransform BottomPanel => _bottomPanel;

}
