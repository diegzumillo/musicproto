using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VarUI : MonoBehaviour {

	public int varValue; //this is just for show
	public int myInd;
	//public Layer.Variable myVariable; //wait.. why am i not using this from the start? -> because it doesnt work
	Layer myLayer;
	//LayerEditor editor;

	public void Assign(string v){
		myLayer.vars [myInd].Assign (v);
	}

	public void Initialize(Layer l){
		myLayer = l;
		myInd = l.CreateVariable (varValue);
	}

	// Use this for initialization
	void Start () {
		//ref to layereditor
		//editor = GameObject.Find("UserInterface").GetComponent<LayerEditor>() as LayerEditor;
	}

	//assim tipo, nao eh a coisa mais elegante do universo..
	public void UpdateAll(){
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
