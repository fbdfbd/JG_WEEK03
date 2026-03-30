using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FinalDayTerritoryCountPresentationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapController territoryMapController;
    [SerializeField] private TerritoryIncomePopupController popupController;

    [Header("Enemy Count")]
    [SerializeField] private Color enemyPopupColor = new Color(1f, 0.55f, 0.45f, 1f);

    [Header("Player Count")]
    [SerializeField] private Color playerPopupColor = new Color(0.52f, 0.84f, 1f, 1f);

    [Header("Motion")]
    [SerializeField] private float popupInterval = 0.08f;
    [SerializeField] private float sideTransitionPause = 0.25f;
    [SerializeField] private float finishPause = 0.35f;
    [SerializeField] private float pulseScale = 1.16f;
    [SerializeField] private float pulseDuration = 0.1f;

    private void Awake()
    {
        ResolveReferences();
    }

    public IEnumerator Play(FinalDayTerritoryCountResult countResult, CampaignOutcomeResult outcomeResult)
    {
        if (countResult == null || !outcomeResult.IsFinished)
        {
            yield break;
        }

        ResolveReferences();

        yield return PlaySideCountSequence(countResult.EnemyTileIds, enemyPopupColor);

        if (countResult.EnemyTileCount > 0 && countResult.PlayerTileCount > 0)
        {
            yield return new WaitForSeconds(sideTransitionPause);
        }

        yield return PlaySideCountSequence(countResult.PlayerTileIds, playerPopupColor);
        yield return new WaitForSeconds(finishPause);
    }

    private IEnumerator PlaySideCountSequence(IReadOnlyList<int> tileIds, Color popupColor)
    {
        if (tileIds == null || tileIds.Count == 0 || popupController == null)
        {
            yield break;
        }

        for (int i = 0; i < tileIds.Count; i++)
        {
            int tileId = tileIds[i];
            if (territoryMapController != null
                && territoryMapController.TryGetTileView(tileId, out TerritoryTileView tileView)
                && tileView != null)
            {
                tileView.PlayTurnPulseFx(pulseScale, pulseDuration);
            }

            popupController.ShowPopup(new TerritoryPopupEntry(
                tileId,
                $"+{i + 1}",
                popupColor));

            if (i < tileIds.Count - 1)
            {
                yield return new WaitForSeconds(popupInterval);
            }
        }
    }

    private void ResolveReferences()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>(FindObjectsInactive.Include);
        popupController ??= GetComponent<TerritoryIncomePopupController>();
        popupController ??= FindFirstObjectByType<TerritoryIncomePopupController>(FindObjectsInactive.Include);
    }
}
