using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {


	public ClickManager clickManager;
	public MapManager map;
	public int tileX;
	public int tileY;

	public enum TileType { Empty, Grassland, Forest, Mountain }
	public TileType tileType;

	public GameObject tileVisualPrefab;

	public bool occupied = false;
	public GameObject occupyingUnit;

	public bool walkable = true;
	public float movementCost = 1;


	public bool selected;
	public Shader shaderNormal;
	public Shader shaderOutline;

	void Start(){
		
	}



	void OnMouseUp() {
		//map.ClickEvent (tileX, tileY, this);
		//map.GeneratePathTo (this);
		clickManager.receiveClick(this);
	}

	void Update(){


	}

	void OnMouseOver(){
		if (clickManager.canClick) {
			clickManager.SelectTile (this);
			setOutline (true);
			if (isOccupied ()) {
				if (!getOccupyingUnit ().GetComponent<Unit> ().isSelected ()) {
					getOccupyingUnit ().GetComponent<Unit> ().setOutline (true);
				}
			}
		}
	}

	void OnMouseExit(){
		setOutline (false);
		if (isOccupied ()) {
			if (!getOccupyingUnit ().GetComponent<Unit> ().isSelected ()) {
				getOccupyingUnit ().GetComponent<Unit> ().setOutline (false);
			}
		}
	}


	public void Setup(){
		switch (tileType) {
		case TileType.Grassland:
			walkable = true;
			movementCost = 1;
			break;
		case TileType.Forest:
			walkable = true;
			movementCost = 2;
			break;
		case TileType.Mountain:
			walkable = false;
			movementCost = Mathf.Infinity;
			break;
		default:
			Debug.Log ("Failed to load TileType");
			break;

		}
	}

	public int getTileX(){
		return tileX;
	}

	public void setTileX(int num){
		tileX = num;
	}

	public int getTileY(){
		return tileY;
	}

	public void setTileY (int num){
		tileY = num;
	}

	public TileType getTileType(){
		return tileType;
	}

	public void setTileType(TileType newType){
		tileType = newType;
	}

	public void setTileVisualPrefab(GameObject newPrefab){
		tileVisualPrefab = newPrefab;
	}

	public GameObject getTileVisualPrefab(){
		return tileVisualPrefab;
	}

	public bool isWalkable(){
		return walkable;
	}

	public float getMovementCost(){
		return movementCost;
	}

	public void setMovementCost(float num){
		movementCost = num;
	}

	public void setIsWalkable(bool b){
		walkable = b;
	}

	public void ShowColor(string yes){

	}

	public void setIsOccupied(bool b, GameObject unit){
		occupied = b;
		occupyingUnit = unit;
		//setIsSelected (b);
	}

	public bool isOccupied(){
		return occupied;
	}

	public GameObject getOccupyingUnit(){
		return occupyingUnit;
	}


	public void setOutline(bool b){
		if (b)
			transform.GetChild(0).gameObject.GetComponent<Renderer>().material.shader = shaderOutline;
		else
			transform.GetChild(0).gameObject.GetComponent<Renderer>().material.shader = shaderNormal;

	}


	public void setIsSelected(bool b){
		selected = b;
		setOutline (b);

	}

	public bool isSelected(){
		return selected;
	}

	public void setHighlighted(bool b, string color){
		if (b) {
			if(color == "Blue"){
				transform.FindChild ("HighlightBlue").gameObject.SetActive (true);
				transform.FindChild("HighlightRed").gameObject.SetActive(false);
			} if (color == "Red") {
				transform.FindChild ("HighlightBlue").gameObject.SetActive (false);
				transform.FindChild("HighlightRed").gameObject.SetActive(true);
			}		
		}
	}

	public void setHighlighted(bool b){
		if (!b) {
			transform.FindChild ("HighlightBlue").gameObject.SetActive (false);
			transform.FindChild("HighlightRed").gameObject.SetActive(false);
		}
	}
}

