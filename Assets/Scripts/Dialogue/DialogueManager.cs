using UnityEngine;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static DialogueManager Instance;

    [Header("References")]
    [SerializeField] private DialogueContainer[] dialogueContainers;
    [SerializeField] private AudioSource aSource;
    [HideInInspector] private DialogueContainer currentContainer;

    [Header("Dialogue Details")]
    [HideInInspector] private Dialogue currentDialogue;
    [HideInInspector] private int currentSentenceIndex;

    [Header("Flags")]
    [HideInInspector] private bool isTyping;
    [HideInInspector] private bool skip;
    [HideInInspector] public bool dialoguing; // Used in DialogueBox.cs (open animations)

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (currentDialogue == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping) skip = true;
            else NextSentence();
        }
    }

    // Helper Functions --------------------------------------------------------
    public void StartDialogue(Dialogue dialogue)
    {
        dialoguing = true;

        currentDialogue = dialogue;
        currentSentenceIndex = 0;
        currentContainer = dialogueContainers[dialogue.containerIndex];

        NextSentence();
    }

    private void NextSentence()
    {
        if (currentSentenceIndex >= currentDialogue.sentences.Length)
        {
            EndDialogue();
            return;
        }

        string sentence = currentDialogue.sentences[currentSentenceIndex];

        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));

        currentSentenceIndex++;
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        skip = false;

        currentContainer.SetTextInstant(sentence);

        int total = sentence.Length;

        for (int i = 0; i <= total; i++)
        {
            if (skip)
            {
                currentContainer.ShowFullText();
                break;
            }

            currentContainer.SetVisibleCharacters(i);

            if (i % 4 == 0 && i < total) aSource.PlayOneShot(currentDialogue.soundToPlay);

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

        currentContainer.ShowNextIndicator(true);
        isTyping = false;
    }

    private void EndDialogue()
    {
        currentDialogue = null;

        dialoguing = false;
    }
}