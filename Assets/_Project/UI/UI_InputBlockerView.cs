using UnityEngine;

[DisallowMultipleComponent]
public sealed class UI_InputBlockerView : MonoBehaviour
{
    [SerializeField] private GameObject rootObject;
    [SerializeField] private CanvasGroup canvasGroup;

    public bool IsBlocking
        => rootObject != null
        && rootObject.activeSelf
        && canvasGroup != null
        && canvasGroup.blocksRaycasts;

    private void Awake()
    {
        AssignMissingReferences();
        Hide();
    }

    private void Reset()
    {
        AssignMissingReferences();
    }

    private void OnValidate()
    {
        AssignMissingReferences();
    }

    public void Show()
    {
        AssignMissingReferences();

        if (rootObject != null)
        {
            rootObject.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void Hide()
    {
        AssignMissingReferences();

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }
    }

    private void AssignMissingReferences()
    {
        rootObject ??= gameObject;
        canvasGroup ??= GetComponent<CanvasGroup>();
    }
}
