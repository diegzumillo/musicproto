using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class Connector : MonoBehaviour {


	public enum ConnectorType{tri, circ};
	public bool male = false;

	public ConnectorType myType;
	public Connector connectedTo;
	public UILineRenderer line;
	public LayerEditor editor;

	bool connecting = false;
	public bool connected = false;

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
		//pointer enter listener:
		EventTrigger.Entry pointerin = new EventTrigger.Entry( );
		pointerin.eventID = EventTriggerType.PointerEnter ;
		pointerin.callback.AddListener( ( data ) => { HoverIn( (PointerEventData)data ); } );
		trigger.triggers.Add( pointerin );

		//pointer exit listener:
		EventTrigger.Entry pointerOut = new EventTrigger.Entry( );
		pointerOut.eventID = EventTriggerType.PointerExit ;
		pointerOut.callback.AddListener( ( data ) => { HoverOut( (PointerEventData)data ); } );
		trigger.triggers.Add( pointerOut );

		//ref to layereditor
		editor = GameObject.Find("UserInterface").GetComponent<LayerEditor>() as LayerEditor;
	}
	public void HoverOut( PointerEventData data )
	{
		editor.connectorHovering = null;
	}
	public void HoverIn( PointerEventData data )
	{
		editor.connectorHovering = this;
	}

	public void EndDrag( PointerEventData data )
	{
		connecting = false;
		if (editor.connectorHovering == null || editor.connectorHovering == this
			|| (editor.connectorHovering.myType != myType )
			|| (editor.connectorHovering.male == male)
		) {
			line.Points [1] = Vector2.zero;
			//editor.UI.terminal.Add ("Aborting connection");
			line.SetAllDirty ();
			return;
		}

		//stabilishing connection
		if (male) {
			connectedTo = editor.connectorHovering;
			connected = true;
			line.Points [1] = -transform.position + connectedTo.transform.position;
			line.SetAllDirty ();
			//editor.UI.terminal.Add ("Connecting from male end");
		} else {
			editor.connectorHovering.connected = true;
			editor.connectorHovering.connectedTo = this;
			editor.connectorHovering.line.Points [1] = transform.position - editor.connectorHovering.transform.position;;
			editor.connectorHovering.line.SetAllDirty ();
			line.Points [1] = Vector2.zero;
			//editor.UI.terminal.Add ("Connecting from female end");
			line.SetAllDirty ();
		}
		transform.parent.transform.parent.BroadcastMessage ("UpdateAll");
	}
	public void OnDrag( PointerEventData data )
	{
		if (connected)
			Disconnect ();
		connecting = true;
		//Debug.Log( "dragginnnnnnnnng" );
	}

	public void Disconnect(){
		connected = false;
		connectedTo = null;
		transform.parent.transform.parent.BroadcastMessage ("UpdateAll");
	}
	
	// Update is called once per frame
	void Update () {
		if (connecting) {
			Vector3 relPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, -1);
			relPos -= transform.position;
			line.Points[1] = relPos;
			line.SetAllDirty ();
		}
	}
	public void UpdatePosition(){
		if (connected && male) {
			line.Points [1] = connectedTo.transform.position - transform.position;
			line.SetAllDirty ();
		}
	}
}
