using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Click Manager
/// 
/// Processes the click commmands of the player
/// this includes saving the selected tile, selected unit, and all the other goodies
/// 
/// What should I do next?
/// 
/// Finish up the move->location->choose to attack or whatever.
/// Also make it so you can't walk into an opponent's place with the 'Show map highlight'
/// Setup interaction between two enemies for combat
/// 
/// </summary>




public class ClickManager : MonoBehaviour {

	public GameObject selectedUnit;
	public Tile selectedTile;
	[SerializeField] private List<Tile> validTilesList; //used for highlighting movement
	public Tile chosenTile = null; //where the unit will move

	[HideInInspector] public MapManager mapManager;
	[HideInInspector] public UnitManager unitManager;

	private GameObject Hologram;
	private bool isHologram = false;

	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();
		unitManager = GetComponent<UnitManager> ();
		selectedUnit = null;
		selectedTile = null;
		//chosenTile = null;

		Hologram = transform.FindChild ("Hologram").gameObject;
		Hologram.transform.parent = null;
	}
	
	// Update is called once per frame
	void Update () {
		if (selectedUnit) {
			MoveHologram ();
		} 

		if (Input.GetKeyDown (KeyCode.Escape))
			DeselectUnit ();

		/*if (chosenTile != null) {
			//This highlights the area when you arrive
			int x = chosenTile.getTileX ();
			int y = chosenTile.getTileY ();
		}*/

	}

	public void receiveClick(Tile tile){
		if (selectedTile == null) 
			return;


		//NO UNIT SELECTED
		if (selectedUnit == null) {
			//can we select this unit?
			if (tile.isOccupied ()){
				switch (tile.getOccupyingUnit ().GetComponent<Unit> ().getState ()) {
				case (Unit.State.Ready):
					Debug.Log ("Selecting new unit");
					chosenTile = null;
					SelectUnit (tile.getOccupyingUnit ());

					selectedUnit.GetComponent<Unit> ().setState (Unit.State.ChooseMove);

					//highlight moves of our unit
					mapManager.cleanValidMovesTilesList ();
					mapManager.showValidMoves (selectedUnit, tile, selectedUnit.GetComponent<Unit> ().getSpeed (), "Move");
					validTilesList = mapManager.getValidMovesTilesList (); //retrieve list of tiles
					HighlightTiles ("Blue"); //of valid tiles list

					break;
				default:
					Debug.Log ("Unit State: " + tile.getOccupyingUnit ().GetComponent<Unit> ().getState ());
					break;
				}



			} else {
				//tile not occupied. can't do anything...
				return;
			}
			return;
		}

		if (selectedUnit != null) {
			Unit.State unitState = selectedUnit.GetComponent<Unit> ().getState ();
			switch(unitState){

			//MOVING
			case (Unit.State.ChooseMove):
				if(validTilesList.Contains(tile)){
					chosenTile = tile;

					RemoveHighlight ();
					mapManager.cleanValidMovesTilesList (); //removes old map manager
					mapManager.showValidMoves (selectedUnit, chosenTile, selectedUnit.GetComponent<Unit> ().getWeaponRange (), "Attack");
						//this one sets the list again
					validTilesList = mapManager.getValidMovesTilesList (); //retrieve list of tiles
					//if we are ranged, remove the ones closest to us


					HighlightTiles ("Red"); //of valid tiles list, do...
					selectedUnit.GetComponent<Unit>().setState(Unit.State.ChooseAction);

					//If you are ranged only, remove the closest squares to you...
					if (selectedUnit.GetComponent<Unit> ().isRanged () && !selectedUnit.GetComponent<Unit> ().isMelee ()) {
						mapManager.cleanValidMovesTilesList (); //removes old map manager
						mapManager.showValidMoves (selectedUnit, chosenTile, 1, "Attack");
						validTilesList = mapManager.getValidMovesTilesList ();
						for (int i = 1; i < 5; i++) {
							RemoveHighlight(validTilesList [i]);
						}
					}


					mapManager.cleanValidMovesTilesList ();
					mapManager.showValidMoves (selectedUnit, chosenTile, selectedUnit.GetComponent<Unit> ().getWeaponRange (), "Attack");
					validTilesList = mapManager.getValidMovesTilesList ();

				}
				break;
				//ACTION
			case(Unit.State.ChooseAction):
				if (validTilesList.Contains (tile)) {
					//Click self, end movement without attacking
					if (tile == chosenTile) {
						mapManager.MoveUnitTowards (chosenTile, selectedUnit);
						selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
						DeselectUnit ();


					} else if (tile.isOccupied () && tile.getOccupyingUnit() != selectedUnit) {
						//check if we clicked on ally or friendly...
						switch (tile.getOccupyingUnit ().GetComponent<Unit> ().faction) {
						case UnitManager.Faction.Enemy:
							Debug.Log ("Targeted enemy");
							//Calculate weapon range
							int dist = mapManager.GetTileDistance (tile, chosenTile);
							if (dist == 1 && selectedUnit.GetComponent<Unit> ().isMelee()) {
								selectedUnit.GetComponent<Unit> ().setIsAttacking(true);
								selectedUnit.GetComponent<Unit> ().setTarget (tile.getOccupyingUnit ().GetComponent<Unit>());
								selectedUnit.GetComponent<Unit> ().setDist (dist);


								mapManager.MoveUnitTowards (chosenTile, selectedUnit);
								selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
								DeselectUnit ();


							} else if (dist >= 2 && selectedUnit.GetComponent<Unit> ().isRanged() && selectedUnit.GetComponent<Unit>().getWeaponRange() >= dist) {
								selectedUnit.GetComponent<Unit> ().setIsAttacking(true);
								selectedUnit.GetComponent<Unit> ().setTarget (tile.getOccupyingUnit ().GetComponent<Unit>());
								selectedUnit.GetComponent<Unit> ().setDist (dist);


								mapManager.MoveUnitTowards (chosenTile, selectedUnit);
								selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
								DeselectUnit ();


							} else {
								Debug.Log ("Failed to attack, dist = " + dist);
								selectedUnit.GetComponent<Unit> ().setIsAttacking(false);
								selectedUnit.GetComponent<Unit> ().setTarget (null);
							}

							break;
						case UnitManager.Faction.Ally:
							Debug.Log ("Hit Ally");
							break;
						default:  //end of checking what enemy we hit
							Debug.Log ("Weird tag hit");
							break; 
						}

					} else {
						Debug.Log ("Selected an empty attack space...");
					}
				}
				break;
			default: //end of checking our selected unit's state.
				break;
			}
			return;
		}
	}
		

	public void StartHologram(){
		/*if (!isHologram) {
			/*GameObject holo = Instantiate (selectedUnit.GetComponent<Unit> ().getVisualPrefab (), Hologram.transform.position, Quaternion.identity) as GameObject;
			holo.GetComponent<Renderer> ().material.shader = Shader.Find ("Unlit/Transparent"); 
			holo.transform.parent = Hologram.transform;


			isHologram = true;
		}*/
		isHologram = true;
	}

	public void MoveHologram(){
		if (selectedTile && !chosenTile && validTilesList.Contains(selectedTile))
			//Hologram.transform.position = mapManager.TileCoordToWorldCoord (selectedTile.getTileX (), selectedTile.getTileY ());
			selectedUnit.transform.position = mapManager.TileCoordToWorldCoord (selectedTile.getTileX (), selectedTile.getTileY ());
			
	}

	public void StopHologram(){
		/*if (Hologram.transform.childCount > 0) {
			Destroy (Hologram.transform.GetChild (0).gameObject);
			isHologram = false;
		}*/

		selectedUnit.transform.position = mapManager.TileCoordToWorldCoord (
			selectedUnit.GetComponent<Unit>().getTileX (), selectedUnit.GetComponent<Unit>().getTileY ());
		isHologram = false;
	}

	public void SelectUnit(GameObject unit){
		unit.GetComponent<Unit> ().setIsSelected (true);
		selectedUnit = unit;
		StartHologram ();
	}

	public void SelectTile(Tile tile){
		tile.setIsSelected (true);
		selectedTile = tile;
	}

	public void Deselect(){
		DeselectUnit();
		DeselectTile();
	}

	public void DeselectUnit(){
		if (selectedUnit != null) {
			StopHologram ();


			selectedUnit.GetComponent<Unit> ().setIsSelected (false);
			Unit.State unitState = selectedUnit.GetComponent<Unit> ().getState ();
			if(unitState == Unit.State.ChooseAction || unitState == Unit.State.ChooseMove)
				selectedUnit.GetComponent<Unit>().setState(Unit.State.Ready);
			selectedUnit = null;

			chosenTile = null;

			RemoveHighlight ();
			mapManager.cleanMap ();
			mapManager.cleanValidMovesTilesList ();
		}
	}

	public void DeselectTile(){
		if (selectedTile != null) {
			selectedTile.setIsSelected (false);
			selectedTile = null;
		}
	}



	public void HighlightTiles(string color){

		//If cleaning things up...
		if (color == "None") {
			foreach (Tile tile in validTilesList) {
				tile.setHighlighted (false);
			}
			return;
		}


		//highlight colours anyways
		foreach (Tile tile in validTilesList) {
			tile.setHighlighted (true, color);
		}


		//then edit
		if (color == "Red") { //sets center piece as blue in choose action
			validTilesList [0].setHighlighted (true, "Blue");
			//here I can do a foreach to search for an ally
		}
	}

	public void RemoveHighlight(){
		HighlightTiles ("None");
	}

	public void RemoveHighlight(Tile tile){
		tile.setHighlighted (false);
	}

}

/*FOR ACTUALLY MOVING
//map to new tile
mapManager.GeneratePathTo (selectedUnit, tile);
//if you have enough movement...
if (selectedUnit.GetComponent<Unit> ().hasEnoughMove()) {

	//change occupied space of the selected unit before moving
	mapManager.tileArray [selectedUnit.GetComponent<Unit> ().getTileX (),
		selectedUnit.GetComponent<Unit> ().getTileY ()].setIsOccupied (false, null);
	mapManager.tileArray [selectedUnit.GetComponent<Unit> ().getTileX (),
		selectedUnit.GetComponent<Unit> ().getTileY ()].setIsSelected (false);

	//our new tile is now our selected one
	tile.setIsOccupied (true, selectedUnit);

	//removes the fake hologram
	DestroyHologram ();
	//remove highlighted tiles
	mapManager.resetMap ();
	return;
	//Deselect ();
} else {
	Debug.Log ("Not enough movement");
	selectedUnit.GetComponent<Unit> ().setCurrentPath (null);
}
//if your remaning movement is equal to paths amount, deselct this unit
return;*/