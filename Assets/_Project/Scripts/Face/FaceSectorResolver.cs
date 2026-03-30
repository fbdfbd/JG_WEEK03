using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FaceSectorResolver : MonoBehaviour
{
    [SerializeField] private FaceMapGenerator faceBoard;

    private readonly List<int> cachedFaceIndexes = new List<int>();

    public bool IsReady
    {
        get
        {
            return faceBoard != null && faceBoard.IsInitialized;
        }
    }

    public void RegisterFaceBoard(FaceMapGenerator board)
    {
        faceBoard = board;
    }

    public FaceSectorRange CreateSectorFromCenter(int centerFaceIndex, int halfWidth)
    {
        if (!TryEnsureBoard())
        {
            return new FaceSectorRange(centerFaceIndex, halfWidth);
        }

        int wrappedCenterFaceIndex = faceBoard.GetWrappedFaceIndex(centerFaceIndex);
        return new FaceSectorRange(wrappedCenterFaceIndex, halfWidth);
    }

    public bool ContainsFace(FaceSectorRange sectorRange, int faceIndex)
    {
        if (!TryEnsureBoard())
        {
            return false;
        }

        int wrappedTargetFaceIndex = faceBoard.GetWrappedFaceIndex(faceIndex);

        for (int offset = -sectorRange.HalfWidth; offset <= sectorRange.HalfWidth; offset++)
        {
            int currentFaceIndex = faceBoard.GetWrappedFaceIndex(sectorRange.CenterFaceIndex + offset);
            if (currentFaceIndex == wrappedTargetFaceIndex)
            {
                return true;
            }
        }

        return false;
    }

    public void CollectFaceIndexes(FaceSectorRange sectorRange, List<int> results)
    {
        results.Clear();

        if (!TryEnsureBoard())
        {
            return;
        }

        for (int offset = -sectorRange.HalfWidth; offset <= sectorRange.HalfWidth; offset++)
        {
            int currentFaceIndex = faceBoard.GetWrappedFaceIndex(sectorRange.CenterFaceIndex + offset);
            results.Add(currentFaceIndex);
        }
    }

    public bool TryGetNearestFaceIndexFromWorldPoint(Vector2 worldPoint, out int faceIndex)
    {
        faceIndex = 0;

        if (!TryEnsureBoard())
        {
            return false;
        }

        int faceCount = faceBoard.GetFaceCount();
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < faceCount; i++)
        {
            if (!faceBoard.TryGetFaceWorldPose(i, out Vector3 faceWorldPosition, out _))
            {
                continue;
            }

            float currentDistance = Vector2.SqrMagnitude((Vector2)faceWorldPosition - worldPoint);
            if (currentDistance >= shortestDistance)
            {
                continue;
            }

            shortestDistance = currentDistance;
            faceIndex = i;
        }

        return shortestDistance < float.MaxValue;
    }

    public bool TryBuildSectorOutline(FaceSectorRange sectorRange, List<Vector3> worldPoints)
    {
        worldPoints.Clear();

        if (!TryEnsureBoard())
        {
            return false;
        }

        CollectFaceIndexes(sectorRange, cachedFaceIndexes);

        if (cachedFaceIndexes.Count == 0)
        {
            return false;
        }

        Vector3 boardCenter = faceBoard.GetBoardCenterWorldPosition();
        worldPoints.Add(boardCenter);

        int firstFaceIndex = cachedFaceIndexes[0];
        if (!faceBoard.TryGetFaceWorldEdge(firstFaceIndex, out Vector3 firstStartPoint, out _))
        {
            return false;
        }

        worldPoints.Add(firstStartPoint);

        for (int i = 0; i < cachedFaceIndexes.Count; i++)
        {
            int currentFaceIndex = cachedFaceIndexes[i];
            if (!faceBoard.TryGetFaceWorldEdge(currentFaceIndex, out _, out Vector3 currentEndPoint))
            {
                return false;
            }

            worldPoints.Add(currentEndPoint);
        }

        worldPoints.Add(boardCenter);
        return true;
    }

    private bool TryEnsureBoard()
    {
        if (faceBoard == null)
        {
            faceBoard = FindFirstObjectByType<FaceMapGenerator>();
        }

        return faceBoard != null && faceBoard.IsInitialized;
    }
}
