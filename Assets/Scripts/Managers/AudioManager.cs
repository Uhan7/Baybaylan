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
    [SerializeField] public AudioSource sfxSource;
    [Tooltip("Dedicated source for Attack and Heal animation audio. Its pitch can change without affecting other SFX.")]
    [SerializeField] private AudioSource resultSfxSource;

    private Coroutine delayedResultSfxCoroutine;
    private float resultSfxBasePitch = 1f;
    private bool resultSfxBasePitchCached;

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
        SetSFXVolume(SettingsHolder.SFXVolume);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Helper Functions --------------------------------------------------------
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null) bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = clampedVolume;
        if (resultSfxSource != null) resultSfxSource.volume = clampedVolume;
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

    public void SetResultSFXPitchMultiplier(float multiplier)
    {
        EnsureResultSfxSource();
        if (resultSfxSource == null) return;

        resultSfxSource.pitch = resultSfxBasePitch * Mathf.Clamp(multiplier, 0.05f, 3f);
    }

    public void ResetResultSFXPitch()
    {
        if (resultSfxSource != null)
            resultSfxSource.pitch = resultSfxBasePitch;
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

        if (!resultSfxBasePitchCached)
        {
            resultSfxBasePitch = resultSfxSource.pitch;
            resultSfxBasePitchCached = true;
        }
    }
}
