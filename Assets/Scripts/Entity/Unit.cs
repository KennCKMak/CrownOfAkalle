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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {


	public enum State { Ready, ChooseMove, ChooseAction, Action, Attack, Done, Dead }
	[SerializeField] protected State unitState;
	[SerializeField] protected int unitID;
	[HideInInspector] protected GameObject simPrefab;
	[HideInInspector] protected Animator animator;
	private UnitSFX unitSFX;

	public bool selected;
	public List<GameObject> Parts; //parts that may need to be changed
	[HideInInspector]public Shader shaderNormal;
	[HideInInspector]public Shader shaderOutline;
	[HideInInspector]public Material factionColour;

	[SerializeField] protected int tileX;
	[SerializeField] protected int tileY;
	[HideInInspector]public MapManager map;
	List<Node> currentPath = null;

	[HideInInspector]public UnitManager unitManager;
	[HideInInspector] public CombatManager combatManager;
	[HideInInspector] public GameManager game;

	public UnitManager.Faction faction;

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
	[SerializeField]protected float HealthPerUnit;
	[SerializeField]protected int Defense;
	[SerializeField]protected bool shielded;
	private GameObject HealthBarPrefab;
	private HealthBar healthBar;


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

    private Outline[] outlineComponents;
    private OutlineEffect.OutlineColor outlineColor;


	void Start(){
		if (!(unitID >= 0)){
			Debug.Log ("Failed ID");
			Destroy (gameObject);
		}
		transform.position = new Vector3 (tileX, 0.57f, tileY);
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

		UpdateHealthBar ();

	}



	public void SetUpUnit(){

		animator = this.gameObject.GetComponent<Animator> ();
		checkDefense (); //setting armoured tag
		if(isArmoured() || isShielded())
			setSpeed (getSpeed () - 1);
		if (getDefense () > 20)
			setSpeed (getSpeed () - 1);

		if (MeleeWeapon != MeleeWeaponType.None)
			melee = true;
		if (RangedWeapon != RangedWeaponType.None)
			ranged = true;

		setHealth (MaxHealth);
		HealthPerUnit = MaxHealth / MaxUnitSize;

		foreach (GameObject part in Parts) {
            if(part.name != "WK_Horse_A")
			    part.GetComponent<Renderer> ().material = factionColour;
		}
		CreateHealthBar ();

		SetUpUnitSFX ();

        SetUpUnitOutline();


        setOutline (false);

		NewTurn ();
	}

	public void checkState(){
		switch (unitState) {
		case State.Ready:
		case State.ChooseAction:
		case State.ChooseMove:
		case State.Done:
			attacking = false;
			animator.SetBool ("isIdle", true);
			animator.SetBool ("isMoving", false);
			animator.SetBool ("isAttacking", false);
			break;
		case State.Action:
			animator.SetBool ("isIdle", false);
			animator.SetBool ("isMoving", true);
			if (attacking)
				animator.SetBool ("isAttacking", true);
			else
				animator.SetBool ("isAttacking", false);
			break;

		case State.Attack:
			animator.SetBool ("isIdle", false);
			animator.SetBool ("isMoving", false);
			animator.SetBool ("isAttacking", false);
			animator.SetInteger ("AnimVariance", Random.Range (1, 2 + 1));
			animator.SetTrigger ("Attack");
			attacking = false;
			break;
		case State.Dead:
			animator.SetBool ("isIdle", false);
			animator.SetBool ("isMoving", false);
			animator.SetBool ("isAttacking", false);
			animator.SetInteger ("AnimVariance", Random.Range(1, 2+1));
			animator.SetBool ("isDead", true);
			break;
		default:
			break;

		}
	}

	//________UNIT STATE_______//
	public void setState(State newState){ //used to access the other states of units
		unitState = newState;

		checkState ();
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
        Debug.Log("called w/ " + newColor.ToString());
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

	public Shader getShaderOutline(){
		return shaderOutline;
	}


	public void setIsSelected(bool b){
		selected = b;
		setOutline (b);

	}

	public bool isSelected(){
		return selected;
	}

	public GameObject getSimPrefab(){
		return simPrefab;
	}

	public void setSimPrefab(GameObject newPrefab){
		simPrefab = newPrefab;
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
			if (!attacking) {
				setState (State.Done);
				game.click.canClick = true; //longer way to request for click manager
				unitManager.checkEndTurn();
			} else if(attacking) {
				setState(State.Attack);

				transform.LookAt (map.TileCoordToWorldCoord(target.GetComponent<Unit>().getTileX(), target.GetComponent<Unit>().getTileY ()));
				target.transform.LookAt (map.TileCoordToWorldCoord(getTileX(), getTileY()));
					
				combatManager.RequestCombatResolve (this, target, dist);
			}
		}
		else{
			transform.position = Vector3.MoveTowards (transform.position, map.TileCoordToWorldCoord (getTileX (), getTileY ()), 3 * Time.deltaTime);
			transform.LookAt (map.TileCoordToWorldCoord (getTileX (), getTileY ()));
		}
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
		float percentage = (float)getHealth() / (float)getMaxHealth();
		if (Health > 0){
			setUnitSize(Mathf.CeilToInt(MaxUnitSize * percentage));
		} else {
			setUnitSize (0);
			//setState (State.Dead);
			unitManager.requestDelete (this.gameObject);
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

	public float getHealthPerUnit(){
		return HealthPerUnit;
	}

	public bool isAlive(){
		if (Health > 0)
			return true;
		else
			return false;
	}

	public void SetHealthBarPrefab(GameObject prefab){
		HealthBarPrefab = prefab;
	}

	public void CreateHealthBar(){
		GameObject newHealthBar = Instantiate (HealthBarPrefab) as GameObject;
		newHealthBar.transform.SetParent(game.ui.canvas.transform);
		newHealthBar.GetComponent<HealthBar> ().SetTarget (this.transform);
		newHealthBar.GetComponent<HealthBar> ().hpCamera = game.camManager.CameraStrategy;
		healthBar = newHealthBar.GetComponent<HealthBar> ();
	}

	public void UpdateHealthBar(){
		if (healthBar)
			healthBar.UpdateHealthBar (Health, MaxHealth);
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
			num = 5;
		return num;
	}


	public Animator getAnimator(){

		return this.animator;
	}

	public void SetUpUnitSFX(){
		if (!gameObject.GetComponent<UnitSFX> ())
			gameObject.AddComponent<UnitSFX> ();
		gameObject.GetComponent<UnitSFX> ().audioManager = game.audioManager;
		gameObject.GetComponent<UnitSFX> ().GetUnitInformation (this);
	}



    //Control directions
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
