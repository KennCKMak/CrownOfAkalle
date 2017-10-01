using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour {

	public GameObject TemplateUnitPrefab;

	public enum UnitName {
		TemplateUnit,
		SwordsmanUnit
	}

	public GameObject[] unitObjArray;
	public Unit[] unitArray;
	int ArraySize;
	//public List<GameObject> Units;
	// Use this for initialization
	void Start () {
		ArraySize = 50;
		unitObjArray = new GameObject[ArraySize];
		unitArray = new Unit[ArraySize];
		EmptyArrays ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	protected void EmptyArrays(){
		for (int i = 0; i < ArraySize; i++) {
			unitObjArray [i] = null;
			unitArray [i] = null;
		}
	}

	public void CreateUnit(UnitName unitName, int x, int y){
		Debug.Log ("starting");
		int newID;
		bool loopRunning = true;
		while (loopRunning == true) {
			newID = Random.Range (0, ArraySize);
			if (unitObjArray [newID] == null) {
				Debug.Log ("Spawned with ID: " + newID);
				GameObject newUnit = Instantiate (getAssociatedPrefab (unitName));
				Unit unitScript = newUnit.GetComponent<Unit> ();
				unitScript.setTileX (x);
				unitScript.setTileY (y);

				unitObjArray [newID] = newUnit;
				unitArray [newID] = unitScript;
				unitScript.setUnitID (newID);
				loopRunning = false;
			}
		}
	}

	public void DeleteUnit(GameObject unitObj){
		int ID = unitObj.GetComponent<Unit> ().getUnitID ();
		if (unitObjArray [ID] == unitObj) {
			unitObjArray [ID] = null;
			unitArray [ID] = null;
			Destroy (unitObj);
		} else {
			Debug.Log ("Attempted deletion of an incorrect id.");
		}
	}

	protected GameObject getAssociatedPrefab(UnitName unitName){
		GameObject prefab;

		switch (unitName) {

		case UnitName.TemplateUnit:
		default:
			prefab = TemplateUnitPrefab;
			break;
		}
		return prefab;

	}



}
