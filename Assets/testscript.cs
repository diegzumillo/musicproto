using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testscript : MonoBehaviour {

	public List<string> teste;
	public string s;
	public string i = "123";

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnGUI(){
		s = GUILayout.TextField (s);
		if (GUILayout.Button ("add")) {
			teste.Add (s);
		}

		i = GUILayout.TextField (i);
		int j; 
		j = int.Parse (i);
		if(GUILayout.Button("delete at pos "+i)){						
			teste.RemoveAt (j);
		}

		if (GUILayout.Button ("print all")) {
			for (int k = 0; k < teste.Count; k++) {
				print ("index " + k.ToString ());
				print (teste [k]);
			}
		}
	}
}
