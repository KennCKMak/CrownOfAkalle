using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour {

	public GameManager game;
	public ClickManager clickManager;
	public MapManager map;
	public int tileX;
	public int tileY;
    public float heightVariance;

	public enum TileType { Empty, Grassland, Stone, Forest, Mountain, Water, Bridge }
	public TileType tileType;

	public GameObject tileVisualPrefab;

	public bool occupied = false;
	public GameObject occupyingUnit;

	public bool walkable = true;
	public float movementCost = 1;


	public bool selected;
	public Shader shaderNormal;
	public Shader shaderOutline;

	private GameObject highlightBlack;



	void Start(){
		
	}



	void OnMouseUp() {
		//map.ClickEvent (tileX, tileY, this);
		//map.GeneratePathTo (this);
		if (GameManager.paused)
			return;
		if (EventSystem.current.IsPointerOverGameObject ()) 
			if(!checkMouseForHP ())
				return;
		
		clickManager.receiveClick(this);
	}

	void Update(){

	}



	void OnMouseEnter(){
		//Debug.Log (EventSystem.current.currentSelectedGameObject.transform.name);
		if (EventSystem.current.IsPointerOverGameObject () || GameManager.paused)
			return;

	
		
		if (clickManager.selectedUnit)
			AudioManager.instance.PlaySFX ("TileHover");

	}

	void OnMouseOver(){
		if (GameManager.paused) 
			return;
		
		if (EventSystem.current.IsPointerOverGameObject ())
			if (!checkMouseForHP ())
				return;
		if (clickManager.canClick) {
			clickManager.SelectTile (this);
			setOutline (true);
			if (isOccupied ()) {
				
				if (!getOccupyingUnit ().GetComponent<Unit> ().isSelected ()) {
					getOccupyingUnit ().GetComponent<Unit> ().setOutline (true);
					clickManager.setHoveredUnit (getOccupyingUnit ());
				}
			}
		}

	}

	bool checkMouseForHP(){
		PointerEventData pointer = new PointerEventData (EventSystem.current);
		pointer.position = Input.mousePosition;
		List<RaycastResult> raycastResults = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (pointer, raycastResults);
		if (raycastResults.Count > 0) {
			foreach (var go in raycastResults) {
				if (go.gameObject.tag == "HealthBar")
					return true;
			}
		}
		return false;
	}

	void OnMouseExit(){
		setOutline (false);
		if (isOccupied ()) {
			if (!getOccupyingUnit ().GetComponent<Unit> ().isSelected ()) {
				getOccupyingUnit ().GetComponent<Unit> ().setOutline (false);
				if(clickManager.selectedUnit != clickManager.hoveredUnit)
					clickManager.setHoveredUnit (null);
			}
		}
	}


	public void Setup(){
		switch (tileType) {
		case TileType.Grassland:
        case TileType.Stone:
            case TileType.Bridge:
			walkable = true;
			movementCost = 1;
			break;
		case TileType.Forest:
			walkable = true;
			movementCost = 2;
			break;
		case TileType.Mountain:
        case TileType.Water:
			walkable = false;
			movementCost = Mathf.Infinity;
			break;
		default:
			Debug.Log ("Failed to load TileType: " + tileType.ToString());
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
		setTileVisualPrefab (map.tilePrefabArray [(int)newType]);
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


	public void setOutline(bool b)
    {
		transform.FindChild ("HighlightBlack").gameObject.SetActive (b);
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
			}
            if (color == "Red")
            {
                transform.FindChild("HighlightBlue").gameObject.SetActive(false);
                transform.FindChild("HighlightRed").gameObject.SetActive(true);
            }
		}
        else
        {
            transform.FindChild("HighlightBlue").gameObject.SetActive(false);
            transform.FindChild("HighlightRed").gameObject.SetActive(false);
        }
	}

	public void setHighlighted(bool b){

        
		if (!b) {
			transform.FindChild ("HighlightBlue").gameObject.SetActive (false);
			transform.FindChild("HighlightRed").gameObject.SetActive(false);
		}
        
	}
}

