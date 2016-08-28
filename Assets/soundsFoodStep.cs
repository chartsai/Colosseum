using UnityEngine;
using System.Collections;

public class soundsFoodStep : MonoBehaviour {

	public GameObject gameProjectSounds;
	public AudioClip soundFootStep;

	private AudioSource source;
	private float volLowRange = .5f;
	private float volHighRange = 1.0f;

	// Use this for initialization
	void Start () {
		source = GetComponent<AudioSource>();

	}

	// Update is called once per frame
	void Update () {
		float vol = Random.Range (volLowRange, volHighRange);
		source.PlayOneShot(soundFootStep,vol);
	}

}
