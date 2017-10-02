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
	//public Tile chosenTile;

	public MapManager mapManager;
	public UnitManager unitManager;

	private GameObject Hologram;
	private bool hasHologram = false;

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
			if (tile.getIsOccupied ()){
				if (tile.getOccupyingUnit ().GetComponent<Unit> ().getState () == Unit.State.Ready) {
					Debug.Log ("Selecting new unit");
					SelectUnit (tile.getOccupyingUnit ());
					mapManager.showValidMoves (tile, selectedUnit.GetComponent<Unit> ().getSpeed(), "Blue");
					return;
				}
			} else {
				//tile not occupied. can't do anything...
				return;
			}
		}

		//UNIT ALREADY SELECTED
		if (selectedUnit != null) {
			Unit.State unitState = selectedUnit.GetComponent<Unit> ().getState ();
			//MOVING

			//Nothing on the tile?
			if (!tile.getIsOccupied ()) {
				if (unitState == Unit.State.Ready) {
					Debug.Log ("Tile not occupied: Moving");
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
						selectedTile = tile;

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
					return;
				}
			}



			//We have a unit, and we just clicked on another unit
			if (tile.getIsOccupied()) {
				if (tile.getOccupyingUnit () == selectedUnit) {
					//Deselecting if we clicked on ourselves...
					Debug.Log("Moved to my own location");
				} else {
					if (unitState == Unit.State.Ready) {
						Debug.Log ("Trying to attack another thing");
						switch (tile.getOccupyingUnit ().GetComponent<Unit> ().faction) {
						case UnitManager.Faction.Enemy:
							Debug.Log ("Targeted enemy enemy");

							//Calculate weapon range
							int dist = mapManager.GetTileDistance (tile, selectedTile);
							Debug.Log ("Dist = " + dist);
							if (dist == 1 && selectedUnit.GetComponent<Unit>().getMeleeWeaponType() != Unit.MeleeWeaponType.None) {
								Debug.Log ("Initiating Melee attack");
							} else if (dist == 2 && selectedUnit.GetComponent<Unit> ().getRangedWeaponType () != Unit.RangedWeaponType.None) {
								Debug.Log ("Initiating Ranged Attack");
							} else {
								Debug.Log ("Failed to attack!");
							}


							break;


						case UnitManager.Faction.Ally:
							Debug.Log ("Hit Ally");
							break;


						default:
							Debug.Log ("Weird tag hit");
							break;
						}
					}
				} 
				return;

			}

		}



	}
		

	public void DrawHologram(){
		if (!hasHologram) {
			Debug.Log ("Spawning hologram");
			GameObject holo = Instantiate (selectedUnit.GetComponent<Unit> ().getVisualPrefab (), Hologram.transform.position, Quaternion.identity) as GameObject;
			holo.GetComponent<Renderer> ().material.shader = Shader.Find ("Unlit/Transparent"); 
			holo.transform.parent = Hologram.transform;
			hasHologram = true;
		}
	}

	public void MoveHologram(){
		if(selectedTile)
			Hologram.transform.position = mapManager.TileCoordToWorldCoord (selectedTile.getTileX (), selectedTile.getTileY ());
	}

	public void DestroyHologram(){
		if (Hologram.transform.childCount > 0) {
			Destroy (Hologram.transform.GetChild (0).gameObject);
			hasHologram = true;
		}
	}

	public void SelectUnit(GameObject unit){
		unit.GetComponent<Unit> ().setIsSelected (true);
		selectedUnit = unit;
		DrawHologram ();
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
			selectedUnit.GetComponent<Unit> ().setIsSelected (false);
			selectedUnit = null;
		}
		DestroyHologram ();
		mapManager.resetMap ();
	}

	public void DeselectTile(){
		if (selectedTile != null) {
			selectedTile.setIsSelected (false);
			selectedTile = null;
		}
	}

}

/*		if (selectedUnit != null && selectedTile != null) {
			Unit.State unitState = selectedUnit.GetComponent<Unit> ().getState ();
			//MOVING
			if (!tile.getIsOccupied ()) {
				if (unitState == Unit.State.Ready) {
					Debug.Log ("Tile not occupied: Moving");
					//map to new tile
					mapManager.GeneratePathTo (selectedUnit, tile);
					//if you have enough movement...
					if (selectedUnit.GetComponent<Unit> ().getRemaininingMovement () >= selectedUnit.GetComponent<Unit> ().getCurrentPathCount ()) {

						//change occupied space of the selected unit before moving
						mapManager.tileArray [selectedUnit.GetComponent<Unit> ().getTileX (),
							selectedUnit.GetComponent<Unit> ().getTileY ()].setIsOccupied (false, null);
						mapManager.tileArray [selectedUnit.GetComponent<Unit> ().getTileX (),
							selectedUnit.GetComponent<Unit> ().getTileY ()].setIsSelected (false);
						
						//our new tile is now our selected one
						tile.setIsOccupied (true, selectedUnit);
						selectedTile = tile;
						selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
						return;
						//Deselect ();
					} else {
						Debug.Log ("Not enough movement");
						selectedUnit.GetComponent<Unit> ().setCurrentPath (null);
					}
					//if your remaning movement is equal to paths amount, deselct this unit
					return;
				}
			}
				


			//We have a unit, and we just clicked on another unit
			if (tile.getIsOccupied()) {
				if (tile.getOccupyingUnit () == selectedUnit) {
					//Deselecting if we clicked on ourselves...
					selectedUnit.GetComponent<Unit> ().setState (Unit.State.Action);
					Debug.Log("Moved to my own location");
				} else {
					if (unitState == Unit.State.Ready || unitState == Unit.State.Action) {
						Debug.Log ("Trying to attack another thing");
						switch (tile.getOccupyingUnit ().GetComponent<Unit> ().faction) {
						case UnitManager.Faction.Enemy:
							Debug.Log ("Targeted enemy enemy");

							//Calculate weapon range
							int dist = mapManager.GetTileDistance (tile, selectedTile);
							Debug.Log ("Dist = " + dist);
							if (dist == 1 && selectedUnit.GetComponent<Unit>().getMeleeWeaponType() != Unit.MeleeWeaponType.None) {
								Debug.Log ("Initiating Melee attack");
							} else if (dist == 2 && selectedUnit.GetComponent<Unit> ().getRangedWeaponType () != Unit.RangedWeaponType.None) {
								Debug.Log ("Initiating Ranged Attack");
							} else {
								Debug.Log ("Failed to attack!");
							}


							break;


						case UnitManager.Faction.Ally:
							Debug.Log ("Hit Ally");
							break;


						default:
							Debug.Log ("Weird tag hit");
							break;
						}
					}
				} 
				return;

			}
		
		}


		//OTHERWISE, We CHECK IF THERE IS A UNIT
		//AND SET THAT AS OUR UNIT
		if (selectedUnit == null && selectedTile == null) {
			//can we select this unit?
			if (tile.getIsOccupied ()){
				if (tile.getOccupyingUnit ().GetComponent<Unit> ().getState () == Unit.State.Ready) {
					Debug.Log ("Selecting new unit");
					Deselect ();
					SelectTile (tile);
					SelectUnit (tile.getOccupyingUnit ());
					return;
				} else {
					Debug.Log ("Selected a Non-ready unit");
					Deselect ();
				}
			} else {
				return;
			}
		} else {
			Deselect ();
			Debug.Log ("MISMATCH ON UNIT AND TILE");
		}*/