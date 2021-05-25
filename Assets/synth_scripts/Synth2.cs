using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Synth2: MonoBehaviour {
	public int position = 0;
	public int samplerate = 44100;
	public float frequency = 440;
	public AnimationCurve waveShape;
	public float[] notes;
	public AudioClip[] clips;
	public float localTimer = 0.0f;
	public float freqDispl = 0.0f;

	public Rect windowRect = new Rect(20, 20, 250, 500);
	public int synthIndex = 0;

	//6 audiosources for 6 voices
	public int numVoices =6;
	public AudioSource[] audSource;
	public GameObject[] voices;
	//filters
	public AudioReverbFilter[] reverb;
	public AudioLowPassFilter[] lowPass;

	//exposed parameters
	public string synthName;
	public float phase = 0.0f;
	public float volumeKnob = 0.5f;
	public float reverbKnob = 0.5f;
	public float lowPassKnob = 22000.0f;
	public float fallOff = 0.0f;
	//matrix to define curve(s) Might need something better for the final editor
	public int curveMatrixDim = 10;

	//debug stuff
	//float test=0.0f;
	//int curNote = 34;
	public int controls = 0;
	public Texture2D blacksquare;
	public Texture2D whitesquare;

	public enum curveTypes{sine,square,saw,noise,custom};
	public curveTypes curShape ;

	void Start() {
		CreateAudioSources ();
		UpdateClips ();

		waveShape = new AnimationCurve (new Keyframe (0, -1), new Keyframe (1, 1));
		waveShape.postWrapMode = WrapMode.Loop;	
		//debug stuff. the temporary curve editor thing
		for (int i = 1; i < (curveMatrixDim-1); i++) {
			waveShape.AddKey ((float)i / (curveMatrixDim-1), waveShape.Evaluate ((float)i / (curveMatrixDim-1)));
		}

	}
	void PlayNote(int thisNote){
		//search for available audiosource
		//and play note there
		//(can it ever occur that the sequencer tells to play a note already playing? to keep in mind)
		foreach (AudioSource a in audSource) {
			if (a.isPlaying) {
				//print ("audiosource already playing a note");
				if (a.clip == clips[thisNote]){
					//print ("and it's the same note");
				}
				else{
					//print ("a different note. looking for another audiosource");
				}
			} else {
				a.clip = clips [thisNote];
				//reset stuff like envelopes
				a.volume = volumeKnob;
				//play and exit
				a.Play ();
				//a.PlayScheduled(AudioSettings.dspTime+1);
				break;
			}
		}


	}
	void StopNote(int thisNote){
		foreach (AudioSource a in audSource) {
			if (a.isPlaying && a.clip == clips[thisNote]) {
				//print ("Found audiosource playing the note");
				a.Pause ();
			}
		}
	}
	void CreateAudioSources(){
		audSource = new AudioSource[numVoices];
		voices = new GameObject[numVoices];
		reverb = new AudioReverbFilter[numVoices];
		lowPass = new AudioLowPassFilter[numVoices];
		for (int i = 0; i<=numVoices-1; i++) {
			voices [i] = new GameObject ("voice" + i.ToString ());
			voices [i].transform.parent = transform;
			audSource[i] = voices[i].AddComponent <AudioSource>() as AudioSource;
			audSource [i].loop = true;
			audSource [i].dopplerLevel = 0.0f;
			audSource [i].volume = 0.5f;
			//add effects components
			reverb[i] = voices[i].AddComponent <AudioReverbFilter>() as AudioReverbFilter;
			lowPass[i] = voices[i].AddComponent <AudioLowPassFilter>() as AudioLowPassFilter;
		}

	}

	//debug var
	public float elapst = 0.0f;

	void Update(){

		//mais debug
		foreach (AudioSource a in audSource) {
			//a.volume = (Mathf.Sin (3 * elapst) + 1) / 10.0f;
			if (fallOff != 0.0f)
				a.volume -= fallOff * Time.deltaTime;
		}
		elapst += Time.deltaTime;

		if (Input.GetKeyDown (KeyCode.Z)) {
			print ("aqyui");
			audSource [0].UnPause ();
			audSource[0].PlayScheduled(AudioSettings.dspTime+1);
			audSource[0].PlayScheduled(AudioSettings.dspTime+2);
			audSource[0].PlayScheduled(AudioSettings.dspTime+3);
			audSource[0].PlayScheduled(AudioSettings.dspTime+4);
		}

		if (controls == 0) {
			//debug piano keyboard
			if (Input.GetKeyDown (KeyCode.A)) {			
				PlayNote (30);
			}
			if (Input.GetKeyUp (KeyCode.A)) {			
				StopNote (30);
			}
			if (Input.GetKeyDown (KeyCode.S)) {
				PlayNote (31);
			}
			if (Input.GetKeyUp (KeyCode.S)) {			
				StopNote (31);
			}
			if (Input.GetKeyDown (KeyCode.D)) {
				PlayNote (32);
			}
			if (Input.GetKeyUp (KeyCode.D)) {			
				StopNote (32);
			}
			if (Input.GetKeyDown (KeyCode.F)) {
				PlayNote (33);
			}
			if (Input.GetKeyUp (KeyCode.F)) {			
				StopNote (33);
			}
			if (Input.GetKeyDown (KeyCode.G)) {
				PlayNote (34);
			}
			if (Input.GetKeyUp (KeyCode.G)) {			
				StopNote (34);
			}
			if (Input.GetKeyDown (KeyCode.H)) {
				PlayNote (35);
			}
			if (Input.GetKeyUp (KeyCode.H)) {			
				StopNote (35);
			}
			if (Input.GetKeyDown (KeyCode.J)) {
				PlayNote (36);
			}
			if (Input.GetKeyUp (KeyCode.J)) {			
				StopNote (36);
			}
			if (Input.GetKeyDown (KeyCode.K)) {
				PlayNote (37);
			}
			if (Input.GetKeyUp (KeyCode.K)) {			
				StopNote (37);
			}
			if (Input.GetKeyDown (KeyCode.L)) {
				PlayNote (38);
			}
			if (Input.GetKeyUp (KeyCode.L)) {			
				StopNote (38);
			}
		}
		if (controls == 1) {
			//debug piano keyboard
			if (Input.GetKeyDown (KeyCode.Z)) {			
				PlayNote (30);
			}
			if (Input.GetKeyUp (KeyCode.Z)) {			
				StopNote (30);
			}
			if (Input.GetKeyDown (KeyCode.X)) {
				PlayNote (31);
			}
			if (Input.GetKeyUp (KeyCode.X)) {			
				StopNote (31);
			}
			if (Input.GetKeyDown (KeyCode.C)) {
				PlayNote (32);
			}
			if (Input.GetKeyUp (KeyCode.C)) {			
				StopNote (32);
			}
			if (Input.GetKeyDown (KeyCode.V)) {
				PlayNote (33);
			}
			if (Input.GetKeyUp (KeyCode.V)) {			
				StopNote (33);
			}
			if (Input.GetKeyDown (KeyCode.B)) {
				PlayNote (34);
			}
			if (Input.GetKeyUp (KeyCode.B)) {			
				StopNote (34);
			}
			if (Input.GetKeyDown (KeyCode.N)) {
				PlayNote (35);
			}
			if (Input.GetKeyUp (KeyCode.N)) {			
				StopNote (35);
			}
			if (Input.GetKeyDown (KeyCode.M)) {
				PlayNote (36);
			}
			if (Input.GetKeyUp (KeyCode.M)) {			
				StopNote (36);
			}
		}
	}

	void UpdateEffects(){
		foreach (AudioReverbFilter r in reverb) {
			r.decayTime = reverbKnob;
		}
		foreach (AudioLowPassFilter l in lowPass) {
			l.cutoffFrequency = lowPassKnob;
		}
		foreach (AudioSource a in audSource) {
			a.volume = volumeKnob;
		}
	}

	void UpdateClips(){
		//create all clips for every note

		float a = Mathf.Pow(2.0f,1.0f/12.0f);
		//print (a);
		notes = new float[48];
		clips = new AudioClip[48];
		notes [34] = 440.0f+freqDispl; //middle A
		//float localFreq = frequency;
		for (int i = 0; i <= 47; i++) {
			int n = i - 34;
			notes [i] = notes [34] * Mathf.Pow(a,n);
			frequency = notes [i];
			string clipname = "note" + n.ToString ();
			clips[i] = AudioClip.Create(clipname, samplerate *2, 1, samplerate, false, OnAudioRead, OnAudioSetPosition);
			//print (notes [i]);
		}

	}
	void OnAudioRead(float[] data) {
		int count = 0;
		while (count < data.Length) {
			if(curShape == curveTypes.square)
				data[count] = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate)); //square wave
			if(curShape == curveTypes.sine)
				data[count] =        Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate); //sine wave
			if(curShape == curveTypes.custom)
				data[count] = waveShape.Evaluate(frequency * position / samplerate); //custom wave
			if(curShape == curveTypes.noise)
				data[count] = 2*Random.value-1;// noise
			if(curShape == curveTypes.saw)
				data[count] = 2*(position*frequency / samplerate - Mathf.Floor(1/2 + position *frequency / samplerate) );// saw

			//testing adding noise to sine
			//data[count] = Mathf.Sin(2 * Mathf.PI * frequency * position / samplerate)/2 + (2*Random.value-1)/2;

			//sine wave interference test
			//data [count] = Mathf.Sin (2 * Mathf.PI * frequency * position / samplerate) / 2
			//	+ Mathf.Sin (2 * Mathf.PI * (frequency+ 1.1f) * position / samplerate) / 2;

			//custom wave interference test
			//data [count] = waveShape.Evaluate (frequency * position / samplerate * 2)/2
			//	+ waveShape2.Evaluate ( frequency*(phase+0.5f) * (position) / samplerate * 2)/2;

			//desisti da ideia de duas ondas. e' mais inteligente permitir 
			//usar mais de um sintetizador na definicao de cada instrumento

			position++;
			count++;
		}
	}
	void OnAudioSetPosition(int newPosition) {
		position = newPosition;
	}

	void OnGUI(){		
		windowRect = GUILayout.Window (synthIndex, windowRect, SynthParametersWindow, synthName);
	}

	void SynthParametersWindow(int windowID){
		GUI.DragWindow (new Rect (0, 0, 250, 20));

		//only debug stuff
		synthName = GUILayout.TextField(synthName);
		GUILayout.Label ("volume");
		volumeKnob = GUILayout.HorizontalSlider (volumeKnob,0,1);
		GUILayout.Label ("falloff");
		fallOff = GUILayout.HorizontalSlider (fallOff,0,1);
		GUILayout.Label ("reverb");
		reverbKnob = GUILayout.HorizontalSlider (reverbKnob, 0.1f, 20.0f);
		GUILayout.Label ("lowpass");
		lowPassKnob = GUILayout.HorizontalSlider (lowPassKnob,0,22000);
		GUILayout.Label ("frequency");
		freqDispl = GUILayout.HorizontalSlider (freqDispl,-100,100);

		if (GUILayout.Button ("Update clips and effects")) {
			UpdateClips ();
			UpdateEffects ();
		}

		GUILayout.BeginVertical ();
		for (int i = 0; i < curveMatrixDim ; i++) {
			GUILayout.BeginHorizontal ();
			for (int j = 0; j < curveMatrixDim; j++) {				
				//if(waveShape.Evaluate((float)j/curveMatrixDim) > (2*(float)i/curveMatrixDim-1)){
				if(waveShape[j].value > (2*(float)i/curveMatrixDim-1)){
					if (GUILayout.Button ("X", GUILayout.Width (20), GUILayout.Height (20))) {
						Keyframe newk= waveShape [j];
						newk.value = 2 * (float)i / curveMatrixDim - 1;
						waveShape.MoveKey (j, newk);
						UpdateClips ();
					}
				}else{
					if (GUILayout.Button (" ", GUILayout.Width (20), GUILayout.Height (20))) {
						Keyframe newk2= waveShape [j];
						newk2.value = 2 * (float)i / curveMatrixDim - 1;
						waveShape.MoveKey (j, newk2);
						UpdateClips ();
					}
				}
				waveShape.SmoothTangents (j, 0);
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical ();

		GUILayout.Label (curShape.ToString ());
		if (GUILayout.Button ("sine wave")) {
			curShape = curveTypes.sine;
			UpdateClips ();
		}
		if (GUILayout.Button ("saw")){
			curShape = curveTypes.saw;
			UpdateClips ();
		}
		if (GUILayout.Button ("custom")){
			curShape = curveTypes.custom;
			UpdateClips ();
		}
		if (GUILayout.Button ("noise")){
			curShape = curveTypes.noise;
			UpdateClips ();
		}
		if (GUILayout.Button ("square")){
			curShape = curveTypes.square;
			UpdateClips ();
		}

	}
}

