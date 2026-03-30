using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryInfoPanelBridge : MonoBehaviour
{
    public bool IsVisible { get; private set; }
    public int? CurrentTileId { get; private set; }
    public TerritoryTileInfoViewModel CurrentViewModel { get; private set; }

    public event Action<int> ShowRequested;
    public event Action<TerritoryTileInfoViewModel> ViewModelChanged;
    public event Action HideRequested;

    public void Show(TerritoryTileInfoViewModel viewModel)
    {
        if (viewModel == null)
        {
            return;
        }

        IsVisible = true;
        CurrentTileId = viewModel.TileId;
        CurrentViewModel = viewModel;
        ShowRequested?.Invoke(viewModel.TileId);
        ViewModelChanged?.Invoke(viewModel);
    }

    public void ShowForTile(int tileId)
    {
        TerritoryTileInfoViewModel fallbackViewModel = new TerritoryTileInfoViewModel(
            tileId,
            0,
            0,
            $"타일 {tileId}",
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            false);

        Show(fallbackViewModel);
    }

    public void Hide()
    {
        if (!IsVisible && !CurrentTileId.HasValue)
        {
            return;
        }

        IsVisible = false;
        CurrentTileId = null;
        CurrentViewModel = null;
        HideRequested?.Invoke();
    }
}
