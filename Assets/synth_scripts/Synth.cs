using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Voice {
	public float instGain = 0.0f;
	public float instCurFreq = 440.0f; //dont need
	public int instCurNote = 34; //dont need
	public float[] gain ;
	public float[] curFreq;
	public int[] curNote;
	public int pos;
	public bool[] startNote; //dont need
	public float phase; //dont need
	public bool[] playing;
	public bool[] restart;

	public float dGain;
	public float dFreq;

	public Voice(int nt){	
		gain = new float[nt];
		curFreq = new float[nt];
		curNote = new int[nt];
		startNote = new bool[nt];
		restart = new bool[nt];
		playing = new bool[nt];
		phase = 2 * Random.value;
		pos = 0;

		dGain = 0;
		dFreq = 440;

		for (int t=0; t < nt; t++) {
			restart [t] = false;
			playing [t] = false;
			gain [t] = 0;
			curFreq [t] = 0;
			curNote [t] = 0;
			startNote [t] = false;
		}
	}
	public bool IsAvailable(){
		if (dGain == 0.0f)
			return true;
		else return false;
	}
	public void PlayNote(float f, float v){			
		dGain = v;
		dFreq = f;
		pos = 0;
		//print ("voice sounding");
	}
	public void Stop(){
		dGain = 0.0f;
		//print ("voice stopping");
	}
	public bool AvailableAt(int n, int t){
		if (gain [t] == 0)
			return true;
		else if (curNote [t] == n)
			return true;		
		return false;
	}
	public void PlayNoteAt(int n,float f, int t){
		gain [t] = 0.17f;
		curNote [t] = n;
		curFreq [t] = f; //lazy work...
		startNote[t] = true;
	}
	public void StopAt(int t){
		gain [t] = 0;
	}
}

public class Synth: MonoBehaviour {

	public int position = 0;
	public int samplerate = 44100;
	public float frequency = 440;
	public AnimationCurve waveShape;
	public float[] notes;
	public AudioClip[] clips;
	public float localTimer = 0.0f; //remove
	public float freqDispl = 0.0f; //remove

	public Rect windowRect = new Rect(20, 20, 280, 500); //temporary
	public int synthIndex = 0; //temporary. only used by window

	public MainTrack mainTrack;
	public Instrument instrument;

	//6 audiosources for 6 voices
	public int numVoices =6;
	public AudioSource audSource;
	//filters
	public AudioReverbFilter reverb; //remove and add it as single component
	public AudioLowPassFilter lowPass; //remove and add it as simgle component

	//exposed parameters TEMPORARY STUFF
	public string synthName;
	public float volumeKnob = 0.5f; //replace with envelopes?
	public float reverbKnob = 0.5f; 
	public float lowPassKnob = 22000.0f;
	public float fallOff = 0.0f;
	public float fadeIn = 0.0f;

	//matrix to define curve(s) Might need something better for the final editor
	public int curveMatrixDim = 10;
	public enum curveTypes{sine,square,saw,noise,custom};
	public curveTypes curShape ;
	public Texture2D noisyTex;
	public float[] noisyArray;

	//tempo and metronome stuff TO BE MOVED SOMEHWEREELSE EVENTUALLY
	public float gain = 0.5F;
	public int signatureHi = 4;
	public int signatureLo = 4;
//	private float amp = 0.0F;
//	private float phase = 0.0F;
//	private int accent;
	//private bool running = false;


	//debug stuff
	public int controls = 0;
	public float teste1=0.0f;
	public float teste2=0.0f;
	public bool debool = false;

	//class for voices. number of voices could be dynamic?
	//NOVO PLANO:
	//(pra eu nao esquecer)
	//a ideia agora é fazer uma track pra cada voice
	//cada alteracao na track geral chama uma funcao que
	//atualiza a track das vozes tambem. O motivo é aquela função
	//OnAudioFilterRead. Ela não se comunica muito bem com eventos
	//externos, entao tudo precisa estar mapeado antes de dar play
	//cada nota, cada mudança de volume etc.

