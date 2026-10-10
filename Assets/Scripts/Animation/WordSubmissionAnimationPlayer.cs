using System.Collections;
using NaughtyAttributes;
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

    [ShowIf(nameof(HasSelectedAnimation)), Header("Animator")]
    [Tooltip("The Animator that contains the Attack and Heal states.")]
    [SerializeField] private Animator targetAnimator;
    [ShowIf(nameof(HasSelectedAnimation)), SerializeField, Min(0)] private int animatorLayer;
    [ShowIf(nameof(HasSelectedAnimation)), SerializeField] private string idleStateName = "Base Layer.Idle";
    [ShowIf(nameof(IsAttackSelected)), SerializeField] private string attackStateName = "Base Layer.Attack";
    [ShowIf(nameof(IsHealSelected)), SerializeField] private string healStateName = "Base Layer.Heal";

    [ShowIf(nameof(HasSelectedAnimation)), Header("Story Target")]
    [SerializeField] private StoryTargetAnimationPlayer storyTarget;
    [ShowIf(nameof(IsAttackSelected))]
    [Tooltip("The Attack clip reaches the target on frame 11 at 60 FPS.")]
    [SerializeField, Min(0f)] private float targetHitDelay = 11f / 60f;

    [ShowIf(nameof(IsHealSelected)), Header("Heal Effects")]
    [Tooltip("Particles sent from the protagonist toward the story target.")]
    [SerializeField] private ParticleSystem healMagicParticles;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Speeds up only the Heal animation. Effect timings below are measured in real game seconds.")]
    [SerializeField, Min(0.01f)] private float healAnimationSpeed = 1.75f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Time from the start of Heal before the projectile begins emitting.")]
    [SerializeField, Min(0f)] private float healMagicDelay = 23f / 60f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Time from the start of Heal before the magic projectile stops emitting.")]
    [SerializeField, Min(0f)] private float healEffectsStopDelay = 0.58f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("The target waits for every projectile particle to finish before starting its dim pulse.")]
    [SerializeField, Min(0f)] private float targetHealDelay = 0.92f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Where to pause the Heal state while the target effect plays. 0.7 is frame 84 of the current 120-frame clip.")]
    [SerializeField, Range(0f, 1f)] private float healPoseHoldNormalizedTime = 0.7f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Fallback hold time used only when no Story Target is available.")]
    [SerializeField, Min(0f)] private float healPoseHoldDuration = 0.8f;
    [ShowIf(nameof(IsHealSelected))]
    [Tooltip("Small beat after the target aura appears before the protagonist returns.")]
    [SerializeField, Min(0f)] private float healReturnDelayAfterTarget;

    [ShowIf(nameof(HasSelectedAnimation)), Header("Safety")]
    [Tooltip("Stops a looping or misconfigured state from blocking the rest of the turn forever.")]
    [SerializeField, Min(0.1f)] private float maximumWaitSeconds = 10f;

    private float animatorSpeedBeforeHeal = float.NaN;
    private float animatorSpeedDuringHeal = 1f;
    private bool healTargetSequenceFinished;

    public SuccessfulWordAnimation AnimationAfterSuccessfulWord
    {
        get => animationAfterSuccessfulWord;
        set => animationAfterSuccessfulWord = value;
    }

    private bool HasSelectedAnimation() => animationAfterSuccessfulWord != SuccessfulWordAnimation.DoNothing;

    private bool IsAttackSelected() => animationAfterSuccessfulWord == SuccessfulWordAnimation.Attack;

    private bool IsHealSelected() => animationAfterSuccessfulWord == SuccessfulWordAnimation.Heal;

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
        RestoreAnimatorSpeed();
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
            BeginHealAnimatorSpeed();
            healTargetSequenceFinished = storyTarget == null;
            healEffects = StartCoroutine(PlayHealEffects());
            if (storyTarget != null)
                targetResponse = StartCoroutine(PlayTargetHealSequence());
        }

        targetAnimator.Play(stateHash, animatorLayer, 0f);
        if (animation == SuccessfulWordAnimation.Heal)
            StartCoroutine(HoldHealPose(stateHash));

        yield return WaitForStateToFinish(stateHash, stateName);
        ReturnToIdle();
        if (animation == SuccessfulWordAnimation.Heal) RestoreAnimatorSpeed();

        if (healEffects != null) yield return healEffects;
        if (targetResponse != null) yield return targetResponse;
    }

    private IEnumerator PlayHealEffects()
    {
        StopHealParticles(true);

        float magicTime = Mathf.Max(0f, healMagicDelay);
        float stopTime = Mathf.Max(magicTime, healEffectsStopDelay);

        if (magicTime > 0f) yield return new WaitForSeconds(magicTime);
        PlayParticleSystem(healMagicParticles);

        if (stopTime > magicTime) yield return new WaitForSeconds(stopTime - magicTime);
        StopHealParticles(false);
    }

    private IEnumerator HoldHealPose(int stateHash)
    {
        float deadline = Time.unscaledTime + maximumWaitSeconds;
        float holdPoint = Mathf.Clamp01(healPoseHoldNormalizedTime);

        while (Time.unscaledTime < deadline)
        {
            AnimatorStateInfo stateInfo = targetAnimator.GetCurrentAnimatorStateInfo(animatorLayer);
            if (stateInfo.fullPathHash == stateHash && stateInfo.normalizedTime >= holdPoint)
                break;

            yield return null;
        }

        if (Time.unscaledTime >= deadline) yield break;

        targetAnimator.speed = 0f;

        if (storyTarget == null)
        {
            if (healPoseHoldDuration > 0f) yield return new WaitForSeconds(healPoseHoldDuration);
        }
        else
        {
            while (!healTargetSequenceFinished && Time.unscaledTime < deadline)
                yield return null;

            if (healReturnDelayAfterTarget > 0f)
                yield return new WaitForSeconds(healReturnDelayAfterTarget);
        }

        targetAnimator.speed = animatorSpeedDuringHeal;
    }

    private IEnumerator PlayTargetHealSequence()
    {
        yield return storyTarget.PlayHealResponseAfterDelay(targetHealDelay);
        healTargetSequenceFinished = true;
    }

    private void BeginHealAnimatorSpeed()
    {
        animatorSpeedBeforeHeal = targetAnimator.speed;
        animatorSpeedDuringHeal = animatorSpeedBeforeHeal * Mathf.Max(0.01f, healAnimationSpeed);
        targetAnimator.speed = animatorSpeedDuringHeal;
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

        if (healMagicParticles != null) healMagicParticles.Stop(true, behavior);
    }

    private void RestoreAnimatorSpeed()
    {
        if (targetAnimator == null || float.IsNaN(animatorSpeedBeforeHeal)) return;

        targetAnimator.speed = animatorSpeedBeforeHeal;
        animatorSpeedBeforeHeal = float.NaN;
        animatorSpeedDuringHeal = 1f;
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
