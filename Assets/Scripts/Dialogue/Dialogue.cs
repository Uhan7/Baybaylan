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
	Default,
	Black,
	Dim
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
	[Tooltip("Default uses the normal dialogue dim. Black completely covers the background. Dim uses a vignette.")]
	public DialogueBackgroundMode backgroundMode;

	[ShowIf("backgroundMode", DialogueBackgroundMode.Dim)]
	[AllowNesting]
	[Range(0f, 1f), Tooltip("How dark the vignette is. 0 is invisible and 1 is fully dark.")]
	public float vignetteIntensity;

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
