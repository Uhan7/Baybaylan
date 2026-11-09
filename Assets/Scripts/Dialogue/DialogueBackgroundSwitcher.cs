using UnityEngine;
using UnityEngine.UI;

public class DialogueBackgroundSwitcher : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite mountainBackground;
    [SerializeField] private Sprite castleBackground;
    [SerializeField] private AudioClip mountainBgm;
    [SerializeField] private AudioClip mountainAmbience;
    [SerializeField] private AudioClip castleBgm;
    [SerializeField] private AudioClip castleAmbience;

    private bool castleRevealHandled;

    private void LateUpdate()
    {
        if (backgroundImage == null) return;
        if (backgroundImage.sprite != castleBackground)
        {
            castleRevealHandled = false;
            return;
        }

        if (castleRevealHandled || !backgroundImage.isActiveAndEnabled ||
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
        backgroundImage.sprite = background;
    }
}
