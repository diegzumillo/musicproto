using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SumUI : MonoBehaviour {

	public Layer.Sum myBlock;
	public Layer myLayer;

	public Connector inConnector;
	public Connector outConnector;
	public Connector operandA;
	public Connector operandB;
	public Connector result;

	//mypanel changes every time you open it on layer editor
	public GameObject myPanel;

	public void Act(){
		myBlock.Act ();
	}

	public void Initialize(Layer l){
		myLayer = l;
		myBlock = myLayer.CreateSum ();
		GetComponent<BlockUI> ().block = myBlock;
	}

	// Use this for initialization
	void Start () {

	}

	//assim tipo, nao eh a coisa mais elegante do universo..
	public void UpdateAll(){
		BlockUI b;
		VarUI v;

		if ( outConnector.connected) {			
			b = outConnector.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
			myBlock.nextBlock = b.block;
		} else {
			myBlock.nextBlock = null;
		}

		if (operandA.connected) {
			v = operandA.connectedTo.transform.parent.GetComponent<VarUI> () as VarUI;
			myBlock.a = v.myInd ;
		} else {
			myBlock.a = -1;
		}
		if (operandB.connected) {			
			v = operandB.connectedTo.transform.parent.GetComponent<VarUI> () as VarUI;
			myBlock.b = v.myInd;
		} else {
			myBlock.b = -1;
		}
		if (result.connected) {			
			v = result.connectedTo.transform.parent.GetComponent<VarUI> () as VarUI;
			myBlock.c = v.myInd;
		} else {
			myBlock.c = -1;
		}

	}

	// Update is called once per frame
	void Update () {

	}
}
