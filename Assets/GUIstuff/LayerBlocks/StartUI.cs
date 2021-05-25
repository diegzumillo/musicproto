using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUI : MonoBehaviour {

	public Layer.StartBlock myBlock;
	public Layer myLayer;

	public Connector outStandard;

	//mypanel changes everytime you open it on layer editor
	public GameObject myPanel;

	public void Act(){
		myBlock.Act ();
	}

	public void Initialize(Layer l){
		myLayer = l;
		myBlock = myLayer.CreateStart ();
		GetComponent<BlockUI> ().block = myBlock;
	}

	public void UpdateAll(){
		BlockUI b;

		if (outStandard.connected) {			
			b = outStandard.connectedTo.transform.parent.GetComponent<BlockUI> () as BlockUI;
			myBlock.nextBlock = b.block;
		} else {
			myBlock.nextBlock = null;
		}

	}
}
