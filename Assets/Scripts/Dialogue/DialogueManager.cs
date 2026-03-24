using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{

	public TextMeshProUGUI nameText;
	public TextMeshProUGUI dialogueText;
	public Image chara;

	public float textSpeed;
	public float textPunctSpeed;
	public GameObject nextIndicator;

	private Queue<string> sentences;

	private Animator anim;
	public static bool open;
	private bool skip;
	public static bool canNext;

	private AudioSource aSource;
	private AudioClip soundToPlay;

	public static bool endConvo;

	void Start()
	{
		aSource = GetComponent<AudioSource>();
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

    public IEnumerator StartDialogue(Dialogue dialogue)
	{
		endConvo = false;
		skip = false;
		canNext = false;
		nextIndicator.SetActive(false);
		nameText.text = dialogue.name;
		chara.sprite = dialogue.character;
		soundToPlay = dialogue.soundToPlay;
		gameObject.SetActive(true);
		open = true;

		dialogueText.text = " ";

		yield return new WaitForSeconds(.3f);

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
		dialogueText.text = "";
		foreach (char letter in sentence.ToCharArray())
		{
			if (!skip && !canNext)
			{
				if (dialogueText.text.Length % 4 == 0) aSource.PlayOneShot(soundToPlay);
				dialogueText.text += letter;
				if (letter == '.' || letter == ',' || letter == '!' || letter == '?')
					yield return new WaitForSeconds(textPunctSpeed);
				else yield return new WaitForSeconds(textSpeed);
			}
			if (skip)
			{
				dialogueText.text = sentence.ToString();
			}
			if (dialogueText.text == sentence.ToString())
            {
				nextIndicator.SetActive(true);
				canNext = true;
			}
		}
	}

	public void EndDialogue()
	{
		open = false;
		endConvo = true;
	}

}