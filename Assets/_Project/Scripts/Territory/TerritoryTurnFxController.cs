using System.Collections;
using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryTurnFxController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapController territoryMapController;

    [Header("FX Settings")]
    [SerializeField] private float tilePulseScale = 1.12f;
    [SerializeField] private float tilePulseDuration = 0.18f;
    [SerializeField] private float tilePulseInterval = 0.05f;

    private Coroutine _fxRoutine;

    private void Awake()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>();
    }

    // 다음 날 전환 결과를 받아, 바뀐 타일들을 순서대로 강조한다.
    public void Play(TerritoryDayAdvanceResult dayAdvanceResult)
    {
        if (!dayAdvanceResult.WasSuccessful || dayAdvanceResult.TileChangeSet == null || !dayAdvanceResult.TileChangeSet.HasAnyChange)
        {
            return;
        }

        if (_fxRoutine != null)
        {
            StopCoroutine(_fxRoutine);
        }

        _fxRoutine = StartCoroutine(PlayRoutine(dayAdvanceResult.TileChangeSet));
    }

    public float GetPlaybackDuration(TerritoryDayAdvanceResult dayAdvanceResult)
    {
        if (dayAdvanceResult == null || !dayAdvanceResult.WasSuccessful || dayAdvanceResult.TileChangeSet == null)
        {
            return 0f;
        }

        int tileCount =
            dayAdvanceResult.TileChangeSet.ResolvedPendingCaptureTileIds.Count
            + dayAdvanceResult.TileChangeSet.NewPendingCaptureTileIds.Count
            + dayAdvanceResult.TileChangeSet.EnemyOccupiedTileIds.Count
            + dayAdvanceResult.TileChangeSet.PlayerOccupiedTileIds.Count;

        if (tileCount <= 0)
        {
            return 0f;
        }

        return (tilePulseDuration * 2f) + (tilePulseInterval * Mathf.Max(0, tileCount - 1)) + 0.02f;
    }

    private IEnumerator PlayRoutine(TerritoryTileChangeSet tileChangeSet)
    {
        yield return PlayTileList(tileChangeSet.ResolvedPendingCaptureTileIds);
        yield return PlayTileList(tileChangeSet.NewPendingCaptureTileIds);
        yield return PlayTileList(tileChangeSet.EnemyOccupiedTileIds);
        yield return PlayTileList(tileChangeSet.PlayerOccupiedTileIds);

        _fxRoutine = null;
    }

    private IEnumerator PlayTileList(System.Collections.Generic.IReadOnlyList<int> tileIds)
    {
        if (tileIds == null || territoryMapController == null)
        {
            yield break;
        }

        for (int index = 0; index < tileIds.Count; index++)
        {
            int tileId = tileIds[index];
            if (territoryMapController.TryGetTileView(tileId, out TerritoryTileView tileView))
            {
                tileView.PlayTurnPulseFx(tilePulseScale, tilePulseDuration);
                yield return new WaitForSeconds(tilePulseInterval);
            }
        }
    }
}
