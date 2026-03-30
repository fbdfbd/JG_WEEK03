using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerEquipmentService : MonoBehaviour
{
    public int AdditionalHitFaceCountPerSide { get; private set; }
    public float AdditionalHitDamageMultiplier { get; private set; } = 1f;
    public bool EnableBossWeakPointSpawn { get; private set; }
    public bool EnableHoldToMove { get; private set; }
    public float HoldMoveRepeatIntervalScale { get; private set; } = 1f;
    public bool EnableInputAdjust { get; private set; }
    public bool UsePlayerTint { get; private set; }
    public Color PlayerTintColor { get; private set; } = Color.white;

    public event Action<PlayerVisualState> VisualStateChanged;

    private void Awake()
    {
        ResetFeatures();
    }

    public void ResetFeatures()
    {
        ApplyFeatureLoadout(PlayerFeatureLoadout.Empty);
    }

    public void ApplyFeatureLoadout(PlayerFeatureLoadout featureLoadout)
    {
        AdditionalHitFaceCountPerSide = featureLoadout.AdditionalHitFaceCountPerSide;
        AdditionalHitDamageMultiplier = featureLoadout.AdditionalHitDamageMultiplier > 0f
            ? featureLoadout.AdditionalHitDamageMultiplier
            : 1f;
        EnableBossWeakPointSpawn = featureLoadout.EnableBossWeakPointSpawn;
        EnableHoldToMove = false;
        HoldMoveRepeatIntervalScale = 1f;
        EnableInputAdjust = false;
        UsePlayerTint = featureLoadout.UsePlayerTint;
        PlayerTintColor = featureLoadout.PlayerTintColor;

        NotifyVisualStateChanged();
    }

    public PlayerVisualState GetVisualState()
    {
        return new PlayerVisualState(UsePlayerTint, PlayerTintColor);
    }

    private void NotifyVisualStateChanged()
    {
        VisualStateChanged?.Invoke(GetVisualState());
    }

    private float SanitizeScale(float scale)
    {
        return scale <= 0f ? 1f : scale;
    }
}
