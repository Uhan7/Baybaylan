using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class DialogueSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Dialogues")]
    [SerializeField] private List<Dialogue> dialogues;

    [Header("Flags")]
    [ReadOnly, SerializeField] private bool hasCompleted;

    private int currentIndex = 0;
    private bool isRunning = false;

    // Main Functions ----------------------------------------------------------

    // Helper Functions --------------------------------------------------------
    public void StartDialogueSet()
    {
        if (hasCompleted || isRunning || dialogues.Count == 0) return;

        isRunning = true;
        currentIndex = 0;

        PlayNextDialogue();
    }

    private void PlayNextDialogue()
    {
        if (currentIndex >= dialogues.Count)
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
        // Unsucscribe, don't want duplicate calls
        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;

        currentIndex++;
        PlayNextDialogue();
    }

    private void CompleteSet()
    {
        hasCompleted = true;
        isRunning = false;

        Debug.Log("Dialogue Set Completed!");
    }
}