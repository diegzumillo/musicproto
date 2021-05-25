using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class teste : MonoBehaviour {

	public float gain = 0.0f;
	public float frequency = 440f;
	public float gain2 = 0.0f;
	public float frequency2 = 440f;
	public float position = 0.0f;
	public int samplerate = 44100;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	void OnAudioFilterRead(float[] data, int channels) {
		for (var i = 0; i < data.Length; ++i) {
			//349.228
			data [i] = gain * Mathf.Sign ( Mathf.Sin (2 * Mathf.PI * frequency * position / samplerate))
				+gain2 *Mathf.Sign ( Mathf.Sin (2 * Mathf.PI * frequency2 * position / samplerate));

			position++;
		}
	}
}
