using UnityEngine;
using UnityEngine.UI;

// Canvas graphics do not receive Unity lighting. Display this Light's color and
// intensity through an authored UI halo, without replacing the UI's material.
[ExecuteAlways]
public class UILightOverlay : MonoBehaviour
{
    [SerializeField] private Image haloImage;
    [SerializeField] private Color glowColor = Color.white;
    private float runtimeStrength = 1f;
    public void SetStrength(float strength) => runtimeStrength = Mathf.Clamp01(strength);
    [Tooltip("Light intensity that makes the UI halo fully visible.")]
    [SerializeField, Min(0.01f)] private float fullGlowIntensity = 2f;

    private Light lightSource;

    private void OnEnable() => Refresh();
    private void OnValidate() => Refresh();
    private void LateUpdate() => Refresh();
    private void OnDidApplyAnimationProperties() => Refresh();

    private void Refresh()
    {
        if (haloImage == null) return;
        if (lightSource == null) lightSource = GetComponent<Light>();

        Color color = lightSource != null ? lightSource.color : glowColor;
        color.a = lightSource == null ? glowColor.a : lightSource.enabled && isActiveAndEnabled
            ? Mathf.Clamp01(lightSource.intensity / Mathf.Max(0.01f, fullGlowIntensity))
            : 0f;
        color.a *= Application.isPlaying ? runtimeStrength : 1f;
        haloImage.color = color;
    }

    private void OnDisable()
    {
        if (haloImage == null) return;
        Color color = haloImage.color;
        color.a = 0f;
        haloImage.color = color;
    }
}
