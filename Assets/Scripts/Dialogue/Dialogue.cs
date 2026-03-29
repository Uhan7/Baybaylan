using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Dialogue : ScriptableObject
{
	public int containerIndex = 0;
	[TextArea(3, 10)] public string[] sentences;
	public AudioClip soundToPlay;
	public float textSpeed = 0.02f;
	public float textPunctSpeed = 0.15f;
}