using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instrument : MonoBehaviour {

	public Synth[] synths;
	public Color activeColor;
	public Color inactiveColor;
	public GameObject notePrefab;
	public string instrumentName = "new instrument";
	public int instrumentIndex;
	public MainTrack mainTrack;
	public UserInterface UI;
	public bool dynamicPlay = true;
	public int[,] myTrack; //not using this
	public List<Note> notes;
	public List<List<Note>> layerNotes;
	public List<GameObject> activeNotes; //the go representing the active notes
	public bool[] availVoices;
	public int numVoices = 6;



	void Start () {
		synths = new Synth[1];
		GameObject go = new GameObject();
		go.transform.parent = transform;
		synths [0] = go.AddComponent<Synth> () as Synth;
		synths [0].synthName = "synth1";
		synths [0].synthIndex = 0;
		synths [0].instrument = this;
		synths [0].mainTrack = mainTrack;

		availVoices = new bool[numVoices];

		activeNotes = new List<GameObject> ();
		notes = new List<Note> ();
		layerNotes = new List<List<Note>> ();
		layerNotes.Add (new List<Note> ());
		layerNotes.Add (new List<Note> ());

		float h = Random.value;
		activeColor = Color.HSVToRGB (h, 1, 1);
		inactiveColor = Color.HSVToRGB (h, 0.5f, 0.5f);

		UI = mainTrack.UI;

	}

	public void TurnDynamicPlay(bool b){
		
		dynamicPlay = b;
		//tem mais alguma coisa pra fazer aqui?...

	}

	public void AddSynth(){
		mainTrack.Stop ();

		Synth[] tempArray = new Synth[synths.Length + 1];
		for (int i = 0; i < synths.Length; i++) {
			tempArray [i] = synths [i];
		}
		synths = tempArray;
		int lastInt = synths.Length - 1;
		GameObject go = new GameObject ();
		go.transform.parent = transform;
		synths[lastInt] = go.AddComponent<Synth> () as Synth;
		string name = "synth"+synths.Length.ToString();
		synths [lastInt].synthName = name;
		synths [lastInt].instrument = this;
		synths [lastInt].mainTrack = mainTrack;
	}
	public void SelfDestruct(){
		//TODO remove all other layers too
		foreach (GameObject n in activeNotes) {
			GameObject.Destroy (n);
		}
		foreach (Synth s in synths) {
			RemoveSynth (s);
		}
	}
	public void RemoveSynth(Synth rms){
		Synth[] tempArray = new Synth[synths.Length - 1];
		int j = 0;
		for (int i = 0; j < synths.Length - 1; i++) {
			if (synths [j] != rms) {
				tempArray [i] = synths [j];
			} else
				i--;
			j++;
			i++;
		}
		synths = tempArray;
	}


	public void RemoveNote(Note n, int layer){
		foreach (Note m in layerNotes[layer]) {
			if (m.note == n.note && m.duration == n.duration && m.start == n.start) {
				notes.Remove (m);
				UpdateAllSynths();
				return;
			}
		}
		if (layer == mainTrack.activeLayer) {
			//notes = layerNotes [layer];
			UpdateActiveNotes ();
		}
	}
	public void InsertNote(int n, int s, int d, int layer){
		int tentativeVoice = -1;
		for (int i = 0; i < numVoices; i++)
			availVoices [i] = true;

		//check for conflict
		foreach (Note m in layerNotes[layer]) {

			if (m.start == s) {
				if (m.note == n) {
					//print ("this note already exists");
					return;
				} else {
					//print ("excluding voice index "+m.voice.ToString());
					availVoices [m.voice] = false;
				}
			} 
			if (m.start < s) {
				if (m.start + m.duration - 1 > s) {
					if (m.note == n) {
						//print ("note already started before");
						return;
					} else {
						//print ("excluding voice index "+m.voice.ToString());
						availVoices [m.voice] = false;
					}
				}
			} else {
				if (s + d > m.start) {
					if (m.note == n) {
						//print ("note too long to fit. Reducing the size");
						d = m.start - s;
					} else {
						//print ("excluding voice index "+m.voice.ToString());
						availVoices [m.voice] = false;
					}
				}
			}

		}
		//now we see if there is an available voice
		for (int b = 0; b < numVoices; b++) {
			if (availVoices [b]) {
				tentativeVoice = b;
				break;
			}
		}
		if (tentativeVoice == -1) {
			print ("no voice available");
			UI.terminal.Add ("No voice available");
			return;
		}


		Note newNote = new Note (n, s, d, this);
		newNote.voice = tentativeVoice;
		newNote.myLayer = layer;
		layerNotes[layer].Add(newNote);

		//update synthesizers:
		//for now just update all synths cause im lazy but eventually i should
		//make it only update the specific note for performance
		if (layer == mainTrack.activeLayer) {
			notes = layerNotes [layer];
			//newNoteGO.SetActive (true);
			UpdateAllSynths ();
			UpdateActiveNotes ();
		}
	}


	public void SwitchSelection(bool b){
		//b true -> this is now selected
		if (b) {
			foreach (GameObject n in activeNotes) {
				LineRenderer line = n.GetComponent<LineRenderer> () as LineRenderer;
				line.startColor = activeColor;
				line.endColor = activeColor;
				line.sortingOrder = 1;
			}
		} else {
			foreach (GameObject n in activeNotes) {
				LineRenderer line = n.GetComponent<LineRenderer> () as LineRenderer;
				line.startColor = inactiveColor;
				line.endColor = inactiveColor;
				line.sortingOrder = 0;
			}
		}
	}




	public void UpdateAllSynths(){
		foreach (Synth s in synths) {
			//clear everything
			//print("clearing synth "+s.synthName);
			s.ClearAll ();

			foreach (Note n in layerNotes[mainTrack.activeLayer]) {
				s.InsertNote (n);
			}
		}
	}

	
	// Update is called once per frame
	void Update () {


	}

	//dynamic play stuff
	//hacky way of doing this
	public int lastPlayedNote = -1;
	public void PlayNote(int n, float t){
		if (t > 0) {
			if (lastPlayedNote != -1)
				return;
			lastPlayedNote = n;
			Invoke ("StopNote", t);
		}
			
		foreach (Synth s in synths) {
			s.PlayNote (n);
		}
	}
	void StopNote(){
		foreach (Synth s in synths) {
			s.StopNote (lastPlayedNote);
			lastPlayedNote = -1;
		}
	}
	void StopNote(int n){
		foreach (Synth s in synths) {
			s.StopNote (n);
		}
	}

	public void UpdateActiveNotes(){
		//for now ill just switch active layer to the same, to update
		//everything all at once
		SwitchActiveLayer(mainTrack.activeLayer);
	}

	public void SwitchActiveLayer(int l){
		//destroy all game objects in the activenotes array
		if (activeNotes != null) {
			foreach (GameObject g in activeNotes)
				Destroy (g);
			activeNotes.Clear();
		}
		//recreate all gameobjects in activenotes array from active layer l
		foreach (Note n in layerNotes[l]) {
			GameObject nnote = GameObject.Instantiate (mainTrack.notePrefab);
			BoxCollider box = nnote.AddComponent<BoxCollider> () as BoxCollider;
			box.size = new Vector3 (n.duration , 1, 1);
			box.center = new Vector3 (n.duration / 2 +n.start, n.note, 1);
			LineRenderer line = nnote.GetComponent<LineRenderer>() as LineRenderer;
			line.SetPosition (0, new Vector3 (n.start, n.note, 0));
			line.SetPosition (1, new Vector3 (n.start+n.duration-1, n.note, 0));
			line.startColor = activeColor;
			line.endColor = activeColor;
			nnote.GetComponent<NoteRepresentation> ().myNote = n;
			activeNotes.Add (nnote);
		}
		UpdateAllSynths ();
	}

}
