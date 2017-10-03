using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSim : MonoBehaviour {

	[SerializeField]public CombatManager combatManager;
	[HideInInspector]public Animator animator;

	public enum UnitSide
	{
		None,
		Attacker,
		Defender

	}
	[SerializeField]protected UnitSide mySide = UnitSide.None;

	[SerializeField]protected float health;
	[SerializeField]protected int damage;
	[SerializeField]protected int defense;
	[SerializeField]protected int speed;



	protected float range;

	protected bool invuln;

	protected bool isDead;

	public bool hasAction = false;
	public bool canAttack = false;

	public List<GameObject> EnemyList;
	protected GameObject target;

	protected float elapsedTime = 1.00f;
	protected float attackSpeed = 1.00f;


	// Use this for initialization
	void Start () {
		
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
		hasAction = true;
		invuln = false;
		FindTarget ();
		animator.SetBool ("isIdle", false);
	}

	public void StopSim(){
		hasAction = false;
		animator.SetBool ("isIdle", true);
		animator.SetBool ("isAttacking", false);
		animator.SetBool ("isMoving", false);
	}

	public void CheckHP(){
		if (health <= 0) {
			Died ();
		}
	}

	public void Died(){
		isDead = true;
		hasAction = false;
		combatManager.addDeath (mySide);
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
		if(target){
			if (Vector3.Distance (target.transform.position, transform.position) < range) {
				Attack ();
				animator.SetBool ("isMoving", false);
			} else {
				Move ();
				animator.SetBool ("isMoving", true);
			}
		}
	}


	public void Move() {
		if(Vector3.Distance(transform.position, target.transform.position) > range+8) {
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed/4 * Time.deltaTime);
			animator.SetBool ("isAttacking", false);
		} else {
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed/2 * Time.deltaTime);
			animator.SetBool ("isAttacking", true);
		}
			transform.LookAt (target.transform.position);
	}

	public void Attack() { 
		if (elapsedTime > attackSpeed) {
			target.GetComponent<UnitSim> ().takeDamage (damage);
			animator.SetTrigger ("Attack");

			elapsedTime = 0.0f;
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


}
