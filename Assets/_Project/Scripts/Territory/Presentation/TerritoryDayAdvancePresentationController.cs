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

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        ResolveReferences();
    }

    public IEnumerator Play(TerritoryDayAdvanceResult dayAdvanceResult)
    {
        if (dayAdvanceResult == null || !dayAdvanceResult.HasPresentationWork)
        {
            yield break;
        }

        ResolveReferences();
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

    private void ResolveReferences()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>(FindObjectsInactive.Include);
        territoryTurnView ??= FindFirstObjectByType<UI_TerritoryTurnView>(FindObjectsInactive.Include);
        territoryTurnFxController ??= FindFirstObjectByType<TerritoryTurnFxController>(FindObjectsInactive.Include);
        panelTransitionController ??= GetComponent<TerritoryPanelTransitionController>();
        incomePopupController ??= GetComponent<TerritoryIncomePopupController>();
        enemyExpansionFxController ??= GetComponent<TerritoryEnemyExpansionFxController>();
        panelTransitionController ??= FindFirstObjectByType<TerritoryPanelTransitionController>(FindObjectsInactive.Include);
        incomePopupController ??= FindFirstObjectByType<TerritoryIncomePopupController>(FindObjectsInactive.Include);
        enemyExpansionFxController ??= FindFirstObjectByType<TerritoryEnemyExpansionFxController>(FindObjectsInactive.Include);
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
