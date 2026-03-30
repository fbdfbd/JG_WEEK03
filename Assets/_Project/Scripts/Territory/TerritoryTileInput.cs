using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class TerritoryTileInput : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;

    private int _tileId = -1;
    private bool _isBound;
    private bool _canInteract = true;
    private bool _isHovering;

    public int TileId => _tileId;
    public Collider2D TargetCollider => targetCollider;
    public bool CanReceivePointerInput => _isBound && _canInteract && isActiveAndEnabled;

    public event Action<int> Clicked;
    public event Action<int> HoverEntered;
    public event Action<int> HoverExited;

    private void Awake()
    {
        targetCollider ??= GetComponent<Collider2D>();
    }

    public void Bind(int tileId)
    {
        _tileId = tileId;
        _isBound = true;
    }

    public void SetInteractable(bool canInteract)
    {
        _canInteract = canInteract;

        if (targetCollider != null)
        {
            targetCollider.enabled = canInteract;
        }

        if (!canInteract)
        {
            NotifyPointerExit();
        }
    }

    public void NotifyPointerEnter()
    {
        if (!CanReceivePointerInput || _isHovering)
        {
            return;
        }

        _isHovering = true;
        HoverEntered?.Invoke(_tileId);
    }

    public void NotifyPointerExit()
    {
        if (!_isBound || !_isHovering)
        {
            return;
        }

        _isHovering = false;
        HoverExited?.Invoke(_tileId);
    }

    public void NotifyPointerClick()
    {
        if (!CanReceivePointerInput)
        {
            return;
        }

        Clicked?.Invoke(_tileId);
    }

    private void OnDisable()
    {
        NotifyPointerExit();
    }
}
