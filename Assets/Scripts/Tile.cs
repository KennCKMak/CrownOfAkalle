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

	public bool isOccupied = false;
	public GameObject occupyingUnit;

	public bool isWalkable = true;
	public float movementCost = 1;


	public bool isSelected;
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
		clickManager.SelectTile (this);
		setOutline (true);
	}

	void OnMouseExit(){
		setOutline (false);
	}


	public void Setup(){
		switch (tileType) {
		case TileType.Grassland:
			isWalkable = true;
			movementCost = 1;
			break;
		case TileType.Forest:
			isWalkable = true;
			movementCost = 2;
			break;
		case TileType.Mountain:
			isWalkable = false;
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

	public bool getIsWalkable(){
		return isWalkable;
	}

	public float getMovementCost(){
		return movementCost;
	}

	public void setMovementCost(float num){
		movementCost = num;
	}

	public void setIsWalkable(bool b){
		isWalkable = b;
	}

	public void ShowColor(string yes){

	}

	public void setIsOccupied(bool b, GameObject unit){
		isOccupied = b;
		occupyingUnit = unit;
		//setIsSelected (b);
	}

	public bool getIsOccupied(){
		return isOccupied;
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
		isSelected = b;
		setOutline (b);

	}

	public bool getIsSelected(){
		return isSelected;
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

