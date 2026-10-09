using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using System;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private class AksyonEvent
    {
        public int aksyonNumber = 1;
        public UnityEvent eventOnAksyon;
    }

    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static GameManager Instance;

    [Header("Configurations")]
    [SerializeField] public LevelConfig config; // References whole game

    [Header("Wordlists")]
    [SerializeField] private TextAsset[] wordlists;
    [HideInInspector] public HashSet<string> validWords = new HashSet<string>();
    [SerializeField] public List<string> wordsUsed = new List<string>();

    [Header("Winning Attack Slow Motion")]
    [Tooltip("Only slows time when the submitted word reaches the required Mahika and the selected result animation is Attack.")]
    [SerializeField] private bool enableWinningAttackSlowMotion = true;
    [Tooltip("Real-time duration of the effect. This is not stretched by Time.timeScale.")]
    [SerializeField, Min(0f)] private float winningAttackSlowMotionDuration = 1.8f;
    [Tooltip("X is normalized real time (0-1). Y is a multiplier for the time scale that was active before the effect.")]
    [SerializeField] private AnimationCurve winningAttackTimeScaleCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.18f, 0.45f),
        new Keyframe(0.72f, 0.45f),
        new Keyframe(1f, 1f));

    [Header("Winning Attack Character Swap")]
    [SerializeField] private bool enableWinningAttackCharacterSwap = true;
    [Tooltip("Optional. If empty, Character Image is found automatically on the story target.")]
    [SerializeField] private Image winningAttackCurrentCharacterImage;
    [Tooltip("Optional. If empty, Next Character Image is found automatically on the story target.")]
    [SerializeField] private Image winningAttackNextCharacterImage;
    [Tooltip("Scaled game-time seconds after the winning attack starts. Match this to the attack's impact frame.")]
    [SerializeField, Min(0f)] private float winningAttackCharacterSwapDelay = 0.6f;
    [Tooltip("Real-time duration of the crossfade, so slow motion does not make the fade sluggish.")]
    [SerializeField, Min(0f)] private float winningAttackCharacterSwapFadeDuration = 0.15f;
    [Tooltip("X is normalized fade time. Y is the blend from the current image to the next image.")]
    [SerializeField] private AnimationCurve winningAttackCharacterSwapCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine winningAttackSlowMotionCoroutine;
    private Coroutine winningAttackCharacterSwapCoroutine;
    private bool controlsTimeScale;
    private float timeScaleBeforeWinningAttack = 1f;
    private float fixedDeltaTimeBeforeWinningAttack = 0.02f;

    //[Header("Other Screens")]
    //[SerializeField] private GameObject winScreen;
    //[SerializeField] private GameObject loseScreen;

    [Header("Events")]
    [SerializeField] private UnityEvent eventOnWin;
    [SerializeField] private UnityEvent eventOnLose;
    [SerializeField] private List<AksyonEvent> eventsOnAksyon = new List<AksyonEvent>();

    [Header("Should not be here but idgaf")]
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip loseSFX;
    [SerializeField] private AudioClip winAmbience;

    // Main Functions ----------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadWordlist();
    }

    private void Start()
    {
        if (config == null) return;

        // Initialize Mahika Manager
        if (MahikaManager.Instance != null)
            MahikaManager.Instance.Initialize(config);

        if (DahonNgKawayanUI.Instance != null)
            DahonNgKawayanUI.Instance.getLevelConfig(config);

        PrepareWinningAttackCharacterSwap();
    }

    // Helper Functions --------------------------------------------------------
    public void aquireAlahasAfterWin()
    {
        if(!config.alahasAquiredAfterWin)
            return;

        if (TalaAlahasHolder.Instance != null)
            TalaAlahasHolder.Instance.Unlock(config.alahasAquiredAfterWin);
    }

    public void InvokeEventsOnAksyon(int aksyonNumber)
    {
        foreach (AksyonEvent aksyonEvent in eventsOnAksyon)
        {
            if (aksyonEvent != null && aksyonEvent.aksyonNumber == aksyonNumber)
                aksyonEvent.eventOnAksyon?.Invoke();
        }
    }

    public Coroutine StartWinningAttackSlowMotion()
    {
        if (!enableWinningAttackSlowMotion ||
            winningAttackSlowMotionDuration <= 0f ||
            winningAttackTimeScaleCurve == null ||
            winningAttackTimeScaleCurve.length == 0)
        {
            return null;
        }

        if (winningAttackSlowMotionCoroutine != null)
        {
            StopCoroutine(winningAttackSlowMotionCoroutine);
            winningAttackSlowMotionCoroutine = null;
            RestoreTimeScaleAfterWinningAttack();
        }

        winningAttackSlowMotionCoroutine = StartCoroutine(PlayWinningAttackSlowMotion());
        return winningAttackSlowMotionCoroutine;
    }

    public Coroutine StartWinningAttackCharacterSwap()
    {
        if (!enableWinningAttackCharacterSwap || !ResolveWinningAttackCharacterImages())
            return null;

        if (winningAttackCharacterSwapCoroutine != null)
            StopCoroutine(winningAttackCharacterSwapCoroutine);

        winningAttackCharacterSwapCoroutine = StartCoroutine(PlayWinningAttackCharacterSwap());
        return winningAttackCharacterSwapCoroutine;
    }

    private IEnumerator PlayWinningAttackCharacterSwap()
    {
        if (winningAttackCharacterSwapDelay > 0f)
            yield return new WaitForSeconds(winningAttackCharacterSwapDelay);

        Color currentStartColor = winningAttackCurrentCharacterImage.color;
        Color nextEndColor = winningAttackNextCharacterImage.color;

        winningAttackNextCharacterImage.gameObject.SetActive(true);
        SetImageAlpha(winningAttackNextCharacterImage, 0f);

        if (winningAttackCharacterSwapFadeDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < winningAttackCharacterSwapFadeDuration)
            {
                float normalizedTime = Mathf.Clamp01(elapsed / winningAttackCharacterSwapFadeDuration);
                float blend = winningAttackCharacterSwapCurve == null || winningAttackCharacterSwapCurve.length == 0
                    ? normalizedTime
                    : Mathf.Clamp01(winningAttackCharacterSwapCurve.Evaluate(normalizedTime));

                SetImageAlpha(winningAttackCurrentCharacterImage, Mathf.Lerp(currentStartColor.a, 0f, blend));
                SetImageAlpha(winningAttackNextCharacterImage, Mathf.Lerp(0f, nextEndColor.a, blend));

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        SetImageAlpha(winningAttackCurrentCharacterImage, 0f);
        SetImageAlpha(winningAttackNextCharacterImage, nextEndColor.a);
        winningAttackCurrentCharacterImage.gameObject.SetActive(false);
        winningAttackCharacterSwapCoroutine = null;
    }

    private void PrepareWinningAttackCharacterSwap()
    {
        if (!enableWinningAttackCharacterSwap || !ResolveWinningAttackCharacterImages()) return;

        winningAttackCurrentCharacterImage.gameObject.SetActive(true);
        SetImageAlpha(winningAttackCurrentCharacterImage, 1f);
        winningAttackNextCharacterImage.gameObject.SetActive(false);
    }

    private bool ResolveWinningAttackCharacterImages()
    {
        if (winningAttackCurrentCharacterImage != null && winningAttackNextCharacterImage != null)
            return winningAttackCurrentCharacterImage != winningAttackNextCharacterImage;

        StoryTargetAnimationPlayer storyTarget = FindFirstObjectByType<StoryTargetAnimationPlayer>();
        if (storyTarget == null) return false;

        Transform portraitHolder = storyTarget.transform.Find("Portrait Holder");
        if (portraitHolder == null) return false;

        Image[] images = portraitHolder.GetComponentsInChildren<Image>(true);
        foreach (Image image in images)
        {
            if (winningAttackCurrentCharacterImage == null && image.name == "Character Image")
                winningAttackCurrentCharacterImage = image;
            else if (winningAttackNextCharacterImage == null && image.name == "Next Character Image")
                winningAttackNextCharacterImage = image;
        }

        return winningAttackCurrentCharacterImage != null &&
            winningAttackNextCharacterImage != null &&
            winningAttackCurrentCharacterImage != winningAttackNextCharacterImage;
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private IEnumerator PlayWinningAttackSlowMotion()
    {
        timeScaleBeforeWinningAttack = Time.timeScale;
        fixedDeltaTimeBeforeWinningAttack = Time.fixedDeltaTime;
        controlsTimeScale = true;

        float elapsed = 0f;
        while (elapsed < winningAttackSlowMotionDuration)
        {
            float normalizedTime = Mathf.Clamp01(elapsed / winningAttackSlowMotionDuration);
            float timeScaleMultiplier = Mathf.Max(0.01f, winningAttackTimeScaleCurve.Evaluate(normalizedTime));

            Time.timeScale = timeScaleBeforeWinningAttack * timeScaleMultiplier;
            Time.fixedDeltaTime = fixedDeltaTimeBeforeWinningAttack * timeScaleMultiplier;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreTimeScaleAfterWinningAttack();
        winningAttackSlowMotionCoroutine = null;
    }

    private void RestoreTimeScaleAfterWinningAttack()
    {
        if (!controlsTimeScale) return;

        Time.timeScale = timeScaleBeforeWinningAttack;
        Time.fixedDeltaTime = fixedDeltaTimeBeforeWinningAttack;
        controlsTimeScale = false;
    }

    private void OnDisable()
    {
        if (winningAttackSlowMotionCoroutine != null)
        {
            StopCoroutine(winningAttackSlowMotionCoroutine);
            winningAttackSlowMotionCoroutine = null;
        }

        if (winningAttackCharacterSwapCoroutine != null)
        {
            StopCoroutine(winningAttackCharacterSwapCoroutine);
            winningAttackCharacterSwapCoroutine = null;
        }

        RestoreTimeScaleAfterWinningAttack();
    }

    private void LoadWordlist()
    {
        foreach (TextAsset wordlist in wordlists)
        {
            foreach (string rawWord in wordlist.text.Split('\n'))
            {
                string word = rawWord.Trim();
                if (!string.IsNullOrEmpty(word)) validWords.Add(word);
            }
        }
        //print(validWords.Count);
    }

    public void EndRound()
    {
        bool didWin = MahikaManager.Instance.DidWin();

        if (BackgroundsManager.Instance != null) BackgroundsManager.Instance.ShowEndingBG(didWin);

        if (didWin == true)
        {
            AudioManager.Instance.bgmSource.resource = winAmbience;
            AudioManager.Instance.bgmSource.Play();
            AudioManager.Instance.sfxSource.PlayOneShot(winSFX);

            eventOnWin?.Invoke();

            //winScreen.SetActive(true);
        }

        else
        {
            AudioManager.Instance.bgmSource.Stop();
            AudioManager.Instance.sfxSource.PlayOneShot(loseSFX);

            eventOnLose?.Invoke();

            //loseScreen.SetActive(true);
        }
    }
}
