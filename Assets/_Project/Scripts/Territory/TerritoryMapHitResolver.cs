using System.Collections.Generic;
using UnityEngine;

public sealed class TerritoryMapHitResolver
{
    private readonly struct HitCandidate
    {
        public TerritoryTileInput TileInput { get; }
        public int SortingOrder { get; }
        public float DistanceSqr { get; }

        public HitCandidate(TerritoryTileInput tileInput, int sortingOrder, float distanceSqr)
        {
            TileInput = tileInput;
            SortingOrder = sortingOrder;
            DistanceSqr = distanceSqr;
        }
    }

    private readonly LayerMask _hitLayers;
    private readonly float _pointerAssistRadius;

    public TerritoryMapHitResolver(LayerMask hitLayers, float pointerAssistRadius)
    {
        _hitLayers = hitLayers;
        _pointerAssistRadius = Mathf.Max(0f, pointerAssistRadius);
    }

    public TerritoryTileInput ResolveTileInput(Camera targetCamera, Vector2 pointerScreenPosition)
    {
        if (targetCamera == null)
        {
            return null;
        }

        Vector3 pointerWorldPosition = targetCamera.ScreenToWorldPoint(
            new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, GetScreenToWorldDepth(targetCamera)));
        pointerWorldPosition.z = 0f;

        TerritoryTileInput tileInput = ResolveFromColliders(
            Physics2D.OverlapPointAll(pointerWorldPosition, _hitLayers),
            pointerWorldPosition);

        if (tileInput != null || _pointerAssistRadius <= 0f)
        {
            return tileInput;
        }

        return ResolveFromColliders(
            Physics2D.OverlapCircleAll(pointerWorldPosition, _pointerAssistRadius, _hitLayers),
            pointerWorldPosition);
    }

    private TerritoryTileInput ResolveFromColliders(Collider2D[] colliders, Vector3 pointerWorldPosition)
    {
        if (colliders == null || colliders.Length == 0)
        {
            return null;
        }

        List<HitCandidate> candidates = new List<HitCandidate>(colliders.Length);
        HashSet<int> visitedTileIds = new HashSet<int>();

        for (int index = 0; index < colliders.Length; index++)
        {
            Collider2D collider = colliders[index];
            if (collider == null)
            {
                continue;
            }

            TerritoryTileInput tileInput = collider.GetComponent<TerritoryTileInput>();
            if (tileInput == null)
            {
                tileInput = collider.GetComponentInParent<TerritoryTileInput>();
            }

            if (tileInput == null || !tileInput.CanReceivePointerInput || !visitedTileIds.Add(tileInput.TileId))
            {
                continue;
            }

            TerritoryTileView tileView = tileInput.GetComponent<TerritoryTileView>();
            int sortingOrder = tileView != null ? tileView.SortingOrder : 0;
            float distanceSqr = ((Vector2)collider.bounds.center - (Vector2)pointerWorldPosition).sqrMagnitude;
            candidates.Add(new HitCandidate(tileInput, sortingOrder, distanceSqr));
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        candidates.Sort(CompareCandidates);
        return candidates[0].TileInput;
    }

    private static int CompareCandidates(HitCandidate left, HitCandidate right)
    {
        int sortingOrderComparison = right.SortingOrder.CompareTo(left.SortingOrder);
        if (sortingOrderComparison != 0)
        {
            return sortingOrderComparison;
        }

        return left.DistanceSqr.CompareTo(right.DistanceSqr);
    }

    private static float GetScreenToWorldDepth(Camera cameraComponent)
    {
        if (cameraComponent.orthographic)
        {
            return Mathf.Abs(cameraComponent.transform.position.z);
        }

        return Mathf.Abs(cameraComponent.transform.position.z);
    }
}
