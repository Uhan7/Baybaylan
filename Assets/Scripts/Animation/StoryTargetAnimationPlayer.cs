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
    [Tooltip("Usually the Portrait Holder. Every active UI Graphic below it briefly darkens when healed.")]
    [SerializeField] private Transform healVisualRoot;
    [Tooltip("RGB multiplier at the darkest point. 0.196 is approximately a value of 50 out of 255.")]
    [SerializeField, Range(0f, 1f)] private float healDarkness = 50f / 255f;
    [SerializeField, Min(0f)] private float healDarkenDuration = 0.18f;
    [SerializeField, Min(0f)] private float healDarkHoldDuration = 0.12f;
    [SerializeField, Min(0f)] private float healRestoreDuration = 0.4f;
    [SerializeField] private AnimationCurve healDarkenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve healRestoreCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Safety")]
    [SerializeField, Min(0.1f)] private float maximumWaitSeconds = 5f;

    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
        healVisualRoot = transform.Find("Portrait Holder");
    }

    public IEnumerator PlayHealResponseAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f) yield return new WaitForSeconds(delaySeconds);
        yield return PlayHealResponse();
    }

    public IEnumerator PlayHealResponse()
    {
        Transform visualRoot = healVisualRoot != null ? healVisualRoot : transform.Find("Portrait Holder");
        if (visualRoot == null)
        {
            Debug.LogWarning($"{nameof(StoryTargetAnimationPlayer)} on {name} needs a Heal Visual Root.", this);
            yield break;
        }

        Graphic[] graphics = visualRoot.GetComponentsInChildren<Graphic>(false);
        if (graphics.Length == 0) yield break;

        Color[] originalColors = new Color[graphics.Length];
        Color[] darkColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
        {
            originalColors[i] = graphics[i].color;
            Color original = originalColors[i];
            darkColors[i] = new Color(
                original.r * healDarkness,
                original.g * healDarkness,
                original.b * healDarkness,
                original.a);
        }

        yield return FadeGraphics(graphics, originalColors, darkColors, healDarkenDuration, healDarkenCurve);
        if (healDarkHoldDuration > 0f) yield return new WaitForSeconds(healDarkHoldDuration);
        yield return FadeGraphics(graphics, darkColors, originalColors, healRestoreDuration, healRestoreCurve);

        SetGraphicColors(graphics, originalColors);
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

    private static IEnumerator FadeGraphics(
        Graphic[] graphics,
        Color[] from,
        Color[] to,
        float duration,
        AnimationCurve curve)
    {
        if (duration <= 0f)
        {
            SetGraphicColors(graphics, to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float curvedProgress = curve == null ? progress : curve.Evaluate(progress);

            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                    graphics[i].color = Color.LerpUnclamped(from[i], to[i], curvedProgress);
            }

            yield return null;
        }

        SetGraphicColors(graphics, to);
    }

    private static void SetGraphicColors(Graphic[] graphics, Color[] colors)
    {
        for (int i = 0; i < graphics.Length; i++)
        {
            if (graphics[i] != null) graphics[i].color = colors[i];
        }
    }
}
