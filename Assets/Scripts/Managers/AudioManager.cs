using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class AudioManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] public AudioSource bgmSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] public AudioSource sfxSource;
    [Tooltip("Dedicated source for Attack and Heal animation audio.")]
    [SerializeField] private AudioSource resultSfxSource;

    private Coroutine delayedResultSfxCoroutine;
    private readonly Dictionary<AudioSource, float> audioBasePitches = new Dictionary<AudioSource, float>();
    private bool gameplayBgmStarted;
    private Coroutine bgmFadeCoroutine;
    private Coroutine sceneAudioFadeCoroutine;
    private bool sceneAudioFading;
    private float bgmFadeMultiplier = 1f;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureResultSfxSource();

        SetBGMVolume(SettingsHolder.BGMVolume);
        SetAmbienceVolume(SettingsHolder.AmbienceVolume);
        SetSFXVolume(SettingsHolder.SFXVolume);
    }

    private void Start()
    {
        if (ambienceSource != null && ambienceSource.clip != null)
            ambienceSource.Play();
    }

    private void OnDestroy()
    {
        ResetAllAudioPitch();
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
        ResetAllAudioPitch();
    }

    // Helper Functions --------------------------------------------------------
    public void SetBGMVolume(float volume)
    {
        if (!sceneAudioFading && bgmSource != null)
            bgmSource.volume = Mathf.Clamp01(volume) * bgmFadeMultiplier;
    }

    public void SetAmbienceVolume(float volume)
    {
        if (sceneAudioFading) return;
        if (ambienceSource != null) ambienceSource.volume = Mathf.Clamp01(volume);
    }

    public void StartGameplayBGM()
    {
        if (sceneAudioFading || gameplayBgmStarted || bgmSource == null || bgmSource.clip == null) return;

        gameplayBgmStarted = true;
        bgmSource.Play();
    }

    public void SetLevelAudio(AudioClip bgm, AudioClip ambience)
    {
        if (sceneAudioFading) return;
        if (bgmSource != null && bgm != null && bgmSource.clip != bgm)
        {
            if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
            bgmFadeMultiplier = 1f;
            SetBGMVolume(SettingsHolder.BGMVolume);
            bgmSource.Stop();
            bgmSource.clip = bgm;
            if (gameplayBgmStarted) bgmSource.Play();
        }

        if (ambienceSource != null && ambience != null && ambienceSource.clip != ambience)
        {
            ambienceSource.clip = ambience;
            ambienceSource.Play();
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sceneAudioFading) return;
        float clampedVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = clampedVolume;
        if (resultSfxSource != null) resultSfxSource.volume = clampedVolume;
    }

    public Coroutine FadeOutBGM()
    {
        if (sceneAudioFading || bgmSource == null || !bgmSource.isPlaying) return null;
        if (bgmFadeCoroutine == null)
            bgmFadeCoroutine = StartCoroutine(FadeBGM());
        return bgmFadeCoroutine;
    }

    private IEnumerator FadeBGM()
    {
        float elapsed = 0f;
        while (elapsed < 1f && bgmSource != null)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmFadeMultiplier = 1f - Mathf.Clamp01(elapsed);
            SetBGMVolume(SettingsHolder.BGMVolume);
            yield return null;
        }

        if (bgmSource != null) bgmSource.Stop();
        bgmFadeMultiplier = 1f;
        SetBGMVolume(SettingsHolder.BGMVolume);
        bgmFadeCoroutine = null;
    }

    public Coroutine FadeOutSceneAudio()
    {
        if (sceneAudioFading) return sceneAudioFadeCoroutine;
        sceneAudioFading = true;
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = null;
        sceneAudioFadeCoroutine = StartCoroutine(FadeSceneAudio());
        return sceneAudioFadeCoroutine;
    }

    private IEnumerator FadeSceneAudio()
    {
        var initialVolumes = new Dictionary<AudioSource, float>();
        float elapsed = 0f;
        while (elapsed < 1f)
        {
            // Include independent UI/dialogue sources and sounds created during exit.
            foreach (AudioSource source in FindObjectsByType<AudioSource>(
                FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!initialVolumes.ContainsKey(source))
                    initialVolumes.Add(source, source.volume);
            }

            elapsed += Time.unscaledDeltaTime;
            float multiplier = 1f - Mathf.Clamp01(elapsed);
            foreach (KeyValuePair<AudioSource, float> entry in initialVolumes)
            {
                if (entry.Key != null) entry.Key.volume = entry.Value * multiplier;
            }
            yield return null;
        }

        foreach (AudioSource source in initialVolumes.Keys)
        {
            if (source != null) source.Stop();
        }
        sceneAudioFadeCoroutine = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayResultSFX(AudioClip clip, float delaySeconds = 0f)
    {
        if (clip == null) return;

        EnsureResultSfxSource();
        if (resultSfxSource == null) return;

        if (delayedResultSfxCoroutine != null)
            StopCoroutine(delayedResultSfxCoroutine);

        resultSfxSource.Stop();
        delayedResultSfxCoroutine = StartCoroutine(
            PlayResultSFXAfterDelay(clip, Mathf.Max(0f, delaySeconds)));
    }

    public void SetAllAudioPitchMultiplier(float multiplier)
    {
        EnsureResultSfxSource();

        // Include independent tile, UI, and dialogue sources, as well as the
        // manager's music, ambience, and SFX. Remember each source's original
        // pitch once, including sources created while the effect is running.
        foreach (AudioSource source in FindObjectsByType<AudioSource>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!audioBasePitches.ContainsKey(source))
                audioBasePitches.Add(source, source.pitch);
        }

        float pitchMultiplier = Mathf.Clamp(multiplier, 0.05f, 3f);
        foreach (KeyValuePair<AudioSource, float> entry in audioBasePitches)
        {
            if (entry.Key != null)
                entry.Key.pitch = entry.Value * pitchMultiplier;
        }
    }

    public void ResetAllAudioPitch()
    {
        foreach (KeyValuePair<AudioSource, float> entry in audioBasePitches)
        {
            if (entry.Key != null)
                entry.Key.pitch = entry.Value;
        }
        audioBasePitches.Clear();
    }

    private IEnumerator PlayResultSFXAfterDelay(AudioClip clip, float delaySeconds)
    {
        // A scaled wait keeps the audio cue aligned with its animation when the
        // winning Attack time curve slows the game down.
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        resultSfxSource.PlayOneShot(clip);
        delayedResultSfxCoroutine = null;
    }

    private void EnsureResultSfxSource()
    {
        if (resultSfxSource == null)
        {
            GameObject sourceObject = new GameObject("Result SFX Source");
            sourceObject.transform.SetParent(transform, false);
            resultSfxSource = sourceObject.AddComponent<AudioSource>();
            resultSfxSource.playOnAwake = false;
            resultSfxSource.spatialBlend = 0f;
            resultSfxSource.volume = sfxSource != null
                ? sfxSource.volume
                : SettingsHolder.SFXVolume;

            if (sfxSource != null)
                resultSfxSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }
    }
}
