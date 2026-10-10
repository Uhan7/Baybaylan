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

    [Header("Heal Effects")]
    [Tooltip("Soft aura emitted around the protagonist while casting Heal.")]
    [SerializeField] private ParticleSystem healAuraParticles;
    [Tooltip("Particles sent from the protagonist toward the story target.")]
    [SerializeField] private ParticleSystem healMagicParticles;
    [SerializeField, Min(0f)] private float healAuraDelay = 0.35f;
    [SerializeField, Min(0f)] private float healMagicDelay = 0.72f;
    [Tooltip("Time from the start of Heal before both emitters stop producing new particles.")]
    [SerializeField, Min(0f)] private float healEffectsStopDelay = 1.35f;
    [Tooltip("Time from the start of Heal before the target begins its darken-and-restore response.")]
    [SerializeField, Min(0f)] private float targetHealDelay = 0.92f;

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

        StopHealParticles(true);
    }

    private void OnDisable()
    {
        StopHealParticles(true);
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

        Coroutine targetResponse = null;
        Coroutine healEffects = null;
        if (animation == SuccessfulWordAnimation.Attack && storyTarget != null)
            targetResponse = StartCoroutine(storyTarget.PlayHitAfterDelay(targetHitDelay));
        else if (animation == SuccessfulWordAnimation.Heal)
        {
            healEffects = StartCoroutine(PlayHealEffects());
            if (storyTarget != null)
                targetResponse = StartCoroutine(storyTarget.PlayHealResponseAfterDelay(targetHealDelay));
        }

        targetAnimator.Play(stateHash, animatorLayer, 0f);
        yield return WaitForStateToFinish(stateHash, stateName);
        ReturnToIdle();

        if (healEffects != null) yield return healEffects;
        if (targetResponse != null) yield return targetResponse;
    }

    private IEnumerator PlayHealEffects()
    {
        StopHealParticles(true);

        float auraTime = Mathf.Max(0f, healAuraDelay);
        float magicTime = Mathf.Max(auraTime, healMagicDelay);
        float stopTime = Mathf.Max(magicTime, healEffectsStopDelay);

        if (auraTime > 0f) yield return new WaitForSeconds(auraTime);
        PlayParticleSystem(healAuraParticles);

        if (magicTime > auraTime) yield return new WaitForSeconds(magicTime - auraTime);
        PlayParticleSystem(healMagicParticles);

        if (stopTime > magicTime) yield return new WaitForSeconds(stopTime - magicTime);
        StopHealParticles(false);
    }

    private static void PlayParticleSystem(ParticleSystem particles)
    {
        if (particles == null) return;
        particles.Play(true);
    }

    private void StopHealParticles(bool clear)
    {
        ParticleSystemStopBehavior behavior = clear
            ? ParticleSystemStopBehavior.StopEmittingAndClear
            : ParticleSystemStopBehavior.StopEmitting;

        if (healAuraParticles != null) healAuraParticles.Stop(true, behavior);
        if (healMagicParticles != null) healMagicParticles.Stop(true, behavior);
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
