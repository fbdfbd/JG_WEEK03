using UnityEngine;

/// <summary>Attach to the battle scene's Main Camera. Strength is in world units.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CombatCameraShake : MonoBehaviour
{
    [Header("Confirmed Hit")]
    [Min(0f)] [SerializeField] private float hitStrength = 0.06f;
    [Min(0f)] [SerializeField] private float hitDuration = 0.08f;

    [Header("Player Damaged")]
    [Min(0f)] [SerializeField] private float damageStrength = 0.18f;
    [Min(0f)] [SerializeField] private float damageDuration = 0.16f;

    [Header("Motion")]
    [Min(0.01f)] [SerializeField] private float frequency = 35f;
    [SerializeField] private bool useUnscaledTime = true;

    private Vector3 restLocalPosition;
    private float strength;
    private float duration;
    private float elapsed;
    private float noiseSeed;
    private bool isShaking;

    public static void RequestConfirmedHit()
    {
        CombatCameraShake shake = GetMainCameraShake();
        if (shake != null)
        {
            shake.PlayShake(shake.hitStrength, shake.hitDuration);
        }
    }

    public static void RequestPlayerDamaged()
    {
        CombatCameraShake shake = GetMainCameraShake();
        if (shake != null)
        {
            shake.PlayShake(shake.damageStrength, shake.damageDuration);
        }
    }

    private static CombatCameraShake GetMainCameraShake()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.TryGetComponent(out CombatCameraShake shake)
            || !shake.isActiveAndEnabled)
        {
            return null;
        }

        return shake;
    }

    private void PlayShake(float requestedStrength, float requestedDuration)
    {
        if (requestedStrength <= 0f || requestedDuration <= 0f)
        {
            return;
        }

        // A weak hit must not interrupt a stronger damage shake. Repeated hits
        // restart the envelope without adding amplitudes or moving the origin.
        if (isShaking && requestedStrength < strength)
        {
            return;
        }

        if (!isShaking)
        {
            restLocalPosition = transform.localPosition;
        }

        strength = requestedStrength;
        duration = requestedDuration;
        elapsed = 0f;
        noiseSeed = Random.Range(0f, 1000f);
        isShaking = true;
    }

    private void LateUpdate()
    {
        if (!isShaking)
        {
            return;
        }

        elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (elapsed >= duration)
        {
            StopShake();
            return;
        }

        float fade = 1f - elapsed / duration;
        float sampleTime = elapsed * frequency;
        Vector2 offset = new Vector2(
            Mathf.PerlinNoise(noiseSeed, sampleTime) * 2f - 1f,
            Mathf.PerlinNoise(noiseSeed + 100f, sampleTime) * 2f - 1f);

        transform.localPosition = restLocalPosition
            + new Vector3(offset.x, offset.y, 0f) * (strength * fade);
    }

    private void OnDisable()
    {
        StopShake();
    }

    public void StopShake()
    {
        if (!isShaking)
        {
            return;
        }

        transform.localPosition = restLocalPosition;
        isShaking = false;
    }
}
