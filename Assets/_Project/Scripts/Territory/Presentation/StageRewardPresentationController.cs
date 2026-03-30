using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageRewardPresentationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryIncomePopupController popupController;

    [Header("View")]
    [SerializeField] private Color rewardPopupColor = new Color(1f, 0.86f, 0.24f, 1f);

    private void Awake()
    {
        ResolveReferences();
    }

    public IEnumerator Play(TerritoryStageRewardApplicationResult rewardResult)
    {
        if (!rewardResult.HasGoldReward)
        {
            yield break;
        }

        ResolveReferences();
        if (popupController == null)
        {
            yield break;
        }

        yield return popupController.PlayPopup(new TerritoryPopupEntry(
            rewardResult.SourceTileId,
            $"+{rewardResult.AppliedGold}G",
            rewardPopupColor));
    }

    private void ResolveReferences()
    {
        popupController ??= GetComponent<TerritoryIncomePopupController>();
        popupController ??= FindFirstObjectByType<TerritoryIncomePopupController>(FindObjectsInactive.Include);
    }
}
