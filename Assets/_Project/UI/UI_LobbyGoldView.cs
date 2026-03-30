using TMPro;
using UnityEngine;

public sealed class UI_LobbyGoldView : MonoBehaviour
{
    [SerializeField] private TMP_Text _goldText;

    private void Awake()
    {
        _goldText ??= GetComponentInChildren<TMP_Text>(true);
    }

    public void SetGold(int gold)
    {
        if (_goldText == null)
        {
            return;
        }

        _goldText.text = LobbyUiText.Gold(gold);
    }
}
