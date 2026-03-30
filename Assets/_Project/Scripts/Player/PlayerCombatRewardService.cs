using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStatus))]
public sealed class PlayerCombatRewardService : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerComboTracker playerComboTracker;

    private void Awake()
    {
        playerStatus ??= GetComponent<PlayerStatus>();
        playerComboTracker ??= GetComponent<PlayerComboTracker>();

        if (playerComboTracker == null)
        {
            playerComboTracker = gameObject.AddComponent<PlayerComboTracker>();
        }
    }

    public void HandleConfirmedHit(in ProjectileHitData hitData, int appliedDamage)
    {
        if (playerStatus == null || appliedDamage <= 0)
        {
            return;
        }

        playerStatus.RestoreGaugeOnConfirmedHit();
        playerStatus.RegisterConfirmedHitWithoutTakingDamage();
        playerComboTracker?.RegisterConfirmedHit();
    }

    public void HandlePlayerDamaged()
    {
        playerComboTracker?.ResetCombo();
    }
}
