using System;
using DG.Tweening;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Transform animationTarget;
    [SerializeField] private Ease adjacentMoveEase = Ease.OutCubic;
    [SerializeField] private Ease acrossEnterEase = Ease.InQuad;
    [SerializeField] private Ease acrossExitEase = Ease.OutExpo;

    private Sequence _activeSequence;

    public bool IsPlayingAnimation => _activeSequence != null && _activeSequence.IsActive() && _activeSequence.IsPlaying();

    private void Awake()
    {
        animationTarget ??= transform;
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void SnapToPose(Vector3 worldPosition, float worldRotationDegrees)
    {
        StopAnimation();
        animationTarget.position = worldPosition;
        SetWorldRotation(worldRotationDegrees);
    }

    public void PlayMoveAnimation(PlayerMoveContext moveContext, float moveDuration, Action onComplete)
    {
        StopAnimation();

        float sanitizedDuration = Mathf.Max(0.01f, moveDuration);

        _activeSequence = moveContext.UsesCenterPath
            ? CreateAcrossDiameterSequence(moveContext, sanitizedDuration, onComplete)
            : CreateAdjacentMoveSequence(moveContext, sanitizedDuration, onComplete);
    }

    public void StopAnimation()
    {
        if (_activeSequence == null)
        {
            return;
        }

        _activeSequence.Kill();
        _activeSequence = null;
    }

    private Sequence CreateAdjacentMoveSequence(PlayerMoveContext moveContext, float moveDuration, Action onComplete)
    {
        float targetRotationDegrees = GetClosestRotationDegrees(moveContext.TargetRotationDegrees);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(animationTarget.DOMove(moveContext.TargetWorldPosition, moveDuration).SetEase(adjacentMoveEase));
        sequence.Join(animationTarget.DORotate(new Vector3(0f, 0f, targetRotationDegrees), moveDuration, RotateMode.Fast).SetEase(adjacentMoveEase));
        sequence.OnComplete(() => CompleteAnimation(moveContext, onComplete));
        sequence.SetTarget(animationTarget);
        return sequence;
    }

    private Sequence CreateAcrossDiameterSequence(PlayerMoveContext moveContext, float moveDuration, Action onComplete)
    {
        float firstLegDuration = moveDuration * 0.45f;
        float secondLegDuration = moveDuration - firstLegDuration;
        float travelRotationDegrees = GetTravelRotationDegrees(moveContext.StartWorldPosition, moveContext.TargetWorldPosition);
        float settledRotationDegrees = GetClosestRotationDegrees(moveContext.TargetRotationDegrees);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(animationTarget.DOMove(moveContext.BoardCenterWorldPosition, firstLegDuration).SetEase(acrossEnterEase));
        sequence.Join(animationTarget.DORotate(new Vector3(0f, 0f, travelRotationDegrees), firstLegDuration, RotateMode.Fast).SetEase(Ease.OutSine));
        sequence.Append(animationTarget.DOMove(moveContext.TargetWorldPosition, secondLegDuration).SetEase(acrossExitEase));
        sequence.Join(animationTarget.DORotate(new Vector3(0f, 0f, settledRotationDegrees), secondLegDuration, RotateMode.Fast).SetEase(Ease.OutCubic));
        sequence.OnComplete(() => CompleteAnimation(moveContext, onComplete));
        sequence.SetTarget(animationTarget);
        return sequence;
    }

    private void CompleteAnimation(PlayerMoveContext moveContext, Action onComplete)
    {
        animationTarget.position = moveContext.TargetWorldPosition;
        SetWorldRotation(moveContext.TargetRotationDegrees);
        _activeSequence = null;
        onComplete?.Invoke();
    }

    private float GetClosestRotationDegrees(float targetRotationDegrees)
    {
        float currentRotationDegrees = animationTarget.eulerAngles.z;
        return currentRotationDegrees + Mathf.DeltaAngle(currentRotationDegrees, targetRotationDegrees);
    }

    private float GetTravelRotationDegrees(Vector3 startWorldPosition, Vector3 targetWorldPosition)
    {
        Vector2 direction = targetWorldPosition - startWorldPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return animationTarget.eulerAngles.z;
        }

        float targetRotationDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return GetClosestRotationDegrees(targetRotationDegrees);
    }

    private void SetWorldRotation(float worldRotationDegrees)
    {
        animationTarget.rotation = Quaternion.Euler(0f, 0f, worldRotationDegrees);
    }
}
