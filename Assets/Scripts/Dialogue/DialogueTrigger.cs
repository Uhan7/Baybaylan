using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class DialogueTrigger : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Dialogue Reference")]
	[SerializeField] private Dialogue dialogue;
    [SerializeField] private DialogueSet dialogueSet;

    [Header("Properties")]
    [SerializeField] private bool activateOnEnable;
    [SerializeField] private bool isRepeatable;

    [Header("Flags")]
    [ReadOnly, SerializeField] private bool alreadyTriggered;

    // Main Functions ----------------------------------------------------------
    private void OnEnable()
    {
        if (activateOnEnable) Invoke("TriggerDialogue", 2f);
    }

    // Helper Functions --------------------------------------------------------
    public void TriggerDialogue()
	{
        if (alreadyTriggered && !isRepeatable) return;

        if (dialogueSet != null) dialogueSet.StartDialogueSet();
        else if (dialogue != null) DialogueManager.Instance.StartDialogue(dialogue);
        else Debug.LogError("Triggering null dialogue");

        alreadyTriggered = true;
    }
}