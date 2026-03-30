using System.Collections;
using UnityEngine;

public enum BossMineState
{
    Throwing,
    Armed,
    Detonating,
    Expired
}

public enum BossMineDetonationReason
{
    TimerElapsed,
    PlayerStep
}

[DisallowMultipleComponent]
public sealed class BossMineInstance : MonoBehaviour
{
    private BossFaceHazardController owner;
    private FaceMapGenerator faceBoard;
    private BossMineSpec mineSpec;

    private Transform mineVisual;
    private Coroutine lifeRoutine;
    private BossMineState currentState;
    private Vector3 initialVisualScale = Vector3.one;

    public int FaceIndex { get; private set; }
    public int Damage => mineSpec.Damage;
    public int BlastHalfWidth => mineSpec.BlastHalfWidth;
    public bool CanDetonateOnStep => currentState == BossMineState.Armed && mineSpec.DetonateOnStep;

    public void Initialize(
        BossFaceHazardController ownerController,
        FaceMapGenerator board,
        int faceIndex,
        BossMineSpec spec,
        Vector3 throwStartWorldPosition)
    {
        owner = ownerController;
        faceBoard = board;
        FaceIndex = faceIndex;
        mineSpec = spec;

        SpawnMineVisual();
        transform.position = throwStartWorldPosition;

        lifeRoutine = StartCoroutine(RunLifeCycle());
    }

    public void RequestDetonation(BossMineDetonationReason detonationReason)
    {
        if (owner == null || currentState != BossMineState.Armed)
        {
            return;
        }

        owner.TryDetonateMine(this, detonationReason);
    }

    public void HandleDetonationStarted(BossMineDetonationReason detonationReason)
    {
        if (lifeRoutine != null)
        {
            StopCoroutine(lifeRoutine);
            lifeRoutine = null;
        }

        currentState = BossMineState.Detonating;
    }

    public void PlayDetonationAndDestroy()
    {
        StartCoroutine(PlayDetonationRoutine());
    }

    public void ExpireWithoutDetonation()
    {
        currentState = BossMineState.Expired;
        Destroy(gameObject);
    }

    private IEnumerator RunLifeCycle()
    {
        if (!TryGetTargetPose(out Vector3 targetWorldPosition, out float targetWorldRotationDegrees))
        {
            currentState = BossMineState.Expired;
            Destroy(gameObject);
            yield break;
        }

        currentState = BossMineState.Throwing;
        yield return PlayThrowRoutine(targetWorldPosition, targetWorldRotationDegrees);

        currentState = BossMineState.Armed;

        if (!mineSpec.DetonateOnTimer)
        {
            yield break;
        }

        if (mineSpec.FuseDuration > 0f)
        {
            yield return new WaitForSeconds(mineSpec.FuseDuration);
        }

        if (currentState != BossMineState.Armed)
        {
            yield break;
        }

        owner?.TryDetonateMine(this, BossMineDetonationReason.TimerElapsed);
    }

    private IEnumerator PlayThrowRoutine(Vector3 targetWorldPosition, float targetWorldRotationDegrees)
    {
        float duration = mineSpec.ThrowDuration;
        if (duration <= 0f)
        {
            transform.position = targetWorldPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, targetWorldRotationDegrees);
            yield break;
        }

        Vector3 startWorldPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0f, 0f, targetWorldRotationDegrees);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 currentWorldPosition = Vector3.Lerp(startWorldPosition, targetWorldPosition, t);
            currentWorldPosition.y += CalculateArcOffset(t);

            transform.position = currentWorldPosition;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        transform.position = targetWorldPosition;
        transform.rotation = endRotation;
    }

    private IEnumerator PlayDetonationRoutine()
    {
        float duration = mineSpec.DetonationDuration;
        Vector3 startScale = initialVisualScale;
        Vector3 targetScale = initialVisualScale * mineSpec.DetonationScaleMultiplier;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (mineVisual != null)
            {
                mineVisual.localScale = Vector3.Lerp(startScale, targetScale, t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private float CalculateArcOffset(float normalizedTime)
    {
        if (Mathf.Approximately(mineSpec.ThrowArcHeight, 0f))
        {
            return 0f;
        }

        return 4f * mineSpec.ThrowArcHeight * normalizedTime * (1f - normalizedTime);
    }

    private void SpawnMineVisual()
    {
        if (mineSpec.MineViewPrefab == null)
        {
            mineVisual = transform;
            initialVisualScale = mineVisual.localScale;
            return;
        }

        GameObject viewInstance = Instantiate(mineSpec.MineViewPrefab, transform);
        mineVisual = viewInstance.transform;
        mineVisual.localPosition = Vector3.zero;
        mineVisual.localRotation = Quaternion.identity;
        initialVisualScale = mineVisual.localScale;
    }

    private bool TryGetTargetPose(out Vector3 worldPosition, out float worldRotationDegrees)
    {
        worldPosition = transform.position;
        worldRotationDegrees = transform.eulerAngles.z;

        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            return false;
        }

        return faceBoard.TryGetFaceWorldPose(FaceIndex, out worldPosition, out worldRotationDegrees);
    }
}
