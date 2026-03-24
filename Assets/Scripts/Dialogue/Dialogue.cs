using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue
{
	public AudioClip soundToPlay;
	public float textSpeed;
	public float textPunctSpeed;

	[TextArea(3, 10)]
	public string[] sentences;
	
}