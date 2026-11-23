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
	[ReadOnly, Range(0f, 1f), Tooltip("Dim dialogue automatically uses a vignette intensity of 0.5.")]
	public float vignetteIntensity;

	[Min(0f), Tooltip("Seconds used to fade to the selected background mode. Use 0 for the default fade duration.")]
	public float backgroundFadeDuration;

	[TextArea(3, 10)] public string sentence;
}

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
	public const float DimVignetteIntensity = 0.5f;

	public DialogueSentence[] sentences;
	public AudioClip soundToPlay;
	public float textSpeed = 0.022f;
	public float textPunctSpeed = 0.24f;

	private void OnValidate()
	{
		if (sentences == null) return;

		for (int i = 0; i < sentences.Length; i++)
			if (sentences[i].backgroundMode == DialogueBackgroundMode.Dim)
				sentences[i].vignetteIntensity = DimVignetteIntensity;
	}
}
