using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class DialogueSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private ObjectsManager objsManager;

    [Header("Dialogues")]
    [SerializeField] private Dialogue[] dialogues;

    [Header("Extra Behaviors")]
    [SerializeField] private bool useObjectsManager;
    [ShowIf("useObjectsManager"), SerializeField] private bool activateBefore;
    [ShowIf("useObjectsManager"), SerializeField] private bool activateAfter;
    [ShowIf("useObjectsManager"), SerializeField] private bool deactivateBefore;
    [ShowIf("useObjectsManager"), SerializeField] private bool deactivateAfter;

    [Header("Flags")]
    [ReadOnly, SerializeField] private bool hasCompleted;

    private int currentIndex = 0;
    private bool isRunning = false;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (useObjectsManager) objsManager = GetComponent<ObjectsManager>();
    }

    // Helper Functions --------------------------------------------------------
    public void StartDialogueSet()
    {
        if (hasCompleted || isRunning || dialogues.Length == 0) return;

        if (objsManager != null)
        {
            if (activateBefore) objsManager.ActivateObjects();
            if (deactivateBefore) objsManager.DeactivateObjects();
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

        if (objsManager != null)
        {
            if (activateAfter) objsManager.ActivateObjects();
            if (deactivateAfter) objsManager.DeactivateObjects();
        }

        Debug.Log("Dialogue Set in " + name + " is Completed.");
    }
}