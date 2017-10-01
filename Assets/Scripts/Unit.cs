/// <summary>
/// A Unit is an entire squadron of multiple soldiers and is used as a singular entity until the simulation.
/// All actual units in the game will inherit this class and branch off 
/// 
/// The unit will currently branch off into -> Melee, Ranged, and Cavalry
/// 
/// 
///
/// 
/// </summary>


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {


	protected enum State { Ready, Selected, Done, Dead }
	[SerializeField] protected State unitState;
	[SerializeField] protected int unitID;

	[SerializeField] protected int tileX;
	[SerializeField] protected int tileY;
	public CombatManager combatManager;
	public MapManager map;
	List<Node> currentPath = null;


	[SerializeField] protected enum MeleeWeaponType { None, Sword, Spear, Mace };
	[SerializeField] protected enum RangedWeaponType { None, Bow, Crossbow};

	[SerializeField]protected GameObject UnitPrefab; //Model representation
	[SerializeField]protected int Speed; //How many tiles
	[SerializeField]protected float remainingMovement;

	[SerializeField]protected int UnitSize; 
	[SerializeField]protected int MaxUnitSize;

	[SerializeField]protected int Health;
	[SerializeField]protected int MaxHealth;
	[SerializeField]protected int Defense;
	[SerializeField]protected bool Shielded;

	[SerializeField]protected bool isMounted;
	[SerializeField]protected bool isArmoured;
	[SerializeField]protected bool isRanged;

	[SerializeField]protected MeleeWeaponType MeleeWeapon;
	[SerializeField]protected int MeleeExpertise;
	[SerializeField]protected int MeleeAttack;

	[SerializeField]protected RangedWeaponType RangedWeapon;
	[SerializeField]protected int RangedExpertise;
	[SerializeField]protected int RangedAttack;

	void Start(){
		if (unitID == null){
			Destroy (gameObject);
			Debug.Log ("Failed ID");
		}
		transform.position = new Vector3 (tileX, 1.25f, tileY);


		combatManager = GameObject.Find("GameManager").GetComponent<CombatManager>();
		map = GameObject.Find ("GameManager").GetComponent<MapManager> ();
	}
	void LateStart(){

		//on start, tell map that the tile I'm on is mine
		map.tileArray[tileX, tileY].setIsOccupied(true);

	}

	void Awake(){
		unitState = State.Ready;
		checkDefense (); //setting armoured tag
		if(unitIsArmoured() || unitIsShielded())
			setSpeed (getSpeed () - 1);
		if (unitIsMounted ())
			setSpeed (getSpeed () * 2);
		if (getDefense () > 20)
			setSpeed (getSpeed () - 1);
		restoreMovement ();

		setHealth (MaxHealth);
	}

	void Update(){
		if (currentPath != null) {
			drawCurrentPath ();
		}
		Move ();

		if (!isAlive()) {
			Debug.Log ("Unit dead");
			Destroy (gameObject, 2f);
			transform.GetComponent<Unit> ().enabled = false;
		}
	}

	void OnMouseUp(){
		//selected
		Debug.Log("I was touched!");
		if (unitState != State.Dead) {
			if (unitState == State.Ready) {
				//the other unit
				if(map.selectedUnit != null)
					map.selectedUnit.GetComponent<Unit> ().setState (0);
				map.selectedUnit = this.gameObject;
				unitState = State.Selected;
			} 
		//selected again
		else if (unitState == State.Selected && map.selectedUnit == this) {
				map.selectedUnit = null;
				unitState = State.Ready;
			}
		}


	}


	//________UNIT STATE_______//
	public void setState(int num){ //used to access the other states of units
		unitState = (State)num;
	}

	public int getState(){
		return (int)unitState;
	}

	public void setUnitID(int num){
		unitID = num;
	}

	public int getUnitID(){
		return unitID;
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

	public void setCurrentPath(List<Node> newPath){
		currentPath = newPath;
	}

	public void drawCurrentPath(){
		int currNode = 0;
		while (currNode < currentPath.Count-1) {
			Vector3 start = map.TileCoordToWorldCoord (currentPath [currNode].x, currentPath [currNode].y) + 
				new Vector3(0, 1, 0);
			Vector3 end = map.TileCoordToWorldCoord (currentPath [currNode+1].x, currentPath [currNode+1].y) + 
				new Vector3(0, 1, 0);

			Debug.DrawLine (start, end, Color.black);
			currNode++;
		}
	}
	//command to move to the next node
	public void MoveNextTile(){
		if (currentPath == null)
			return;
		if (remainingMovement <= 0) {
			return;

		}
			//remove movement cost	
		transform.position = map.TileCoordToWorldCoord(getTileX(), getTileY());
		remainingMovement -= 1;//(int)map.CostToEnterTile(currentPath[0].x, currentPath[0]. y,currentPath[1].x, currentPath[1].y);

		//move
		setTileX(currentPath[1].x);  
		setTileY(currentPath [1].y);
		

		currentPath.RemoveAt (0);

		if(currentPath.Count == 1){
				//we only have on tile left in the path, so that must be our dest, 
				//and we are on it, so we can now clear pathfinding info
				currentPath = null;
		}
	}
	//the moving 
	void Move(){
		if (Vector3.Distance (transform.position, map.TileCoordToWorldCoord (getTileX (), getTileY ())) < 0.02f) {
			MoveNextTile ();
		}
		
		transform.position = Vector3.MoveTowards (transform.position, map.TileCoordToWorldCoord (getTileX(), getTileY()), Speed/2 * Time.deltaTime);
		if (remainingMovement == 0)
			currentPath = null;
	}

	void MoveTowards(){

	}

	void restoreMovement(){
		remainingMovement = Speed;
	}



	//__________GET AND SET FUNCTIONS FOR VARIABLES_________//
	public GameObject getUnitPrefab(){
		return UnitPrefab;
	}
	public void setUnitPrefab(GameObject newPrefab){
		UnitPrefab = newPrefab;
	}

	public int getSpeed(){
		return Speed;
	}

	public void setSpeed(int num){
		Speed = num;
	}


	//_____UNIT SIZE_____//
	public int getUnitSize(){
		return UnitSize;
	}

	public void setUnitSize(int num) {
		UnitSize = num;
	}

	public int getMaxUnitSize(){
		return MaxUnitSize;
	}

	public void setMaxUnitSize(int num){
		MaxUnitSize = num;
	}

	private void updateUnitSize(){
		if (Health > 0){
			float percentage = (float)getHealth() / (float)getMaxHealth();
			setUnitSize(Mathf.CeilToInt(MaxUnitSize * percentage));
		} else {
			setHealth (0);
			Debug.Log("Unit dead");
		}
	}
		

	//_____HEALTH, DEFENSE, SHIELD_____//
	public int getHealth(){
		return Health;
	}

	public void addHealth(int num){
		Health += num;
		updateUnitSize ();
	}

	public void setHealth(int num){
		Health = num;
		updateUnitSize ();
	}

	public void takeDamage(int num){
		Health -= num;
		updateUnitSize ();

	}

	public int getMaxHealth(){
		return MaxHealth;
	}

	public void setMaxHealth(int num){
		MaxHealth = num;
	}

	public bool isAlive(){
		if (Health > 0)
			return true;
		else
			return false;
	}

	public int getDefense(){
		return Defense;
	}

	public void setDefense(int num){
		Defense = num;
	}

	//_____MODIFIERS, EXPERTISE & DAMAGE_____//
	public bool unitIsShielded(){
		return Shielded;
	}

	public void setUnitIsShielded(bool b){
		Shielded = b;
	}


	public bool unitIsMounted(){
		return isMounted;
	}

	public void setUnitIsMounted(bool b){
		isMounted = b;
	}

	public bool unitIsArmoured(){
		return isArmoured;
	}

	public void setUnitIsArmoured(bool b){
		isArmoured = b;
	}

	public void checkDefense(){
		if (getDefense () > 20)
			setUnitIsArmoured (true);
	}

	public bool unitIsRanged(){
		return isRanged;
	}

	public void setUnitIsRanged(bool b){
		isRanged = b;
	}



	//MELEE
	public void setMeleeWeaponType(int num){
		MeleeWeapon = (MeleeWeaponType)num;
	}

	public int getMeleeWeaponType(){
		return (int)MeleeWeapon;
	}

	public void setMeleeExpertise(int num){
		MeleeExpertise = num;
	}

	public int getMeleeExpertise(){
		return MeleeExpertise;
	}

	public void setMeleeAttack(int num){
		MeleeAttack = num;
	}

	public int getMeleeAttack(){
		return MeleeAttack;
	}

	//RANGED
	public void setRangedeWeaponType(int num){
		RangedWeapon = (RangedWeaponType)num;
	}

	public int getRangedWeaponType(){
		return (int)RangedWeapon;
	}

	public void setRangedExpertise(int num){
		MeleeExpertise = num;
	}

	public int getRangedExpertise(){
		return RangedExpertise;
	}

	public void setRangedAttack(int num){
		RangedAttack = num;
	}

	public int getRangedAttack(){
		return RangedAttack;
	}

}
