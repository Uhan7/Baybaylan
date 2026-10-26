using TMPro;
using UnityEngine;

public class TitleScreenMenu : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private TMP_Text playLabel;
    [SerializeField] private RectTransform levelSelectButton;
    [SerializeField] private RectTransform exitButton;
    [SerializeField, Min(0f)] private float buttonSpacing = 124f;
    [SerializeField] private AudioClip buttonSFX;

    [Header("Intro Animation")]
    [SerializeField] private RectTransform sidePanel;
    [SerializeField] private RectTransform gameLogo;
    [SerializeField, Min(0f)] private float panelDelay = 0.5f;
    [SerializeField, Min(0.01f)] private float panelDuration = 0.75f;
    [SerializeField, Min(0f)] private float logoDelay = 1f;
    [SerializeField, Min(0.01f)] private float logoDuration = 0.77f;
    [SerializeField, Min(0f)] private float buttonDelay = 1.43f;
    [SerializeField, Min(0.01f)] private float buttonDuration = 1f;
    [SerializeField, Min(0f)] private float buttonStagger = 0.17f;
    [SerializeField] private float logoStartOffset = 405f;
    [SerializeField] private float buttonStartOffset = -554f;

    private RectTransform[] visibleButtons;
    private Vector2[] buttonPositions;
    private Vector2 panelPosition;
    private Vector2 logoPosition;
    private CanvasGroup menuInteraction;
    private float elapsed;
    private float introDuration;
    private bool isAnimating;
    private bool hasRenderedFirstFrame;

    private void Start()
    {
        bool hasPlayed = SaveManager.Instance != null && SaveManager.Instance.HasPlayed;
        playLabel.text = hasPlayed ? "Tumuloy" : "Maglaro";
        levelSelectButton.gameObject.SetActive(hasPlayed);

        RectTransform playButton = (RectTransform)transform;
        Vector2 nextPosition = playButton.anchoredPosition - Vector2.up * buttonSpacing;
        levelSelectButton.anchoredPosition = nextPosition;
        exitButton.anchoredPosition = nextPosition - Vector2.up * (hasPlayed ? buttonSpacing : 0f);
        visibleButtons = hasPlayed
            ? new[] { playButton, levelSelectButton, exitButton }
            : new[] { playButton, exitButton };

        RegisterButtonSFX(playButton);
        RegisterButtonSFX(levelSelectButton);
        RegisterButtonSFX(exitButton);

        // Animate from the chosen layout, so a hidden button never leaves a gap.
        buttonPositions = new Vector2[visibleButtons.Length];
        for (int i = 0; i < visibleButtons.Length; i++)
            buttonPositions[i] = visibleButtons[i].anchoredPosition;
        panelPosition = sidePanel.anchoredPosition;
        logoPosition = gameLogo.anchoredPosition;
        menuInteraction = sidePanel.GetComponent<CanvasGroup>();
        if (menuInteraction == null) menuInteraction = sidePanel.gameObject.AddComponent<CanvasGroup>();
        menuInteraction.interactable = false;
        menuInteraction.blocksRaycasts = false;
        introDuration = Mathf.Max(panelDelay + panelDuration, logoDelay + logoDuration,
            buttonDelay + buttonDuration + buttonStagger * (visibleButtons.Length - 1));
        elapsed = 0f;
        hasRenderedFirstFrame = false;
        isAnimating = true;
        ApplyIntro(0f);
    }

    private void RegisterButtonSFX(RectTransform buttonTransform)
    {
        if (buttonTransform == null || !buttonTransform.TryGetComponent(out UnityEngine.UI.Button button))
            return;

        button.onClick.AddListener(PlayButtonSFX);
    }

    private void PlayButtonSFX()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySFX(buttonSFX);
        if (AudioManager.Instance.bgmSource != null)
            AudioManager.Instance.bgmSource.Stop();
    }

    private void Update()
    {
        AdvanceIntro(Time.unscaledDeltaTime);
    }

    private void AdvanceIntro(float frameDuration)
    {
        if (!isAnimating) return;
        // The first Play Mode frames can include seconds of editor startup work.
        // Show the initial pose first, then keep loading hitches from skipping the intro.
        if (!hasRenderedFirstFrame)
        {
            hasRenderedFirstFrame = true;
            return;
        }
        elapsed += Mathf.Clamp(frameDuration, 0f, 0.1f);
        ApplyIntro(elapsed);
        if (elapsed < introDuration) return;
        isAnimating = false;
        menuInteraction.interactable = true;
        menuInteraction.blocksRaycasts = true;
    }

    private void ApplyIntro(float time)
    {
        sidePanel.anchoredPosition = panelPosition + Vector2.left * (sidePanel.rect.width + 7f) *
            (1f - Progress(time, panelDelay, panelDuration));
        gameLogo.anchoredPosition = logoPosition + Vector2.up * logoStartOffset *
            (1f - Progress(time, logoDelay, logoDuration));
        for (int i = 0; i < visibleButtons.Length; i++)
            visibleButtons[i].anchoredPosition = buttonPositions[i] + Vector2.up * buttonStartOffset *
                (1f - Progress(time, buttonDelay + i * buttonStagger, buttonDuration));
    }

    private static float Progress(float time, float delay, float duration)
    {
        return Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((time - delay) / Mathf.Max(0.01f, duration)));
    }
}
