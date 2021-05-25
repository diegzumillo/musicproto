using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;



public class UserInterface : MonoBehaviour{



	/* the interface is separated by these lines:
	 * > menus are aligned at the top of the screen and 
	 * fill the entire screen width and are delimited below to menuLine
	 * > then a tab region just below menuline and above tabLine
	 * this region is to switch between track maker, instrument maker and
	 * layer maker. Maybe more tabs later? Idunno..
	 * >the big middle of the screen is defined by the active tab
	 * and is defined below by shortcut line, which is adjusted 
	 * from the bottom of the screen.
	 * >the very bottom has a shortcut area. It comes with a play button
	 * for the main track as standard but can be configured to add 
	 * for example instrument controls
	 * ----------------------------------- screen top
	 * menu items
	 * ----------------------------------- menu line
	 * tab1 / tab2 / tab3  
	 * ----------------------------------- tab line
	 * 
	 *          stuff
	 * 
	 * ----------------------------------- shortcut line
	 * shortcuts
	 * ----------------------------------- screen bottom
	 */
	//ui vars
	public float menuLine = 20; //defined from the top
	public float tabLine = 30; //defined from menu line
	public float shortcutLine = 40; //defined from the bottom
	//these are for my convenience: (i never used them!)
	public float menuTop, menuBottom, tabTop, tabBottom,
	mainTop, mainBottom, shortcutTop, shortcutBottom;
	public bool mouseOverTrack;
	public GUISkin skin;

	//main track UI vars
	public float topToolsLine = 50;
	public Vector2 scrollMainTrack = Vector2.zero;
	public int noteSize = 1;
	//testing new way
	public Camera cam;
	public SequecerGridElement[] pointingAt;
	public bool mouseOverNote;
	public Note manipulatedNote;
	public Material horLines;
	public Material vertLines;
	public Material timeLineMat;
	public Transform timeLine;

	//instruments UI vars
	public float instrumentSideBar = 200;
	public float synthSideBar = 100;
	public Vector2 scrollViewInstrument = Vector2.zero;
	public Vector2 scrollViewSynth = Vector2.zero;
	public Instrument selInstrument;
	public Synth selSynth;

	//Terminal thing... not sure how to present this information just yet though
	public List<string> terminal;
	public bool showTerminal = false;
	public Rect terminalRect;
	public Vector2 terminalScroll = Vector2.zero;

	//layer stuff
	public Canvas layerCreationCanvas;

	public enum tabs {maintrack,instrument,layermaker};
	public tabs curTab = tabs.maintrack;

	//references to other scripts
	public MainTrack mainTrack; //set in inspector

	//debug and tests
	int test1 = 0;



	void Start () {

		mainTrack.UI = this;

		//eventually ill move to the new ui system..
		menuTop = 0;
		menuBottom = menuLine;
		tabTop = menuLine;
		tabBottom = tabLine + tabTop;
		mainTop = tabBottom;
		shortcutBottom = Screen.height;
		shortcutTop = shortcutBottom - shortcutLine;
		mainBottom = shortcutTop;

		//testing a new method
		pointingAt = new SequecerGridElement[2];
		GameObject tempgo;
		//making timeline thing
		tempgo = GameObject.CreatePrimitive (PrimitiveType.Cube);
		timeLine = tempgo.transform;
		timeLine.position = new Vector3 (-1, mainTrack.trackNotesDim/2, 1);
		timeLine.localScale = new Vector3(1,(float)mainTrack.trackNotesDim,1);
		timeLine.parent = transform;
		timeLine.GetComponent<Renderer> ().material = timeLineMat;


		for (int tick = 0; tick < mainTrack.trackTimeDim; tick++) {
			tempgo = GameObject.CreatePrimitive (PrimitiveType.Cube);
			tempgo.tag = "Tick";
			tempgo.transform.position = new Vector3 (tick, mainTrack.trackNotesDim/2, 1);
			tempgo.transform.localScale = new Vector3(1,(float)mainTrack.trackNotesDim,1);
			tempgo.transform.parent = transform;
			tempgo.GetComponent<Renderer> ().material = vertLines;
			SequecerGridElement tempseq = tempgo.AddComponent<SequecerGridElement> () as SequecerGridElement;
			tempseq.type = SequecerGridElement.TypeGrid.tick;
			tempseq.index = tick;
			tempgo.name = "tick_" + tick.ToString ();
		}
		for (int line = mainTrack.trackNotesDim - 1; line >= 0; line--) {
			tempgo = GameObject.CreatePrimitive (PrimitiveType.Cube);
			tempgo.tag = "NoteElement";
			tempgo.transform.position = new Vector3 (mainTrack.trackTimeDim/2, line, 1);
			tempgo.transform.localScale = new Vector3((float)mainTrack.trackTimeDim,1,1);
			tempgo.transform.parent = transform;
			tempgo.GetComponent<Renderer> ().material = horLines;
			SequecerGridElement tempseq = tempgo.AddComponent<SequecerGridElement> () as SequecerGridElement;
			tempseq.type = SequecerGridElement.TypeGrid.note;
			tempseq.index = line;
			tempgo.name = "note_" + line.ToString ();
		}



		//terminal stuff
		terminal = new List<string>();
		terminal.Add ("User Interface Start");
		terminalRect = new Rect (0, Screen.height / 2, Screen.width / 2, Screen.height / 2);
	}
	

