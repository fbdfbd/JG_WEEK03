using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class UI_StageBossStatusView : MonoBehaviour
{
    [SerializeField] private GameObject _bossStatusPanel;
    [SerializeField] private TMP_Text _bossNameText;
    [SerializeField] private TMP_Text _bossHpSliderValueText;
    [SerializeField] private Slider _bossHpSlider;


    public GameObject BossStatusPanel => _bossStatusPanel;

    public void Show()
    {
        if (_bossStatusPanel != null)
        {
            _bossStatusPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (_bossStatusPanel != null)
        {
            _bossStatusPanel.SetActive(false);
        }
    }

    public void SetBossName(string bossName)
    {
        if (_bossNameText != null)
        {
            _bossNameText.text = bossName;
        }
    }

    public void RenderHealth(int currentValue, int maxValue)
    {
        int safeMaxValue = Mathf.Max(1, maxValue);

        if (_bossHpSlider != null)
        {
            _bossHpSlider.minValue = 0f;
            _bossHpSlider.maxValue = safeMaxValue;
            _bossHpSlider.value = Mathf.Clamp(currentValue, 0f, safeMaxValue);
        }

        if (_bossHpSliderValueText != null)
        {
            _bossHpSliderValueText.text = $"{currentValue}/{safeMaxValue}";
        }
    }

    public void SetBossHPSliderVaule(float value)
    {
        _bossHpSlider.value = value;
        _bossHpSliderValueText.text = value.ToString();
    }

    public void Init(string bossName)
    {
        SetBossName(bossName);
    }

}
