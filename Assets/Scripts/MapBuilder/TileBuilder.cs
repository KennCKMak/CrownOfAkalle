using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuilder : MonoBehaviour {

	[HideInInspector] public MapBuilderManager mapBuilder;

	public int tileX;
	public int tileY;

	public Tile.TileType tileType;
	public GameObject tileVisualPrefab;

	public bool occupied = false;
	public GameObject occupyingUnit;
	public UnitManager.UnitName unitType;
	public UnitManager.Faction unitFaction;

	public bool selected;

    private Outline[] outlineObjects;

    void Awake()
    {
        //outlineObjects = GetComponentsInChildren<Outline>();
    }

	void Start(){
		
	}



	void OnMouseUp() {
		//mapBuilder.ClickEvent (gameObject);
	}

	void Update(){


	}

	void OnMouseOver(){
		setOutline (true);
		if (isOccupied ()) {
			if (!getOccupyingUnit ().GetComponent<UnitBuilder> ().isSelected ()) {
				getOccupyingUnit ().GetComponent<UnitBuilder> ().setOutline (true);
			}
		}
		if (Input.GetKey (KeyCode.Mouse0)) {
			mapBuilder.ClickEvent (gameObject);
		}
	}

	void OnMouseExit(){
		setOutline (false);
		if (isOccupied ()) {
			if (!getOccupyingUnit ().GetComponent<UnitBuilder> ().isSelected ()) {
				getOccupyingUnit ().GetComponent<UnitBuilder> ().setOutline (false);
			}
		}
	}


	public void Setup(){
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



	public void setTileType(Tile.TileType newType, GameObject newPrefab){
		tileType = newType;
		tileVisualPrefab = newPrefab;
	}

	public int getTileType(){
		return (int)tileType;
	}

	public GameObject getTileVisualPrefab(){
		return tileVisualPrefab;
	}

	public void GenerateVisual(){
		GameObject visual = Instantiate (tileVisualPrefab, this.transform.position, Quaternion.identity) as GameObject;
		visual.transform.parent = transform;
		visual.GetComponent<BoxCollider> ().enabled = false;
		outlineObjects = GetComponentsInChildren<Outline>();
	}

	public void DestroyVisual(){
		
		setOutline (false);
		outlineObjects = null;
		try{
			Destroy(transform.GetChild(0).gameObject);
		}catch{
		}
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

	public int getUnitType(){
		return (int)(getOccupyingUnit ().GetComponent<UnitBuilder> ().unitType);
	}

	public int getUnitFaction(){
		return (int)(getOccupyingUnit ().GetComponent<UnitBuilder> ().faction);
	}

	public void setOutline(bool b)
    {
		if (outlineObjects == null)
			return;
       	foreach (Outline outline in outlineObjects)
        {
			if(outline != null)
           		 outline.enabled = b;
        }
    }


    public void setIsSelected(bool b){
		selected = b;
		setOutline (b);

	}

	public bool isSelected(){
		return selected;
	}
		
		
}

