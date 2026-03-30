public sealed class TerritoryStageEntryContextFactory
{
    // 로비의 타일 선택 정보를 스테이지 씬이 바로 이해할 수 있는 값 데이터로 변환한다.
    public StageEntryContext Create(
        TerritoryStageRequest stageRequest,
        TerritoryTileState tileState,
        TerritoryRunState runState,
        TerritoryStageSceneSettings sceneSettings)
    {
        if (tileState == null || runState == null || sceneSettings == null)
        {
            return null;
        }

        return new StageEntryContext
        {
            RunId = runState.RunId,
            TileId = stageRequest.TileId,
            Day = stageRequest.Day,
            StageDefinitionId = sceneSettings.GetStageDefinitionId(stageRequest.StageType),
            SceneName = sceneSettings.GetSceneName(stageRequest.StageType),
            StageType = stageRequest.StageType,
            PrimaryActionType = stageRequest.PrimaryActionType,
            TileOwnerType = tileState.Owner,
            TileContentType = tileState.ContentType,
            IsDefense = stageRequest.IsDefense,
            IsEnemyStartTile = tileState.IsEnemyStart,
        };
    }
}