	void Update () {
		
		if (Input.GetKeyDown (KeyCode.F1)) {
			if (showTerminal)
				showTerminal = false;
			else
				showTerminal = true;
		}


		if (curTab == tabs.maintrack) {

			//to visualize position of playback
			if (mainTrack.playing) {
				timeLine.position = new Vector3(mainTrack.trackTick, mainTrack.trackNotesDim/2, 1);
			}

			//is the cursor on the tracker or on the gui?
			float mx = Input.mousePosition.x;
			float my = Input.mousePosition.y;
			if (my < Screen.height-( topToolsLine + menuLine + tabLine) && my > shortcutLine
				&& mx>0 && mx< Screen.width)
				mouseOverTrack = true;
			else
				mouseOverTrack = false;

			//camera movement
			cam.transform.position += new Vector3 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"), 0);
			if (Input.GetButton ("Fire3")) {
				cam.transform.position -= new Vector3(Input.GetAxis ("Mouse X"),Input.GetAxis ("Mouse Y"),0) ;
			}

			//--------------
			//TEMPORARY
			//eventualmente issso vai ser trocado pra uma interface 
			//dependente da layer ativa. 
			//temporary note size edit
			if (manipulatedNote == null) {
				if (Input.GetAxis ("Mouse ScrollWheel") > 0)
					noteSize++;
				else if (Input.GetAxis ("Mouse ScrollWheel") < 0)
					noteSize--;
				if (noteSize < 1)
					noteSize = 1;
			} else {
				//change this to resize selected note
				if (Input.GetAxis ("Mouse ScrollWheel") > 0)
					noteSize++;
				else if (Input.GetAxis ("Mouse ScrollWheel") < 0)
					noteSize--;
				if (noteSize < 1)
					noteSize = 1;
			}

			//detecting where the mouse is on the tracker. position and note.

			manipulatedNote = null;
			RaycastHit[] hit = Physics.RaycastAll(cam.ScreenPointToRay(Input.mousePosition));
			if (hit.Length > 0) {
				foreach (RaycastHit r in hit) {
					if (r.collider.tag == "Note") {						

						Note tempnote  = r.collider.gameObject.GetComponent<NoteRepresentation> ().myNote ;
						if (selInstrument == tempnote.myInst) {
							manipulatedNote = tempnote;
						} else {
							//manipulatedNote = null;
							//someething to do here?
						}
					} else {
						if(r.transform.tag == "NoteElement")
							pointingAt [0] = r.transform.GetComponent<SequecerGridElement> () as SequecerGridElement;
						if(r.transform.tag == "Tick")
							pointingAt [1] = r.transform.GetComponent<SequecerGridElement> () as SequecerGridElement;
					}
				}
			}



			//depending on where the mouse is. we have different function for clicks
			if (manipulatedNote == null) {
				if (mouseOverTrack && Input.GetButtonDown ("Fire1")) {
					int n, t;
					//this is obsolete. i already make this check when making the pointingat array
					if (pointingAt [0].type == SequecerGridElement.TypeGrid.note) {
						n = pointingAt [0].index;
						t = pointingAt [1].index;
					} else {
						n = pointingAt [1].index;
						t = pointingAt [0].index;
					}
					if (selInstrument != null) {
						selInstrument.InsertNote (n, t, noteSize, 0);
						selInstrument.PlayNote (n, 0.2f);
					}
				
				}
			} else { //different things happen if we click on a note
				if (Input.GetButtonDown ("Fire2")) {
					manipulatedNote.myInst.RemoveNote (manipulatedNote, 0);
					manipulatedNote = null;
				}
			}
		}		
	}


