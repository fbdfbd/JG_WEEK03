using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class BossBaseWeakSectorDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossSectorCombatController sectorCombatController;

    [Header("Base Weak Sector")]
    [SerializeField] private bool assignWeakSectorOnStart = true;
    [SerializeField] private bool randomizeStartingFace = true;
    [SerializeField] private int startingCenterFaceIndex;
    [SerializeField] private int sectorHalfWidth = 1;

    [Header("Movement")]
    [SerializeField] private float minimumMoveInterval = 2f;
    [SerializeField] private float maximumMoveInterval = 3f;

    private Coroutine baseWeakSectorRoutine;
    private int lastAssignedCenterFaceIndex = -1;

    private void Awake()
    {
        if (sectorCombatController == null)
        {
            sectorCombatController = GetComponentInParent<BossSectorCombatController>();
        }
    }

    private void OnEnable()
    {
        StartBaseWeakSectorLoop();
    }

    private void OnDisable()
    {
        StopBaseWeakSectorLoop();
    }

    public void RegisterSectorCombatController(BossSectorCombatController combatController)
    {
        sectorCombatController = combatController;
    }

    public bool TryAssignBaseWeakSectorImmediately()
    {
        return TryAssignBaseWeakSector(true);
    }

    private void StartBaseWeakSectorLoop()
    {
        if (baseWeakSectorRoutine != null)
        {
            return;
        }

        baseWeakSectorRoutine = StartCoroutine(RunBaseWeakSectorLoop());
    }

    private void StopBaseWeakSectorLoop()
    {
        if (baseWeakSectorRoutine == null)
        {
            return;
        }

        StopCoroutine(baseWeakSectorRoutine);
        baseWeakSectorRoutine = null;
    }

    private IEnumerator RunBaseWeakSectorLoop()
    {
        if (assignWeakSectorOnStart)
        {
            TryAssignBaseWeakSector(true);
        }

        while (true)
        {
            float waitDuration = GetNextMoveInterval();
            if (waitDuration > 0f)
            {
                yield return new WaitForSeconds(waitDuration);
            }
            else
            {
                yield return null;
            }

            TryAssignBaseWeakSector(false);
        }
    }

    private bool TryAssignBaseWeakSector(bool isFirstAssignment)
    {
        if (sectorCombatController == null)
        {
            return false;
        }

        int centerFaceIndex;
        if (!TrySelectCenterFaceIndex(isFirstAssignment, out centerFaceIndex))
        {
            return false;
        }

        FaceSectorRange sectorRange;
        if (!sectorCombatController.TryCreateSectorRange(
            BossSectorTargetingMode.FixedFaceIndex,
            centerFaceIndex,
            sectorHalfWidth,
            out sectorRange))
        {
            return false;
        }

        lastAssignedCenterFaceIndex = centerFaceIndex;
        sectorCombatController.SetBaseWeakSector(sectorRange);
        return true;
    }

    private bool TrySelectCenterFaceIndex(bool isFirstAssignment, out int centerFaceIndex)
    {
        centerFaceIndex = 0;

        if (sectorCombatController == null)
        {
            return false;
        }

        int faceCount;
        if (!sectorCombatController.TryGetFaceCount(out faceCount))
        {
            return false;
        }

        if (faceCount <= 0)
        {
            return false;
        }

        if (isFirstAssignment && !randomizeStartingFace)
        {
            centerFaceIndex = Mathf.Abs(startingCenterFaceIndex) % faceCount;
            return true;
        }

        if (faceCount == 1)
        {
            centerFaceIndex = 0;
            return true;
        }

        int randomFaceIndex = Random.Range(0, faceCount);
        if (randomFaceIndex == lastAssignedCenterFaceIndex)
        {
            randomFaceIndex = (randomFaceIndex + 1) % faceCount;
        }

        centerFaceIndex = randomFaceIndex;
        return true;
    }

    private float GetNextMoveInterval()
    {
        float clampedMinimum = Mathf.Max(0f, minimumMoveInterval);
        float clampedMaximum = Mathf.Max(clampedMinimum, maximumMoveInterval);

        if (Mathf.Approximately(clampedMinimum, clampedMaximum))
        {
            return clampedMinimum;
        }

        return Random.Range(clampedMinimum, clampedMaximum);
    }
}
