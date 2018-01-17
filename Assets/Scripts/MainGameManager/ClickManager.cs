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
	public GameObject hoveredUnit;
	public Tile selectedTile;
	[SerializeField] private List<Tile> validTilesList; //used for highlighting movement
	public Tile chosenTile = null; //where the unit will move


	GameManager game;
	[HideInInspector] public MapManager mapManager;
	[HideInInspector] public UnitManager unitManager;
	[HideInInspector] public TurnManager turnManager;
    
    private bool isHologram;

	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();
		mapManager = GetComponent<MapManager> ();
		unitManager = GetComponent<UnitManager> ();
		turnManager = GetComponent<TurnManager> ();
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


		/*if (chosenTile != null) {
			//This highlights the area when you arrive
			int x = chosenTile.getTileX ();
			int y = chosenTile.getTileY ();
		}*/

	}

	public void receiveClick(Tile tile){
		if (!canClick)
			return;
		if (selectedTile == null)
			return;
		
		//NO UNIT SELECTED
		if (selectedUnit == null) {
			//can we select this unit?
			if (tile.isOccupied ()) { 
				if (tile.getOccupyingUnit ().GetComponent<Unit> ().faction == turnManager.getCurrentTurn ()) {
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


						game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseMove);

						break;
					default:
						//Debug.Log ("Unit State: " + tile.getOccupyingUnit ().GetComponent<Unit> ().getState ());
						break;
					}
				}
				return;
			}
			return;
		}

        //UNIT IS SELECTED
		if (selectedUnit != null) {
			Unit.State unitState = selectedUnit.GetComponent<Unit> ().getState ();
			switch(unitState){

			//MOVING
			case (Unit.State.ChooseMove):
				if(validTilesList.Contains(tile)){
					chosenTile = tile;

					RemoveHighlight ();
					mapManager.cleanValidMovesTilesList (); //removes old map list
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
							try{
								RemoveHighlight(validTilesList [i]);
							}catch{
							}
						}
						mapManager.cleanValidMovesTilesList ();
						mapManager.showValidMoves (selectedUnit, chosenTile, selectedUnit.GetComponent<Unit> ().getWeaponRange (), "Attack");
						validTilesList = mapManager.getValidMovesTilesList ();
					}


					game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseAction);


				}
				break;
				//ACTION
			case(Unit.State.ChooseAction):
				if (validTilesList.Contains (tile)) {
					//Click self, end movement without attacking
					if (tile == chosenTile) {
						mapManager.MoveUnitTowards (chosenTile, selectedUnit);
						selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
						game.camManager.CameraStrategy.GetComponent<CameraControl> ().FocusAt (selectedUnit);
						DeselectUnit ();

						canClick = false;

					} else if (tile.isOccupied () && tile.getOccupyingUnit() != selectedUnit) {
						//check if we clicked on ally or friendly...
						if (tile.getOccupyingUnit ().GetComponent<Unit> ().faction != selectedUnit.GetComponent<Unit>().faction) {

							//Calculate weapon range
							int dist = mapManager.GetTileDistance (tile, chosenTile);
							if (dist == 1 && selectedUnit.GetComponent<Unit> ().isMelee()) {
								selectedUnit.GetComponent<Unit> ().setIsAttacking(true);
								selectedUnit.GetComponent<Unit> ().setTarget (tile.getOccupyingUnit ().GetComponent<Unit>());
								selectedUnit.GetComponent<Unit> ().setDist (dist);


								mapManager.MoveUnitTowards (chosenTile, selectedUnit);
								selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
								game.camManager.CameraStrategy.GetComponent<CameraControl> ().FocusAt (selectedUnit);
								DeselectUnit ();


								canClick = false;
							} else if (dist >= 2 && selectedUnit.GetComponent<Unit> ().isRanged() && selectedUnit.GetComponent<Unit>().getWeaponRange() >= dist) {
								selectedUnit.GetComponent<Unit> ().setIsAttacking(true);
								selectedUnit.GetComponent<Unit> ().setTarget (tile.getOccupyingUnit ().GetComponent<Unit>());
								selectedUnit.GetComponent<Unit> ().setDist (dist);


								mapManager.MoveUnitTowards (chosenTile, selectedUnit);
								selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
								game.camManager.CameraStrategy.GetComponent<CameraControl> ().FocusAt (selectedUnit);
								DeselectUnit ();


								canClick = false;

							} else {
								Debug.Log ("Failed to attack, dist = " + dist);
								selectedUnit.GetComponent<Unit> ().setIsAttacking(false);
								selectedUnit.GetComponent<Unit> ().setTarget (null);
								return;
							}
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
        //if I have selected a unit, but do not have a chosen tile within bounds
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
		hoveredUnit = null;
		StartHologram ();
		game.ui.UpdateUnitStatsText();
		game.ui.UpdateHoveredUnitStatsText ();
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


            //If we cancel while selecting an action
            if (selectedUnit.GetComponent<Unit>().getState() == Unit.State.ChooseAction)
            {
                selectedUnit.GetComponent<Unit>().setState(Unit.State.ChooseMove);
                Tile currentUnitTile = mapManager.tileArray[selectedUnit.GetComponent<Unit>().getTileX(), selectedUnit.GetComponent<Unit>().getTileY()];

                mapManager.cleanMap();
                mapManager.cleanValidMovesTilesList();
                mapManager.showValidMoves(selectedUnit, currentUnitTile, selectedUnit.GetComponent<Unit>().getSpeed(), "Move");
                validTilesList = mapManager.getValidMovesTilesList(); //retrieve list of tiles
                HighlightTiles("Blue"); //of valid tiles list

                chosenTile = null;
				if (game.turn.currentTurn == UnitManager.Faction.Player)
					game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseMove);
				else
					game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
				
                return;
            }
            else //otherwise just stop everything
            {

                StopHologram();


                selectedUnit.GetComponent<Unit>().setIsSelected(false);
                Unit.State unitState = selectedUnit.GetComponent<Unit>().getState();
                if (unitState == Unit.State.ChooseAction || unitState == Unit.State.ChooseMove)
                    selectedUnit.GetComponent<Unit>().setState(Unit.State.Ready);
                selectedUnit = null;

                chosenTile = null;

                RemoveHighlight();
                mapManager.cleanMap();
                mapManager.cleanValidMovesTilesList();

				game.ui.updateText();

				if (game.turn.currentTurn == UnitManager.Faction.Player)
					game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseUnit);
				else 
					game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
				

                return;
            }
		}
	}

	public void setHoveredUnit(GameObject unit){
		hoveredUnit = unit;
		game.ui.UpdateHoveredUnitStatsText ();
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
