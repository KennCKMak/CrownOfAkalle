using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Click Manager
/// 
/// Processes the click commmands of the player
/// this includes saving the selected tile, selected unit, and all the other goodies
/// 
/// 
/// 
/// </summary>




public class ClickManager : MonoBehaviour {

	public bool canClick;

	public GameObject selectedUnit;
	public Tile selectedTile;
	[SerializeField] private List<Tile> validTilesList; //used for highlighting movement
	public Tile chosenTile = null; //where the unit will move

	[HideInInspector] public MapManager mapManager;
	[HideInInspector] public UnitManager unitManager;

	private bool isHologram = false;

	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();
		unitManager = GetComponent<UnitManager> ();
		selectedUnit = null;
		selectedTile = null;
		//chosenTile = null;

		canClick = true;

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
		if (!canClick) {
			return;
		}
		if (selectedTile == null) {
			return;
		}


		//NO UNIT SELECTED
		if (selectedUnit == null) {
			//can we select this unit?
			if (tile.isOccupied ()){
				switch (tile.getOccupyingUnit ().GetComponent<Unit> ().getState ()) {
				case (Unit.State.Ready):
					//Debug.Log ("Selecting new unit");
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
						for (int i = 1; i < 5-1; i++) {
							RemoveHighlight(validTilesList [i]);
							Debug.Log ("removed red highlight");
						}
						mapManager.cleanValidMovesTilesList ();
						mapManager.showValidMoves (selectedUnit, chosenTile, selectedUnit.GetComponent<Unit> ().getWeaponRange (), "Attack");
						validTilesList = mapManager.getValidMovesTilesList ();
					}

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
								return;
							}
							canClick = false;

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
		isHologram = true;
	}

	public void MoveHologram(){
		if (selectedTile && !chosenTile && validTilesList.Contains(selectedTile))
			selectedUnit.transform.position = mapManager.TileCoordToWorldCoord (selectedTile.getTileX (), selectedTile.getTileY ());
			
	}

	public void StopHologram(){
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