	public Voice[] voice;

	//PERLIN NOISE is a good idea but hard to make it work
	//went back to simple noise
	void UpdateNoise(){
		noisyArray = new float[2048];
		for (int i = 0; i < 2048; i++) {
			noisyArray [i] = 2*Random.value-1;
		}
	}

	void Start() {

		UpdateNoise ();

		//create audio source and filters
		audSource = gameObject.AddComponent<AudioSource> () as AudioSource;
		audSource.dopplerLevel = 0.0f;
		audSource.volume = 0.5f;
		audSource.playOnAwake = false;
		reverb = gameObject.AddComponent<AudioReverbFilter> () as AudioReverbFilter;
		lowPass = gameObject.AddComponent<AudioLowPassFilter> () as AudioLowPassFilter;

		//shape wave stuff
		waveShape = new AnimationCurve (new Keyframe (0, -0.9f), new Keyframe (1, 0.9f));
		waveShape.postWrapMode = WrapMode.Loop;
		waveShape.preWrapMode = WrapMode.Loop;
		//debug stuff. the temporary curve editor thing
		for (int i = 1; i < (curveMatrixDim-1); i++) {
			waveShape.AddKey ((float)i / (curveMatrixDim-1), waveShape.Evaluate ((float)i / (curveMatrixDim-1)));
		}

		//create new voices
		voice = new Voice[numVoices];
		for (int i=0; i<numVoices; i++) {				
			voice[i] = new Voice(mainTrack.trackTimeDim);
		}

		//create frequency-note table
		float a = Mathf.Pow(2.0f,1.0f/12.0f);
		//print (a);
		notes = new float[mainTrack.trackNotesDim];
		//clips = new AudioClip[48];
		notes [34] = 440.0f+freqDispl; //middle A
		//float localFreq = frequency;
		for (int i = 0; i < mainTrack.trackNotesDim; i++) {
			int n = i - 34;
			notes [i] = notes [34] * Mathf.Pow(a,n);
		}

		//instrument.UpdateEntireTrack ();
	}

	public void ClearAll(){
		foreach (Voice v in voice) {
			for (int t = 0; t < mainTrack.trackTimeDim; t++) {
				v.restart [t] = false;
				v.playing [t] = false;
				v.gain [t] = 0;
				v.curFreq [t] = 0;
				v.curNote [t] = 0;
			}
		}
	}

	public void InsertNote(Note n){
		voice [n.voice].restart [n.start] = true;
		voice [n.voice].gain [n.start] = 1;
		voice [n.voice].playing [n.start] = true;
		voice [n.voice].curFreq [n.start] = notes[n.note];
		if (n.duration > 1) {
			for (int t = 1; t < n.duration; t++) {
				voice [n.voice].playing[n.start+t] = true;
				voice [n.voice].curFreq [n.start + t] = notes[n.note];
			}
		}
	}

	public void PlayNoteAt(int n, int t){
		//here is where synth delegates notes to the voices
		//bool foundAvailable = false;
		if (voice == null) {
			return;
		}
		foreach (Voice v in voice) {
			if (v.AvailableAt (n, t)) {
				v.PlayNoteAt (n, notes [n], t);
				//foundAvailable = true;
				break;
			}
		}
		//if (!foundAvailable)
		//	print ("no voice available");
	}
	public void StopNoteAt(int n, int t){
		if (voice == null) {
		//	print ("not ready yet");
			return;
		}
		foreach (Voice v in voice) {
			if (v.curNote [t] == n)
				v.StopAt (t);
		}
	}

	//REMOVE THIS I DONT NEED IT ANYMORE
	public void UpdateTrack(int n, int t){
		//here is where synth delegates notes to the voices
		bool foundAvailable = false;
		if (n == -1) {
			foreach (Voice v in voice) {
				v.StopAt (t);
				print ("silencing every voice at " + t.ToString ());
			}
			return;
		}
		foreach (Voice v in voice) {
			if (v.AvailableAt (n, t)) {
				v.PlayNoteAt (n, notes [n], t);
				foundAvailable = true;
				break;

			}
		}
		if (!foundAvailable)
			print ("no voice available");
	}

