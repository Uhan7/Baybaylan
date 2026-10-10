using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StoryTargetAnimationPlayer : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField, Min(0)] private int animatorLayer;
    [SerializeField] private string idleStateName = "Base Layer.Idle";
    [SerializeField] private string hitStateName = "Base Layer.Hit";

    [Header("Impact")]
    [SerializeField] private bool shakeScreenOnHit = true;
    [SerializeField, Min(0f)] private float screenShakeDuration = 0.12f;
    [SerializeField, Min(0f)] private float screenShakeIntensity = 8f;

    [Header("Heal Response")]
    [Tooltip("Only this character image pulses during Heal; the paper/background is left unchanged.")]
    [SerializeField] private Graphic healCharacterGraphic;
    [Tooltip("Aura emitted by the target after the healing projectile reaches it.")]
    [SerializeField] private ParticleSystem healAuraParticles;
    [SerializeField, Min(0f)] private float healAuraEmissionDuration = 0.25f;
    [Tooltip("RGB multiplier at the darkest point. 0.196 is approximately a value of 50 out of 255.")]
    [SerializeField, Range(0f, 1f)] private float healDarkness = 50f / 255f;
    [SerializeField, Min(0f)] private float healDarkenDuration = 0.22f;
    [SerializeField, Min(0f)] private float healDarkHoldDuration = 0.18f;
    [SerializeField, Min(0f)] private float healRestoreDuration = 0.4f;
    [SerializeField] private AnimationCurve healDarkenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve healRestoreCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Safety")]
    [SerializeField, Min(0.1f)] private float maximumWaitSeconds = 5f;

    private Image healPulseOverlay;
    private Image healPulseSource;

    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
        Transform character = transform.Find("Portrait Holder/Character Image");
        if (character != null) healCharacterGraphic = character.GetComponent<Graphic>();
    }

    private void Awake()
    {
        StopHealAura(true);
    }

    private void OnDisable()
    {
        StopHealAura(true);
        HideHealPulseOverlay();
    }

    public IEnumerator PlayHealResponseAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f) yield return new WaitForSeconds(delaySeconds);
        yield return PlayHealResponse();
    }

    public IEnumerator PlayHealResponse()
    {
        Image characterImage = ResolveVisibleHealCharacterImage();

        if (characterImage == null)
        {
            Debug.LogWarning($"{nameof(StoryTargetAnimationPlayer)} on {name} needs a Character Image for its heal pulse.", this);
            yield return PlayHealAura();
            yield break;
        }

        Image pulseOverlay = GetOrCreateHealPulseOverlay(characterImage);
        float darkestOverlayAlpha = (1f - healDarkness) * characterImage.color.a;
        yield return FadeOverlayAlpha(pulseOverlay, 0f, darkestOverlayAlpha, healDarkenDuration, healDarkenCurve);
        if (healDarkHoldDuration > 0f) yield return new WaitForSeconds(healDarkHoldDuration);
        yield return FadeOverlayAlpha(pulseOverlay, darkestOverlayAlpha, 0f, healRestoreDuration, healRestoreCurve);
        HideHealPulseOverlay();
        yield return PlayHealAura();
    }

    private Image ResolveVisibleHealCharacterImage()
    {
        Image assignedImage = healCharacterGraphic as Image;
        if (IsVisibleCharacterImage(assignedImage)) return assignedImage;

        Transform portraitHolder = transform.Find("Portrait Holder");
        if (portraitHolder == null) return assignedImage;

        Image fallback = assignedImage;
        Image[] images = portraitHolder.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (image.name != "Character Image" && image.name != "Next Character Image") continue;
            if (fallback == null) fallback = image;
            if (IsVisibleCharacterImage(image)) return image;
        }

        return fallback;
    }

    private static bool IsVisibleCharacterImage(Image image)
    {
        return image != null &&
            image.gameObject.activeInHierarchy &&
            image.enabled &&
            image.color.a > 0.001f;
    }

    private Image GetOrCreateHealPulseOverlay(Image source)
    {
        if (healPulseOverlay == null || healPulseSource != source)
        {
            if (healPulseOverlay != null) Destroy(healPulseOverlay.gameObject);

            GameObject overlayObject = new GameObject("Heal Pulse Overlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform overlayRect = overlayObject.GetComponent<RectTransform>();
            overlayRect.SetParent(source.rectTransform, false);
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            overlayRect.localRotation = Quaternion.identity;
            overlayRect.localScale = Vector3.one;

            healPulseOverlay = overlayObject.GetComponent<Image>();
            healPulseOverlay.raycastTarget = false;
            healPulseSource = source;
        }

        healPulseOverlay.sprite = source.sprite;
        healPulseOverlay.type = source.type;
        healPulseOverlay.preserveAspect = source.preserveAspect;
        healPulseOverlay.fillCenter = source.fillCenter;
        healPulseOverlay.fillMethod = source.fillMethod;
        healPulseOverlay.fillAmount = source.fillAmount;
        healPulseOverlay.fillClockwise = source.fillClockwise;
        healPulseOverlay.fillOrigin = source.fillOrigin;
        healPulseOverlay.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        healPulseOverlay.gameObject.SetActive(true);
        SetOverlayAlpha(healPulseOverlay, 0f);
        return healPulseOverlay;
    }

    private void HideHealPulseOverlay()
    {
        if (healPulseOverlay == null) return;
        SetOverlayAlpha(healPulseOverlay, 0f);
        healPulseOverlay.gameObject.SetActive(false);
    }

    private IEnumerator PlayHealAura()
    {
        StopHealAura(true);
        if (healAuraParticles == null) yield break;

        healAuraParticles.Play(true);
        if (healAuraEmissionDuration > 0f)
            yield return new WaitForSeconds(healAuraEmissionDuration);

        StopHealAura(false);
    }

    private void StopHealAura(bool clear)
    {
        if (healAuraParticles == null) return;

        ParticleSystemStopBehavior behavior = clear
            ? ParticleSystemStopBehavior.StopEmittingAndClear
            : ParticleSystemStopBehavior.StopEmitting;
        healAuraParticles.Stop(true, behavior);
    }

    public IEnumerator PlayHitAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f) yield return new WaitForSeconds(delaySeconds);
        yield return PlayHit();
    }

    public IEnumerator PlayHit()
    {
        if (!CanPlayState(hitStateName, out int stateHash)) yield break;

        if (shakeScreenOnHit)
            ScreenShake.ShakeGlobal(screenShakeDuration, screenShakeIntensity);

        targetAnimator.Play(stateHash, animatorLayer, 0f);
        yield return WaitForStateToFinish(stateHash, hitStateName);
        ReturnToIdle();
    }

    private bool CanPlayState(string stateName, out int stateHash)
    {
        stateHash = 0;

        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{nameof(StoryTargetAnimationPlayer)} on {name} needs an Animator with a controller.", this);
            return false;
        }

        if (animatorLayer < 0 || animatorLayer >= targetAnimator.layerCount)
        {
            Debug.LogWarning($"Animator layer {animatorLayer} does not exist on {targetAnimator.name}.", this);
            return false;
        }

        stateHash = Animator.StringToHash(stateName);
        if (targetAnimator.HasState(animatorLayer, stateHash)) return true;

        Debug.LogWarning($"Animator state '{stateName}' was not found on {targetAnimator.name}.", this);
        return false;
    }

    private IEnumerator WaitForStateToFinish(int stateHash, string stateName)
    {
        float deadline = Time.unscaledTime + maximumWaitSeconds;
        bool enteredState = false;

        while (Time.unscaledTime < deadline)
        {
            AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(animatorLayer);
            bool isRequestedState = stateInfo.fullPathHash == stateHash;

            if (isRequestedState)
            {
                enteredState = true;
                if (!targetAnimator.IsInTransition(animatorLayer) && stateInfo.normalizedTime >= 1f)
                    yield break;
            }
            else if (enteredState && !targetAnimator.IsInTransition(animatorLayer))
            {
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"Animator state '{stateName}' exceeded the {maximumWaitSeconds:0.##} second wait limit.", this);
    }

    private void ReturnToIdle()
    {
        if (string.IsNullOrWhiteSpace(idleStateName)) return;

        int idleStateHash = Animator.StringToHash(idleStateName);
        if (targetAnimator.HasState(animatorLayer, idleStateHash))
            targetAnimator.Play(idleStateHash, animatorLayer, 0f);
    }

    private static IEnumerator FadeOverlayAlpha(
        Image overlay,
        float from,
        float to,
        float duration,
        AnimationCurve curve)
    {
        if (duration <= 0f)
        {
            SetOverlayAlpha(overlay, to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float curvedProgress = curve == null ? progress : curve.Evaluate(progress);

            SetOverlayAlpha(overlay, Mathf.LerpUnclamped(from, to, curvedProgress));

            yield return null;
        }

        SetOverlayAlpha(overlay, to);
    }

    private static void SetOverlayAlpha(Image overlay, float alpha)
    {
        if (overlay != null) overlay.color = new Color(0f, 0f, 0f, alpha);
    }
}
