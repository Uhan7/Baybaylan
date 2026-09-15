using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[Serializable]
public struct DialogueSentence
{
	public enum SpeakerPosition : UInt16
	{
		NONE = 0,
		LEFT = 1,
		RIGHT = 2,
		BOTH = 3
	}
	public DialogueSentence.SpeakerPosition speakerPosition;
	public UnityEngine.Sprite leftCharacterSprite;
	public UnityEngine.Sprite rightCharacterSprite;
	[TextArea(3, 10)] public string sentence;
}

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
	public DialogueSentence[] sentences;
	public AudioClip soundToPlay;
	public float textSpeed = 0.02f;
	public float textPunctSpeed = 0.15f;
}