	//testingstuff
	public int localTrackTick = 0;
	public double initAudioTime = AudioSettings.dspTime;
	public double prevAudioTime;
	public double thisTickTime = 0;
	public int curTick = 0;

	void OnAudioFilterRead(float[] data, int channels) {
		//double dT = AudioSettings.dspTime - initialAudioTime;
		//double samplesPerClick = samplerate * 60.0/bpm *4.0 /signaturelo

		//testing
		teste1 = data.Length;

		if (voice == null)
			return;

		int simultVoices = 0;

		double dt = AudioSettings.dspTime - prevAudioTime;
		thisTickTime += dt;
		teste2 = (float)dt;

		if (instrument.dynamicPlay) {
			for (var i = 0; i < data.Length; i=i+channels) {
				data [i] = 0.0f;
				if(voice == null) return;

				simultVoices = 0;
				foreach (Voice v in voice)
					if (v.dGain > 0)
						simultVoices++;
				if (simultVoices == 0)
					simultVoices = 1;
				
				foreach (Voice v in voice) {
					//need to implement dynamic version of fall off and etc
					if (v.dGain != 0) {	
						float g = Mathf.Clamp01 (v.dGain - v.pos * fallOff/1000 )  / (simultVoices + 1) ;
						//float g = Mathf.Clamp01 (v.dGain - v.pos * fallOff/1000 ) ;
						if (curShape == curveTypes.square)
							data [i] += g * Mathf.Sign (Mathf.Sin (2 * Mathf.PI * (v.dFreq + freqDispl) * position / samplerate)); //square wave
						if (curShape == curveTypes.sine)
							data [i] += g * Mathf.Sin (2 * Mathf.PI * (v.dFreq + freqDispl) * position / samplerate); //sine wave
						if (curShape == curveTypes.custom)
							data [i] += g * waveShape.Evaluate ((v.dFreq + freqDispl) * (float)position / samplerate); //custom wave
						if (curShape == curveTypes.noise)
							data [i] += g * noisyArray [i];// noise
						if (curShape == curveTypes.saw)
							data [i] += g * 2 * (position * (v.dFreq + freqDispl) / samplerate 
								- Mathf.Floor (1 / 2 + position * (v.dFreq + freqDispl) / samplerate));// saw
						v.pos ++;
					}
				}
				//just in case
				if (data [i] > 1) {					
					data [i] = 1;
				} else if (data [i] < -1) {	
					data [i] = -1;
				}				
				if (channels == 2) data[i + 1] = data[i];
				position++;
			}
		} else if (mainTrack.currentPlayMode == MainTrack.playModes.precise && mainTrack.playing) {

			//double samplesPerTick = samplerate * 60.0F / TempoSettings.bpm * 4.0F / signatureLo;
			//double sample = AudioSettings.dspTime * samplerate;
			int dataLen = data.Length;// / channels;
			int n = 0;
			debool = true;
			while (n < dataLen) {
				int clickpos = 0;
				
				double temp = (AudioSettings.dspTime - mainTrack.startTick )*mainTrack.bpm/10 ; 
				//this could be cleaner

				int prevT = curTick;
				curTick = Mathf.FloorToInt (Mathf.Repeat((float)temp,mainTrack.trackTimeDim));
				int nextT = curTick + 1;
				if (nextT == mainTrack.trackTimeDim)
					nextT = 0;				
				int T = curTick;
				if (prevT != T) {
					//this is the intersection. i guess i have to do things here
					foreach (Voice v in voice) {
						if (v.restart [T]) {
							v.pos = 0;
							v.instGain = v.gain [T];
							//print ("restarting");
						}
					}
					T = nextT;
					thisTickTime = 0;
				}

				//modulate gain by number of voices
				simultVoices = 0;
				foreach (Voice v in voice)
					if (v.playing [T])
						simultVoices++;
				if (simultVoices == 0)
					simultVoices = 1;
					
				data [n] = 0;
				foreach (Voice v in voice) {
					
					//nao sei como fazer fade in funcionar..
					if(fadeIn !=0)						
						//v.instGain = Mathf.Clamp01 (Mathf.Lerp(0,v.instGain, v.pos*fadeIn/10000000) - v.pos * fallOff/10000000 );
						v.instGain = Mathf.Clamp01 (Mathf.Lerp(0,v.instGain, v.pos*fadeIn/10000000) - v.pos * fallOff/10000000 );
					else
						v.instGain = Mathf.Clamp01 (v.instGain - v.pos * fallOff/10000000 );
					float g = v.instGain / (simultVoices+1);

					float f = v.curFreq [T] + freqDispl;


					if (curShape == curveTypes.square)
						data [n] +=  g* Mathf.Sign (Mathf.Sin (2 * Mathf.PI * f  * v.pos / samplerate +v.phase)); //square wave
					if (curShape == curveTypes.sine)
						data [n] +=  g * Mathf.Sin (2 * Mathf.PI * f * (v.pos) / samplerate); //sine wave
					if (curShape == curveTypes.custom)
						data [n] +=  g * waveShape.Evaluate (f * (float) (v.pos) / samplerate +v.phase); //custom wave
					if (curShape == curveTypes.noise)
						data [n] +=  g * noisyArray [ (int)Mathf.Repeat(position,noisyArray.Length)];// noise						
					if (curShape == curveTypes.saw)
						data [n] +=  g * 2 * ( (v.pos+v.phase) * f / samplerate - 
							Mathf.Floor (1 / 2 +  (v.pos+v.phase) * f / samplerate));// saw
					
					if(v.playing[T])
						v.pos++;
				}

				//just in case
				if (data [n] > 1) {					
					data [n] = 1;
				} else if (data [n] < -1) {	
					data [n] = -1;
				}
				if (channels == 2) data[n + 1] = data[n];
				n = n+channels;
				position++;
				clickpos++; //i dont know what im doing
			}


		}

		prevAudioTime = AudioSettings.dspTime;

	}



