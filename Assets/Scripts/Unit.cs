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


	public enum State { Ready, ChooseMove, ChooseAction, Action, Attack, Done, Dead }
	[SerializeField] protected State unitState;
	[SerializeField] protected int unitID;
	[HideInInspector] protected GameObject visualPrefab;

	public bool selected;
	[HideInInspector]public Shader shaderNormal;
	[HideInInspector]public Shader shaderOutline;

	[SerializeField] protected int tileX;
	[SerializeField] protected int tileY;
	[HideInInspector]public MapManager map;
	List<Node> currentPath = null;

	[HideInInspector]public UnitManager unitManager;
	public UnitManager.Faction faction;

	[HideInInspector] public CombatManager combatManager;

	//STATS FOR THE UNIT
	public enum MeleeWeaponType { None, Sword, Spear, Mace };
	public enum RangedWeaponType { None, Bow, Crossbow};

	[SerializeField]protected int Speed; //How many tiles
	[SerializeField]protected float remainingMovement;
	protected bool moving = false;

	[SerializeField]protected int UnitSize; 
	[SerializeField]protected int MaxUnitSize;

	[SerializeField]protected int Health;
	[SerializeField]protected int MaxHealth;
	[SerializeField]protected int Defense;
	[SerializeField]protected bool shielded;

	[SerializeField]protected bool mounted;
	[SerializeField]protected bool armoured;


	[SerializeField]protected Unit target = null;
	[SerializeField]protected bool melee;
	[SerializeField]protected bool ranged;
	[SerializeField]protected bool attacking;
	protected int dist; //dist from you to target, given from click

	[SerializeField]protected MeleeWeaponType MeleeWeapon;
	[SerializeField]protected int MeleeExpertise;
	[SerializeField]protected int MeleeAttack;

	[SerializeField]protected RangedWeaponType RangedWeapon;
	[SerializeField]protected int RangedExpertise;
	[SerializeField]protected int RangedAttack;

	void Start(){
		if (!(unitID >= 0)){
			Debug.Log ("Failed ID");
			Destroy (gameObject);
		}
		transform.position = new Vector3 (tileX, 1.25f, tileY);


	}


	void Awake(){

	}

	void Update(){
		if (currentPath != null) {
			drawCurrentPath ();
		}
		if (unitState == State.Action){
			Move ();
		}

		if (!isAlive()) {
			Debug.Log ("Unit dead");
			Destroy (gameObject, 2f);
			transform.GetComponent<Unit> ().enabled = false;
		}
	}

	void OnMouseUp(){


	}

	public void SetupUnit(){
		
		checkDefense (); //setting armoured tag
		if(isArmoured() || isShielded())
			setSpeed (getSpeed () - 1);
		if (isMounted ())
			setSpeed (getSpeed () * 2);
		if (getDefense () > 20)
			setSpeed (getSpeed () - 1);

		if (MeleeWeapon != MeleeWeaponType.None)
			melee = true;
		if (RangedWeapon != RangedWeaponType.None)
			ranged = true;

		setHealth (MaxHealth);
		NewTurn ();
	}

	//________UNIT STATE_______//
	public void setState(State newState){ //used to access the other states of units
		unitState = newState;
	}

	public State getState(){
		return unitState;
	}

	public void setUnitID(int num){
		unitID = num;
	}

	public int getUnitID(){
		return unitID;
	}

	public void setOutline(bool b){
		if (b) {
			transform.GetChild (0).gameObject.GetComponent<Renderer> ().material.shader = shaderOutline;
		} else {
			transform.GetChild(0).gameObject.GetComponent<Renderer>().material.shader = shaderNormal;
		}
	}


	public void setIsSelected(bool b){
		selected = b;
		setOutline (b);

	}

	public bool isSelected(){
		return selected;
	}

	public GameObject getVisualPrefab(){
		return visualPrefab;
	}

	public void setVisualPrefab(GameObject newPrefab){
		visualPrefab = newPrefab;
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

	public bool hasEnoughMove(){
		if (currentPath == null)
			return true;
		if (currentPath.Count - 1 <= remainingMovement)
			return true;
		else
			return false;
	}

	public int getCurrentPathCount(){
		if (currentPath == null)
			return -1;
		return currentPath.Count-1;
	}

	public void drawCurrentPath(){
		if (currentPath == null) {
			return;
		}

		int currNode = 0; //cycles through and draws a vector
		while (currNode < currentPath.Count-1) {
			Vector3 start = map.TileCoordToWorldCoord (currentPath [currNode].x, currentPath [currNode].y) + 
				new Vector3(0, 1, 0);
			Vector3 end = map.TileCoordToWorldCoord (currentPath [currNode+1].x, currentPath [currNode+1].y) + 
				new Vector3(0, 1, 0);
			Debug.DrawLine (start, end, Color.white);
			currNode++;
		}
	}

	//command to move to the next node
	public void MoveNextTile(){
		if (currentPath == null)
			return;
		if (remainingMovement <= 0)
			return;
		
		transform.position = map.TileCoordToWorldCoord(getTileX(), getTileY());
		//remove movement cost	
		remainingMovement -= (int)map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

		//move
		setTileX(currentPath [1].x);  
		setTileY(currentPath [1].y);

		currentPath.RemoveAt (0);
		if(currentPath.Count == 1){
			endPath();
			//we only have on tile left in the path, so that must be our dest, 
			//and we are on it, so we can now clear pathfinding info
		}
	}
	//the moving 
	void Move(){
		if (Vector3.Distance (transform.position, map.TileCoordToWorldCoord (getTileX (), getTileY ())) < 0.02f) {
			MoveNextTile ();
		}



		if (Vector3.Distance (transform.position, map.TileCoordToWorldCoord (getTileX (), getTileY ())) < 0.01f && currentPath == null) {
			if (!attacking)
				unitState = State.Done;
			else if(attacking) {
				Debug.Log ("Here I am");
				unitState = State.Attack;
				combatManager.RequestCombatResolve (this, target, dist);
			}
		}
		else
			transform.position = Vector3.MoveTowards (transform.position, map.TileCoordToWorldCoord (getTileX (), getTileY ()), Speed / 2 * Time.deltaTime);
		
	}

	public bool isMoving(){
		return moving;
	}

	public void setIsMoving(bool b){
		moving = b;
	}

	void endPath(){
		currentPath = null;
	}

	public void NewTurn(){
		remainingMovement = Speed;
		setState (State.Ready);
	}



	//__________GET AND SET FUNCTIONS FOR VARIABLES_________//
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
	public bool isShielded(){
		return shielded;
	}

	public void setIsShielded(bool b){
		shielded = b;
	}


	public bool isMounted(){
		return mounted;
	}

	public void setIsMounted(bool b){
		mounted = b;
	}

	public bool isArmoured(){
		return armoured;
	}

	public void setIsArmoured(bool b){
		armoured = b;
	}

	public void checkDefense(){
		if (getDefense () > 20)
			setIsArmoured (true);
	}

	public bool isMelee(){
		return melee;
	}

	public void setIsMelee(bool b){
		melee = b;
	}

	public bool isRanged(){
		return ranged;
	}

	public void setIsRanged(bool b){
		ranged = b;
	}

	//ATTACKING
	public bool isAttacking(){
		return attacking;
	}

	public void setIsAttacking(bool b){
		attacking = b;
	} 

	public void setTarget(Unit targetUnit){
		target = targetUnit;
	}

	public Unit getTarget(){
		return target;
	}

	public void setDist(int num){
		dist = num;
	}

	public int getDist(int num){
		return dist;
	}
	

	//MELEE
	public void setMeleeWeaponType(MeleeWeaponType newMeleeWeapon){
		MeleeWeapon = newMeleeWeapon;
	}

	public MeleeWeaponType getMeleeWeaponType(){
		return MeleeWeapon;
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
	public void setRangedWeaponType(RangedWeaponType newRangedWeapon){
		RangedWeapon = newRangedWeapon;
	}

	public RangedWeaponType getRangedWeaponType(){
		return RangedWeapon;
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

	public int getWeaponRange(){
		int num = 0;
		if (MeleeWeapon != MeleeWeaponType.None)
			num = 1;
		if (RangedWeapon != RangedWeaponType.None)
			num = 2;
		return num;
	}

}
