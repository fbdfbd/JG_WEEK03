using UnityEngine;
using UnityEngine.UI;

public class UI_InventoryCostImageView : MonoBehaviour
{
    [SerializeField] private Image _costImage;
    [SerializeField] private Color _activeColor = new Color(1f, 0.95f, 0.6f, 1f);
    [SerializeField] private Color _inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private void Awake()
    {
        EnsureReferences();
        EnsureDefaultColors();
    }

    private void OnValidate()
    {
        EnsureReferences();
        EnsureDefaultColors();
    }

    private void Reset()
    {
        EnsureReferences();
        EnsureDefaultColors();
    }

    public void SetActive(bool active)
    {
        if (_costImage == null)
        {
            return;
        }

        _costImage.enabled = true;
        _costImage.color = active ? _activeColor : _inactiveColor;
    }

    private void EnsureReferences()
    {
        _costImage ??= GetComponent<Image>();
    }

    private void EnsureDefaultColors()
    {
        if (IsUnsetColor(_activeColor))
        {
            _activeColor = Color.white;
        }

        if (IsUnsetColor(_inactiveColor))
        {
            _inactiveColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        }
    }

    private bool IsUnsetColor(Color color)
    {
        return Mathf.Approximately(color.r, 0f)
            && Mathf.Approximately(color.g, 0f)
            && Mathf.Approximately(color.b, 0f)
            && Mathf.Approximately(color.a, 0f);
    }
}
