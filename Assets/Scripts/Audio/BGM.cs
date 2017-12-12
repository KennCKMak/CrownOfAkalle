using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BGM {


	public string name;
	public AudioClip clip1;
	public AudioClip clip2;

	[Range(0.0f, 1.0f)]
	public float volume1 = 1.0f;
	[Range(0.1f, 3.0f)]
	public float pitch1 = 1.0f;


	[Range(0.0f, 1.0f)]
	public float volume2 = 1.0f;
	[Range(0.1f, 3.0f)]
	public float pitch2 = 1.0f;
}
