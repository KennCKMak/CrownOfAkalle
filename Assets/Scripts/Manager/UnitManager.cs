using System.Collections;
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
	public enum Faction{
		Ally,
		Enemy,
		Neutral
	}

	public enum UnitName {
		TemplateUnit,
		SwordsmanUnit,
		ArcherUnit
	}
	int numberOfUnits = 3;
	public GameObject[] UnitPrefabs;
	public GameObject[] UnitSimPrefabs;





	public GameObject[] unitObjArray;
	public Unit[] unitArray;
	private List<GameObject> deadUnits;
	int ArraySize;
	//public List<GameObject> Units;


	MapManager mapManager;
	CombatManager combatManager;

	protected Shader shaderStandard;
	protected Shader shaderOutlineBlack;
	protected Shader shaderOutlineGreen;
	protected Shader shaderOutlineRed;

	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();	
		combatManager = GetComponent<CombatManager> ();


		ArraySize = 20;

		unitObjArray = new GameObject[ArraySize];
		unitArray = new Unit[ArraySize];
		deadUnits = new List<GameObject> ();
		EmptyArrays ();

		shaderStandard = Shader.Find ("Standard");
		shaderOutlineBlack = Shader.Find ("Outline/Black");
		shaderOutlineGreen = Shader.Find ("Outline/Green");
		shaderOutlineRed = Shader.Find ("Outline/Red");

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

				unitScript.faction = faction;
				unitScript.shaderNormal = shaderStandard;
				if(faction == Faction.Ally)
					unitScript.shaderOutline = shaderOutlineGreen;
				else if(faction == Faction.Enemy)
					unitScript.shaderOutline = shaderOutlineRed;
				else 
					unitScript.shaderOutline = shaderOutlineBlack;
				//setting script values
				unitScript.map = mapManager;
				unitScript.combatManager = combatManager;
				unitScript.unitManager = this;
				//setting ID-Array relation
				unitObjArray [newID] = newUnit;
				unitArray [newID] = unitScript;
				unitScript.setUnitID (newID);

				unitScript.setSimPrefab (getAssociatedSimPrefab (unitName));

				unitScript.SetupUnit ();

				loopRunning = false;
			}
		}
	}

	public void ScanForDeadUnits(){
		if (deadUnits.Count > 0) {
			foreach (GameObject unit in deadUnits) {
				DeleteUnit (unit);
			}
		}
		deadUnits.Clear ();
	}


	public void requestDelete(GameObject deleted){
		deadUnits.Add (deleted);
	}

	public void DeleteUnit(GameObject deletedUnit){
		int id = deletedUnit.GetComponent<Unit>().getUnitID();
		mapManager.tileArray [unitArray [id].getTileX (), unitArray [id].getTileY ()].setIsOccupied (false, null);
		unitObjArray [id] = null;
		unitArray [id] = null;
		Destroy (deletedUnit.gameObject, 2f);
	}

	protected GameObject getAssociatedPrefab(UnitName unitName){
		return UnitPrefabs[(int)unitName];

	}

	protected GameObject getAssociatedSimPrefab(UnitName unitName){
		return UnitSimPrefabs[(int)unitName];

	}

	public void RestoreMovement(Faction faction){
		for (int i = 0; i < ArraySize; i++) {
			if (unitObjArray [i] != null && unitArray [i].faction == faction) {
				unitArray [i].NewTurn ();
			}
		}
	}


}
