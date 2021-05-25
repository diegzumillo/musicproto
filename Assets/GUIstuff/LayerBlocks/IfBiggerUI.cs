using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IfBiggerUI : MonoBehaviour {

	public Layer.IfBigger myBlock;
	public Layer myLayer;

	public Connector inCon;
	public Connector outTrue;
	public Connector outFalse;
	public Connector outStandard;
	public Connector operandA;
	public Connector operandB;

	//mypanel changes every time you open it on layer editor
	public GameObject myPanel;

	public void Act(){
		myBlock.Act ();
	}

	public void Initialize(Layer l){
		myLayer = l;
		myBlock = myLayer.CreateIfBigger ();
		GetComponent<BlockUI> ().block = myBlock;
	}

	// Use this for initialization
	void Start () {
		
	}

	//assim tipo, nao eh a coisa mais elegante do universo..
	public void UpdateAll(){
		BlockUI b;
		VarUI v;

		if (outFalse.connected) {
			b = outFalse.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
			myBlock.falseBlock = b.block;
		} else {
			myBlock.falseBlock = null;
		}
		if ( outTrue.connected) {			
			b = outTrue.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
			myBlock.trueBlock = b.block;
		} else {
			myBlock.trueBlock = null;
		}
		if ( outStandard.connected) {			
			b = outStandard.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
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

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
