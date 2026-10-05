using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }

    [Header("Shake Targets")]
    [Tooltip("UI roots or world transforms to shake together. A root Canvas uses its direct children when this list is empty.")]
    [SerializeField] private List<Transform> shakeTargets = new List<Transform>();

    [Header("Default Shake")]
    [SerializeField, Min(0f)] private float defaultDuration = 0.2f;
    [SerializeField, Min(0f)] private float defaultIntensity = 8f;
    [SerializeField, Min(0.01f)] private float frequency = 30f;
    [SerializeField] private AnimationCurve strengthOverTime =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private bool useUnscaledTime = true;

    private readonly List<Transform> activeTargets = new List<Transform>();
    private Vector3 appliedOffset;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Multiple {nameof(ScreenShake)} components are active. Using {name} as the global instance.", this);
        }

        Instance = this;
    }

    private void OnDisable()
    {
        StopShake();
        if (Instance == this) Instance = null;
    }

    [ContextMenu("Test Default Shake")]
    public void Shake()
    {
        Shake(defaultDuration, defaultIntensity);
    }

    public void Shake(float intensityMultiplier)
    {
        Shake(defaultDuration, defaultIntensity * Mathf.Max(0f, intensityMultiplier));
    }

    public void Shake(float duration, float intensity)
    {
        StopShake();

        duration = Mathf.Max(0f, duration);
        intensity = Mathf.Max(0f, intensity);
        if (duration <= 0f || intensity <= 0f) return;

        CacheActiveTargets();
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, intensity));
    }

    public static bool ShakeGlobal()
    {
        if (Instance == null) return false;
        Instance.Shake();
        return true;
    }

    public static bool ShakeGlobal(float duration, float intensity)
    {
        if (Instance == null) return false;
        Instance.Shake(duration, intensity);
        return true;
    }

    public static bool ShakeGlobal(float intensityMultiplier)
    {
        if (Instance == null) return false;
        Instance.Shake(intensityMultiplier);
        return true;
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        RemoveAppliedOffset();
        activeTargets.Clear();
    }

    private IEnumerator ShakeRoutine(float duration, float intensity)
    {
        float elapsed = 0f;
        float horizontalSeed = Random.value * 1000f;
        float verticalSeed = Random.value * 1000f;

        while (elapsed < duration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float strengthMultiplier = strengthOverTime != null && strengthOverTime.length > 0
                ? strengthOverTime.Evaluate(normalizedTime)
                : 1f - normalizedTime;
            float currentStrength = intensity * strengthMultiplier;
            float sampleTime = elapsed * frequency;

            float horizontal = Mathf.PerlinNoise(horizontalSeed, sampleTime) * 2f - 1f;
            float vertical = Mathf.PerlinNoise(verticalSeed, sampleTime) * 2f - 1f;
            ApplyOffset(new Vector3(horizontal, vertical, 0f) * currentStrength);

            yield return null;
        }

        RemoveAppliedOffset();
        activeTargets.Clear();
        shakeCoroutine = null;
    }

    private void CacheActiveTargets()
    {
        activeTargets.Clear();

        foreach (Transform target in shakeTargets)
        {
            if (target != null && !activeTargets.Contains(target)) activeTargets.Add(target);
        }

        if (activeTargets.Count > 0) return;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.isRootCanvas)
        {
            foreach (Transform child in transform) activeTargets.Add(child);
        }

        if (activeTargets.Count == 0) activeTargets.Add(transform);
    }

    private void ApplyOffset(Vector3 newOffset)
    {
        foreach (Transform target in activeTargets)
        {
            if (target == null) continue;
            target.localPosition = target.localPosition - appliedOffset + newOffset;
        }

        appliedOffset = newOffset;
    }

    private void RemoveAppliedOffset()
    {
        if (appliedOffset == Vector3.zero) return;

        foreach (Transform target in activeTargets)
        {
            if (target != null) target.localPosition -= appliedOffset;
        }

        appliedOffset = Vector3.zero;
    }
}
