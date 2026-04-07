using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using NaughtyAttributes;

public class DialogueSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Dialogues")]
    [SerializeField] private Dialogue[] dialogues;

    [Header("Events")]
    [SerializeField] private UnityEvent eventBeforeDialogue;
    [SerializeField] private UnityEvent eventAfterDialogue;

    [Header("Flags")]
    [ReadOnly, SerializeField] private bool hasCompleted;

    private int currentIndex = 0;
    private bool isRunning = false;

    // Main Functions ----------------------------------------------------------

    // Helper Functions --------------------------------------------------------
    public void StartDialogueSet()
    {
        if (hasCompleted || isRunning || dialogues.Length == 0) return;

        eventBeforeDialogue?.Invoke();

        if (DialogueManager.Instance.dimmer != null) // This dimmer shi also feels unclean as hell
        {
            DialogueManager.Instance.dimmer.raycastTarget = true;
            DialogueManager.Instance.dimmer.GetComponent<Animator>().Play("image_fade_in");
        }

        isRunning = true;
        currentIndex = 0;

        PlayNextDialogue();
    }

    private void PlayNextDialogue()
    {
        if (currentIndex >= dialogues.Length)
        {
            CompleteSet();
            return;
        }

        // Subscribe
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;

        DialogueManager.Instance.StartDialogue(dialogues[currentIndex]);
    }

    private void HandleDialogueEnd()
    {
        //Unsubscribe, don't want duplicate calls
        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;

        currentIndex++;
        PlayNextDialogue();
    }

    private void CompleteSet()
    {
        hasCompleted = true;
        isRunning = false;

        eventAfterDialogue?.Invoke();
        if (DialogueManager.Instance.dimmer != null)
        {
            DialogueManager.Instance.dimmer.raycastTarget = false;
            DialogueManager.Instance.dimmer.GetComponent<Animator>().Play("image_fade_out");
        }
    }
}