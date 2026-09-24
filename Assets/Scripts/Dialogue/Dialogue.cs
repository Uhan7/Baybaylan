using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public struct DialogueProfile
{
	public UnityEngine.Sprite sprite;
	public bool isTalking;
}

[Serializable]
public struct DialogueSentence
{
	public bool isFourSpeakers;
	public DialogueProfile leftPrimaryProfile;

	[ShowIf("isFourSpeakers")]
	[AllowNesting]
	public DialogueProfile leftSecondaryProfile;
	public DialogueProfile rightPrimaryProfile;

	[ShowIf("isFourSpeakers")]
	[AllowNesting]
	public DialogueProfile rightSecondaryProfile;
	[TextArea(3, 10)] public string sentence;
}

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
	public DialogueSentence[] sentences;
	public AudioClip soundToPlay;
	public float textSpeed = 0.022f;
	public float textPunctSpeed = 0.24f;
}