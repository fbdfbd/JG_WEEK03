using UnityEngine;

public sealed class TerritoryEventActionService
{
    public TerritoryEventSelectionSession CreateSession(
        TerritoryPrimaryActionRequest actionRequest,
        SOTerritoryEventCatalog eventCatalog)
    {
        if (!actionRequest.IsValid
            || !actionRequest.IsEventAction
            || eventCatalog == null
            || !eventCatalog.HasDefinitions)
        {
            return null;
        }

        SOTerritoryEventDefinition eventDefinition = eventCatalog.ResolveDefinition(actionRequest.TileId, actionRequest.Day);
        return eventDefinition != null
            ? new TerritoryEventSelectionSession(actionRequest, eventDefinition)
            : null;
    }

    public TerritoryEventActionResult ExecuteOption(
        TerritoryEventSelectionSession session,
        int optionIndex,
        TerritoryMapService mapService,
        PlayerProfileService playerProfileService,
        TerritoryRunState runState,
        TerritoryStageSceneSettings stageSceneSettings,
        TerritoryStageEntryContextFactory stageEntryContextFactory)
    {
        if (session == null || session.EventDefinition == null)
        {
            return null;
        }

        TerritoryEventOptionDefinition option = session.EventDefinition.GetOption(optionIndex);
        if (option == null)
        {
            return null;
        }

        ApplyImmediateRewards(option, playerProfileService);
        ApplyImmediateCapture(option, session.ActionRequest.TileId, mapService);

        if (!option.StartBattle)
        {
            return new TerritoryEventActionResult();
        }

        if (mapService == null
            || runState == null
            || stageSceneSettings == null
            || stageEntryContextFactory == null
            || !mapService.TryGetTileState(session.ActionRequest.TileId, out TerritoryTileState tileState))
        {
            return null;
        }

        TerritoryStageType stageType = option.OverrideBattleStageType
            ? option.BattleStageTypeOverride
            : session.ActionRequest.StageType;

        TerritoryStageRequest stageRequest = new TerritoryStageRequest(
            session.ActionRequest.TileId,
            session.ActionRequest.Day,
            stageType,
            session.ActionRequest.PrimaryActionType,
            session.ActionRequest.TargetOwnerType,
            session.ActionRequest.ContentType);

        StageEntryContext stageEntryContext = stageEntryContextFactory.Create(
            stageRequest,
            tileState,
            runState,
            stageSceneSettings);

        if (stageEntryContext == null)
        {
            return null;
        }

        stageEntryContext.SourceEventId = session.EventDefinition.EventId;
        stageEntryContext.SourceEventTitle = session.EventDefinition.Title;
        stageEntryContext.RewardGoldMultiplier = Mathf.Max(1, option.BattleRewardGoldMultiplier);
        stageEntryContext.BonusRewardItemId = option.BattleRewardConsumableId ?? string.Empty;
        stageEntryContext.BonusRewardItemCount = Mathf.Max(0, option.BattleRewardConsumableCount);

        return new TerritoryEventActionResult(stageRequest, stageEntryContext);
    }

    private static void ApplyImmediateRewards(
        TerritoryEventOptionDefinition option,
        PlayerProfileService playerProfileService)
    {
        if (option == null || playerProfileService == null)
        {
            return;
        }

        if (option.GainGold > 0)
        {
            playerProfileService.AddGold(option.GainGold);
        }

        if (!string.IsNullOrWhiteSpace(option.RewardConsumableId))
        {
            playerProfileService.AddConsumable(
                option.RewardConsumableId,
                Mathf.Max(1, option.RewardConsumableCount));
        }
    }

    private static void ApplyImmediateCapture(
        TerritoryEventOptionDefinition option,
        int tileId,
        TerritoryMapService mapService)
    {
        if (option == null || !option.CaptureTileImmediately || mapService == null)
        {
            return;
        }

        mapService.ClearTilePendingCapture(tileId);
        mapService.SetTileOwner(tileId, TerritoryTileOwnerType.Player);
    }
}
