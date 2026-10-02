using UnityEngine;
using UnityEngine.Events;
using System.Collections;
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
    [Min(0f), SerializeField] private float eventAfterDialogueDelay = 0.5f;

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
        while (currentIndex < dialogues.Length &&
               (dialogues[currentIndex] == null ||
                dialogues[currentIndex].sentences == null ||
                dialogues[currentIndex].sentences.Length == 0))
        {
            Debug.LogWarning($"Skipping empty Dialogue at index {currentIndex} in {name}.", this);
            currentIndex++;
        }

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

        if (DialogueManager.Instance.dimmer != null)
        {
            DialogueManager.Instance.dimmer.raycastTarget = false;
            DialogueManager.Instance.dimmer.GetComponent<Animator>().Play("image_fade_out");
        }
        StartCoroutine(InvokeEventAfterDialogue());
    }

    private IEnumerator InvokeEventAfterDialogue()
    {
        if (eventAfterDialogueDelay > 0f)
            yield return new WaitForSeconds(eventAfterDialogueDelay);

        eventAfterDialogue?.Invoke();
    }
}
