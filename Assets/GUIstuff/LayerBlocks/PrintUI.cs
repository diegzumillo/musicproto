using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintUI : MonoBehaviour {

	public Layer.Print myBlock;
	public Layer myLayer;

	public Connector inCon;
	public Connector outStandard;
	public Connector aCon;
	public Connector bCon;



	//mypanel changes everytime you open it on layer editor
	public GameObject myPanel;

	public void Act(){
		myBlock.Act ();
	}

	public void Initialize(Layer l){
		myLayer = l;
		myBlock = myLayer.CreatePrint ();
		GetComponent<BlockUI> ().block = myBlock;
	}

	// Use this for initialization
	void Start () {

	}

	//assim tipo, nao eh a coisa mais elegante do universo..
	public void UpdateAll(){
		BlockUI b;
		VarUI v;

		if (outStandard.connected) {			
			b = outStandard.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
			myBlock.nextBlock = b.block;
		} else {
			myBlock.nextBlock = null;
		}

		if (aCon.connected) {				
			v = aCon.connectedTo.transform.parent.GetComponent<VarUI> () as VarUI;
			myBlock.operand = v.myInd;
		} else if (bCon.connected) {
			v = bCon.connectedTo.transform.parent.GetComponent<VarUI> () as VarUI;
			myBlock.operand = v.myInd;
		} else {
			myBlock.operand =  -1;
		}

	}

	// Update is called once per frame
	void Update () {

	}
}
