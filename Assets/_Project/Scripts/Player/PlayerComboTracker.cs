using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerComboTracker : MonoBehaviour
{
    [Header("Combo")]
    [SerializeField] private float comboResetDelay = 2f;
    [SerializeField] private int currentCombo;
    [SerializeField] private int highestCombo;

    private float lastConfirmedHitTime = -1f;

    public int CurrentCombo => currentCombo;
    public int HighestCombo => highestCombo;

    public event Action<int> ComboChanged;

    private void Update()
    {
        if (currentCombo <= 0 || comboResetDelay <= 0f)
        {
            return;
        }

        if (Time.time - lastConfirmedHitTime < comboResetDelay)
        {
            return;
        }

        ResetCombo();
    }

    public void RegisterConfirmedHit()
    {
        currentCombo++;
        highestCombo = Mathf.Max(highestCombo, currentCombo);
        lastConfirmedHitTime = Time.time;
        NotifyComboChanged();
    }

    public void ResetCombo()
    {
        if (currentCombo == 0)
        {
            return;
        }

        currentCombo = 0;
        lastConfirmedHitTime = -1f;
        NotifyComboChanged();
    }

    private void NotifyComboChanged()
    {
        ComboChanged?.Invoke(currentCombo);
    }
}
