using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSim : MonoBehaviour {

	[SerializeField]public CombatManager combatManager;
	[HideInInspector]public Animator animator;
	[HideInInspector] public GameManager game;
	private UnitSFX unitSFX;

	public enum UnitSide
	{
		None,
		Attacker,
		Defender

	}
	[SerializeField]protected UnitSide mySide = UnitSide.None;
	private float randTimeStart = 0.02f; //maximum amount of time before force move

	[SerializeField]protected float health;
	[SerializeField]protected int damage;
	[SerializeField]protected int defense;
	[SerializeField]protected int speed;


	protected bool mounted;
	protected Unit.MeleeWeaponType MeleeWeapon;
	protected Unit.RangedWeaponType RangedWeapon;


	public List<GameObject> Parts; //parts that may need to be changed
	[HideInInspector]public Material factionColour;


	protected float range;

	protected bool invuln;

	protected bool isDead;

	public bool hasAction = false;
	public bool canAttack = false;

	public List<GameObject> EnemyList;
	[SerializeField] protected GameObject target;

	protected float elapsedTime = 1.00f;
	[SerializeField] protected float attackSpeed;

	protected float animationRange = 0.0f;
	protected string combatType;

	//Used to control audio
	public int otherAllies = 1;

	void Awake(){

	}
	// Use this for initialization
	void Start () {

		SetUpUnitSFX ();
		foreach (GameObject part in Parts) {
			part.GetComponent<Renderer> ().material = factionColour;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!isDead)
			CheckHP ();
		if (hasAction && canAttack) {
			CheckTarget ();
			Action ();
		}
	}

	public void StartSim(){
		float num = ((float)Random.Range (1, 20 + 1) / 100 * Mathf.Pow (-1, Random.Range (1, 2 + 1)));
			attackSpeed = 1.20f + num;


		hasAction = true;
		invuln = false;
		FindTarget ();
		elapsedTime = attackSpeed;
		animator.SetBool ("isIdle", false);
	}

	public void StartSimDelayed(){
		Invoke ("StartSim", Random.value * randTimeStart);
	}

	public void StopSim(){
		hasAction = false;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		animator.SetBool ("isIdle", true);
		animator.SetBool ("isAttacking", false);
		animator.SetBool ("isMoving", false);
	}

	public void CheckHP(){
		if (health <= 0) {
            Died();
		}
	}

	public void Died()
    {
        if (combatManager.RequestDeathPermission(mySide))
        {
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            isDead = true;
            animator.SetBool("isDead", true);
            animator.SetInteger("AnimVariance", Random.Range(1, 2 + 1));

            hasAction = false;
        }
        combatManager.addDeath(mySide);
    }

	public void takeDamage(int damage){
		if (!invuln) {
			health -= (damage - defense);
			CheckHP ();
		}
	}

	public void CheckTarget(){
		if (!hasTarget()) {
			target = FindTarget ();
		} else {
			if (target.GetComponent<UnitSim> ().isDead) {
				target = FindTarget ();
			}
		}
		if (target == null) {
			animator.SetBool ("isIdle", true);
			animator.SetBool ("isAttacking", false);
			animator.SetBool ("isMoving", false);
			//Debug.Log ("All enemies dead.");
			StopSim ();
		}
	}



	public GameObject FindTarget(){
		float lowest = 99999;
		GameObject currentTarget = null;
		foreach (GameObject unit in EnemyList) {
			if (!unit.GetComponent<UnitSim> ().isDead) {
				float dist = Vector3.Distance (unit.transform.position, transform.position);
				if (dist < lowest) {
					lowest = dist;
					currentTarget = unit;
				}
			}
		}
		return currentTarget;
	}

	public bool hasTarget(){
		if (target == null) 
			return false;
		if (!target.GetComponent<UnitSim> ().isDead)
			return true;
		else
			return false;
	}

	public void Action(){

		//setting attack dist
		if (combatType == "Melee")
			animationRange = range/2;
		else
			animationRange = range*2.5f;
		
		if(target){
			if (Vector3.Distance (target.transform.position, transform.position) < animationRange) {
				animator.SetBool ("isMoving", false);
				animator.SetBool ("isIdle", false);
				Attack ();
			} else {
				animator.SetBool ("isMoving", true);
				Move ();
			}
		}
	}


	public void Move() {

		//setting move distance before run. Higher range means less travel to go
		if (combatType == "Melee")
			animationRange = range + 10;
		else
			animationRange = range * 2.5f;


		if(Vector3.Distance(transform.position, target.transform.position) > animationRange) {
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed/4 * Time.deltaTime);
			animator.SetBool ("isAttacking", false);
		} else {
			//running
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed/2 * Time.deltaTime);
			animator.SetBool ("isAttacking", true);
		}
		if(target != null)
			transform.LookAt (target.transform.position);
	}

	public void Attack() { 
		if (elapsedTime > attackSpeed) {
			if (target.GetComponent<UnitSim> ().isMounted () && MeleeWeapon == Unit.MeleeWeaponType.Spear)
				damage *= 3;
			target.GetComponent<UnitSim> ().takeDamage (damage);
			animator.SetInteger ("AnimVariance", Random.Range(1, 2+1));
			animator.SetTrigger ("Attack");
			elapsedTime = 0.0f;	
			float num = ((float)Random.Range (1, 20 + 1) / 100 * Mathf.Pow (-1, Random.Range (1, 2 + 1)));
			attackSpeed = 1.20f + num;
			animator.SetBool ("isIdle", true);
		}
		elapsedTime += Time.deltaTime;

	}

	public void setHealth(float num){
		health = num;
	}

	public void setDamage(int num){
		damage = num;
	}
	public void setDefense(int num){
		defense = num;
	}

	public void setSpeed(int num){
		speed = num;
	}

	public void setRange(int num){
		range = num;
	}

	public void setUnitSide(UnitSide side){
		mySide = side;
	}

	public void becomeInvuln(){
		invuln = true;
	}

	public void setCombatType(string type){
		combatType = type;
	}


	public bool isMounted(){
		return mounted;
	}

	public void setIsMounted(bool b){
		mounted = b;
	}

	public void setMeleeWeaponType(Unit.MeleeWeaponType newMeleeWeapon){
		MeleeWeapon = newMeleeWeapon;
	}

	public Unit.MeleeWeaponType getMeleeWeaponType(){
		return MeleeWeapon;
	}

	public void setRangedWeaponType(Unit.RangedWeaponType newRangedWeapon){
		RangedWeapon = newRangedWeapon;
	}

	public Unit.RangedWeaponType getRangedWeaponType(){
		return RangedWeapon;
	}
		
	//___soundeffects_//


	public void SetUpUnitSFX(){
		if (!gameObject.GetComponent<UnitSFX> ())
			gameObject.AddComponent<UnitSFX> ();
		gameObject.GetComponent<UnitSFX> ().audioManager = game.audioManager;
		gameObject.GetComponent<UnitSFX> ().GetUnitSimInformation (this, otherAllies);
	}
}
