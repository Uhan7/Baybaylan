using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class DialogueTrigger : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Dialogue Reference")]
	[SerializeField] private Dialogue dialogue;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueTrigger nextDialogue;

    [Header("Properties")]
    [SerializeField] private bool activateOnEnable;
    [SerializeField] private bool isRepeatable;

    [Header("Flags")]
    [ReadOnly, SerializeField] private bool alreadyTriggered;
    [HideInInspector] private bool waitingForEnd;

    // Main Functions ----------------------------------------------------------
    private void OnEnable()
    {
        if (activateOnEnable) TriggerDialogue();
    }

    private void Update()
    {
        if (waitingForEnd && DialogueManager.endConvo) //Destroy(gameObject, .1f);
        if (alreadyTriggered && nextDialogue != null && DialogueManager.endConvo)
        {
            nextDialogue.gameObject.SetActive(true);
            nextDialogue.TriggerDialogue();
        }
    }

    // Helper Functions --------------------------------------------------------
    public void TriggerDialogue()
	{
        if (alreadyTriggered && !isRepeatable) return;

        StartCoroutine(dialogueManager.StartDialogue(dialogue));
        waitingForEnd = true;

        alreadyTriggered = true;
    }
}