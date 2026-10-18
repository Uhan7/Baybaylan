using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CursorUILight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image overlayImage;

    [Header("Light Shape")]
    [FormerlySerializedAs("clearRadius")]
    [SerializeField, Min(0f)] private float innerRadius = 10f;
    [FormerlySerializedAs("edgeSoftness")]
    [SerializeField, Min(1f)] private float outerRadius = 2200f;
    [SerializeField] private Vector2 cursorOffset;

    [Header("Darkness")]
    [SerializeField] private Color darknessColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float darknessOpacity = 0.55f;

    [Header("Movement")]
    [Tooltip("0 follows instantly. Higher values add smooth movement.")]
    [SerializeField, Min(0f)] private float followSharpness;
    [SerializeField] private bool clampToScreen = true;

    private static readonly int LightCenterId = Shader.PropertyToID("_LightCenter");
    private static readonly int RectSizeId = Shader.PropertyToID("_RectSize");
    private static readonly int InnerRadiusId = Shader.PropertyToID("_InnerRadius");
    private static readonly int OuterRadiusId = Shader.PropertyToID("_OuterRadius");
    private static readonly int DarknessColorId = Shader.PropertyToID("_DarknessColor");

    private Material runtimeMaterial;
    private RectTransform overlayRect;
    private Vector2 currentScreenPosition;
    private bool hasPosition;

    private void Awake()
    {
        if (overlayImage == null)
        {
            overlayImage = GetComponentInChildren<Image>();
        }

        if (overlayImage == null || overlayImage.material == null)
        {
            Debug.LogError("Cursor UI Light needs an overlay Image with the cursor-light material.", this);
            enabled = false;
            return;
        }

        overlayRect = overlayImage.rectTransform;
        runtimeMaterial = new Material(overlayImage.material)
        {
            name = overlayImage.material.name + " (Runtime)"
        };
        overlayImage.material = runtimeMaterial;
        overlayImage.raycastTarget = false;
    }

    private void LateUpdate()
    {
        Vector2 targetPosition = (Vector2)Input.mousePosition + cursorOffset;

        if (clampToScreen)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, 0f, Screen.width);
            targetPosition.y = Mathf.Clamp(targetPosition.y, 0f, Screen.height);
        }

        if (!hasPosition || followSharpness <= 0f)
        {
            currentScreenPosition = targetPosition;
            hasPosition = true;
        }
        else
        {
            float blend = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            currentScreenPosition = Vector2.Lerp(currentScreenPosition, targetPosition, blend);
        }

        UpdateMaterial(currentScreenPosition);
    }

    private void UpdateMaterial(Vector2 screenPosition)
    {
        if (runtimeMaterial == null || overlayRect == null)
        {
            return;
        }

        Canvas canvas = overlayImage.canvas;
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect, screenPosition, eventCamera, out Vector2 localPosition))
        {
            return;
        }

        Rect rect = overlayRect.rect;
        Vector2 rectSize = rect.size;
        if (rectSize.x <= 0f || rectSize.y <= 0f)
        {
            return;
        }

        Vector2 normalizedCenter = new Vector2(
            Mathf.InverseLerp(rect.xMin, rect.xMax, localPosition.x),
            Mathf.InverseLerp(rect.yMin, rect.yMax, localPosition.y));

        Color finalDarknessColor = darknessColor;
        finalDarknessColor.a *= darknessOpacity;

        runtimeMaterial.SetVector(LightCenterId, normalizedCenter);
        runtimeMaterial.SetVector(RectSizeId, rectSize);
        runtimeMaterial.SetFloat(InnerRadiusId, innerRadius);
        runtimeMaterial.SetFloat(OuterRadiusId, Mathf.Max(innerRadius + 1f, outerRadius));
        runtimeMaterial.SetColor(DarknessColorId, finalDarknessColor);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
