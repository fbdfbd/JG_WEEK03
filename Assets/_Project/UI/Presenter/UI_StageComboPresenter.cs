using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_StageComboPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UI_StageComboView comboView;
    [SerializeField] private PlayerComboTracker comboTracker;

    [Header("Options")]
    [SerializeField] private int visibleComboThreshold = 1;
    [SerializeField] private bool hideViewWhenTargetIsMissing = true;

    private bool hasRequiredReferences;

    private void Awake()
    {
        hasRequiredReferences = ValidateReferences();
    }

    private void OnEnable()
    {
        if (!hasRequiredReferences)
        {
            return;
        }

        SubscribeToCombo();
        RefreshView();
    }

    private void OnDisable()
    {
        UnsubscribeFromCombo();
    }

    public void SetTarget(PlayerComboTracker targetComboTracker)
    {
        UnsubscribeFromCombo();
        comboTracker = targetComboTracker;
        SubscribeToCombo();
        RefreshView();
    }

    public void RefreshNow()
    {
        RefreshView();
    }

    private bool ValidateReferences()
    {
        if (comboView != null)
        {
            return true;
        }

        Debug.LogError($"{nameof(UI_StageComboPresenter)} requires a {nameof(UI_StageComboView)} reference.", this);
        return false;
    }

    private void SubscribeToCombo()
    {
        if (comboTracker == null)
        {
            return;
        }

        comboTracker.ComboChanged -= HandleComboChanged;
        comboTracker.ComboChanged += HandleComboChanged;
    }

    private void UnsubscribeFromCombo()
    {
        if (comboTracker == null)
        {
            return;
        }

        comboTracker.ComboChanged -= HandleComboChanged;
    }

    private void RefreshView()
    {
        if (comboView == null)
        {
            return;
        }

        if (comboTracker == null)
        {
            if (hideViewWhenTargetIsMissing)
            {
                comboView.HideCombo();
            }

            return;
        }

        int currentCombo = comboTracker.CurrentCombo;
        if (currentCombo < visibleComboThreshold)
        {
            comboView.HideCombo();
            return;
        }

        comboView.ShowCombo(currentCombo);
    }

    private void HandleComboChanged(int comboCount)
    {
        if (comboView == null)
        {
            return;
        }

        if (comboCount < visibleComboThreshold)
        {
            comboView.HideCombo();
            return;
        }

        comboView.ShowCombo(comboCount);
    }
}
