using UnityEngine;

public sealed class StageManager : MonoBehaviour
{
    [SerializeField] private CampaignRunService campaignRunService;
    [SerializeField] private StageFlowService stageFlowService;
    [SerializeField] private SOStageRewardSettings rewardSettings;

    private readonly TerritoryStageRewardResolver _rewardResolver = new TerritoryStageRewardResolver();

    private void Awake()
    {
        ResolveReferences();
    }

    public bool TryCompleteCurrentStage(
        bool wasSuccess,
        int rewardGold = -1,
        string rewardItemId = "",
        int rewardItemCount = 0)
    {
        ResolveReferences();

        if (campaignRunService == null
            || stageFlowService == null
            || !campaignRunService.TryGetPendingStageEntry(out StageEntryContext stageEntryContext)
            || stageEntryContext == null)
        {
            return stageFlowService != null && stageFlowService.LoadLobbyScene();
        }

        int resolvedRewardGold = rewardGold >= 0
            ? rewardGold
            : _rewardResolver.ResolveGoldReward(stageEntryContext, wasSuccess, rewardSettings);

        StageResult stageResult = new StageResult
        {
            RunId = stageEntryContext.RunId,
            TileId = stageEntryContext.TileId,
            WasSuccess = wasSuccess,
            StageType = stageEntryContext.StageType,
            PrimaryActionType = stageEntryContext.PrimaryActionType,
            RewardGold = Mathf.Max(0, resolvedRewardGold),
            RewardItemId = rewardItemId ?? string.Empty,
            RewardItemCount = Mathf.Max(0, rewardItemCount),
            ClearedDay = stageEntryContext.Day
        };

        campaignRunService.SetPendingStageResult(stageResult);
        return stageFlowService.LoadLobbyScene();
    }

    private void ResolveReferences()
    {
        if (GameManager.I == null)
        {
            return;
        }

        campaignRunService ??= GameManager.I.CampaignRunService;
        stageFlowService ??= GameManager.I.StageFlowService;
    }
}
