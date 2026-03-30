using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryStageResultApplier : MonoBehaviour
{
    private TerritoryMapService _mapService;

    public void Initialize(TerritoryMapService mapService)
    {
        _mapService = mapService;
    }

    // 스테이지 결과를 영토 상태에 반영한다.
    public bool ApplyResult(TerritoryStageRequest stageRequest, TerritoryStageResult stageResult)
    {
        if (_mapService == null || !_mapService.TryGetTileState(stageRequest.TileId, out TerritoryTileState tileState))
        {
            return false;
        }

        if (stageRequest.IsDefense)
        {
            if (stageResult.IsSuccess)
            {
                _mapService.ClearTilePendingCapture(tileState.TileId);
            }

            return true;
        }

        if (!stageResult.IsSuccess)
        {
            return true;
        }

        _mapService.ClearTilePendingCapture(tileState.TileId);
        _mapService.SetTileOwner(tileState.TileId, TerritoryTileOwnerType.Player);
        return true;
    }
}
