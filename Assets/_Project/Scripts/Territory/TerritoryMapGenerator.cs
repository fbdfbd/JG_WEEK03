using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryMapGenerator : MonoBehaviour
{
    [SerializeField] private Transform tileContainer;
    [SerializeField] private TerritoryTileView tilePrefab;

    private readonly List<TerritoryTileView> _generatedViews = new List<TerritoryTileView>();
    private readonly Dictionary<int, TerritoryTileView> _viewByTileId = new Dictionary<int, TerritoryTileView>();

    public Transform TileContainer => tileContainer;
    public TerritoryTileView TilePrefab => tilePrefab;
    public IReadOnlyList<TerritoryTileView> GeneratedViews => _generatedViews;

    private void Awake()
    {
        tileContainer ??= transform;
    }

    public bool Build(TerritoryMapLayoutData layoutData)
    {
        if (layoutData == null)
        {
            Debug.LogWarning("TerritoryMapGenerator requires a valid layout.");
            return false;
        }

        if (tileContainer == null)
        {
            Debug.LogWarning("TerritoryMapGenerator requires a tile container.");
            return false;
        }

        if (tilePrefab == null)
        {
            Debug.LogWarning("TerritoryMapGenerator requires a tile prefab.");
            return false;
        }

        ClearGeneratedTiles();

        for (int index = 0; index < layoutData.Tiles.Count; index++)
        {
            TerritoryTileLayoutData tileLayout = layoutData.Tiles[index];
            TerritoryTileView tileView = Instantiate(tilePrefab, tileContainer);
            tileView.name = $"TerritoryTile_{tileLayout.TileId:00}";
            tileView.Bind(tileLayout);

            _generatedViews.Add(tileView);
            _viewByTileId[tileLayout.TileId] = tileView;
        }

        return true;
    }

    public bool TryGetView(int tileId, out TerritoryTileView tileView)
    {
        return _viewByTileId.TryGetValue(tileId, out tileView);
    }

    public void ClearGeneratedTiles()
    {
        for (int index = 0; index < _generatedViews.Count; index++)
        {
            TerritoryTileView tileView = _generatedViews[index];
            if (tileView == null)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(tileView.gameObject);
            }
            else
            {
                DestroyImmediate(tileView.gameObject);
            }
        }

        _generatedViews.Clear();
        _viewByTileId.Clear();
    }
}