	//dynamic stuff:
	public void PlayNote(float thisFrequency){
		//look for an available voice
		for (int i = 0; i < numVoices; i++) {
			if (voice [i].IsAvailable ()) {
				voice [i].PlayNote (thisFrequency, 0.5f);
				break;
			}
		}
	}
	public void PlayNote(int thisNote, float speed){
		//look for an available voice
		for (int i = 0; i < numVoices; i++) {
			if (voice [i].IsAvailable ()) {
				voice [i].PlayNote (notes[thisNote], speed);
				break;
			}
		}
	}
	public void StopNote(float thisFrequency){
		for (int i = 0; i < numVoices; i++) {
			if (voice [i].dFreq == thisFrequency)
				voice [i].Stop ();
		}
	}

	//-1 sends a signal to stop regardless of the note
	public void StopNote(int thisNote){		
		for (int i = 0; i < numVoices; i++) {
			if(thisNote == -1) voice [i].Stop ();
			else if ( voice [i].dFreq == notes[thisNote] )
				voice [i].Stop ();
		}
	}


	void Update(){
		
		//iterate over all voices to update its state
		//
		UpdateNoise();

		//dynamic mode stuff
		if (instrument.dynamicPlay) {
			
			foreach (Voice v in voice) {
				v.instGain = 0;
			}

			if (controls == 0) {

				/*for (int n = 0; n < 100; n++) {
					float curK = MidiJack.MidiMaster.GetKey (n);
					//PlayNote (40, curK);
					if (MidiJack.MidiMaster.GetKeyDown (n)) {
						print (curK.ToString ());
						print ("key 40");
						PlayNote (n, curK);
					} 
					if (MidiJack.MidiMaster.GetKeyUp (n)) {
						StopNote (n);
					}
				}*/

				//debug piano keyboard
				if (Input.GetKeyDown (KeyCode.A) ) {			
					PlayNote (30, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.A)) {			
					StopNote (30);
				}
				if (Input.GetKeyDown (KeyCode.S)) {
					PlayNote (31, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.S)) {			
					StopNote (31);
				}
				if (Input.GetKeyDown (KeyCode.D)) {
					PlayNote (32, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.D)) {			
					StopNote (32);
				}
				if (Input.GetKeyDown (KeyCode.F)) {
					PlayNote (33, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.F)) {			
					StopNote (33);
				}
				if (Input.GetKeyDown (KeyCode.G)) {
					PlayNote (34, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.G)) {			
					StopNote (34);
				}
				if (Input.GetKeyDown (KeyCode.H)) {
					PlayNote (35, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.H)) {			
					StopNote (35);
				}
				if (Input.GetKeyDown (KeyCode.J)) {
					PlayNote (36, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.J)) {			
					StopNote (36);
				}
				if (Input.GetKeyDown (KeyCode.K)) {
					PlayNote (37, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.K)) {			
					StopNote (37);
				}
				if (Input.GetKeyDown (KeyCode.L)) {
					PlayNote (38, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.L)) {			
					StopNote (38);
				}


				//medium notes
				#region
				if (Input.GetKeyDown (KeyCode.Z)) {			
					PlayNote (10, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.Z)) {			
					StopNote (10);
				}
				if (Input.GetKeyDown (KeyCode.X)) {
					PlayNote (11, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.X)) {			
					StopNote (11);
				}
				if (Input.GetKeyDown (KeyCode.C)) {
					PlayNote (12, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.C)) {			
					StopNote (12);
				}
				if (Input.GetKeyDown (KeyCode.V)) {
					PlayNote (13, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.V)) {			
					StopNote (13);
				}
				if (Input.GetKeyDown (KeyCode.B)) {
					PlayNote (14, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.B)) {			
					StopNote (14);
				}
				if (Input.GetKeyDown (KeyCode.N)) {
					PlayNote (15, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.N)) {			
					StopNote (15);
				}
				if (Input.GetKeyDown (KeyCode.M)) {
					PlayNote (16, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.M)) {			
					StopNote (16);
				}
				#endregion

				//low notes
				if (Input.GetKeyDown (KeyCode.Q)) {			
					PlayNote (00, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.Q)) {			
					StopNote (00);
				}
				if (Input.GetKeyDown (KeyCode.W)) {
					PlayNote (01, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.W)) {			
					StopNote (01);
				}
				if (Input.GetKeyDown (KeyCode.E)) {
					PlayNote (02, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.E)) {			
					StopNote (02);
				}
				if (Input.GetKeyDown (KeyCode.R)) {
					PlayNote (03, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.R)) {			
					StopNote (03);
				}
				if (Input.GetKeyDown (KeyCode.T)) {
					PlayNote (04, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.T)) {			
					StopNote (04);
				}
				if (Input.GetKeyDown (KeyCode.Y)) {
					PlayNote (05, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.Y)) {			
					StopNote (05);
				}
				if (Input.GetKeyDown (KeyCode.U)) {
					PlayNote (06, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.U)) {			
					StopNote (06);
				}
				if (Input.GetKeyDown (KeyCode.I)) {
					PlayNote (07, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.I)) {			
					StopNote (07);
				}
				if (Input.GetKeyDown (KeyCode.O)) {
					PlayNote (08, 0.5f);
				}
				if (Input.GetKeyUp (KeyCode.O)) {			
					StopNote (08);
				}

			}
		}
	}


	void OnAudioSetPosition(int newPosition) { //REMOVE IT? ORA QUE SERVE MESMO?
		position = newPosition;
	}

}

