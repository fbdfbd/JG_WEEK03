using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossFaceHazardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BossSectorCombatController sectorCombatController;
    [SerializeField] private FaceMapGenerator faceBoard;
    [SerializeField] private PlayerMovement playerMovement;

    private readonly Dictionary<int, BossMineInstance> activeMinesByFace = new Dictionary<int, BossMineInstance>();
    private readonly List<int> candidateFaceIndexes = new List<int>();

    private Transform hazardRoot;

    private void Awake()
    {
        ResolveReferences();
        EnsureHazardRoot();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToPlayerMovement();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerMovement();
        ClearAllMines();
    }

    public bool HasAvailableFace(bool allowSpawnOnPlayerFace)
    {
        return TryGetRandomAvailableFaceIndex(allowSpawnOnPlayerFace, out _);
    }

    public bool TryGetRandomAvailableFaceIndex(bool allowSpawnOnPlayerFace, out int faceIndex)
    {
        faceIndex = 0;

        if (!TryBuildCandidateFaceIndexes(allowSpawnOnPlayerFace))
        {
            return false;
        }

        faceIndex = candidateFaceIndexes[Random.Range(0, candidateFaceIndexes.Count)];
        return true;
    }

    public bool TrySpawnMine(int faceIndex, BossMineSpec mineSpec, Vector3 throwStartWorldPosition)
    {
        ResolveReferences();

        if (sectorCombatController == null || faceBoard == null || !faceBoard.IsInitialized)
        {
            return false;
        }

        int wrappedFaceIndex = faceBoard.GetWrappedFaceIndex(faceIndex);
        if (activeMinesByFace.ContainsKey(wrappedFaceIndex))
        {
            return false;
        }

        EnsureHazardRoot();

        GameObject mineObject = new GameObject($"BossMine_{wrappedFaceIndex}");
        mineObject.transform.SetParent(hazardRoot, false);

        BossMineInstance mineInstance = mineObject.AddComponent<BossMineInstance>();
        mineInstance.Initialize(this, faceBoard, wrappedFaceIndex, mineSpec, throwStartWorldPosition);

        activeMinesByFace.Add(wrappedFaceIndex, mineInstance);
        return true;
    }

    public bool TryDetonateMine(BossMineInstance mineInstance, BossMineDetonationReason detonationReason)
    {
        if (!TryUnregisterMine(mineInstance))
        {
            return false;
        }

        mineInstance.HandleDetonationStarted(detonationReason);
        ApplyMineDamage(mineInstance);
        mineInstance.PlayDetonationAndDestroy();
        return true;
    }

    public void NotifyMineExpired(BossMineInstance mineInstance)
    {
        if (!TryUnregisterMine(mineInstance))
        {
            return;
        }

        mineInstance.ExpireWithoutDetonation();
    }

    private void ApplyMineDamage(BossMineInstance mineInstance)
    {
        if (sectorCombatController == null || mineInstance == null)
        {
            return;
        }

        FaceSectorRange blastRange = new FaceSectorRange(mineInstance.FaceIndex, mineInstance.BlastHalfWidth);
        FaceSectorAttackData attackData = new FaceSectorAttackData(
            blastRange,
            mineInstance.Damage,
            0f,
            0f);

        sectorCombatController.TryApplyDamageToPlayer(attackData);
    }

    private bool TryUnregisterMine(BossMineInstance mineInstance)
    {
        if (mineInstance == null)
        {
            return false;
        }

        if (!activeMinesByFace.TryGetValue(mineInstance.FaceIndex, out BossMineInstance registeredMine))
        {
            return false;
        }

        if (registeredMine != mineInstance)
        {
            return false;
        }

        activeMinesByFace.Remove(mineInstance.FaceIndex);
        return true;
    }

    private bool TryBuildCandidateFaceIndexes(bool allowSpawnOnPlayerFace)
    {
        candidateFaceIndexes.Clear();
        ResolveReferences();

        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            return false;
        }

        int blockedPlayerFaceIndex = -1;
        if (!allowSpawnOnPlayerFace && playerMovement != null && playerMovement.IsInitialized)
        {
            blockedPlayerFaceIndex = faceBoard.GetWrappedFaceIndex(playerMovement.CurrentFaceIndex);
        }

        int faceCount = faceBoard.GetFaceCount();
        for (int i = 0; i < faceCount; i++)
        {
            if (activeMinesByFace.ContainsKey(i))
            {
                continue;
            }

            if (i == blockedPlayerFaceIndex)
            {
                continue;
            }

            candidateFaceIndexes.Add(i);
        }

        return candidateFaceIndexes.Count > 0;
    }

    private void HandlePlayerFaceChanged(int faceIndex)
    {
        ResolveReferences();

        if (faceBoard == null || !faceBoard.IsInitialized)
        {
            return;
        }

        int wrappedFaceIndex = faceBoard.GetWrappedFaceIndex(faceIndex);
        if (!activeMinesByFace.TryGetValue(wrappedFaceIndex, out BossMineInstance mineInstance))
        {
            return;
        }

        if (!mineInstance.CanDetonateOnStep)
        {
            return;
        }

        mineInstance.RequestDetonation(BossMineDetonationReason.PlayerStep);
    }

    private void ResolveReferences()
    {
        sectorCombatController ??= GetComponentInParent<BossSectorCombatController>();
        faceBoard ??= FindFirstObjectByType<FaceMapGenerator>();
        playerMovement ??= FindFirstObjectByType<PlayerMovement>();
    }

    private void SubscribeToPlayerMovement()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.FaceIndexChanged -= HandlePlayerFaceChanged;
        playerMovement.FaceIndexChanged += HandlePlayerFaceChanged;
    }

    private void UnsubscribeFromPlayerMovement()
    {
        if (playerMovement == null)
        {
            return;
        }

        playerMovement.FaceIndexChanged -= HandlePlayerFaceChanged;
    }

    private void EnsureHazardRoot()
    {
        if (hazardRoot != null)
        {
            return;
        }

        Transform existingRoot = transform.Find("Hazards");
        if (existingRoot != null)
        {
            hazardRoot = existingRoot;
            return;
        }

        GameObject rootObject = new GameObject("Hazards");
        hazardRoot = rootObject.transform;
        hazardRoot.SetParent(transform, false);
    }

    private void ClearAllMines()
    {
        foreach (KeyValuePair<int, BossMineInstance> pair in activeMinesByFace)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value.gameObject);
            }
        }

        activeMinesByFace.Clear();
    }
}
