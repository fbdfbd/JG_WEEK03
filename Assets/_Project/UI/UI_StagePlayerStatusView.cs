using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class UI_StagePlayerStatusView : MonoBehaviour
{
    [SerializeField] private GameObject _playerStatusPanel;

    [SerializeField] private TMP_Text _playerHPSliderValue;
    [SerializeField] private TMP_Text _playerSPSliderValue;

    [SerializeField] private Slider _playerHPSlider;
    [SerializeField] private Slider _playerSPSlider;


    public GameObject PlayerStatusPanel => _playerStatusPanel;

    public void Show()
    {
        if (_playerStatusPanel != null)
        {
            _playerStatusPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (_playerStatusPanel != null)
        {
            _playerStatusPanel.SetActive(false);
        }
    }

    public void RenderHealth(int currentValue, int maxValue)
    {
        ApplySlider(_playerHPSlider, _playerHPSliderValue, currentValue, Mathf.Max(1, maxValue));
    }

    public void RenderSpecialGauge(float currentValue, float maxValue)
    {
        float safeMaxValue = Mathf.Max(1f, maxValue);
        ApplySlider(_playerSPSlider, _playerSPSliderValue, currentValue, safeMaxValue);
    }

    public void SetPlayerHPSliderVaule(float value)
    {
        _playerHPSlider.value = value;
        _playerHPSliderValue.text = value.ToString();
    }

    public void SetPlayerSPSliderVaule(float value)
    {
        _playerSPSlider.value = value;
        _playerSPSliderValue.text = value.ToString();
    }

    private void ApplySlider(Slider slider, TMP_Text valueText, float currentValue, float maxValue)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = maxValue;
            slider.value = Mathf.Clamp(currentValue, 0f, maxValue);
        }

        if (valueText != null)
        {
            valueText.text = $"{Mathf.RoundToInt(currentValue)}/{Mathf.RoundToInt(maxValue)}";
        }
    }
}
