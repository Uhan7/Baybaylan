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
    [SerializeField] private AudioSource aSource;

    [Header("Dialogue Details")]
    [ReadOnly, SerializeField] private Dialogue currentDialogue;
    [HideInInspector] private int currentSentenceIndex;

    [Header("Actions")]
    [HideInInspector] public Action OnDialogueEnd;

    [Header("Flags")]
    [HideInInspector] private bool isTyping;
    [HideInInspector] private bool skip;
    [ReadOnly, SerializeField] public bool dialoguing; // Used in DialogueBox.cs (open animations)

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
        currentDialogue = dialogue;
        currentSentenceIndex = 0;

        if (dialogueContainer)
        {
            SetProfileSprites(dialogue.sentences[0]);
            dialogueContainer.ClearText();
        }

        dialoguing = true;
        StartCoroutine(StartDelay(0.75f));
    }

    private IEnumerator StartDelay(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
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

        StopAllCoroutines();
        StartCoroutine(TypeSentence(dialogueSentence));

        currentSentenceIndex++;
    }

    private IEnumerator TypeSentence(DialogueSentence dialogueSentence)
    {
        // If the currentContainer is null, break
        if (!dialogueContainer) yield break;

        isTyping = true;
        skip = false;

        SetProfileSprites(dialogueSentence);
        AnimateProfiles(dialogueSentence);

        string sentence = dialogueSentence.sentence;
        dialogueContainer.SetTextInstant(sentence);

        int total = sentence.Length;

        for (int i = 0; i <= total; i++)
        {
            if (skip)
            {
                dialogueContainer.ShowFullText();
                break;
            }

            dialogueContainer.SetVisibleCharacters(i);

            if (i % 5 == 0 && i < total) aSource.PlayOneShot(currentDialogue.soundToPlay);

            if (i == 0) continue;

            char c = sentence[i - 1];

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
    }
}