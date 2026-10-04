using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageBlinker : MonoBehaviour
{
    [Header("Blink Settings")]
    [Tooltip("When enabled, this UI element continuously blinks until disabled.")]
    public bool allowBlink;
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.5f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 1.0f;
    [SerializeField] private bool useUnscaledTime = true;

    private Image targetImage;
    private CanvasGroup targetCanvasGroup;
    private Coroutine blinkRoutine;
    private float restingAlpha = 1f;

    private void Awake()
    {
        if (!ResolveTarget())
        {
            enabled = false;
            return;
        }

        restingAlpha = GetAlpha();
    }

    private void OnEnable()
    {
        if (allowBlink) StartBlinkingRoutine();
    }

    private void OnDisable()
    {
        StopBlinkingRoutine();
    }

    public void SetAllowBlink(bool shouldBlink)
    {
        allowBlink = shouldBlink;

        if (!isActiveAndEnabled) return;

        if (allowBlink) StartBlinkingRoutine();
        else StopBlinkingRoutine();
    }

    private void StartBlinkingRoutine()
    {
        if (!ResolveTarget()) return;
        if (blinkRoutine != null) return;

        restingAlpha = GetAlpha();
        blinkRoutine = StartCoroutine(Blink());
    }

    private void StopBlinkingRoutine()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (targetCanvasGroup || targetImage) SetAlpha(restingAlpha);
    }

    private IEnumerator Blink()
    {
        float elapsedTime = 0f;

        while (true)
        {
            float phase = Mathf.PingPong(elapsedTime / fadeDuration, 1f);
            float alpha = Mathf.Lerp(restingAlpha, minimumAlpha, Mathf.SmoothStep(0f, 1f, phase));
            SetAlpha(alpha);

            elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    private void SetAlpha(float alpha)
    {
        if (targetCanvasGroup)
        {
            targetCanvasGroup.alpha = alpha;
            return;
        }

        Color color = targetImage.color;
        color.a = alpha;
        targetImage.color = color;
    }

    private float GetAlpha()
    {
        return targetCanvasGroup ? targetCanvasGroup.alpha : targetImage.color.a;
    }

    private bool ResolveTarget()
    {
        if (!targetCanvasGroup) targetCanvasGroup = GetComponent<CanvasGroup>();
        if (!targetCanvasGroup && !targetImage) targetImage = GetComponent<Image>();

        if (targetCanvasGroup || targetImage) return true;

        Debug.LogError("ImageBlinker requires an Image or CanvasGroup on the same GameObject.", this);
        return false;
    }
}
