using System;
using UnityEngine;

public enum PlayerMoveCommand
{
    StepLeft,
    StepRight,
    MoveAcrossDiameter
}

public readonly struct PlayerMoveContext
{
    public PlayerMoveCommand MoveCommand { get; }
    public int StartFaceIndex { get; }
    public int TargetFaceIndex { get; }
    public Vector3 StartWorldPosition { get; }
    public Vector3 TargetWorldPosition { get; }
    public Vector3 BoardCenterWorldPosition { get; }
    public float StartRotationDegrees { get; }
    public float TargetRotationDegrees { get; }
    public bool UsesCenterPath => MoveCommand == PlayerMoveCommand.MoveAcrossDiameter;

    public PlayerMoveContext(
        PlayerMoveCommand moveCommand,
        int startFaceIndex,
        int targetFaceIndex,
        Vector3 startWorldPosition,
        Vector3 targetWorldPosition,
        Vector3 boardCenterWorldPosition,
        float startRotationDegrees,
        float targetRotationDegrees)
    {
        MoveCommand = moveCommand;
        StartFaceIndex = startFaceIndex;
        TargetFaceIndex = targetFaceIndex;
        StartWorldPosition = startWorldPosition;
        TargetWorldPosition = targetWorldPosition;
        BoardCenterWorldPosition = boardCenterWorldPosition;
        StartRotationDegrees = startRotationDegrees;
        TargetRotationDegrees = targetRotationDegrees;
    }
}

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private int startFaceIndex = 0;

    public int CurrentFaceIndex { get; private set; }
    public bool IsInitialized { get; private set; }

    public event Action<int> FaceIndexChanged;

    public void RegisterFaceBoard(FaceMapGenerator board)
    {
        faceBoard = board;
        IsInitialized = false;
    }

    public bool InitializeMovement()
    {
        faceBoard ??= FindFirstObjectByType<FaceMapGenerator>();

        if (faceBoard == null)
        {
            Debug.LogError("PlayerMovement could not find a FaceMapGenerator in the scene.");
            return false;
        }

        if (!faceBoard.IsInitialized)
        {
            Debug.LogError("PlayerMovement requires an initialized FaceMapGenerator.");
            return false;
        }

        CurrentFaceIndex = faceBoard.GetWrappedFaceIndex(startFaceIndex);
        IsInitialized = true;
        return true;
    }

    public bool TryGetCurrentPose(out Vector3 worldPosition, out float worldRotationDegrees)
    {
        worldPosition = transform.position;
        worldRotationDegrees = transform.eulerAngles.z;

        if (!TryEnsureReady())
        {
            return false;
        }

        return faceBoard.TryGetFaceWorldPose(CurrentFaceIndex, out worldPosition, out worldRotationDegrees);
    }

    public bool TryBuildMoveContext(PlayerMoveCommand moveCommand, out PlayerMoveContext moveContext)
    {
        moveContext = default;

        if (!TryEnsureReady())
        {
            return false;
        }

        if (!faceBoard.TryGetFaceWorldPose(CurrentFaceIndex, out Vector3 startWorldPosition, out float startRotationDegrees))
        {
            return false;
        }

        int targetFaceIndex = GetTargetFaceIndex(moveCommand);

        if (!faceBoard.TryGetFaceWorldPose(targetFaceIndex, out Vector3 targetWorldPosition, out float targetRotationDegrees))
        {
            return false;
        }

        moveContext = new PlayerMoveContext(
            moveCommand,
            CurrentFaceIndex,
            targetFaceIndex,
            startWorldPosition,
            targetWorldPosition,
            faceBoard.GetBoardCenterWorldPosition(),
            startRotationDegrees,
            targetRotationDegrees);

        return true;
    }

    public bool TryResolveScreenRelativeMoveCommand(int horizontalDirection, out PlayerMoveCommand moveCommand)
    {
        moveCommand = PlayerMoveCommand.StepRight;

        if (!TryEnsureReady())
        {
            return false;
        }

        if (horizontalDirection == 0)
        {
            return false;
        }

        int logicalLeftFaceIndex = faceBoard.GetLeftFaceIndex(CurrentFaceIndex);
        int logicalRightFaceIndex = faceBoard.GetRightFaceIndex(CurrentFaceIndex);

        if (!faceBoard.TryGetFaceWorldPose(logicalLeftFaceIndex, out Vector3 logicalLeftWorldPosition, out _))
        {
            return false;
        }

        if (!faceBoard.TryGetFaceWorldPose(logicalRightFaceIndex, out Vector3 logicalRightWorldPosition, out _))
        {
            return false;
        }

        bool isLogicalLeftOnScreenLeft = logicalLeftWorldPosition.x <= logicalRightWorldPosition.x;

        if (horizontalDirection < 0)
        {
            moveCommand = isLogicalLeftOnScreenLeft
                ? PlayerMoveCommand.StepLeft
                : PlayerMoveCommand.StepRight;

            return true;
        }

        moveCommand = isLogicalLeftOnScreenLeft
            ? PlayerMoveCommand.StepRight
            : PlayerMoveCommand.StepLeft;

        return true;
    }

    public void CommitMove(PlayerMoveContext moveContext)
    {
        CurrentFaceIndex = moveContext.TargetFaceIndex;
        FaceIndexChanged?.Invoke(CurrentFaceIndex);
    }

    private int GetTargetFaceIndex(PlayerMoveCommand moveCommand)
    {
        switch (moveCommand)
        {
            case PlayerMoveCommand.StepLeft:
                return faceBoard.GetLeftFaceIndex(CurrentFaceIndex);

            case PlayerMoveCommand.StepRight:
                return faceBoard.GetRightFaceIndex(CurrentFaceIndex);

            case PlayerMoveCommand.MoveAcrossDiameter:
                return faceBoard.GetOppositeFaceIndex(CurrentFaceIndex);

            default:
                return CurrentFaceIndex;
        }
    }

    private bool TryEnsureReady()
    {
        if (IsInitialized && faceBoard != null && faceBoard.IsInitialized)
        {
            return true;
        }

        Debug.LogWarning("PlayerMovement is not ready. Call InitializeMovement() after the board has been created.");
        return false;
    }
}
