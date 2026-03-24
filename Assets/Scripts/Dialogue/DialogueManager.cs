using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
	// Variables ---------------------------------------------------------------
	[Header("Components")]
	[HideInInspector] private Animator anim;

	[Header("References")]
	[SerializeField] private AudioSource aSource;
	[SerializeField] private TextMeshProUGUI dialogueText;
	[SerializeField] private GameObject nextIndicator;

	[Header("Acquired from DialogueTrigger's Dialogue")]
	[HideInInspector] private Queue<string> sentences;
	[HideInInspector] private AudioClip soundToPlay;
	[HideInInspector] private float textSpeed;
	[HideInInspector] private float textPunctSpeed;

	[Header("Flags")]
	[HideInInspector] private bool skip;

	[Header("Temporary Global Flags")]
	[HideInInspector] public static bool open;
	[HideInInspector] public static bool canNext;
	[HideInInspector] public static bool endConvo;

	// Main Functions ----------------------------------------------------------
	void Start()
	{
		sentences = new Queue<string>();
		anim = GetComponent <Animator>();
	} 

    private void Update()
    {
		anim.SetBool("dialogueOpen", open);

		if (!open) return;
		if (Input.GetMouseButtonDown(0))
		{
			skip = true;
			if (canNext)
			{
				canNext = false;
				nextIndicator.SetActive(false);
				skip = false;
				DisplayNextSentence();
			}
		}
	}

	// Helper Functions --------------------------------------------------------
	public IEnumerator StartDialogue(Dialogue dialogue)
	{
		endConvo = false;
		skip = false;
		canNext = false;
		nextIndicator.SetActive(false);
		soundToPlay = dialogue.soundToPlay;
		textSpeed = dialogue.textSpeed;
		textPunctSpeed = dialogue.textPunctSpeed;
		gameObject.SetActive(true);
		open = true;

		dialogueText.text = " ";

		yield return new WaitForSeconds(.5f);

		sentences.Clear();

		foreach (string sentence in dialogue.sentences)
		{
			sentences.Enqueue(sentence);
		}

		DisplayNextSentence();
	}

	public void DisplayNextSentence()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string sentence = sentences.Dequeue();
		StopAllCoroutines();
		StartCoroutine(TypeSentence(sentence));
	}

	IEnumerator TypeSentence(string sentence)
	{
		dialogueText.text = sentence;
		dialogueText.maxVisibleCharacters = 0;

		int totalChars = sentence.Length;

		for (int i = 0; i <= totalChars; i++)
		{
			if (skip)
			{
				dialogueText.maxVisibleCharacters = totalChars;
				break;
			}

			dialogueText.maxVisibleCharacters = i;

			if (i % 4 == 0 && i < totalChars) aSource.PlayOneShot(soundToPlay);

			if (i == 0) continue;
			char letter = sentence[i - 1];

			if (letter == '.' ||
				letter == ',' ||
				letter == '!' ||
				letter == '?' ||
				letter == ':' ||
				letter == ';') yield return new WaitForSeconds(textPunctSpeed);
			else yield return new WaitForSeconds(textSpeed);
		}

		dialogueText.maxVisibleCharacters = totalChars;
		nextIndicator.SetActive(true);
		canNext = true;
	}

	public void EndDialogue()
	{
		open = false;
		endConvo = true;
	}

}