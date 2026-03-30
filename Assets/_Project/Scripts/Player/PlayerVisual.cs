using DG.Tweening;
using UnityEngine;

public readonly struct PlayerVisualState
{
    public bool UseTint { get; }
    public Color TintColor { get; }

    public PlayerVisualState(bool useTint, Color tintColor)
    {
        UseTint = useTint;
        TintColor = tintColor;
    }
}

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private PlayerEquipmentService playerEquipmentService;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashBlendWeight = 0.8f;
    [SerializeField] private float hitFlashFadeInDuration = 0.04f;
    [SerializeField] private float hitFlashFadeOutDuration = 0.12f;

    private Color _defaultColor = Color.white;
    private PlayerVisualState _equipmentVisualState;
    private Tween _hitFlashTween;
    private float _currentHitFlashWeight;
    private bool _isSubscribedToEquipmentService;

    private void Awake()
    {
        playerSpriteRenderer ??= GetComponent<SpriteRenderer>();
        CaptureDefaultVisual();
        RefreshVisual();
    }

    private void OnEnable()
    {
        TrySubscribeToEquipmentService();
        RefreshVisualFromEquipment();
    }

    private void Start()
    {
        TrySubscribeToEquipmentService();
        RefreshVisualFromEquipment();
    }

    private void OnDisable()
    {
        StopHitFlash();
        UnsubscribeFromEquipmentService();
    }

    public void CaptureDefaultVisual()
    {
        if (playerSpriteRenderer == null)
        {
            return;
        }

        _defaultColor = playerSpriteRenderer.color;
    }

    public void ApplyEquipmentVisual(PlayerVisualState visualState)
    {
        _equipmentVisualState = visualState;
        RefreshVisual();
    }

    public void ClearEquipmentVisual()
    {
        _equipmentVisualState = default;
        RefreshVisual();
    }

    public void PlayHitFlash()
    {
        StopHitFlash();

        _hitFlashTween = DOTween.Sequence()
            .Append(DOVirtual.Float(0f, hitFlashBlendWeight, hitFlashFadeInDuration, SetHitFlashWeight))
            .Append(DOVirtual.Float(hitFlashBlendWeight, 0f, hitFlashFadeOutDuration, SetHitFlashWeight))
            .OnComplete(() =>
            {
                _currentHitFlashWeight = 0f;
                _hitFlashTween = null;
                RefreshVisual();
            })
            .SetTarget(this);
    }

    public void StopHitFlash()
    {
        if (_hitFlashTween == null)
        {
            _currentHitFlashWeight = 0f;
            RefreshVisual();
            return;
        }

        _hitFlashTween.Kill();
        _hitFlashTween = null;
        _currentHitFlashWeight = 0f;
        RefreshVisual();
    }

    public void RefreshVisualFromEquipment()
    {
        if (playerEquipmentService == null)
        {
            RefreshVisual();
            return;
        }

        ApplyEquipmentVisual(playerEquipmentService.GetVisualState());
    }

    private void RefreshVisual()
    {
        if (playerSpriteRenderer == null)
        {
            return;
        }

        Color targetColor = _defaultColor;

        if (_equipmentVisualState.UseTint)
        {
            targetColor = _equipmentVisualState.TintColor;
        }

        if (_currentHitFlashWeight > 0f)
        {
            targetColor = Color.Lerp(targetColor, hitFlashColor, _currentHitFlashWeight);
        }

        playerSpriteRenderer.color = targetColor;
    }

    private void SetHitFlashWeight(float weight)
    {
        _currentHitFlashWeight = Mathf.Clamp01(weight);
        RefreshVisual();
    }

    private void TrySubscribeToEquipmentService()
    {
        if (playerEquipmentService == null)
        {
            playerEquipmentService = GetComponent<PlayerEquipmentService>();
        }

        if (playerEquipmentService == null || _isSubscribedToEquipmentService)
        {
            return;
        }

        playerEquipmentService.VisualStateChanged += HandleVisualStateChanged;
        _isSubscribedToEquipmentService = true;
    }

    private void UnsubscribeFromEquipmentService()
    {
        if (playerEquipmentService == null || !_isSubscribedToEquipmentService)
        {
            return;
        }

        playerEquipmentService.VisualStateChanged -= HandleVisualStateChanged;
        _isSubscribedToEquipmentService = false;
    }

    private void HandleVisualStateChanged(PlayerVisualState visualState)
    {
        ApplyEquipmentVisual(visualState);
    }
}
