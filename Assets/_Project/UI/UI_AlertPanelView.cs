using UnityEngine;
using TMPro;

public class UI_AlertPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text _alertText;

    public void SetText(string text)
    {
        _alertText.text = text;
    }
}
