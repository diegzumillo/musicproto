using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*decidi que vou separar o editor de layer do resto da interface
 * nao vou mais usar o mesmo esquema de selecionar layer e qual ta ativada etc
 * porque layers nao sao que nem os instrumentos. Esse editor precisa do seu 
 * proprio save load etc! e a main track vai ter que ter um load de layer
 * proprio tambem. Entao coisas como selLayer do editor de layer e 
 * da main track nao sao a mesma coisa. Nao eh util deixar tudo junto
 */



public class LayerEditor : MonoBehaviour {

	public class LayerTab {
		public Layer myLayer;
		public RectTransform myPanel;
		public Transform myContentPanel;
		public List<GameObject> layerObjects;
		public LayerTab(){
			layerObjects = new List<GameObject>();
		}
	}

	//public Layer openedLayer;
	public LayerTab curTab;
	//until theres a save/open features ill just keep a list
	//synchronized with the main track:
	public List<LayerTab> loadedLayers;

	public Dropdown dropDownLayers;
	public Canvas layerCreationCanvas;
	public RectTransform layerPanelPrefab;

	//prefabs
	public GameObject IfBlockPrefab;
	public GameObject VarPrefab;
	public GameObject PrinterPrefab;
	public GameObject SumPrefab;
	public GameObject StartPrefab;

	//when dragging connectors i need to know if mouse is over another connector
	public Connector connectorHovering;


	//references to other scripts
	public UserInterface UI; //set in inspector
	public MainTrack mainTrack;

	//the button that calls this is created on editor
	public void NewLayer(){
		//RectTransform newLayerPanel = RectTransform.Instantiate (layerPanelPrefab) as RectTransform;
		//newLayerPanel.transform.SetParent (layerCreationCanvas.transform, false);
		Layer newlayer = mainTrack.AddLayer();
		LayerTab newtab = new LayerTab ();
		newtab.myLayer = newlayer;
		newtab.myPanel = RectTransform.Instantiate (layerPanelPrefab) as RectTransform;

		newtab.myContentPanel = newtab.myPanel.transform.Find ("Viewport").Find ("Content");

		newtab.myPanel.SetParent(layerCreationCanvas.transform, false);
		curTab = newtab;
		loadedLayers.Add (newtab);
		SwitchLayer ();
		UpdateLayerDropDown ();

		// add starting block
		//AddStart();
	}

	void UpdateLayerDropDown(){
		//this is so fucking stupid...
		List<Dropdown.OptionData> templistlayers = new List<Dropdown.OptionData>();

		for (int i = 0 ; i< loadedLayers.Count; i++) {
			Dropdown.OptionData temp = new Dropdown.OptionData (loadedLayers[i].myLayer.myName);
			templistlayers.Add (temp);
		}
		dropDownLayers.ClearOptions ();
		dropDownLayers.AddOptions (templistlayers);
	}

	//setup on inspector
	public void SwitchLayer(){
		//not sure how reliable this method is, but it seems to work so far..
		print ("switching to option " + dropDownLayers.value.ToString());
		foreach (LayerTab t in loadedLayers) {
			t.myPanel.gameObject.SetActive (false);
		}
		curTab = loadedLayers [dropDownLayers.value];
		curTab.myPanel.gameObject.SetActive (true);
	}

	// Use this for initialization
	void Start () {
		UI = GetComponent<UserInterface> () as UserInterface;
		mainTrack = UI.mainTrack;

		//lists init
		loadedLayers = new List<LayerTab>();

		layerCreationCanvas.gameObject.SetActive (false);
		//contentPanel = transform.FindChild ("Content");

		NewLayer ();
		UpdateLayerDropDown ();

	}

	public void AddIfLargerBlock(){
		GameObject newifb = GameObject.Instantiate (IfBlockPrefab);
		newifb.transform.SetParent (curTab.myContentPanel);
		newifb.GetComponent<RectTransform>().localPosition = new Vector3(300,-300,0);
		newifb.GetComponent<IfBiggerUI> ().Initialize (curTab.myLayer);
		curTab.layerObjects.Add (newifb);
	}
	public void AddVariable(){
		GameObject newv = GameObject.Instantiate (VarPrefab);
		newv.transform.SetParent (curTab.myContentPanel);
		newv.GetComponent<RectTransform>().localPosition = new Vector3(300,-300,0);
		newv.GetComponent<VarUI> ().Initialize (curTab.myLayer);
		curTab.layerObjects.Add (newv);
	}
	public void AddPrinter(){
		GameObject newp = GameObject.Instantiate (PrinterPrefab);
		newp.transform.SetParent (curTab.myContentPanel);
		newp.GetComponent<RectTransform>().localPosition = new Vector3(300,-300,0);
		newp.GetComponent<PrintUI> ().Initialize (curTab.myLayer);
		curTab.layerObjects.Add (newp);
	}
	public void AddSum(){
		GameObject news = GameObject.Instantiate (SumPrefab);
		news.transform.SetParent (curTab.myContentPanel);
		news.GetComponent<RectTransform>().localPosition = new Vector3(300,-300,0);
		news.GetComponent<SumUI> ().Initialize (curTab.myLayer);
		curTab.layerObjects.Add (news);
	}
	public void AddStart(){
		GameObject news = GameObject.Instantiate (StartPrefab);
		news.transform.SetParent (curTab.myContentPanel);
		news.GetComponent<RectTransform>().localPosition = new Vector3(300,-200,0);
		news.GetComponent<StartUI> ().Initialize (curTab.myLayer);
		curTab.layerObjects.Add (news);
	}

	//there might be a more clever way of doing this without going through the whole network but whatevs
	public void EstablishConnections(){
		//not using this yet.. maybe dont need it
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
