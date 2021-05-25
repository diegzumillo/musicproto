using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInterfaceNewUI : MonoBehaviour {

	public Sprite selIns;
	public Sprite unselIns;
	public GameObject instrumentList;
	public float test = 0;
	public 

	// Use this for initialization
	void Start () {
		
	}
		
	// Update is called once per frame
	void Update () {
		
	}

	public void AddInstrument(){

	}

	//a function for each menu
	//the numbering needs to be updated for every fucking new item
	public void FileMenu(int f){
		print ("item " + f.ToString ());
		if (f == 2)
			Application.Quit ();
	}

	public void Quit(){
		print ("quitting");
		Application.Quit ();
	}

}
