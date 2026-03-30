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

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
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

    private void ResolveReferences()
    {
        comboView ??= FindFirstObjectByType<UI_StageComboView>(FindObjectsInactive.Include);
        comboTracker ??= FindFirstObjectByType<PlayerComboTracker>(FindObjectsInactive.Include);
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
        if (comboView == null || comboTracker == null)
        {
            ResolveReferences();
        }

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
            ResolveReferences();
        }

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
