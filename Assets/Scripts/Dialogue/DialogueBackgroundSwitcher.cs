using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBackgroundSwitcher : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite mountainBackground;
    [SerializeField] private Sprite castleBackground;
    [Min(0f), SerializeField, Tooltip("Total seconds to fade out, switch the background, and fade back in.")]
    private float backgroundSwitchDuration = 0.6f;
    [SerializeField] private AudioClip mountainBgm;
    [SerializeField] private AudioClip mountainAmbience;
    [SerializeField] private AudioClip castleBgm;
    [SerializeField] private AudioClip castleAmbience;

    private bool castleRevealHandled;
    private Coroutine backgroundSwitchRoutine;
    private Sprite requestedBackground;
    private Color backgroundTint;

    private void OnDisable()
    {
        if (backgroundSwitchRoutine == null) return;

        StopCoroutine(backgroundSwitchRoutine);
        backgroundSwitchRoutine = null;
        backgroundImage.sprite = requestedBackground;
        SetBackgroundTint(backgroundTint);
    }

    private void LateUpdate()
    {
        if (backgroundImage == null) return;
        if (backgroundImage.sprite != castleBackground)
        {
            castleRevealHandled = false;
            return;
        }

        if (castleRevealHandled || backgroundSwitchRoutine != null || !backgroundImage.isActiveAndEnabled ||
            backgroundImage.color.a <= 0f || AudioManager.Instance == null) return;

        Image dimmer = DialogueManager.Instance != null ? DialogueManager.Instance.dimmer : null;
        if (dimmer != null && dimmer.isActiveAndEnabled && dimmer.color.a >= 0.999f) return;

        // Cue once when the castle becomes visible, including dialogue-only scenes.
        AudioManager.Instance.SetLevelAudio(castleBgm, castleAmbience);
        AudioManager.Instance.StartGameplayBGM();
        castleRevealHandled = true;
    }

    public void ShowMountainBackground()
    {
        SetBackground(mountainBackground);
        AudioManager.Instance?.SetLevelAudio(mountainBgm, mountainAmbience);
    }

    public void ShowCastleBackground()
    {
        if (backgroundImage == null || castleBackground == null) return;

        SetBackground(castleBackground);
        castleRevealHandled = false;
    }

    private void SetBackground(Sprite background)
    {
        if (backgroundImage == null || background == null) return;
        if (backgroundSwitchRoutine != null && requestedBackground == background) return;
        if (backgroundSwitchRoutine == null && backgroundImage.sprite == background) return;

        if (backgroundSwitchRoutine == null) backgroundTint = backgroundImage.color;
        if (backgroundSwitchRoutine != null) StopCoroutine(backgroundSwitchRoutine);
        backgroundSwitchRoutine = null;
        requestedBackground = background;

        // Invisible backgrounds can be prepared immediately before their normal reveal.
        if (!isActiveAndEnabled || !backgroundImage.isActiveAndEnabled ||
            backgroundImage.color.a <= 0f || backgroundSwitchDuration <= 0f)
        {
            backgroundImage.sprite = background;
            SetBackgroundTint(backgroundTint);
            return;
        }

        backgroundSwitchRoutine = StartCoroutine(SwitchBackground(background));
    }

    private IEnumerator SwitchBackground(Sprite background)
    {
        float halfDuration = backgroundSwitchDuration * 0.5f;
        yield return FadeTint(Color.black, halfDuration);
        backgroundImage.sprite = background;
        yield return FadeTint(backgroundTint, halfDuration);
        backgroundSwitchRoutine = null;
    }

    private IEnumerator FadeTint(Color target, float duration)
    {
        Color start = backgroundImage.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetBackgroundTint(Color.Lerp(start, target, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetBackgroundTint(target);
    }

    private void SetBackgroundTint(Color tint)
    {
        // Preserve alpha so the existing ImageFader can still reveal/hide the image.
        tint.a = backgroundImage.color.a;
        backgroundImage.color = tint;
    }
}
