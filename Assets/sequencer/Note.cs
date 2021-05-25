using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note {

	public int note;
	public float freq;
	public int start;
	public int duration;
	public int voice;
	public Instrument myInst;
	public int myLayer = 0;

	public float[] noteFreqTable;

	public Note(int n, int s, int d, Instrument i){
		note = n;
		start = s;
		duration = d;
		//go = gameObject;
		myInst = i;

		//create frequency-note table
		float a = Mathf.Pow(2.0f,1.0f/12.0f);
		//print (a);
		noteFreqTable = new float[60];
		//clips = new AudioClip[48];
		noteFreqTable [34] = 440.0f; //middle A
		//float localFreq = frequency;
		for (int j = 0; j < 60; j++) {
			int m = j - 34;
			noteFreqTable [j] = noteFreqTable [34] * Mathf.Pow(a,m);
		}
	}


	public void UpdateFrequency(){
		freq = noteFreqTable [note];
	}


	public void MoveNote(int d){
		note += d;
		UpdateFrequency ();
	}

	public Note Clone(int newLayer){
		Note r = new Note (note, start, duration, myInst);
		r.myLayer = newLayer;
		r.voice = voice;
		return r;
	}


}
