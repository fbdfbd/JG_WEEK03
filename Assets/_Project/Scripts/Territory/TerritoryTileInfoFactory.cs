public static class TerritoryTileInfoFactory
{
    // 패널은 규칙을 몰라도 되도록 표시용 문구만 조립한다.
    public static TerritoryTileInfoViewModel Create(TerritoryTileState tileState, TerritoryTurnState turnState)
    {
        string title = $"타일 {tileState.TileId}";
        string ownerText = ResolveOwnerText(tileState);
        string contentText = ResolveContentText(tileState);
        string statusText = ResolveStatusText(tileState);
        string hintText = ResolveHintText(tileState);
        string primaryActionLabel = ResolvePrimaryActionLabel(tileState);

        return new TerritoryTileInfoViewModel(
            tileState.TileId,
            turnState.CurrentDay,
            turnState.RemainingAttempts,
            title,
            ownerText,
            contentText,
            statusText,
            hintText,
            primaryActionLabel,
            tileState.PrimaryActionType != TerritoryPrimaryActionType.None);
    }

    private static string ResolveOwnerText(TerritoryTileState tileState)
    {
        return tileState.Owner switch
        {
            TerritoryTileOwnerType.Player => "플레이어 영토",
            TerritoryTileOwnerType.Enemy => "적 영토",
            _ => "중립 영토"
        };
    }

    private static string ResolveContentText(TerritoryTileState tileState)
    {
        if (tileState.Owner == TerritoryTileOwnerType.Player)
        {
            return "플레이어 점령지";
        }

        return tileState.ContentType == TerritoryTileContentType.Event
            ? "사건 타일"
            : "보스 타일";
    }

    private static string ResolveStatusText(TerritoryTileState tileState)
    {
        if (tileState.PendingCaptureState == TerritoryPendingCaptureState.EnemyPlanned)
        {
            return "적 점령 예정";
        }

        if (tileState.IsPlayerStart)
        {
            return "플레이어 시작지";
        }

        if (tileState.IsEnemyStart)
        {
            return "적 시작지";
        }

        return "일반 상태";
    }

    private static string ResolvePrimaryActionLabel(TerritoryTileState tileState)
    {
        return tileState.PrimaryActionType switch
        {
            TerritoryPrimaryActionType.Challenge => tileState.ContentType == TerritoryTileContentType.Event ? "사건 진입" : "도전하기",
            TerritoryPrimaryActionType.Attack => "공격하기",
            TerritoryPrimaryActionType.Defend => "방어하기",
            _ => string.Empty
        };
    }

    private static string ResolveHintText(TerritoryTileState tileState)
    {
        return tileState.PrimaryActionType switch
        {
            TerritoryPrimaryActionType.Challenge => "인접한 중립 지역입니다. 도전에 성공하면 플레이어 영토가 됩니다.",
            TerritoryPrimaryActionType.Attack => "인접한 적 영토입니다. 전투에서 이기면 해당 지역을 점령합니다.",
            TerritoryPrimaryActionType.Defend => "적의 점령이 예고된 지역입니다. 이번 턴 안에 방어해야 합니다.",
            _ => tileState.Owner == TerritoryTileOwnerType.Player
                ? "이미 확보한 지역입니다."
                : "현재는 진입 조건을 만족하지 않습니다."
        };
    }
}
