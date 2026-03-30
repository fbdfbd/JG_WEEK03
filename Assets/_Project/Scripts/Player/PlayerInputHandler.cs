using UnityEngine;
using UnityEngine.InputSystem;

public readonly struct HoldMoveSession
{
    public static HoldMoveSession None => new HoldMoveSession(0, PlayerMoveCommand.StepRight);

    public int InputDirection { get; }
    public PlayerMoveCommand MoveCommand { get; }
    public bool IsActive => InputDirection != 0;

    public HoldMoveSession(int inputDirection, PlayerMoveCommand moveCommand)
    {
        InputDirection = inputDirection;
        MoveCommand = moveCommand;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttackController))]
[RequireComponent(typeof(PlayerStatus))]
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActionsAsset;
    [SerializeField] private string gameplayMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string acrossMoveActionName = "Jump";
    [SerializeField] private string fireActionName = "Fire";
    [SerializeField] private string specialAttackActionName = "SpecialAttack";
    [SerializeField] private string holdFireShortcutActionName = "HoldFireShortcut";

    [Header("Input Tuning")]
    [SerializeField] private float horizontalThreshold = 0.5f;
    [SerializeField] private float baseHoldStartDelay = 0.22f;
    [SerializeField] private float baseHoldRepeatInterval = 0.16f;
    [SerializeField] private float acrossMoveGaugeCost = 15f;

    [Header("Debug Overrides")]
    [SerializeField] private bool debugForceEnableHoldToMove;
    [SerializeField] private bool debugForceEnableInputAdjust;
    [SerializeField] private bool debugForceEnableHoldToFire;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttackController playerAttackController;
    [SerializeField] private PlayerSpecialAttackController playerSpecialAttackController;
    [SerializeField] private PlayerStatus playerStatus;

    private InputActionMap fallbackActionMap;
    private InputAction moveAction;
    private InputAction acrossMoveAction;
    private InputAction alternateAcrossMoveAction;
    private InputAction fireAction;
    private InputAction holdFireShortcutAction;
    private InputAction specialAttackAction;
    private HoldMoveSession currentHoldMoveSession = HoldMoveSession.None;
    private float nextHoldMoveTime;
    private bool runtimeEnableHoldToMove;
    private float runtimeHoldMoveRepeatIntervalScale = 1f;
    private bool runtimeEnableInputAdjust;
    private bool runtimeEnableHoldToFire;
    private bool isHoldingFireShortcut;
    private bool ownsAlternateAcrossMoveAction;
    private bool ownsHoldFireShortcutAction;
    private bool ownsSpecialAttackAction;

    private void Awake()
    {
        playerController ??= GetComponent<PlayerController>();
        playerMovement ??= GetComponent<PlayerMovement>();
        playerAttackController ??= GetComponent<PlayerAttackController>();
        playerSpecialAttackController ??= GetComponent<PlayerSpecialAttackController>();
        playerStatus ??= GetComponent<PlayerStatus>();

        if (playerAttackController == null)
        {
            playerAttackController = gameObject.AddComponent<PlayerAttackController>();
        }

        if (playerSpecialAttackController == null)
        {
            playerSpecialAttackController = gameObject.AddComponent<PlayerSpecialAttackController>();
        }

        ResetRuntimeOptions();
        InitializeActions();
    }

    private void OnEnable()
    {
        if (moveAction == null || acrossMoveAction == null || fireAction == null)
        {
            InitializeActions();
        }

        SubscribeActions();
        EnableActions();
    }

    private void OnDisable()
    {
        UnsubscribeActions();
        DisableActions();
        ClearHoldMoveSession();
    }

    private void OnDestroy()
    {
        DisposeOptionalActions();
        fallbackActionMap?.Dispose();
        fallbackActionMap = null;
    }

    private void Update()
    {
        TryProcessContinuousMove();
        TryProcessContinuousFire();
    }

    public void ApplyRuntimeOptions(PlayerInputLoadout inputLoadout)
    {
        runtimeEnableHoldToMove = inputLoadout.EnableHoldToMove;
        runtimeHoldMoveRepeatIntervalScale = Mathf.Max(0.01f, inputLoadout.HoldMoveRepeatIntervalScale);
        runtimeEnableInputAdjust = inputLoadout.EnableInputAdjust;
        runtimeEnableHoldToFire = inputLoadout.EnableHoldToFire;
    }

    public void ResetRuntimeOptions()
    {
        runtimeEnableHoldToMove = false;
        runtimeHoldMoveRepeatIntervalScale = 1f;
        runtimeEnableInputAdjust = false;
        runtimeEnableHoldToFire = false;
        isHoldingFireShortcut = false;
    }

    private void InitializeActions()
    {
        DisposeOptionalActions();

        if (TryBindActionsFromAsset())
        {
            EnsureOptionalActions();
            return;
        }

        CreateFallbackActions();
        EnsureOptionalActions();
    }

    private bool TryBindActionsFromAsset()
    {
        if (inputActionsAsset == null)
        {
            return false;
        }

        InputActionMap gameplayMap = inputActionsAsset.FindActionMap(gameplayMapName, false);
        if (gameplayMap == null)
        {
            Debug.LogWarning($"Input action map '{gameplayMapName}' could not be found. Using fallback input map.");
            return false;
        }

        moveAction = gameplayMap.FindAction(moveActionName, false);
        acrossMoveAction = gameplayMap.FindAction(acrossMoveActionName, false);
        fireAction = gameplayMap.FindAction(fireActionName, false);
        specialAttackAction = gameplayMap.FindAction(specialAttackActionName, false);

        if (moveAction == null || acrossMoveAction == null || fireAction == null)
        {
            Debug.LogWarning($"Required input actions '{moveActionName}', '{acrossMoveActionName}', or '{fireActionName}' could not be found. Using fallback input map.");
            moveAction = null;
            acrossMoveAction = null;
            fireAction = null;
            specialAttackAction = null;
            return false;
        }

        return true;
    }

    private void CreateFallbackActions()
    {
        fallbackActionMap = new InputActionMap(gameplayMapName);
        ownsAlternateAcrossMoveAction = false;
        ownsHoldFireShortcutAction = false;
        ownsSpecialAttackAction = false;

        moveAction = fallbackActionMap.AddAction(moveActionName, InputActionType.Value);
        moveAction.expectedControlType = "Vector2";
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");

        acrossMoveAction = fallbackActionMap.AddAction(acrossMoveActionName, InputActionType.Button);
        acrossMoveAction.expectedControlType = "Button";
        acrossMoveAction.AddBinding("<Keyboard>/space");
        acrossMoveAction.AddBinding("<Keyboard>/x");
        acrossMoveAction.AddBinding("<Gamepad>/buttonSouth");

        fireAction = fallbackActionMap.AddAction(fireActionName, InputActionType.Button);
        fireAction.expectedControlType = "Button";
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");

        specialAttackAction = fallbackActionMap.AddAction(specialAttackActionName, InputActionType.Button);
        specialAttackAction.expectedControlType = "Button";
        specialAttackAction.AddBinding("<Keyboard>/c");
    }

    private void SubscribeActions()
    {
        moveAction.performed -= HandleMovePerformed;
        moveAction.canceled -= HandleMoveCanceled;
        acrossMoveAction.performed -= HandleAcrossMovePerformed;
        fireAction.performed -= HandleFirePerformed;

        if (alternateAcrossMoveAction != null)
        {
            alternateAcrossMoveAction.performed -= HandleAcrossMovePerformed;
        }

        if (holdFireShortcutAction != null)
        {
            holdFireShortcutAction.started -= HandleHoldFireStarted;
            holdFireShortcutAction.canceled -= HandleHoldFireCanceled;
        }

        if (specialAttackAction != null)
        {
            specialAttackAction.performed -= HandleSpecialAttackPerformed;
        }

        moveAction.performed += HandleMovePerformed;
        moveAction.canceled += HandleMoveCanceled;
        acrossMoveAction.performed += HandleAcrossMovePerformed;
        fireAction.performed += HandleFirePerformed;

        if (alternateAcrossMoveAction != null)
        {
            alternateAcrossMoveAction.performed += HandleAcrossMovePerformed;
        }

        if (holdFireShortcutAction != null)
        {
            holdFireShortcutAction.started += HandleHoldFireStarted;
            holdFireShortcutAction.canceled += HandleHoldFireCanceled;
        }

        if (specialAttackAction != null)
        {
            specialAttackAction.performed += HandleSpecialAttackPerformed;
        }
    }

    private void UnsubscribeActions()
    {
        if (moveAction != null)
        {
            moveAction.performed -= HandleMovePerformed;
            moveAction.canceled -= HandleMoveCanceled;
        }

        if (acrossMoveAction != null)
        {
            acrossMoveAction.performed -= HandleAcrossMovePerformed;
        }

        if (alternateAcrossMoveAction != null)
        {
            alternateAcrossMoveAction.performed -= HandleAcrossMovePerformed;
        }

        if (holdFireShortcutAction != null)
        {
            holdFireShortcutAction.started -= HandleHoldFireStarted;
            holdFireShortcutAction.canceled -= HandleHoldFireCanceled;
        }

        if (fireAction != null)
        {
            fireAction.performed -= HandleFirePerformed;
        }

        if (specialAttackAction != null)
        {
            specialAttackAction.performed -= HandleSpecialAttackPerformed;
        }
    }

    private void EnableActions()
    {
        moveAction?.Enable();
        acrossMoveAction?.Enable();
        alternateAcrossMoveAction?.Enable();
        fireAction?.Enable();
        holdFireShortcutAction?.Enable();
        specialAttackAction?.Enable();
    }

    private void DisableActions()
    {
        moveAction?.Disable();
        acrossMoveAction?.Disable();
        alternateAcrossMoveAction?.Disable();
        fireAction?.Disable();
        holdFireShortcutAction?.Disable();
        specialAttackAction?.Disable();
    }

    private void HandleMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveVector = context.ReadValue<Vector2>();
        int horizontalDirection = GetHorizontalDirection(moveVector.x);

        if (horizontalDirection == 0)
        {
            ClearHoldMoveSession();
            return;
        }

        if (ShouldStartNewHoldMoveSession(horizontalDirection))
        {
            StartMoveInput(horizontalDirection);
        }
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        ClearHoldMoveSession();
    }

    private void HandleAcrossMovePerformed(InputAction.CallbackContext context)
    {
        TryStartAcrossMove();
    }

    private void HandleFirePerformed(InputAction.CallbackContext context)
    {
        if (playerAttackController == null)
        {
            return;
        }

        playerAttackController.TryFireBasicAttack();
    }

    private void HandleHoldFireStarted(InputAction.CallbackContext context)
    {
        isHoldingFireShortcut = true;
        TryProcessContinuousFire();
    }

    private void HandleHoldFireCanceled(InputAction.CallbackContext context)
    {
        isHoldingFireShortcut = false;
    }

    private void HandleSpecialAttackPerformed(InputAction.CallbackContext context)
    {
        if (playerSpecialAttackController == null)
        {
            return;
        }

        playerSpecialAttackController.TryFireSpecialAttack();
    }

    private void StartMoveInput(int horizontalDirection)
    {
        if (!TryResolveTapMoveCommand(horizontalDirection, out PlayerMoveCommand moveCommand))
        {
            ClearHoldMoveSession();
            return;
        }

        BeginHoldMoveSession(horizontalDirection, moveCommand);
        playerController.TryRequestMove(moveCommand);
    }

    private void TryProcessContinuousMove()
    {
        if (!IsHoldToMoveEnabled())
        {
            return;
        }

        if (!currentHoldMoveSession.IsActive)
        {
            return;
        }

        if (Time.time < nextHoldMoveTime)
        {
            return;
        }

        if (!TryProcessHoldMove())
        {
            return;
        }

        nextHoldMoveTime = Time.time + GetHoldRepeatInterval();
    }

    private void TryProcessContinuousFire()
    {
        if (!IsHoldToFireEnabled())
        {
            return;
        }

        if (!isHoldingFireShortcut)
        {
            return;
        }

        if (playerAttackController == null)
        {
            return;
        }

        playerAttackController.TryFireBasicAttack();
    }

    private bool TryProcessHoldMove()
    {
        if (!currentHoldMoveSession.IsActive)
        {
            return false;
        }

        return playerController.TryRequestMove(currentHoldMoveSession.MoveCommand);
    }

    private bool TryResolveTapMoveCommand(int horizontalDirection, out PlayerMoveCommand moveCommand)
    {
        moveCommand = PlayerMoveCommand.StepRight;

        if (horizontalDirection == 0)
        {
            return false;
        }

        if (!IsInputAdjustEnabled())
        {
            moveCommand = ResolveLogicalMoveCommand(horizontalDirection);
            return true;
        }

        return playerMovement.TryResolveScreenRelativeMoveCommand(horizontalDirection, out moveCommand);
    }

    private bool ShouldStartNewHoldMoveSession(int horizontalDirection)
    {
        return !currentHoldMoveSession.IsActive
            || currentHoldMoveSession.InputDirection != horizontalDirection;
    }

    private void BeginHoldMoveSession(int horizontalDirection, PlayerMoveCommand moveCommand)
    {
        currentHoldMoveSession = new HoldMoveSession(horizontalDirection, moveCommand);
        nextHoldMoveTime = Time.time + GetHoldStartDelay();
    }

    private void ClearHoldMoveSession()
    {
        currentHoldMoveSession = HoldMoveSession.None;
        nextHoldMoveTime = 0f;
    }

    private PlayerMoveCommand ResolveLogicalMoveCommand(int horizontalDirection)
    {
        return horizontalDirection < 0
            ? PlayerMoveCommand.StepLeft
            : PlayerMoveCommand.StepRight;
    }

    private int GetHorizontalDirection(float horizontalValue)
    {
        if (horizontalValue <= -horizontalThreshold)
        {
            return -1;
        }

        if (horizontalValue >= horizontalThreshold)
        {
            return 1;
        }

        return 0;
    }

    private bool IsHoldToMoveEnabled()
    {
        return debugForceEnableHoldToMove
            || runtimeEnableHoldToMove;
    }

    private bool IsInputAdjustEnabled()
    {
        return debugForceEnableInputAdjust
            || runtimeEnableInputAdjust;
    }

    private bool IsHoldToFireEnabled()
    {
        return debugForceEnableHoldToFire
            || runtimeEnableHoldToFire;
    }

    private float GetHoldRepeatInterval()
    {
        return Mathf.Max(0.01f, baseHoldRepeatInterval * runtimeHoldMoveRepeatIntervalScale);
    }

    private float GetHoldStartDelay()
    {
        return Mathf.Max(0.01f, baseHoldStartDelay * runtimeHoldMoveRepeatIntervalScale);
    }

    private bool TryStartAcrossMove()
    {
        if (playerController == null || playerStatus == null)
        {
            return false;
        }

        if (!playerStatus.TryConsumeScaledSpecialGauge(acrossMoveGaugeCost, out float consumedGauge))
        {
            return false;
        }

        bool moved = playerController.RequestMoveAcrossDiameter();
        if (!moved)
        {
            playerStatus.RestoreSpecialGauge(consumedGauge);
        }

        return moved;
    }

    private void EnsureOptionalActions()
    {
        if (acrossMoveAction != null && !HasKeyboardBinding(acrossMoveAction, "<Keyboard>/x"))
        {
            alternateAcrossMoveAction = CreateStandaloneButtonAction("AcrossMoveShortcut", "<Keyboard>/x");
            ownsAlternateAcrossMoveAction = true;
        }

        if (holdFireShortcutAction == null)
        {
            holdFireShortcutAction = CreateStandaloneButtonAction(holdFireShortcutActionName, "<Keyboard>/z");
            ownsHoldFireShortcutAction = true;
        }

        if (specialAttackAction == null)
        {
            specialAttackAction = CreateStandaloneButtonAction(specialAttackActionName, "<Keyboard>/c");
            ownsSpecialAttackAction = true;
        }
    }

    private InputAction CreateStandaloneButtonAction(string actionName, string bindingPath)
    {
        InputAction action = new InputAction(actionName, InputActionType.Button);
        action.expectedControlType = "Button";
        action.AddBinding(bindingPath);
        return action;
    }

    private void DisposeOptionalActions()
    {
        if (ownsAlternateAcrossMoveAction)
        {
            alternateAcrossMoveAction?.Dispose();
        }

        alternateAcrossMoveAction = null;
        ownsAlternateAcrossMoveAction = false;

        if (ownsHoldFireShortcutAction)
        {
            holdFireShortcutAction?.Dispose();
        }

        holdFireShortcutAction = null;
        ownsHoldFireShortcutAction = false;
        isHoldingFireShortcut = false;

        if (ownsSpecialAttackAction)
        {
            specialAttackAction?.Dispose();
            specialAttackAction = null;
        }

        ownsSpecialAttackAction = false;
    }

    private static bool HasKeyboardBinding(InputAction action, string bindingPath)
    {
        if (action == null)
        {
            return false;
        }

        for (int index = 0; index < action.bindings.Count; index++)
        {
            if (action.bindings[index].path == bindingPath)
            {
                return true;
            }
        }

        return false;
    }
}
