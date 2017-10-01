using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	
	public MapManager map;
	public int tileX;
	public int tileY;

	public enum TileType { Grassland, Forest, Mountain }
	public TileType tileType;

	public GameObject tileVisualPrefab;

	public bool isOccupied = false;
	public bool isWalkable = true;
	public float movementCost = 1;

	void Start(){

	}



	void OnMouseUp() {
		Debug.Log ("Clicked");
		map.ClickEvent (tileX, tileY, this);
		map.GeneratePathTo (this);
	}

	void OnMouseOver(){
	//	transform.GetChild (0).gameObject.SetActive (true);
	}

	void OnMouseExit(){
		//transform.GetChild (0).gameObject.SetActive (false);
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

	public void setIsOccupied(bool b){
		isOccupied = true;
	}

	public bool getIsOccupied(){
		return isOccupied;
	}
}

