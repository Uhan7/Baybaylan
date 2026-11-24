using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public sealed class SunLightFlicker : MonoBehaviour
{
    [Tooltip("Minimum and maximum glow opacity. Keep the values close for subtle sunlight.")]
    [SerializeField] private Vector2 opacityRange = new Vector2(0.15f, 0.3f);
    [Tooltip("Speed of the smooth, irregular brightness changes.")]
    [SerializeField, Min(0f)] private float flickerSpeed = 0.45f;
    [SerializeField] private bool useUnscaledTime = true;

    private CanvasGroup glow;
    private float noiseOffset;
    private float elapsed;

    private void Awake()
    {
        glow = GetComponent<CanvasGroup>();
        noiseOffset = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        elapsed += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * flickerSpeed;
        float brightness = Mathf.PerlinNoise(noiseOffset, elapsed);
        glow.alpha = Mathf.Lerp(
            Mathf.Clamp01(opacityRange.x), Mathf.Clamp01(opacityRange.y), brightness);
    }
}
