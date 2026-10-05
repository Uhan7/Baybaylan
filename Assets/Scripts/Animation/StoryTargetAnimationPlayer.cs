using System.Collections;
using UnityEngine;

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

    [Header("Safety")]
    [SerializeField, Min(0.1f)] private float maximumWaitSeconds = 5f;

    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
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
}
