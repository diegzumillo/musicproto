using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*This script needs a Panel
 */

public class Draggable : MonoBehaviour {

	bool dragging = false;

	// Use this for initialization
	void Start () {
		//things that handle dragging, connecting etc
		EventTrigger trigger = GetComponent<EventTrigger>( );
		//begin dragging listener:
		EventTrigger.Entry begDrag = new EventTrigger.Entry( );
		begDrag.eventID = EventTriggerType.BeginDrag;
		begDrag.callback.AddListener( ( data ) => { OnDrag( (PointerEventData)data ); } );
		trigger.triggers.Add( begDrag );
		//end dragging listener:
		EventTrigger.Entry endDrag = new EventTrigger.Entry( );
		endDrag.eventID = EventTriggerType.EndDrag;
		endDrag.callback.AddListener( ( data ) => { EndDrag( (PointerEventData)data ); } );
		trigger.triggers.Add( endDrag );
	}

	public void OnDrag( PointerEventData data )
	{		
		dragging = true;
		displ = Input.mousePosition - transform.position;
	}
	public void EndDrag( PointerEventData data )
	{		
		dragging = false;
		BroadcastMessage ("UpdatePosition");
	}
	
	// Update is called once per frame
	//initial displ from mouse and pos
	Vector3 displ = Vector3.zero;
	void Update () {
		if (dragging) {
			transform.position = Input.mousePosition -  displ;
			transform.parent.BroadcastMessage ("UpdatePosition");
		}
	}
}
