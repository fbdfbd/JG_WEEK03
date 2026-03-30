using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryEnemyExpansionFxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapController territoryMapController;

    [Header("Motion")]
    [SerializeField] private float sourcePulseScale = 1.08f;
    [SerializeField] private float targetPulseScale = 1.18f;
    [SerializeField] private float strongTargetPulseScale = 1.24f;
    [SerializeField] private float pulseDuration = 0.15f;
    [SerializeField] private float eventInterval = 0.1f;

    private void Awake()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>(FindObjectsInactive.Include);
    }

    public IEnumerator Play(IReadOnlyList<TerritoryEnemyExpansionEvent> expansionEvents)
    {
        if (expansionEvents == null || expansionEvents.Count == 0)
        {
            yield break;
        }

        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>(FindObjectsInactive.Include);

        for (int index = 0; index < expansionEvents.Count; index++)
        {
            PlayExpansionEvent(expansionEvents[index]);
            yield return new WaitForSeconds(eventInterval);
        }

        yield return new WaitForSeconds(pulseDuration * 2f);
    }

    private void PlayExpansionEvent(TerritoryEnemyExpansionEvent expansionEvent)
    {
        if (territoryMapController == null)
        {
            return;
        }

        if (expansionEvent.SourceTileId.HasValue
            && territoryMapController.TryGetTileView(expansionEvent.SourceTileId.Value, out TerritoryTileView sourceTileView))
        {
            sourceTileView.PlayTurnPulseFx(sourcePulseScale, pulseDuration);
        }

        if (!territoryMapController.TryGetTileView(expansionEvent.TargetTileId, out TerritoryTileView targetTileView))
        {
            return;
        }

        float targetScale = expansionEvent.EventType == TerritoryEnemyExpansionEventType.OccupiedPlayerTile
            ? strongTargetPulseScale
            : targetPulseScale;

        targetTileView.PlayTurnPulseFx(targetScale, pulseDuration);
    }
}
