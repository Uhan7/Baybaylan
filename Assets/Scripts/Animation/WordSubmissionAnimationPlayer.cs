using System.Collections;
using UnityEngine;

public class WordSubmissionAnimationPlayer : MonoBehaviour
{
    public enum SuccessfulWordAnimation
    {
        DoNothing,
        Attack,
        Heal
    }

    [Header("Successful Word Result")]
    [SerializeField] private SuccessfulWordAnimation animationAfterSuccessfulWord;

    [Header("Animator")]
    [Tooltip("The Animator that contains the Attack and Heal states.")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField, Min(0)] private int animatorLayer;
    [SerializeField] private string idleStateName = "Base Layer.Idle";
    [SerializeField] private string attackStateName = "Base Layer.Attack";
    [SerializeField] private string healStateName = "Base Layer.Heal";

    [Header("Attack Target")]
    [SerializeField] private StoryTargetAnimationPlayer storyTarget;
    [Tooltip("The Attack clip reaches the target on frame 11 at 60 FPS.")]
    [SerializeField, Min(0f)] private float targetHitDelay = 11f / 60f;

    [Header("Safety")]
    [Tooltip("Stops a looping or misconfigured state from blocking the rest of the turn forever.")]
    [SerializeField, Min(0.1f)] private float maximumWaitSeconds = 10f;

    public SuccessfulWordAnimation AnimationAfterSuccessfulWord
    {
        get => animationAfterSuccessfulWord;
        set => animationAfterSuccessfulWord = value;
    }

    private void Reset()
    {
        targetAnimator = GetComponent<Animator>();
        if (targetAnimator == null) targetAnimator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (storyTarget == null)
            storyTarget = FindFirstObjectByType<StoryTargetAnimationPlayer>();
    }

    public IEnumerator PlaySelectedAnimation()
    {
        yield return PlayAnimation(animationAfterSuccessfulWord);
    }

    public IEnumerator PlayAnimation(SuccessfulWordAnimation animation)
    {
        if (animation == SuccessfulWordAnimation.DoNothing) yield break;

        if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning($"{nameof(WordSubmissionAnimationPlayer)} on {name} needs an Animator with a controller.", this);
            yield break;
        }

        if (animatorLayer < 0 || animatorLayer >= targetAnimator.layerCount)
        {
            Debug.LogWarning($"Animator layer {animatorLayer} does not exist on {targetAnimator.name}.", this);
            yield break;
        }

        string stateName = animation == SuccessfulWordAnimation.Attack
            ? attackStateName
            : healStateName;

        if (string.IsNullOrWhiteSpace(stateName))
        {
            Debug.LogWarning($"No Animator state was configured for {animation} on {name}.", this);
            yield break;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (!targetAnimator.HasState(animatorLayer, stateHash))
        {
            Debug.LogWarning($"Animator state '{stateName}' was not found on {targetAnimator.name}.", this);
            yield break;
        }

        Coroutine targetHit = null;
        if (animation == SuccessfulWordAnimation.Attack && storyTarget != null)
            targetHit = StartCoroutine(storyTarget.PlayHitAfterDelay(targetHitDelay));

        targetAnimator.Play(stateHash, animatorLayer, 0f);
        yield return WaitForStateToFinish(stateHash, stateName);
        ReturnToIdle();

        if (targetHit != null) yield return targetHit;
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
