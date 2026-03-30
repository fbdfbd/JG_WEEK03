using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryIncomePopupController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TerritoryMapController territoryMapController;
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private UI_TerritoryFloatingTextView popupPrefab;

    [Header("Motion")]
    [SerializeField] private float popupInterval = 0.05f;
    [SerializeField] private float popupDuration = 0.8f;
    [SerializeField] private Vector2 popupMoveOffset = new Vector2(0f, 80f);
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.45f, 0f);
    [SerializeField] private Color popupColor = new Color(1f, 0.83f, 0.2f, 1f);

    private void Awake()
    {
        ResolveReferences();
    }

    public IEnumerator Play(TerritoryDailyIncomeResult incomeResult)
    {
        if (incomeResult == null || !incomeResult.HasIncome)
        {
            yield break;
        }

        ResolveReferences();
        List<TerritoryPopupEntry> popupEntries = new List<TerritoryPopupEntry>(incomeResult.Entries.Count);
        for (int index = 0; index < incomeResult.Entries.Count; index++)
        {
            TerritoryDailyIncomeEntry entry = incomeResult.Entries[index];
            popupEntries.Add(new TerritoryPopupEntry(
                entry.TileId,
                $"+{entry.GoldAmount}G",
                popupColor));
        }

        yield return PlaySequence(popupEntries);
    }

    public void ShowPopup(TerritoryPopupEntry popupEntry)
    {
        ResolveReferences();
        SpawnPopup(popupEntry);
    }

    public IEnumerator PlayPopup(TerritoryPopupEntry popupEntry)
    {
        ShowPopup(popupEntry);
        yield return new WaitForSeconds(popupDuration);
    }

    public IEnumerator PlaySequence(IReadOnlyList<TerritoryPopupEntry> popupEntries)
    {
        if (popupEntries == null || popupEntries.Count == 0)
        {
            yield break;
        }

        ResolveReferences();

        for (int index = 0; index < popupEntries.Count; index++)
        {
            SpawnPopup(popupEntries[index]);
            yield return new WaitForSeconds(popupInterval);
        }

        yield return new WaitForSeconds(popupDuration);
    }

    private void ResolveReferences()
    {
        territoryMapController ??= FindFirstObjectByType<TerritoryMapController>(FindObjectsInactive.Include);
        popupCanvas ??= FindPopupCanvas();
        worldCamera ??= Camera.main;
    }

    private void SpawnPopup(TerritoryPopupEntry entry)
    {
        if (popupCanvas == null || territoryMapController == null)
        {
            return;
        }

        if (entry.TileId < 0
            || !territoryMapController.TryGetTileView(entry.TileId, out TerritoryTileView tileView)
            || tileView == null)
        {
            return;
        }

        Vector3 worldPosition = tileView.transform.position + worldOffset;
        if (!TryConvertWorldToCanvasPosition(worldPosition, out Vector2 anchoredPosition))
        {
            return;
        }

        UI_TerritoryFloatingTextView popupView = CreatePopupView();
        if (popupView == null)
        {
            return;
        }

        popupView.Play(entry.Text, entry.Color, anchoredPosition, popupMoveOffset, popupDuration)
            .OnComplete(() => Destroy(popupView.gameObject));
    }

    private UI_TerritoryFloatingTextView CreatePopupView()
    {
        if (popupCanvas == null)
        {
            return null;
        }

        if (popupPrefab != null)
        {
            return Instantiate(popupPrefab, popupCanvas.transform);
        }

        GameObject popupObject = new GameObject("IncomePopup", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(UI_TerritoryFloatingTextView));
        popupObject.transform.SetParent(popupCanvas.transform, false);

        TextMeshProUGUI textView = popupObject.GetComponent<TextMeshProUGUI>();
        textView.alignment = TextAlignmentOptions.Center;
        textView.fontSize = 28f;
        textView.raycastTarget = false;

        return popupObject.GetComponent<UI_TerritoryFloatingTextView>();
    }

    private bool TryConvertWorldToCanvasPosition(Vector3 worldPosition, out Vector2 anchoredPosition)
    {
        anchoredPosition = Vector2.zero;

        if (popupCanvas == null)
        {
            return false;
        }

        RectTransform canvasRect = popupCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            return false;
        }

        Camera eventCamera = popupCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            eventCamera,
            out anchoredPosition);
    }

    private Canvas FindPopupCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int index = 0; index < canvases.Length; index++)
        {
            Canvas currentCanvas = canvases[index];
            if (currentCanvas != null && currentCanvas.name == "Canvas_HUD")
            {
                return currentCanvas;
            }
        }

        return canvases.Length > 0 ? canvases[0] : null;
    }
}
