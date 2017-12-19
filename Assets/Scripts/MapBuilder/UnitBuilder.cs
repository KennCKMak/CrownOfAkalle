/// <summary>
/// A Unit is an entire squadron of multiple soldiers and is used as a singular entity until the simulation.
/// 
/// 
/// There is no branching off. Everything is done here.
/// 
/// In the editor, the person sets the Weapons, move, health defense... basically everything
///
/// 
/// </summary>


using System.Collections.Generic;
using UnityEngine;

public class UnitBuilder : MonoBehaviour {

	[HideInInspector] protected Animator animator;
	public bool selected;

	public int tileX;
	public int tileY;

	public List<GameObject> Parts; //parts that may need to be changed
	[HideInInspector]public Material factionColour;
    [HideInInspector]public Material horseMaterial;

	public UnitManager.UnitName unitType;
	public UnitManager.Faction faction;

    private Outline[] outlineComponents;
    private OutlineEffect.OutlineColor outlineColor;


	void Start(){
	}


	void Awake(){

	}

	void Update(){

	}



	public void SetUpUnit(){

		animator = this.gameObject.GetComponent<Animator> ();
		foreach (GameObject part in Parts) {
            if (part.name != "WK_Horse_A")
                part.GetComponent<Renderer>().material = factionColour;
            else
                part.GetComponent<Renderer>().material = horseMaterial;
		}

		animator.SetBool("isIdle", true);

		if (faction == UnitManager.Faction.Enemy)
			RotateUnitFace ("South");

        SetUpUnitOutline();
		//setOutline (false);
		transform.position = new Vector3 (tileX, 0.08f, tileY);
	}

    public void SetUpUnitOutline()
    {
        //Add Outline Components
        foreach(GameObject part in Parts)
        {
            part.AddComponent<Outline>();
            part.GetComponent<Outline>().color = outlineColor;
        }
        outlineComponents = GetComponentsInChildren<Outline>();
        foreach (Outline outline in outlineComponents)
        {
            outline.enabled = false;
        }
    }

    public void SetOutlineColor(OutlineEffect.OutlineColor newColor)
    {
        this.outlineColor = newColor;
    }

    public void setOutline(bool b){
		if (b) {
            foreach (Outline outline in outlineComponents)
            {
                outline.enabled = true;
            }
        } else
        {
            foreach (Outline outline in outlineComponents)
            {
                outline.enabled = false;
            }
        }
	}

	public void setIsSelected(bool b){
		selected = b;
		setOutline (b);

	}

	public bool isSelected(){
		return selected;
	}

	//_________MOVEMENT, PATHFINDING________//
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

	/// <summary>
	/// Rotates the unit face, NESW
	/// </summary>
	/// <param name="direction">Direction.</param>
    public void RotateUnitFace(string direction)
    {
        switch (direction)
        {
            case "East":
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case "West":
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
            case "South":
                transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                break;

            case "North":
            default:
                break;
        }
    }




}
