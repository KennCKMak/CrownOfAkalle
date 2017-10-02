﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Unit manager.
/// 
/// Holds the list of units that can be spawned
/// Has the array of all units on the field
/// Controls spawning/adding of units, and destroying 
///
/// </summary>

public class UnitManager : MonoBehaviour {

	public GameObject TemplateUnitPrefab;
	public GameObject SwordsmanUnitPrefab;

	public enum UnitName {
		TemplateUnit,
		SwordsmanUnit
	}

	public enum Faction{
		Ally,
		Enemy,
		Neutral
	}

	public GameObject[] unitObjArray;
	public Unit[] unitArray;
	int ArraySize;
	//public List<GameObject> Units;


	MapManager mapManager;
	ClickManager clickManager;

	protected Shader shaderStandard;
	protected Shader shaderOutline;

	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();	
		clickManager = GetComponent<ClickManager> ();


		ArraySize = 20;
		unitObjArray = new GameObject[ArraySize];
		unitArray = new Unit[ArraySize];
		EmptyArrays ();

		shaderStandard = Shader.Find ("Standard");
		shaderOutline = Shader.Find ("Outlined/Custom");

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

	public void CreateUnit(UnitName unitName, int x, int y, Faction faction){
		int newID;
		bool loopRunning = true;
		while (loopRunning == true) {
			newID = Random.Range (0, ArraySize);
			if (unitObjArray [newID] == null) {
				GameObject newUnit = Instantiate (getAssociatedPrefab (unitName));
				Unit unitScript = newUnit.GetComponent<Unit> ();
				//Tile values
				unitScript.setTileX (x);
				unitScript.setTileY (y);
				mapManager.tileArray [x, y].setIsOccupied (true, newUnit);

				//Prefab values
				unitScript.setVisualPrefab(getAssociatedPrefab(unitName).transform.GetChild(0).gameObject);

				unitScript.faction = faction;
				unitScript.shaderNormal = shaderStandard;
				unitScript.shaderOutline = shaderOutline;
				//setting script values
				unitScript.map = mapManager;
				unitScript.clickManager = clickManager;
				unitScript.unitManager = this;
				//setting ID-Array relation
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

		case UnitName.SwordsmanUnit:
			prefab = SwordsmanUnitPrefab;
			break;
		default:
			prefab = TemplateUnitPrefab;
			break;
		}
		return prefab;

	}

	public void RestoreMovement(Faction faction){
		for (int i = 0; i < ArraySize; i++) {
			if (unitObjArray [i] != null && unitArray [i].faction == faction) {
				unitArray [i].NewTurn ();
			}
		}
	}

}
