using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using NaughtyAttributes;
using System.Linq.Expressions;

public class DialogueManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static DialogueManager Instance;

    [Header("References")]
    [SerializeField] private DialogueContainer dialogueContainer;
    [SerializeField] public Image dimmer; // I think this unclean af lol
    [SerializeField] private Sprite vignetteSprite;
    [SerializeField] private AudioSource aSource;

    [Header("Background Presentation")]
    [Range(0f, 1f), SerializeField] private float normalDialogueDimAlpha = 0.75f;
    [Min(0f), SerializeField] private float defaultBackgroundFadeDuration = 0.33333334f;

    [Header("Dialogue Details")]
    [ReadOnly, SerializeField] private Dialogue currentDialogue;
    [HideInInspector] private int currentSentenceIndex;

    [Header("Actions")]
    [HideInInspector] public Action OnDialogueEnd;

    [Header("Flags")]
    [HideInInspector] private bool isTyping;
    [HideInInspector] private bool skip;
    [ReadOnly, SerializeField] public bool dialoguing; // Used in DialogueBox.cs (open animations)

    private Coroutine startDelayRoutine;
    private Coroutine sentenceRoutine;
    private Coroutine backgroundFadeRoutine;
    private Animator dimmerAnimator;
    private Image vignetteOverlay;

    [Header("Dialogue Characters")]
    [Header("Left")]
    [SerializeField] private Animator leftPrimaryAnimator;
    [SerializeField] private Animator leftSecondaryAnimator;
    [SerializeField] private UnityEngine.UI.Image leftPrimaryImage;
    [SerializeField] private UnityEngine.UI.Image leftSecondaryImage;
    [Header("Right")]
    [SerializeField] private Animator rightPrimaryAnimator;
    [SerializeField] private Animator rightSecondaryAnimator;
    [SerializeField] private UnityEngine.UI.Image rightPrimaryImage;
    [SerializeField] private UnityEngine.UI.Image rightSecondaryImage;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        Instance = this;
        CreateVignetteOverlay();
    }

    private void Update()
    {
        if (currentDialogue == null) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping) skip = true;
            else NextSentence();
        }
    }

    // Helper Functions --------------------------------------------------------
    public void StartDialogue(Dialogue dialogue)
    {
        if (dialogue == null || dialogue.sentences == null || dialogue.sentences.Length == 0)
        {
            Debug.LogError("Cannot start a null or empty Dialogue.", dialogue);
            return;
        }

        currentDialogue = dialogue;
        currentSentenceIndex = 0;

        if (dialogueContainer)
        {
            SetProfileSprites(dialogue.sentences[0]);
            dialogueContainer.ClearText();
        }

        dialoguing = true;
        if (startDelayRoutine != null) StopCoroutine(startDelayRoutine);
        startDelayRoutine = StartCoroutine(StartDelay(0.75f));
    }

    private IEnumerator StartDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        startDelayRoutine = null;
        NextSentence();
    }

    private void NextSentence()
    {
        if (currentSentenceIndex >= currentDialogue.sentences.Length)
        {
            EndDialogue();
            return;
        }

        DialogueSentence dialogueSentence = currentDialogue.sentences[currentSentenceIndex];

        if (sentenceRoutine != null) StopCoroutine(sentenceRoutine);
        sentenceRoutine = StartCoroutine(TypeSentence(dialogueSentence));

        currentSentenceIndex++;
    }

    private IEnumerator TypeSentence(DialogueSentence dialogueSentence)
    {
        // If the currentContainer is null, break
        if (!dialogueContainer) yield break;

        isTyping = true;
        skip = false;

        ApplyBackgroundMode(dialogueSentence);
        SetProfileSprites(dialogueSentence);
        AnimateProfiles(dialogueSentence);

        string sentence = dialogueSentence.sentence;
        int total = dialogueContainer.PrepareText(sentence);

        for (int i = 0; i <= total; i++)
        {
            if (skip)
            {
                dialogueContainer.ShowFullText();
                break;
            }

            dialogueContainer.SetVisibleCharacters(i);

            if (i % 5 == 0 && i < total && aSource && currentDialogue.soundToPlay)
                aSource.PlayOneShot(currentDialogue.soundToPlay);

            if (i == 0) continue;

            char c = dialogueContainer.GetVisibleCharacter(i - 1);

            if (c == '.' ||
                c == '…' || // Just a fallback,,, but ideally all ellipsis turn into 3 periods
                c == '–' ||
                c == '-' ||
                c == '~' ||
                c == ',' ||
                c == '!' ||
                c == '?' ||
                c == ':' ||
                c == ';') yield return new WaitForSeconds(currentDialogue.textPunctSpeed);
            else yield return new WaitForSeconds(currentDialogue.textSpeed);
        }

        dialogueContainer.ShowNextIndicator(true);
        isTyping = false;
        sentenceRoutine = null;
    }

    public void ShowDefaultDialogueDim()
    {
        if (!dimmer) return;

        dimmer.raycastTarget = true;
        FadeBackgroundTo(normalDialogueDimAlpha, 0f, defaultBackgroundFadeDuration);
    }

    public void HideDialogueDim()
    {
        if (!dimmer) return;

        dimmer.raycastTarget = false;
        FadeBackgroundTo(0f, 0f, defaultBackgroundFadeDuration, true);
    }

    private void ApplyBackgroundMode(DialogueSentence dialogueSentence)
    {
        switch (dialogueSentence.backgroundMode)
        {
            case DialogueBackgroundMode.Black:
                FadeBackgroundTo(1f, 0f, dialogueSentence.backgroundFadeDuration);
                break;
            case DialogueBackgroundMode.Dim:
                FadeBackgroundTo(0f, dialogueSentence.vignetteIntensity, dialogueSentence.backgroundFadeDuration);
                break;
            default:
                FadeBackgroundTo(normalDialogueDimAlpha, 0f, dialogueSentence.backgroundFadeDuration);
                break;
        }
    }

    private void FadeBackgroundTo(
        float targetDimmerAlpha,
        float targetVignetteAlpha,
        float duration,
        bool restoreAnimatorAfterFade = false)
    {
        if (!dimmer) return;

        if (!dimmerAnimator) dimmerAnimator = dimmer.GetComponent<Animator>();
        if (dimmerAnimator) dimmerAnimator.enabled = false;

        if (backgroundFadeRoutine != null) StopCoroutine(backgroundFadeRoutine);
        backgroundFadeRoutine = StartCoroutine(FadeBackgroundRoutine(
            Mathf.Clamp01(targetDimmerAlpha),
            Mathf.Clamp01(targetVignetteAlpha),
            Mathf.Max(0f, duration),
            restoreAnimatorAfterFade));
    }

    private IEnumerator FadeBackgroundRoutine(
        float targetDimmerAlpha,
        float targetVignetteAlpha,
        float duration,
        bool restoreAnimatorAfterFade)
    {
        Color startDimmerColor = dimmer.color;
        Color startVignetteColor = vignetteOverlay ? vignetteOverlay.color : Color.clear;

        if (duration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                SetImageAlpha(dimmer, Mathf.Lerp(startDimmerColor.a, targetDimmerAlpha, progress));
                SetImageAlpha(vignetteOverlay, Mathf.Lerp(startVignetteColor.a, targetVignetteAlpha, progress));
                yield return null;
            }
        }

        SetImageAlpha(dimmer, targetDimmerAlpha);
        SetImageAlpha(vignetteOverlay, targetVignetteAlpha);
        backgroundFadeRoutine = null;

        if (restoreAnimatorAfterFade && dimmerAnimator)
            dimmerAnimator.enabled = true;
    }

    private void CreateVignetteOverlay()
    {
        if (!dimmer || !vignetteSprite) return;

        GameObject vignetteObject = new GameObject(
            "Dialogue Vignette",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        vignetteObject.layer = dimmer.gameObject.layer;

        RectTransform vignetteTransform = vignetteObject.GetComponent<RectTransform>();
        vignetteTransform.SetParent(dimmer.transform.parent, false);
        vignetteTransform.SetSiblingIndex(dimmer.transform.GetSiblingIndex() + 1);
        vignetteTransform.anchorMin = Vector2.zero;
        vignetteTransform.anchorMax = Vector2.one;
        vignetteTransform.anchoredPosition = Vector2.zero;
        vignetteTransform.sizeDelta = Vector2.zero;

        vignetteOverlay = vignetteObject.GetComponent<Image>();
        vignetteOverlay.sprite = vignetteSprite;
        vignetteOverlay.raycastTarget = false;
        vignetteOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    private static void SetImageAlpha(Image image, float alpha)
    {
        if (!image) return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void SetProfileSprites(DialogueSentence _dialogueSentence)
    {
        // Left Primary Image
        if (null == leftPrimaryImage) Debug.LogError("The Image of the LEFT PRIMARY PROFILE was not assigned to the DialogueManager");
        else SetProfileSprite(leftPrimaryImage, _dialogueSentence.leftPrimaryProfile.sprite);

        // Left Secondary Image
        if (null == leftSecondaryImage) Debug.LogError("The Image of the LEFT SECONDARY PROFILE was not assigned to the DialogueManager");
        else SetProfileSprite(leftSecondaryImage, _dialogueSentence.leftSecondaryProfile.sprite);

        // Right Primary Image
        if (null == rightPrimaryImage) Debug.LogError("The Image of the RIGHT PRIMARY PROFILE was not assigned to the DialogueManager");
        else SetProfileSprite(rightPrimaryImage, _dialogueSentence.rightPrimaryProfile.sprite);

        // Right Secondary Image
        if (null == rightSecondaryImage) Debug.LogError("The Image of the RIGHT SECONDARY PROFILE was not assigned to the DialogueManager");
        else SetProfileSprite(rightSecondaryImage, _dialogueSentence.rightSecondaryProfile.sprite);
    }
    private void SetProfileSprite(UnityEngine.UI.Image _image, UnityEngine.Sprite _sprite = null)
    {
        if (null == _sprite) return;
        _image.sprite = _sprite;
    }

    private void AnimateProfiles(DialogueSentence _dialogueSentence)
    {
        // Left Primary Animator
        if (leftPrimaryAnimator)
            AnimateProfile
            (
                leftPrimaryAnimator,
                _dialogueSentence.leftPrimaryProfile.isTalking,
                _dialogueSentence.leftPrimaryProfile.sprite ?? false
            );
        else Debug.LogError("The Animator of the LEFT PRIMARY PROFILE was not assigned to the DialogueManager");

        // Left Secondary Animator
        if (leftSecondaryAnimator)
            AnimateProfile
            (
                leftSecondaryAnimator,
                _dialogueSentence.leftSecondaryProfile.isTalking,
                _dialogueSentence.leftSecondaryProfile.sprite ?? false
            );
        else Debug.LogError("The Animator of the LEFT SECONDARY PROFILE was not assigned to the DialogueManager");

        // Right Primary Animator
        if (rightPrimaryAnimator)
            AnimateProfile
            (
                rightPrimaryAnimator,
                _dialogueSentence.rightPrimaryProfile.isTalking,
                _dialogueSentence.rightPrimaryProfile.sprite ?? false
            );
        else Debug.LogError("The Animator of the RIGHT PRIMARY PROFILE was not assigned to the DialogueManager");

        // Right Secondary Animator
        if (rightSecondaryAnimator)
            AnimateProfile
            (
                rightSecondaryAnimator,
                _dialogueSentence.rightSecondaryProfile.isTalking,
                _dialogueSentence.rightSecondaryProfile.sprite ?? false
            );
        else Debug.LogError("The Animator of the RIGHT SECONDARY PROFILE was not assigned to the DialogueManager");
    }
    private void AnimateProfile(UnityEngine.Animator _animator, bool _isTalking, bool _isVisible = true)
    {
        if (null == _animator) return;
        _animator.SetBool("isTalking", _isTalking);
        _animator.SetBool("isVisible", _isVisible);
    }

    private void AnimateEndDialogue()
    {
        // Left Primary Animator
        if (leftPrimaryAnimator)
            AnimateProfile
            (
                leftPrimaryAnimator,
                false,
                false
            );
        else Debug.LogError("The Animator of the LEFT PRIMARY PROFILE was not assigned to the DialogueManager");

        // Left Secondary Animator
        if (leftSecondaryAnimator)
            AnimateProfile
            (
                leftSecondaryAnimator,
                false,
                false
            );
        else Debug.LogError("The Animator of the LEFT SECONDARY PROFILE was not assigned to the DialogueManager");

        // Right Primary Animator
        if (rightPrimaryAnimator)
            AnimateProfile
            (
                rightPrimaryAnimator,
                false,
                false
            );
        else Debug.LogError("The Animator of the RIGHT PRIMARY PROFILE was not assigned to the DialogueManager");

        // Right Secondary Animator
        if (rightSecondaryAnimator)
            AnimateProfile
            (
                rightSecondaryAnimator,
                false,
                false
            );
        else Debug.LogError("The Animator of the RIGHT SECONDARY PROFILE was not assigned to the DialogueManager");
    }

    private void EndDialogue()
    {
        if (dialogueContainer) dialogueContainer.ClearText();
        AnimateEndDialogue();

        currentDialogue = null;
        dialoguing = false;

        OnDialogueEnd?.Invoke();

        AlahasSubManager.Instance.dialogueEnded = true;
    }
}
