using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//dont wanna use this anymore
public class TrackElement{
	//works like this:
	// this has the size of instrument number and is true/false for the index of that instrument 
	//if its playing or not
	public bool[] instrument;
	public TrackElement(int n){
		instrument = new bool[n]; //fix a maximum number of instruments per track?
	}
}


public class MainTrack : MonoBehaviour {

	public UserInterface UI;

	public double bpm = 40.0F;
	public double nextTick = 0.0F;
	public int trackTickTime = 0;
	public double startTick;

	public int trackTimeDim = 10;
	public int trackNotesDim = 4;
	//track contains instrument and note information
	// -1 means nothing. 0,1,2,3... defines the instr
	public TrackElement[,] track;
	public int maxNumInstruments = 100;
	public bool playing = false;
	public int trackTick = 0;
	public float trackTime = 0.0f;
	public float trackspeed = 1.0f;


	//maybe this belongs in a better place:
	public enum playModes {precise, dynamic, metronome};
	public playModes currentPlayMode = playModes.precise;

	//instrument management
	public List<Instrument> instrument;
	public int instrumentInd = 0; //eu nao uso isso mais, eu acho
	public List<GameObject> instrGO;

	public GameObject notePrefab;

	//layersuff
	public List<Layer> layer;
	public int activeLayer = 0;

	void Awake(){
		//layer stuff
		//in Awake because ui needs to initialize the fucking dropdown menu in Start
		layer = new List<Layer>();
		Layer firstLayer = gameObject.AddComponent<Layer>() as Layer;
		layer.Add(firstLayer); //this is the basic edit layer 0
		firstLayer.myInd = 0;
	}

	void Start () {
		//nextTick = startTick * 44100;
		nextTick = 0;
		startTick = AudioSettings.dspTime;

		//track initialization
		track = new TrackElement[trackNotesDim,trackTimeDim];
		for (int i = 0; i < trackNotesDim; i++)
			for (int j = 0; j < trackTimeDim; j++)
				track [i, j] = new TrackElement(maxNumInstruments);

		currentPlayMode = playModes.dynamic;

		//instrument management create the first
		instrument = new List<Instrument>();
		instrGO = new List<GameObject> ();
		instrGO.Add (new GameObject ("instrument"));
		instrument.Add(instrGO[0].AddComponent<Instrument> () as Instrument);	
		instrument [0].mainTrack = this;
		instrument [0].instrumentIndex = instrumentInd;
		instrumentInd++;
		instrument [0].notePrefab = notePrefab;


	}

	public void CreateInstrument(){
		GameObject tempgo = new GameObject ("instrument");
		Instrument tempi = tempgo.AddComponent<Instrument> () as Instrument;
		tempi.instrumentName = "New Instrument";
		tempi.mainTrack = this;
		tempi.instrumentIndex = instrumentInd;
		instrumentInd++;
		tempi.notePrefab = notePrefab;
		instrument.Add (tempi);
		instrGO.Add (tempgo);
	}



	public void RemoveInstrument(Instrument i){
		instrument.Remove(i);
		GameObject g = i.gameObject;
		i.SelfDestruct ();
		instrGO.Remove (g);
		Destroy (i.gameObject);
	}

	public void switchSelectedInstrument(Instrument si){
		foreach(Instrument i in instrument){
			if (i == si)
				i.SwitchSelection (true);
			else
				i.SwitchSelection (false);
		}
	}
	

	void Update () {
		if (playing) {
			double t = (AudioSettings.dspTime - startTick )*bpm/10 ;
			trackTick = Mathf.FloorToInt (Mathf.Repeat((float)t,trackTimeDim));
		}

		//dynamic play has to be handled here, for the midi controller to work
		//something to do with how this midi class functions...

		if (currentPlayMode == playModes.dynamic &&  UI.selInstrument != null) {
			//brincando com o tecladinho.. legal mas inutil :P
			float pitch = MidiJack.MidiMaster.GetKnob (MidiJack.MidiChannel.All,1,0f)/10 +1;
			print (pitch.ToString ());
			foreach (Synth s in UI.selInstrument.synths) {
				s.audSource.pitch = pitch;
				if (s.voice == null)
					break;

				//print ("instrumento " + instrumentName);
				foreach (Voice v in s.voice) {
					v.instGain = 0;
				}
				for (int n = 0; n < 100; n++) {
					float curK = MidiJack.MidiMaster.GetKey (n);
					//PlayNote (40, curK);
					if (MidiJack.MidiMaster.GetKeyDown (n)) {
						print (curK.ToString ());
						print ("key 40");
						s.PlayNote (n-10, curK);
					} 
					if (MidiJack.MidiMaster.GetKeyUp (n)) {
						s.StopNote (n-10);
					}
				}
			}
		}

	}

	public void SwitchDynamicInstrument(Instrument i){
		if(!playing){
			foreach (Instrument j in instrument)
				if(j!= null)
					j.TurnDynamicPlay (false);
			i.TurnDynamicPlay (true);
			currentPlayMode = playModes.dynamic;
			//lazy. dont care.
		}
	}

	public void Play(){
		currentPlayMode = playModes.precise;
		foreach(Instrument i in instrument){
			if(i != null)
				i.TurnDynamicPlay (false);
			print ("instrument added to precise play");
		}
		startTick = AudioSettings.dspTime;
		playing = true;

	}
	public void Stop(){
		playing = false;
	}

	//layer stuff:

	public void SwitchActiveLayer(int l){
		activeLayer = l;
		print ("switching layer");
		foreach (Instrument i in instrument)
			i.SwitchActiveLayer (l);
	}

	/*layereditor keeps a list of loaded layers
	 *they are all added in maintrack gameobject already
	 *the function below adds instances of these to a list
	 *This list will not create anything, just set the order
	 *of execution of layers and can repeat layers
	 */

	public Layer AddLayer(){
		if(UI != null && UI.terminal != null)
			UI.terminal.Add ("Adding new layer");
		Layer newlayer = gameObject.AddComponent<Layer> () as Layer;
		newlayer.myInd = layer.Count;
		layer.Add(newlayer);
		foreach (Instrument i in instrument) {
			i.layerNotes.Add (new List<Note> ());
			i.layerNotes [layer.Count-1] = i.layerNotes [layer.Count - 2];
		}
		return newlayer;
	}

	public void ApplyLayers(){
		foreach (Instrument i in instrument) {
			for(int l = 1; l<layer.Count; l++){
				int inCount = 0;
				foreach (Note n in i.layerNotes[l]) {
					inCount++;
				}
				i.layerNotes[l] = layer[l].Action(i.layerNotes[l-1]);

				int outCount = 0;
				foreach (Note n in i.layerNotes[l]) {
					outCount++;
				}
			}

		}
	}

}
