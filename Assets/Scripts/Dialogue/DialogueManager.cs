using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using NaughtyAttributes;

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
    [HideInInspector] private Dialogue currentDialogue;
    [HideInInspector] private int currentSentenceIndex;

    [Header("Actions")]
    [HideInInspector] public Action OnDialogueEnd;

    [Header("Flags")]
    [HideInInspector] private bool isTyping;
    [HideInInspector] private bool skip;
    [ReadOnly, SerializeField] public bool dialoguing; // Used in DialogueBox.cs (open animations)

    [Header("Dialogue Characters")]
    [Header("Left")]
    [SerializeField] private Animator leftAnimator;
    [SerializeField] private Image leftImage;
    [Header("Right")]
    [SerializeField] private Animator rightAnimator;
    [SerializeField] private Image rightImage;

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
            dialogueContainer.ClearText();

        dialoguing = true;
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(0.75f);
        NextSentence();
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

        SetCharacterSprite(dialogueSentence);
        AnimateCharacters(dialogueSentence);

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

            if (i % 6 == 0 && i < total) aSource.PlayOneShot(currentDialogue.soundToPlay);

            if (i == 0) continue;

            char c = sentence[i - 1];

            if (c == '.' ||
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

    private void SetCharacterSprite(DialogueSentence dialogueSentence)
    {
        // Guard
        if (!leftImage)
        {
            Debug.LogError("The Image of the LEFT dialogue character was not assigned to the DialogueManager");
            return;
        }
        if (!rightImage)
        {
            Debug.LogError("The Image of the RIGHT dialogue character was not assigned to the DialogueManager");
            return;
        }

        // Logic
        DialogueSentence.SpeakerPosition speakerPosition = dialogueSentence.speakerPosition;
        UnityEngine.Sprite characterSprite = dialogueSentence.characterSprite;
        switch (speakerPosition)
        {       
            case DialogueSentence.SpeakerPosition.LEFT:
                leftImage.sprite = characterSprite;
                break;
                
            case DialogueSentence.SpeakerPosition.RIGHT:
                rightImage.sprite = characterSprite;
                break;
        }

    }

    private void AnimateCharacters(DialogueSentence dialogueSentence)
    {
        // Guard
        if (!leftAnimator)
        {
            Debug.LogError("The Animator of the LEFT dialogue character was not assigned to the DialogueManager");
            return;
        }
        if (!rightAnimator)
        {
            Debug.LogError("The Animator of the RIGHT dialogue character was not assigned to the DialogueManager"); 
            return;
        }

        // Logic
        DialogueSentence.SpeakerPosition speakerPosition = dialogueSentence.speakerPosition;
        switch (speakerPosition)
        {
            case DialogueSentence.SpeakerPosition.NONE:
                leftAnimator?.SetBool("isTalking", false);
                rightAnimator?.SetBool("isTalking", false);
                break;
                
            case DialogueSentence.SpeakerPosition.LEFT:
                leftAnimator?.SetBool("isTalking", true);
                rightAnimator?.SetBool("isTalking", false);
                break;
                
            case DialogueSentence.SpeakerPosition.RIGHT:
                leftAnimator?.SetBool("isTalking", false);
                rightAnimator?.SetBool("isTalking", true);
                break;
        }
    }

    private void EndDialogue()
    {
        if (dialogueContainer) dialogueContainer.ClearText();

        currentDialogue = null;
        dialoguing = false;

        OnDialogueEnd?.Invoke();
    }
}