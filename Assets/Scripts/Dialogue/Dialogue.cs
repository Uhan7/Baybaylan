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

public enum DialogueBackgroundMode
{
	KeepCurrent,
	Black,
	Dimmed,
	Clear
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

	[Header("Background")]
	[Tooltip("Keep Current preserves the previous line's background. Black fully covers it, Dimmed uses the normal dialogue darkness, and Clear shows it without an overlay.")]
	public DialogueBackgroundMode backgroundMode;
	[Min(0f), Tooltip("Seconds used to fade to the selected background mode. Use 0 for an instant change.")]
	public float backgroundFadeDuration;

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
