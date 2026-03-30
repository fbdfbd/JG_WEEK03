using DG.Tweening;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class TerritoryMapCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Focus")]
    [SerializeField] private float focusedOrthographicSize = 3.8f;
    [SerializeField] private Vector2 focusOffset = Vector2.zero;

    [Header("Motion")]
    [SerializeField] private float focusDuration = 0.28f;
    [SerializeField] private Ease focusEase = Ease.OutCubic;

    private Tween _cameraMoveTween;
    private Tween _cameraZoomTween;
    private Vector3 _defaultCameraPosition;
    private float _defaultOrthographicSize;
    private bool _hasCapturedDefaultPose;

    public Camera TargetCamera => targetCamera;

    private void Awake()
    {
        targetCamera ??= Camera.main;
        CaptureDefaultPose();
    }

    private void OnDisable()
    {
        KillTweens();
    }

    public void FocusOnWorldPosition(Vector3 worldPosition)
    {
        if (!EnsureCameraReady())
        {
            return;
        }

        Vector3 targetPosition = new Vector3(
            worldPosition.x + focusOffset.x,
            worldPosition.y + focusOffset.y,
            _defaultCameraPosition.z);

        PlayCameraMove(targetPosition);
        PlayCameraZoom(focusedOrthographicSize);
    }

    public void ResetFocus()
    {
        if (!EnsureCameraReady())
        {
            return;
        }

        PlayCameraMove(_defaultCameraPosition);
        PlayCameraZoom(_defaultOrthographicSize);
    }

    private bool EnsureCameraReady()
    {
        targetCamera ??= Camera.main;
        if (targetCamera == null)
        {
            Debug.LogWarning("TerritoryMapCameraController requires a target camera.");
            return false;
        }

        if (!_hasCapturedDefaultPose)
        {
            CaptureDefaultPose();
        }

        return _hasCapturedDefaultPose;
    }

    private void CaptureDefaultPose()
    {
        if (targetCamera == null)
        {
            return;
        }

        _defaultCameraPosition = targetCamera.transform.position;
        _defaultOrthographicSize = targetCamera.orthographicSize;
        _hasCapturedDefaultPose = true;
    }

    private void PlayCameraMove(Vector3 targetPosition)
    {
        if (targetCamera == null)
        {
            return;
        }

        if (_cameraMoveTween != null)
        {
            _cameraMoveTween.Kill();
            _cameraMoveTween = null;
        }

        _cameraMoveTween = targetCamera.transform.DOMove(targetPosition, focusDuration)
            .SetEase(focusEase)
            .SetTarget(targetCamera.transform);
    }

    private void PlayCameraZoom(float targetOrthographicSize)
    {
        if (targetCamera == null)
        {
            return;
        }

        if (_cameraZoomTween != null)
        {
            _cameraZoomTween.Kill();
            _cameraZoomTween = null;
        }

        _cameraZoomTween = DOTween.To(
                () => targetCamera.orthographicSize,
                value => targetCamera.orthographicSize = value,
                targetOrthographicSize,
                focusDuration)
            .SetEase(focusEase)
            .SetTarget(targetCamera);
    }

    private void KillTweens()
    {
        if (_cameraMoveTween != null)
        {
            _cameraMoveTween.Kill();
            _cameraMoveTween = null;
        }

        if (_cameraZoomTween != null)
        {
            _cameraZoomTween.Kill();
            _cameraZoomTween = null;
        }
    }
}