	public void TerminalWindow(int windowID){
		//terminalScroll = GUILayout.BeginScrollView (terminalScroll);
		GUILayout.BeginScrollView (new Vector2(0,999));
		GUILayout.BeginVertical ();
		foreach (string s in terminal) {
			GUILayout.Label (s);
		}
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
	}


	//debug var
	public int test2 = 1;

	void OnGUI(){

		GUI.skin = skin;
		GUIStyle st = new GUIStyle ("Box");

		//defining menus area

		GUILayout.BeginArea (new Rect(0,0,Screen.width,menuLine) , st );
		GUILayout.BeginHorizontal ();
		GUILayout.Button ("there", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.Button ("will", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.Button ("be", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.Button ("menus", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.Button ("here", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.Button ("eventually", GUILayout.ExpandWidth(false),GUILayout.ExpandHeight(true));
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		//defining tabs area
		GUILayout.BeginArea (new Rect(0,menuLine,Screen.width,tabLine) , st  );
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Main Track", GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
			curTab = tabs.maintrack;
			layerCreationCanvas.gameObject.SetActive (false);
		}
		if (GUILayout.Button ("Instruments", GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {		
			curTab = tabs.instrument;
			layerCreationCanvas.gameObject.SetActive (false);
		}
		if (GUILayout.Button ("Layer Creation", GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
			curTab = tabs.layermaker;
			layerCreationCanvas.gameObject.SetActive (true);
		}
		
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		//defining main area
		//-------------------------
		//MAIN TRACK USER INTERFACE
		//-------------------------
		#region
		if (curTab == tabs.maintrack) {
			
			//top tool bar:
			GUILayout.BeginArea (new Rect(0,tabLine+menuLine,Screen.width,topToolsLine)  , st);
			GUILayout.BeginHorizontal ();

			GUILayout.BeginVertical();
			if(mainTrack.playing){
				if(GUILayout.Button("Stop",GUILayout.MaxWidth(60))) {
					mainTrack.Stop();
					timeLine.position = new Vector3(-1,mainTrack.trackNotesDim/2, 1);
				}
				if(selInstrument != null) mainTrack.SwitchDynamicInstrument(selInstrument);
			}else{
				if(GUILayout.Button("Play",GUILayout.MaxWidth(80))) mainTrack.Play();
			}

			GUILayout.EndVertical();
			GUILayout.Label("Note size "+noteSize.ToString());

			GUILayout.BeginVertical();
			GUILayout.Label("BPM " );
			mainTrack.bpm = (double)GUILayout.HorizontalSlider ((float)mainTrack.bpm , 0.1f, 1000f);
			GUILayout.EndVertical();

			foreach(Instrument ins in mainTrack.instrument){
				if(GUILayout.Button(ins.instrumentName)){
					selInstrument = ins;
					mainTrack.switchSelectedInstrument(ins);
				}
			}

			GUILayout.EndHorizontal ();
			GUILayout.EndArea();

			//---------------------------------
			//this is where notes appear. this is done in update because they are gameobjects
			//-----------------------------------

			GUILayout.BeginArea (new Rect(Screen.width*0.75f,tabLine+menuLine+topToolsLine,Screen.width*0.25f,Screen.height)  , st);
			GUILayout.BeginVertical();
			if(GUILayout.Button("addlayer"))				
				mainTrack.AddLayer();
			if(GUILayout.Button("update layers"))
				mainTrack.ApplyLayers();
			foreach(Layer l in mainTrack.layer){
				if(mainTrack.activeLayer == l.myInd){
					if(GUILayout.Button("-> "+l.myInd.ToString()+" <-"))
						mainTrack.SwitchActiveLayer(l.myInd);
				}else{
					if(GUILayout.Button(l.myInd.ToString()))
						mainTrack.SwitchActiveLayer(l.myInd);
				}
				
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();

			//--------------------------
		}
		#endregion
		//----------------------------------
		//INSTRUMENT CREATION USER INTERFACE
		//----------------------------------
		#region
		if (curTab == tabs.instrument) {
			float top = menuLine+tabLine, bottom = Screen.height-shortcutLine-tabLine-menuLine;

			//instrument side bar
			GUILayout.BeginArea (new Rect(0,top, instrumentSideBar, bottom)  , st);
			scrollViewInstrument = GUILayout.BeginScrollView(scrollViewInstrument);
			GUILayout.BeginHorizontal(); //this is just to add a vertical line
			GUILayout.BeginVertical();
			if(GUILayout.Button("Create new\ninstrument"))
				mainTrack.CreateInstrument();
			GUILayout.TextArea("",GUI.skin.horizontalSlider); //horizontal line


			foreach(Instrument inst in mainTrack.instrument){
				if(inst != null){
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("X"))
						mainTrack.RemoveInstrument(inst);
					inst.instrumentName = GUILayout.TextField(inst.instrumentName);
					if(selInstrument != inst){
						if(GUILayout.Button(">>")){
							selInstrument = inst;
							mainTrack.switchSelectedInstrument(inst);
							mainTrack.SwitchDynamicInstrument(inst);
						}
					}
					else GUILayout.Label("  ");
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndScrollView();

			GUILayout.EndVertical();
			GUILayout.TextArea("",GUI.skin.verticalSlider);//the vertical line!
			GUILayout.EndHorizontal(); //still the line thing
			GUILayout.EndArea();

			//synthesizers creation and selection side bar
			GUILayout.BeginArea (new Rect(instrumentSideBar,top, instrumentSideBar+synthSideBar, bottom)  , st);
			GUILayout.BeginVertical();
			if(selInstrument != null){
				if(GUILayout.Button("Create new\nsynthesizer"))
					selInstrument.AddSynth();
				GUILayout.TextArea("",GUI.skin.horizontalSlider); //horizontal line	

				foreach(Synth s in selInstrument.synths){
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("X"))
						selInstrument.RemoveSynth(s);
					s.synthName = GUILayout.TextField(s.synthName);
					if(s != selSynth){
						if(GUILayout.Button(">>"))
							selSynth = s;
					}else{
						GUILayout.Label("  ");
					}
					GUILayout.EndHorizontal();
				}
			} else GUILayout.Label("Select an\ninstrument");

			GUILayout.EndVertical();
			GUILayout.EndArea();

			//synth properties area
			GUILayout.BeginArea (new Rect(2*instrumentSideBar+synthSideBar,top, Screen.width-2*instrumentSideBar-synthSideBar, bottom) , st );
			GUILayout.BeginVertical();

			//-------------------

			if(selSynth != null){
				scrollViewSynth = GUILayout.BeginScrollView (scrollViewSynth);

				GUILayout.BeginHorizontal();
				test1 = int.Parse(GUILayout.TextField(test1.ToString()));
				foreach (Voice v in selSynth.voice) {
					GUILayout.VerticalSlider (v.instGain, 0, 1);
				}
				GUILayout.EndHorizontal ();



				selSynth.synthName = GUILayout.TextField(selSynth.synthName);
				GUILayout.Label ("volume");
				selSynth.audSource.volume = GUILayout.HorizontalSlider (selSynth.audSource.volume,0,1);
				GUILayout.Label ("falloff");
				selSynth.fallOff = GUILayout.HorizontalSlider (selSynth.fallOff,0,0.1f);
				GUILayout.Label ("fade in");
				selSynth.fadeIn = GUILayout.HorizontalSlider (selSynth.fadeIn,0,0.1f);
				GUILayout.Label ("reverb");
				selSynth.reverb.decayTime = GUILayout.HorizontalSlider (selSynth.reverb.decayTime, 0.1f, 20.0f);
				GUILayout.Label ("lowpass");
				selSynth.lowPass.cutoffFrequency = GUILayout.HorizontalSlider (selSynth.lowPass.cutoffFrequency,0,22000);
				GUILayout.Label ("frequency");
				selSynth.freqDispl = GUILayout.HorizontalSlider (selSynth.freqDispl,-100,100);

				//this breaks the whole thing
				//if you open this tab while playing the song
				//so i need a better way of updating the curve
				if(mainTrack.playing)
					GUILayout.Label("Stop the track to edit the custom curve");
				else{
					GUILayout.BeginVertical ();
					for (int i = 0; i < selSynth.curveMatrixDim ; i++) {
						GUILayout.BeginHorizontal ();
						for (int j = 0; j < selSynth.curveMatrixDim; j++) {		
							if(selSynth.waveShape[j].value > (2*(float)i/selSynth.curveMatrixDim-1)){
								if (GUILayout.Button ("X", GUILayout.Width (20), GUILayout.Height (20))) {
									Keyframe newk= selSynth.waveShape [j];
									newk.value = 2 * (float)i / selSynth.curveMatrixDim - 1;
									if(selSynth.waveShape.keys[j].value != newk.value)
										selSynth.waveShape.MoveKey (j, newk);
									else{
										print("the same key. not updating");
										terminal.Add("Same key. Not updating");
									}
								}
							}else{
								if (GUILayout.Button (" ", GUILayout.Width (20), GUILayout.Height (20))) {
									Keyframe newk2= selSynth.waveShape [j];
									newk2.value = 2 * (float)i / selSynth.curveMatrixDim - 1;

									if(selSynth.waveShape.keys[j].value != newk2.value)
										selSynth.waveShape.MoveKey (j, newk2);
									else{
										print("the same key. not updating");
										terminal.Add("Same key. Not updating");
									}
								}
							}
							selSynth.waveShape.SmoothTangents (j, 0);
						}
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical ();
				}

				GUILayout.Label (selSynth.curShape.ToString ());
				if (GUILayout.Button ("sine wave")) {
					selSynth.curShape = Synth.curveTypes.sine;
				}
				if (GUILayout.Button ("saw")){
					selSynth.curShape = Synth.curveTypes.saw;
				}
				if (GUILayout.Button ("custom")){
					selSynth.curShape = Synth.curveTypes.custom;
				}
				if (GUILayout.Button ("noise")){
					selSynth.curShape = Synth.curveTypes.noise;
				}
				if (GUILayout.Button ("square")){
					selSynth.curShape = Synth.curveTypes.square;
				}

				GUILayout.EndScrollView ();
			} else {
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label("Select a synthesizer");
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
			}

			//-------------------


			GUILayout.EndVertical();
			GUILayout.EndArea();


		}
		#endregion
		//----------------------------------
		//LAYER CREATION USER INTERFACE
		//----------------------------------
		//using the new ui for this part
		#region
		if (curTab == tabs.layermaker) {
			/*GUILayout.BeginArea (new Rect(0,menuLine+tabLine,Screen.width,Screen.height-shortcutLine-tabLine-menuLine)  , st);
			GUILayout.BeginVertical ();
			GUILayout.Button ("layermaker", GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
			GUILayout.Button ("layermaker", GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
			GUILayout.EndVertical ();
			GUILayout.EndArea ();*/
		}
		#endregion
		//defining shortcuts area
		GUILayout.BeginArea (new Rect(0,Screen.height-shortcutLine,Screen.width,shortcutLine)  , st);
		GUILayout.BeginHorizontal ();
		GUILayout.Button ("shortcuts area", GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true));
		GUILayout.EndHorizontal ();
		GUILayout.Label ("<F1>"+terminal [terminal.Count - 1]);
		GUILayout.EndArea ();

		//terminal window
		if (showTerminal)
			terminalRect = GUILayout.Window (0, terminalRect, TerminalWindow, "Terminal");

	}

	public void AddToTerminal(string s){
		terminal.Add (s);
	}

}
