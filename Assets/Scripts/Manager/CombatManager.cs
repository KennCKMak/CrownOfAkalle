using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {
	GameObject Ally_Sword;
	GameObject Enemy_Sword;

	// Use this for initialization
	void Start () {
	}


	public void Update()
	{
		/*if (Input.GetKeyDown (KeyCode.Space)) {
			StartCombat (Ally_Sword, Enemy_Sword);
		}*/
	}

	void StartCombat(GameObject AttackUnit, GameObject DefendUnit){
		Unit Attacker = AttackUnit.GetComponent<Unit> ();
		Unit Defender = DefendUnit.GetComponent<Unit>(); 

		//TODO: Check if they are dead or not FIRST before engaging

		if (!Attacker.unitIsRanged ())
			ResolveCombatMelee (Attacker, Defender);
		else
			ResolveCombatRanged (Attacker, Defender);


	}

	void ResolveCombatMelee(Unit Attacker, Unit Defender){
		Debug.Log ("started melee combat");
		int damage;
		damage = CalculateDamage (Attacker, Defender, "Melee");
		Defender.takeDamage (damage);

		//NOW IT"S TIME FOR DEFENDER TO ATTACK
		damage = CalculateDamage(Defender, Attacker, "Melee");
		Attacker.takeDamage (damage);
	}

	void ResolveCombatRanged(Unit Attacker, Unit Defender){
		Debug.Log ("started ranged combat");
		int damage;
		damage = CalculateDamage (Attacker, Defender, "Ranged");
		Defender.takeDamage (damage);

		if (Defender.unitIsRanged ()) {
			damage = CalculateDamage (Defender, Attacker, "Ranged");
			Attacker.takeDamage (damage);
		}

	}

	int CalculateDamage(Unit Attacker, Unit Defender, string combatType){
		int totalPool = 0; //expertise added together in melee

		if (combatType == "Melee") //for melee, roll against each other
			totalPool = Defender.getMeleeExpertise () + Attacker.getMeleeExpertise ();
		else if (combatType == "Ranged") { //for ranged, roll against 100 for chance
			totalPool = Attacker.getRangedExpertise ();
			if (Defender.unitIsShielded ()) {
				totalPool -= 25; //-25% chance to hit if shielded
			}
		} else {
			return 0;
		}


		int successfulHits = 0; //amount of hits
		int totalDamage; //damage done this roll
		int rand;

		for (int i = 0; i < Attacker.getUnitSize (); i++) {

			if (combatType == "Melee") {
				rand = Random.Range (0, totalPool + 1); //choose random number in the pool
				//total pool looks like |___DEFENSE___|___ATTACK___|
				if (rand > Defender.getMeleeExpertise ()) { //if random number overcomes them
					successfulHits++;
				}
			}

			if (combatType == "Ranged") {
				rand = Random.Range (0, 100 + 1); //rolls against 100 chance
				if (rand > Attacker.getRangedExpertise ()) {
					successfulHits++;
				}
			}

		}
		totalDamage = successfulHits * (Attacker.getMeleeAttack () - Defender.getDefense ());

		//modifiers
		if (Defender.unitIsMounted () && Attacker.getMeleeWeaponType () == Unit.MeleeWeaponType.Spear)
			totalDamage *= 2;
		if (Defender.unitIsArmoured ()) {
			if (Attacker.getMeleeWeaponType () == Unit.MeleeWeaponType.Mace || Attacker.getRangedWeaponType () == Unit.RangedWeaponType.Crossbow)
				totalDamage *= 2;
		}


		Debug.Log ("Successful Hits: " + successfulHits + ". Damage = " + totalDamage);
		return totalDamage;

	}

}
