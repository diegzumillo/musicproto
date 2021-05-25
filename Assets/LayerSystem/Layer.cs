using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer : MonoBehaviour {
	
	public class Variable{
		public enum Type {integer, floating, boolean, stringText, list};
		public Type myType;
		public string name;
		public Layer myLayer;

		public int initInt;
		public float initFloat;
		public bool initBool;
		public string initString;

		public int myInd;
		public int myInt = 0;
		public float myFloat = 0;
		public bool myBool = false;
		public string myString;
		public List<Variable> myList;

		public Variable(Layer l){
			myLayer = l;
		}

		public Variable (int a)
		{
			myType = Type.integer;
			myInt = a;
		}
		public Variable (float a)
		{
			myType = Type.floating;
			myFloat = a;
		}
		public Variable (bool a)
		{
			myType = Type.boolean;
			myBool = a;
		}
		public Variable (List<Variable> l)
		{
			//how to do this?
			myList = l;

		}
		public Variable(Type t){
			//without initializing any variable
			//this is likely the way I'm 
			//going to use in the ui
			myType = t;
		}
		public void AddToList (Variable v){
			if (myType != Type.list) {
				Debug.Log ("This is not a list");
				return;
			}
			if (myList == null)
				myList = new List<Variable> ();
			myList.Add (v);
		}
		public string Sum(int ind){
			//this takes an index of another var, adds to the value of this var, and returns a string
			int i;
			float f;
			if (myType == Type.integer) {
				if (myLayer.vars [ind].myType == Type.integer) {
					i = myInt + myLayer.vars [ind].myInt;
					return i.ToString ();
				} else if (myLayer.vars [ind].myType == Type.floating) {
					i = myInt + Mathf.RoundToInt (myLayer.vars [ind].myFloat);
					return i.ToString ();
				} else {
					Debug.Log ("tried to add int with non number");
					return "Failed";
				}
			} else if (myType == Type.floating) {
				if (myLayer.vars [ind].myType == Type.floating) {
					f = myFloat + myLayer.vars [ind].myFloat;
					return f.ToString ();
				} else if (myLayer.vars [ind].myType == Type.integer) {
					f = myFloat + (float)myLayer.vars [ind].myInt;
					return f.ToString ();
				} else {
					Debug.Log ("tried to add float with non number");
					return "Failed";
				}
			}
			return "Failed";
		}


		public bool IsLargerThan(Variable x){
			if (myType == x.myType) {
				if (myType == Type.integer) {
					if (myInt > x.myInt)
						return true;
					else
						return false;
				}
				if (myType == Type.floating) {
					if (myFloat > x.myFloat)
						return true;
					else
						return false;
				}
				Debug.Log ("Not an integer or float");
				return false;

			} else {
				Debug.Log ("not the same type");
				return false;
			}
		}

		public void Assign(string value){
			//still need to work out how lists work
			int i;
			float f;
			bool b;

			if (int.TryParse (value, out i)) {
				print ("it's an int");
				myType = Type.integer;
				myInt = i;
			} else if (float.TryParse (value, out f)) {
				print ("it's a float");
				myType = Type.floating;
				myFloat = f;
			}
			else if (bool.TryParse (value, out b)) {
				print ("it's a bool");
				myType = Type.boolean;
				myBool = b;
			} else {
				print ("it's a string");
				myType = Type.stringText;
			}

			print ("Assigning a string");
			myString = value;

		}
	}


	public abstract class Block {
		public Layer layer;
		public Block nextBlock;
		abstract public void Act ();
		abstract public void PrintRelevantData ();
		public Block(){
			//would be nice if I could set layer here instead of in every derived class

		}
		public void Next(){
			if(nextBlock!= null) nextBlock.Act ();
		}
	}

	/*public class VarDeclare : Block {
		//i dont think this needs to be a block
		//instead i'll exposed the list of vars
		//in the interface


		public override void PrintRelevantData(){
		}
		public override void Act(){
			Next ();
		}
	}*/

	public class Sum : Block {
		public int a;
		public int b; 
		public int c; 

		public Sum(Layer l, int x, int y, int z){
			layer = l;
			a = x;
			b = y;
			c = z;
		}
		public override void Act(){
			//var c will be overwritten with new value. whatever it was.
			string output = layer.vars[a].Sum(layer.vars[b].myInd);
			layer.vars [c].Assign (output);
			print ("am i ever here?");
			Next ();
		}
		public override void PrintRelevantData(){
		}
	}

	/*public class Incrementer : Block {
		public int a;
		int increment = 0;
		public Incrementer(Layer l, int x, int y){
			layer = l;
			increment = y;
			a = x;
		}
		public override void Act(){
			layer.vars[a].Add (increment);
			Next ();
		}
		public override void PrintRelevantData(){
		}
	}*/

	public class AddToList : Block{

		public int v;
		public int l;

		public AddToList(Layer l){
			layer = l;
		}

		public override void Act(){
			layer.vars [l].AddToList (layer.vars [v]);
		}
		public override void PrintRelevantData(){
		}
	}

	public class Print : Block{
		string s;
		public int operand;

		public Print(Layer l, string st){
			layer = l;
			s = st; //standard text to print if no operand is found
			operand = -1;
		}
		public override void Act(){
			string toPrint;
			if (operand == -1) {
				toPrint = s;
			} else {
				toPrint = layer.vars [operand].myString;
			}
			layer.UI.terminal.Add (toPrint);
			Next ();
		}
		public override void PrintRelevantData(){
		}
	}
	public class StartBlock : Block{
		
		public StartBlock(Layer l){
			layer = l;
		}
		public override void Act(){			
			Next ();
		}
		public override void PrintRelevantData(){
		}
	}

	public class IfBigger : Block{
		public int a;
		public int b;
		public Block trueBlock;
		public Block falseBlock;
		public IfBigger(Layer l, int x, int y, Block t, Block f){
			a = x;
			b = y;
			trueBlock = t;
			falseBlock = f;
			layer = l;
		}

		public override void PrintRelevantData(){
			print ("my a " + a.ToString () + " (ind)");
			print ("my b " + b.ToString () + " (ind)");
		}

		public override void Act(){
			if ( layer.vars[a].IsLargerThan(layer.vars[b]) ) {
				if (trueBlock == null) {
					print ("no true block");
				} else
				trueBlock.Act ();
			} else {
				if (falseBlock == null) {
					print ("no false block");
				} else
				falseBlock.Act ();
			}
			Next ();
		}
	}


	public int myInd = 0;
	public int myAction = 0;
	public string myName = "layer";
	public UserInterface UI;

	public List<Variable> vars;
	public List<Block> blocks;

	public Sum CreateSum(){
		Sum news = new Sum (this, -1, -1, -1);
		blocks.Add (news);
		return news;
	}

	public IfBigger CreateIfBigger(){
		IfBigger newifb = new IfBigger (this, 0, 1, null, null);
		blocks.Add (newifb);
		return newifb;
	}
	public Print CreatePrint(){
		Print newp = new Print (this, "Print block has no string");
		blocks.Add (newp);
		return newp;
	}
	public StartBlock CreateStart(){
		StartBlock newp = new StartBlock (this);
		blocks.Add (newp);
		return newp;
	}
	public int CreateVariable(int a){
		Variable newv = new Variable (this);
		vars.Add (newv);
		newv.myInd = vars.Count-1;
		return newv.myInd;
	}

	// Use this for initialization
	void Start () {
		UI = GameObject.Find ("UserInterface").GetComponent<UserInterface>() as UserInterface;

		blocks = new List<Block> ();
		vars = new List<Variable> ();

		/*vars.Add (new Variable (5.0f));
		vars.Add (new Variable (10.0f));
		vars.Add (new Variable (Variable.Type.list));
		//print (vars [2].myList.Count.ToString ());

		IfBigger block1 = new IfBigger (this, 0, 1, null, null);
		AddToList block2 = new AddToList (this);
		block2.v = 1;
		block2.l = 2;
		block1.trueBlock = new Print (this, "true. it only adds to list when false");
		block1.falseBlock = block2;

		block1.Act ();

		print (vars [2].myList.Count.ToString ());
		*/
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonUp ("Jump")){
			print ("vars: ");
			print ("num of vars " + vars.Count.ToString ());		
			foreach (Variable v in vars) {		
				print (v.myInt.ToString ()+" ("+v.myInd.ToString ()+")");

			}
			
			print ("ifs: ");
			foreach (Block b in blocks) {		
				b.PrintRelevantData ();
			}
		}
		
	}

	public List<Note> Action(List<Note> inNotes){

		if (myInd == 0) {
			return inNotes;
		}




		//first we create a copy of the notes
		//List<Note> outNotes = new List<Note> ();
		List<Note> outNotes = new List<Note>();
		/*
		foreach (Note m in inNotes) {
			outNotes.Add (m.Clone (myInd));
		}

		foreach (Note n in outNotes) {
			n.MoveNote (myAction);
		}

		UI.terminal.Add ("Layer action finished. Outputting result");
		*/
		return outNotes;
	}

}
