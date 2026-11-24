using System.Collections;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryDayAdvancePresentationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapController territoryMapController;
    [SerializeField] private UI_TerritoryTurnView territoryTurnView;
    [SerializeField] private TerritoryTurnFxController territoryTurnFxController;
    [SerializeField] private TerritoryPanelTransitionController panelTransitionController;
    [SerializeField] private TerritoryIncomePopupController incomePopupController;
    [SerializeField] private TerritoryEnemyExpansionFxController enemyExpansionFxController;

    private bool _hasRequiredReferences;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        _hasRequiredReferences = ValidateReferences();
    }

    public IEnumerator Play(TerritoryDayAdvanceResult dayAdvanceResult)
    {
        if (dayAdvanceResult == null || !dayAdvanceResult.HasPresentationWork)
        {
            yield break;
        }

        if (!_hasRequiredReferences)
        {
            yield break;
        }

        IsPlaying = true;
        SetInteractionEnabled(false);

        yield return PlayTween(panelTransitionController != null ? panelTransitionController.PlayHide() : null);

        if (incomePopupController != null)
        {
            yield return incomePopupController.Play(dayAdvanceResult.IncomeResult);
        }

        if (enemyExpansionFxController != null)
        {
            yield return enemyExpansionFxController.Play(dayAdvanceResult.EnemyExpansionEvents);
        }

        if (territoryTurnFxController != null)
        {
            float playbackDuration = territoryTurnFxController.GetPlaybackDuration(dayAdvanceResult);
            territoryTurnFxController.Play(dayAdvanceResult);

            if (playbackDuration > 0f)
            {
                yield return new WaitForSeconds(playbackDuration);
            }
        }

        yield return PlayTween(panelTransitionController != null ? panelTransitionController.PlayShow() : null);

        SetInteractionEnabled(true);
        if (territoryMapController != null && territoryMapController.TurnService != null)
        {
            territoryTurnView?.Refresh(territoryMapController.TurnService.CurrentState);
        }

        IsPlaying = false;
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        isValid &= ValidateReference(territoryMapController, nameof(territoryMapController));
        isValid &= ValidateReference(territoryTurnView, nameof(territoryTurnView));
        isValid &= ValidateReference(territoryTurnFxController, nameof(territoryTurnFxController));
        isValid &= ValidateReference(panelTransitionController, nameof(panelTransitionController));
        isValid &= ValidateReference(incomePopupController, nameof(incomePopupController));
        isValid &= ValidateReference(enemyExpansionFxController, nameof(enemyExpansionFxController));

        return isValid;
    }

    private bool ValidateReference(UnityEngine.Object reference, string fieldName)
    {
        if (reference != null)
        {
            return true;
        }

        Debug.LogError($"{nameof(TerritoryDayAdvancePresentationController)} requires '{fieldName}' to be assigned.", this);
        return false;
    }

    private void SetInteractionEnabled(bool isEnabled)
    {
        territoryTurnView?.SetNextDayButtonInteractable(isEnabled);
        territoryMapController?.MapPointerInput?.SetInputEnabled(isEnabled);
    }

    private static IEnumerator PlayTween(Tween tween)
    {
        if (tween == null)
        {
            yield break;
        }

        while (tween.IsActive() && tween.IsPlaying())
        {
            yield return null;
        }
    }
}
