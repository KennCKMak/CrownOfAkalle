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
		Player, //blue
		Ally, //purple
		Enemy, //red
		Neutral //black
	}

	public enum UnitName {
		TemplateUnit,
		SwordsmanUnit,
		ArcherUnit,
		KnightUnit,
		SpearmanUnit
	}

	public Material[] FactionColours;
    public Material[] FactionColoursDark;
    public Material[] HorseMaterial = new Material[2];

	public GameObject[] UnitPrefabs;
	public GameObject[] UnitSimPrefabs;


	int ArraySize;
	protected GameObject[] unitObjArray;
	public Unit[] unitArray;
	public List<GameObject> deadUnits;
	//public List<GameObject> Units;

	[HideInInspector] public GameManager game;
	MapManager mapManager;
	CombatManager combatManager;

	[SerializeField] protected GameObject HealthBarPrefab;

	protected Shader shaderStandard;



	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();
		mapManager = GetComponent<MapManager> ();	
		combatManager = GetComponent<CombatManager> ();

		ArraySize = 30;

		unitObjArray = new GameObject[ArraySize];
		unitArray = new Unit[ArraySize];
		deadUnits = new List<GameObject> ();
		EmptyArrays ();

		shaderStandard = Shader.Find ("Standard");


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


	public IEnumerator CreateUnitS(UnitName unitName, int x, int y, Faction faction){
		if (mapManager.tileArray [x, y].isOccupied ()) {
		} else {
			yield return new WaitForSeconds (0.5f);

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
					unitScript.factionColour = FactionColours [(int)faction];
					unitScript.factionColourUsed = FactionColoursDark [(int)faction];

					if (unitScript.isMounted ()) {
						unitScript.horseMaterial = HorseMaterial [0];
						unitScript.horseMaterialUsed = HorseMaterial [1];
					}

					unitScript.shaderNormal = shaderStandard;
					if (faction == Faction.Player) {
						unitScript.SetOutlineColor (OutlineEffect.OutlineColor.Green);
						unitScript.RotateUnitFace ("North");
					} else if (faction == Faction.Enemy) { 
						unitScript.SetOutlineColor (OutlineEffect.OutlineColor.Red);
						unitScript.RotateUnitFace ("South");
					} else
						unitScript.SetOutlineColor (OutlineEffect.OutlineColor.Black);
					//setting script values
					unitScript.game = game;
					unitScript.map = mapManager;
					unitScript.combatManager = combatManager;
					unitScript.unitManager = this;

					unitScript.SetHealthBarPrefab (HealthBarPrefab);
					//setting ID-Array relation
					unitObjArray [newID] = newUnit;
					unitArray [newID] = unitScript;
					unitScript.setUnitID (newID);

					unitScript.setSimPrefab (getAssociatedSimPrefab (unitName));

					unitScript.SetUpUnit ();

					loopRunning = false;
				}
			}
		}
	}

	public void CreateUnit(UnitName unitName, int x, int y, Faction faction){
		if(mapManager.tileArray[x,y].isOccupied())
			return;
		

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
				unitScript.factionColour = FactionColours[(int)faction];
                unitScript.factionColourUsed = FactionColoursDark[(int)faction];

                if (unitScript.isMounted())
                {
                    unitScript.horseMaterial = HorseMaterial[0];
                    unitScript.horseMaterialUsed = HorseMaterial[1];
                }

				unitScript.shaderNormal = shaderStandard;
                if (faction == Faction.Player) {
                    unitScript.SetOutlineColor(OutlineEffect.OutlineColor.Green);
                    unitScript.RotateUnitFace("North");
                } else if (faction == Faction.Enemy) { 
                    unitScript.SetOutlineColor(OutlineEffect.OutlineColor.Red);
                    unitScript.RotateUnitFace("South");
                } else
                    unitScript.SetOutlineColor(OutlineEffect.OutlineColor.Black);
                //setting script values
                unitScript.game = game;
				unitScript.map = mapManager;
				unitScript.combatManager = combatManager;
				unitScript.unitManager = this;

				unitScript.SetHealthBarPrefab (HealthBarPrefab);
				//setting ID-Array relation
				unitObjArray [newID] = newUnit;
				unitArray [newID] = unitScript;
				unitScript.setUnitID (newID);

				unitScript.setSimPrefab (getAssociatedSimPrefab (unitName));

				unitScript.SetUpUnit ();

				loopRunning = false;
			}
		}
	}




	public void requestDelete(GameObject deleted){
		deadUnits.Add (deleted);
	}

	public void ScanForDeadUnits(){
		if (deadUnits.Count > 0) {
			foreach (GameObject unit in deadUnits) {
				DeleteUnit (unit);
			}
		}
		deadUnits.Clear ();
	}

	public void DeleteUnit(GameObject deletedUnit){
		int id = deletedUnit.GetComponent<Unit>().getUnitID();
		mapManager.tileArray [unitArray [id].getTileX (), unitArray [id].getTileY ()].setIsOccupied (false, null);
		unitArray [id].setState (Unit.State.Dead);
		unitObjArray [id] = null;
		unitArray [id] = null;
		Destroy (deletedUnit.gameObject, 4f);
	}

	protected GameObject getAssociatedPrefab(UnitName unitName){
		return UnitPrefabs[(int)unitName];

	}

	protected GameObject getAssociatedSimPrefab(UnitName unitName){
		return UnitSimPrefabs[(int)unitName];

	}

    public void RestoreAllMovement()
    {
        RestoreMovement(Faction.Ally);
        RestoreMovement(Faction.Neutral);
        RestoreMovement(Faction.Player);
        RestoreMovement(Faction.Enemy);
    }

	public void RestoreMovement(Faction faction){
		for (int i = 0; i < ArraySize; i++) {
			if (unitObjArray [i] != null && unitArray [i].faction == faction) {
				unitArray [i].NewTurn ();
			}
		}
	}

	public void DeleteAllUnits(){
		game.click.Deselect ();
		for (int i = 0; i < ArraySize; i++) {
			if (unitObjArray [i] != null) {
				deadUnits.Add(unitObjArray[i]);
			}
		}
		ScanForDeadUnits ();
	}

	public void DeleteAllFactionUnits(Faction faction){

		for (int i = 0; i < ArraySize; i++) {
			if (unitObjArray [i] != null && unitArray [i].faction == faction) {
				deadUnits.Add (unitObjArray [i]);
			}
		}
		ScanForDeadUnits ();
	}

    public void checkEndTurn()
    {
        
        bool endTurn = true;
        Faction currFaction = Faction.Player;// game.turn.getCurrentTurn();
        for (int i = 0; i < ArraySize; i++)
        {
            if (unitArray[i] != null)
            {
                if (unitArray[i].faction == currFaction && unitArray[i].getState() != Unit.State.Done)
                {
                    endTurn = false;
                    return;
                }
            }
        }
        if (endTurn)
        {
            game.turn.switchTurn();
        }
    }

    public GameObject[] returnUnitObjArray()
    {
        return unitObjArray;
    }

    public Unit[] returnUnitArray()
    {
        return unitArray;
    }
}
