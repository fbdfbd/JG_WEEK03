using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class TerritoryMapPointerInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string actionMapName = "UI";
    [SerializeField] private string pointActionName = "Point";
    [SerializeField] private string clickActionName = "Click";

    [Header("Raycast")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask hitLayers = Physics2D.DefaultRaycastLayers;
    [SerializeField] [Min(0f)] private float pointerAssistRadius = 0.12f;
    [SerializeField] private bool ignorePointerWhenOverUi = true;

    private InputActionMap _fallbackActionMap;
    private InputAction _pointAction;
    private InputAction _clickAction;
    private TerritoryTileInput _currentHoverTarget;
    private TerritoryMapHitResolver _hitResolver;
    private bool _hasPendingClick;
    private bool _isInputEnabled = true;

    public event System.Action BackgroundClicked;

    public void SetInputEnabled(bool isInputEnabled)
    {
        _isInputEnabled = isInputEnabled;
        _hasPendingClick = false;

        if (_isInputEnabled || _currentHoverTarget == null)
        {
            return;
        }

        _currentHoverTarget.NotifyPointerExit();
        _currentHoverTarget = null;
    }

    private void Awake()
    {
        targetCamera ??= Camera.main;
        InitializeActions();
        _hitResolver = new TerritoryMapHitResolver(hitLayers, pointerAssistRadius);
    }

    private void OnEnable()
    {
        if (_pointAction == null || _clickAction == null)
        {
            InitializeActions();
        }

        _hitResolver = new TerritoryMapHitResolver(hitLayers, pointerAssistRadius);
        _clickAction.performed += HandleClickPerformed;
        _pointAction.Enable();
        _clickAction.Enable();
    }

    private void OnDisable()
    {
        if (_clickAction != null)
        {
            _clickAction.performed -= HandleClickPerformed;
        }

        _pointAction?.Disable();
        _clickAction?.Disable();

        if (_currentHoverTarget != null)
        {
            _currentHoverTarget.NotifyPointerExit();
            _currentHoverTarget = null;
        }
    }

    private void Update()
    {
        RefreshHoverTarget();
        ProcessPendingClick();
    }

    private void InitializeActions()
    {
        if (TryBindActionsFromAsset())
        {
            return;
        }

        CreateFallbackActions();
    }

    private bool TryBindActionsFromAsset()
    {
        if (inputActionsAsset == null)
        {
            return false;
        }

        InputActionMap actionMap = inputActionsAsset.FindActionMap(actionMapName, false);
        if (actionMap == null)
        {
            Debug.LogWarning($"TerritoryMapPointerInput could not find action map '{actionMapName}'. Using fallback actions.");
            return false;
        }

        _pointAction = actionMap.FindAction(pointActionName, false);
        _clickAction = actionMap.FindAction(clickActionName, false);

        if (_pointAction == null || _clickAction == null)
        {
            Debug.LogWarning($"TerritoryMapPointerInput could not find actions '{pointActionName}' or '{clickActionName}'. Using fallback actions.");
            _pointAction = null;
            _clickAction = null;
            return false;
        }

        return true;
    }

    private void CreateFallbackActions()
    {
        _fallbackActionMap = new InputActionMap(actionMapName);

        _pointAction = _fallbackActionMap.AddAction(pointActionName, InputActionType.PassThrough);
        _pointAction.expectedControlType = "Vector2";
        _pointAction.AddBinding("<Pointer>/position");

        _clickAction = _fallbackActionMap.AddAction(clickActionName, InputActionType.Button);
        _clickAction.expectedControlType = "Button";
        _clickAction.AddBinding("<Pointer>/press");
    }

    private void HandleClickPerformed(InputAction.CallbackContext context)
    {
        if (!_isInputEnabled || !context.ReadValueAsButton())
        {
            return;
        }
        
        _hasPendingClick = true;
    }

    private void RefreshHoverTarget()
    {
        if (!_isInputEnabled)
        {
            if (_currentHoverTarget != null)
            {
                _currentHoverTarget.NotifyPointerExit();
                _currentHoverTarget = null;
            }

            return;
        }

        if (ShouldIgnorePointerBecauseOfUi())
        {
            if (_currentHoverTarget != null)
            {
                _currentHoverTarget.NotifyPointerExit();
                _currentHoverTarget = null;
            }

            return;
        }

        TerritoryTileInput nextHoverTarget = GetTileInputUnderPointer();
        if (_currentHoverTarget == nextHoverTarget)
        {
            return;
        }

        if (_currentHoverTarget != null)
        {
            _currentHoverTarget.NotifyPointerExit();
        }

        _currentHoverTarget = nextHoverTarget;

        if (_currentHoverTarget != null)
        {
            _currentHoverTarget.NotifyPointerEnter();
        }
    }

    private void ProcessPendingClick()
    {
        if (!_isInputEnabled || !_hasPendingClick)
        {
            return;
        }

        _hasPendingClick = false;

        if (ShouldIgnorePointerBecauseOfUi())
        {
            return;
        }

        TerritoryTileInput tileInput = GetTileInputUnderPointer();
        if (tileInput == null)
        {
            BackgroundClicked?.Invoke();
            return;
        }

        tileInput.NotifyPointerClick();
    }

    private TerritoryTileInput GetTileInputUnderPointer()
    {
        if (_pointAction == null)
        {
            return null;
        }

        targetCamera ??= Camera.main;
        if (targetCamera == null)
        {
            return null;
        }

        Vector2 pointerScreenPosition = _pointAction.ReadValue<Vector2>();
        if (_hitResolver == null)
        {
            _hitResolver = new TerritoryMapHitResolver(hitLayers, pointerAssistRadius);
        }

        return _hitResolver.ResolveTileInput(targetCamera, pointerScreenPosition);
    }

    private bool ShouldIgnorePointerBecauseOfUi()
    {
        return ignorePointerWhenOverUi
            && EventSystem.current != null
            && EventSystem.current.IsPointerOverGameObject();
    }
}
