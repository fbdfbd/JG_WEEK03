using TMPro;
using UnityEngine;

public class UI_ToastPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text _toastText;

    public void SetText(string text)
    {
        _toastText.text = text;
    }
}
