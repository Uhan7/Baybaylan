using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

// The prefab owns the effect. Only the horizontal emission area follows the bar.
public sealed class MahikaParticles : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private UIParticle canvasParticles;

    private bool emissionEnabled;

    private void Awake()
    {
        if (particles != null)
            emissionEnabled = particles.emission.enabled;
    }

    private void OnEnable() => UpdateEmissionArea();

    // The layout can change the bar width independently of the Mahika value.
    private void LateUpdate() => UpdateEmissionArea();

    private void UpdateEmissionArea()
    {
        if (fill == null || particles == null || canvasParticles == null) return;

        Rect rect = fill.GetPixelAdjustedRect();
        float filledWidth = rect.width * fill.fillAmount;
        float particleScale = canvasParticles.scale;
        bool hasFill = fill.isActiveAndEnabled && fill.fillAmount >= 0.001f &&
            filledWidth > 0f && particleScale > 0f;
        var emission = particles.emission;
        emission.enabled = emissionEnabled && hasFill;
        if (!hasFill)
        {
            if (particles.particleCount > 0) particles.Clear(false);
            return;
        }

        float centerX = fill.fillOrigin == (int)Image.OriginHorizontal.Right
            ? rect.xMax - filledWidth * 0.5f
            : rect.xMin + filledWidth * 0.5f;

        var shape = particles.shape;
        Vector3 size = shape.scale;
        size.x = filledWidth / particleScale;
        shape.scale = size;

        Vector3 position = shape.position;
        position.x = centerX / particleScale;
        shape.position = position;
    }
}
