using UnityEngine;

public enum PlayerState
{
    Idle,
    Moving,
    Hit,
    Dead
}

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : ObjectBase, IFaceSectorAttackTarget
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerEquipmentService playerEquipmentService;
    [SerializeField] private StagePlayerLoadoutInstaller stagePlayerLoadoutInstaller;
    [SerializeField] private PlayerCombatRewardService playerCombatRewardService;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private StageManager stageManager;

    private PlayerState _playerState;

    public bool IsFaceSectorAttackTargetReady
    {
        get
        {
            return !_isDead && playerMovement != null && playerMovement.IsInitialized;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        playerMovement ??= GetComponent<PlayerMovement>();
        playerAnimation ??= GetComponent<PlayerAnimation>();
        playerStatus ??= GetComponent<PlayerStatus>();
        playerEquipmentService ??= GetComponent<PlayerEquipmentService>();
        stagePlayerLoadoutInstaller ??= GetComponent<StagePlayerLoadoutInstaller>();
        playerCombatRewardService ??= GetComponent<PlayerCombatRewardService>();
        playerVisual ??= GetComponent<PlayerVisual>();

        if (playerEquipmentService == null)
        {
            playerEquipmentService = gameObject.AddComponent<PlayerEquipmentService>();
        }

        if (stagePlayerLoadoutInstaller == null)
        {
            stagePlayerLoadoutInstaller = gameObject.AddComponent<StagePlayerLoadoutInstaller>();
        }

        if (playerCombatRewardService == null)
        {
            playerCombatRewardService = gameObject.AddComponent<PlayerCombatRewardService>();
        }

        if (playerVisual == null)
        {
            playerVisual = gameObject.AddComponent<PlayerVisual>();
        }

        Rb.bodyType = RigidbodyType2D.Kinematic;
        Rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        SetPlayerState(PlayerState.Idle);
    }

    private void Start()
    {
        stagePlayerLoadoutInstaller.ApplyLoadout();
        playerVisual.RefreshVisualFromEquipment();
        stageManager ??= FindFirstObjectByType<StageManager>();
        SyncHealthSnapshotFromStatus();
        _isDead = playerStatus.HasNoHealthRemaining();

        if (!playerMovement.InitializeMovement())
        {
            enabled = false;
            return;
        }

        SnapPlayerToCurrentFace();
    }

    public bool RequestStepLeft()
    {
        return TryRequestMove(PlayerMoveCommand.StepLeft);
    }

    public bool RequestStepRight()
    {
        return TryRequestMove(PlayerMoveCommand.StepRight);
    }

    public bool RequestMoveAcrossDiameter()
    {
        return TryRequestMove(PlayerMoveCommand.MoveAcrossDiameter);
    }

    public override void TakeDamage(int damage)
    {
        if (_isDead)
        {
            return;
        }

        playerStatus.ApplyDamage(damage);
        playerCombatRewardService?.HandlePlayerDamaged();
        SyncHealthSnapshotFromStatus();
        playerVisual.PlayHitFlash();

        if (!playerStatus.HasNoHealthRemaining())
        {
            SetPlayerState(PlayerState.Hit);
            SetPlayerState(PlayerState.Idle);
            return;
        }

        _isDead = true;
        SetPlayerState(PlayerState.Dead);
        OnDeath();
    }

    public bool TryGetCurrentFaceIndex(out int faceIndex)
    {
        faceIndex = 0;

        if (playerMovement == null || !playerMovement.IsInitialized)
        {
            return false;
        }

        faceIndex = playerMovement.CurrentFaceIndex;
        return true;
    }

    public bool TryReceiveFaceSectorAttack(FaceSectorAttackData attackData)
    {
        if (!IsFaceSectorAttackTargetReady || attackData.Damage <= 0)
        {
            return false;
        }

        TakeDamage(attackData.Damage);
        return true;
    }

    protected override void OnDeath()
    {
        playerAnimation.StopAnimation();
        stageManager?.TryCompleteCurrentStage(false);
    }

    public bool TryRequestMove(PlayerMoveCommand moveCommand)
    {
        if (!CanStartMove())
        {
            return false;
        }

        if (!playerMovement.TryBuildMoveContext(moveCommand, out PlayerMoveContext moveContext))
        {
            return false;
        }

        playerMovement.CommitMove(moveContext);
        SetPlayerState(PlayerState.Moving);
        playerAnimation.PlayMoveAnimation(moveContext, GetMoveDuration(moveCommand), HandleMoveAnimationCompleted);
        return true;
    }

    private void HandleMoveAnimationCompleted()
    {
        if (_isDead)
        {
            return;
        }

        SetPlayerState(PlayerState.Idle);
    }

    private void SnapPlayerToCurrentFace()
    {
        if (!playerMovement.TryGetCurrentPose(out Vector3 worldPosition, out float worldRotationDegrees))
        {
            return;
        }

        playerAnimation.SnapToPose(worldPosition, worldRotationDegrees);
    }

    private bool CanStartMove()
    {
        return !_isDead && _playerState == PlayerState.Idle && !playerAnimation.IsPlayingAnimation;
    }

    private float GetMoveDuration(PlayerMoveCommand moveCommand)
    {
        switch (moveCommand)
        {
            case PlayerMoveCommand.MoveAcrossDiameter:
                return playerStatus.GetAcrossMoveDelay();

            case PlayerMoveCommand.StepLeft:
            case PlayerMoveCommand.StepRight:
            default:
                return playerStatus.GetAdjacentMoveDelay();
        }
    }

    private void SyncHealthSnapshotFromStatus()
    {
        _maxHp = playerStatus.GetMaxHealth();
        _hp = playerStatus.GetCurrentHealth();
    }

    private void SetPlayerState(PlayerState nextState)
    {
        _playerState = nextState;
    }
}
