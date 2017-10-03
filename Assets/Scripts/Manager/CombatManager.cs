using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour {


	public bool simulationRunning;
	protected GameManager game;

	List<GameObject> AttackingUnits;
	List<GameObject> DefendingUnits;
	int attackerDeaths = 0;
	int defenderDeaths = 0;
	int requiredAttackerDeaths = 0;
	int requiredDefenderDeaths = 0;
	int initialDefenderTroopCount = 0;
	int initialAttackerTroopCount = 0;

	public float elapsedTime;
	protected float introSimulationTime = 1.0f;
	protected float maxSimulationTime = 16.0f;
	protected float endSimulationTime = 17.0f;
	SimulationState simState;
	public enum SimulationState { Ready, Intro, Playing, Ending, End}


	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();
		AttackingUnits = new List<GameObject> ();
		DefendingUnits = new List<GameObject> ();
		simulationRunning = false;
	}


	public void Update()
	{
		if (simulationRunning) {
			Simulate ();
		}
	}



	public void RequestCombatResolve(Unit initiator, Unit target, int dist){
		initiator.setState (Unit.State.Done);
		game.clickManager.canClick = false;
		game.cameraManager.setCameraState (CameraManager.CameraState.Simulation);

		if (dist == 1) {
			ResolveCombatMelee (initiator, target);
		} else if (dist >= 2) {
			ResolveCombatRanged (initiator, target);
		}


	}

	//sends the appropriate details to the simulation setup
	void ResolveCombatMelee(Unit Attacker, Unit Defender){


		initialAttackerTroopCount = Attacker.getUnitSize();
		initialDefenderTroopCount = Defender.getUnitSize();

		Defender.takeDamage (CalculateDamage (Attacker, Defender, "Melee"));
		requiredDefenderDeaths = initialDefenderTroopCount - Defender.getUnitSize ();
		Debug.Log("Gotten initialDef count: = " +initialDefenderTroopCount);
		Debug.Log("Defender unitsize = " + Defender.getUnitSize());
		Debug.Log ("Required Defender deaths " + requiredDefenderDeaths);

		if (Defender.isMelee ()) {
			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Melee"));
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();
		}

		//Debug.Log ("AttackerDeaths: " + attackerDeaths + ". DefenderDeaths: " + Defender);
		SetupSimulation (Attacker, Defender, "Melee");
	}

	void ResolveCombatRanged(Unit Attacker, Unit Defender){


		initialAttackerTroopCount = Attacker.getUnitSize();
		initialDefenderTroopCount = Defender.getUnitSize();


		Defender.takeDamage (CalculateDamage (Attacker, Defender, "Ranged"));
		requiredDefenderDeaths = initialDefenderTroopCount - Defender.getUnitSize ();


		if (Defender.isRanged ()) {
			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Ranged"));
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();
		}

		//Debug.Log ("AttackerDeaths: " + attackerDeaths + ". DefenderDeaths: " + Defender);
		SetupSimulation (Attacker, Defender, "Ranged");

	}

	//sets up all the simulated prefabs - values, locations
	void SetupSimulation(Unit attacker, Unit defender, string combatType){
		for (int i = 0; i < initialAttackerTroopCount; i++) {
			GameObject newUnit = Instantiate (attacker.getSimPrefab()) as GameObject;
			newUnit.GetComponent<UnitSim> ().setHealth(attacker.getHealthPerUnit ());
			newUnit.GetComponent<UnitSim> ().setUnitSide (UnitSim.UnitSide.Attacker);
			newUnit.GetComponent<UnitSim> ().combatManager = this;

			if (combatType == "Melee") {
				newUnit.GetComponent<UnitSim> ().setDamage(attacker.getMeleeAttack ());
				newUnit.GetComponent<UnitSim> ().setRange (1);
			} else if (combatType == "Ranged") {
				newUnit.GetComponent<UnitSim> ().setDamage(attacker.getRangedAttack ());
				newUnit.GetComponent<UnitSim> ().setRange(attacker.getWeaponRange());
			}
			newUnit.GetComponent<UnitSim>().canAttack = true;

			newUnit.GetComponent<UnitSim>().setDefense(attacker.getDefense ());
			newUnit.GetComponent<UnitSim>().setSpeed(attacker.getSpeed ());

			//newUnit.transform.GetChild (0).gameObject.GetComponent<Renderer>().material.shader = attacker.getShaderOutline();
			newUnit.GetComponent<UnitSim>().animator = newUnit.GetComponent<Animator>();
			AttackingUnits.Add (newUnit);
		}

		for (int i = 0; i < initialDefenderTroopCount; i++) {
			GameObject newUnit = Instantiate (defender.getSimPrefab()) as GameObject;
			newUnit.GetComponent<UnitSim> ().setHealth(defender.getHealthPerUnit ());
			newUnit.GetComponent<UnitSim> ().setUnitSide (UnitSim.UnitSide.Defender);
			newUnit.GetComponent<UnitSim> ().combatManager = this;

			//setting damage and whether we can attack
			if (combatType == "Melee" && defender.isMelee ()) {
				newUnit.GetComponent<UnitSim> ().setDamage(defender.getMeleeAttack ());
				newUnit.GetComponent<UnitSim> ().canAttack = true;
			} else if (combatType == "Ranged" && defender.isRanged ()) {
				if (defender.getWeaponRange () >= attacker.getWeaponRange ()) {
					newUnit.GetComponent<UnitSim> ().setDamage(defender.getRangedAttack ());
					newUnit.GetComponent<UnitSim> ().setRange(defender.getWeaponRange());
					newUnit.GetComponent<UnitSim> ().canAttack = true;
				}
			} else {
				newUnit.GetComponent<UnitSim> ().canAttack = false;
			}

			newUnit.GetComponent<UnitSim>().setDefense(defender.getDefense ());
			newUnit.GetComponent<UnitSim>().setSpeed(defender.getSpeed ());

			//newUnit.transform.GetChild (0).gameObject.GetComponent<Renderer> ().material.shader = defender.getShaderOutline();
			newUnit.GetComponent<UnitSim>().animator = newUnit.GetComponent<Animator>();
			DefendingUnits.Add (newUnit);
		}

		//assigning targets
		foreach (GameObject unit in AttackingUnits)
			unit.GetComponent<UnitSim> ().EnemyList = DefendingUnits;
		foreach (GameObject unit in DefendingUnits)
			unit.GetComponent<UnitSim> ().EnemyList = AttackingUnits;

		//Placing the units
		Vector3 AttackerPos = new Vector3();
		Vector3 DefenderPos = new Vector3();
		switch (combatType) {
		case "Melee": //distance of 7+7 from each other
			AttackerPos = new Vector3 (500.0f, 0.245f, -8);
			DefenderPos = new Vector3 (500.0f, 0.245f, 8);
			SpawnUnitsAt (AttackerPos, attacker.faction, DefenderPos, defender.faction);
			break;
		case "Ranged":
			AttackerPos = new Vector3 (500.0f, 0.245f, -10);
			DefenderPos = new Vector3 (500.0f, 0.245f, 10);
			SpawnUnitsAt (AttackerPos, attacker.faction, DefenderPos, defender.faction);
			break;
		default:
			break;
		}
		Vector3 camPos = game.cameraManager.CameraSimulation.transform.position;
		camPos.z = AttackerPos.z-7;
		game.cameraManager.CameraSimulation.transform.position = camPos;

		StartSimulation ();
	}

	void StartSimulation(){
		elapsedTime = 0.0f;
		simState = SimulationState.Intro;

		simulationRunning = true;
	}

	void Simulate(){
		if (elapsedTime > introSimulationTime && simState == SimulationState.Intro) {
			Debug.Log ("Started");
			foreach (GameObject unit in AttackingUnits) {
				unit.GetComponent<UnitSim> ().StartSim ();
			}

			foreach (GameObject unit in DefendingUnits) {
				unit.GetComponent<UnitSim> ().StartSim ();
			}

			simState = SimulationState.Playing;
		}

		if (attackerDeaths == requiredAttackerDeaths) {
			foreach (GameObject unit in AttackingUnits) {
				unit.GetComponent<UnitSim> ().becomeInvuln ();
			}
		}
		if (defenderDeaths == requiredDefenderDeaths) {
			foreach (GameObject unit in DefendingUnits) {
				unit.GetComponent<UnitSim> ().becomeInvuln ();
			}
		}

		if (attackerDeaths >= requiredAttackerDeaths && defenderDeaths >= requiredDefenderDeaths && simState != SimulationState.Ending) {
			//TODO
			Debug.Log ("Attacker deaths: " + attackerDeaths);
			Debug.Log ("RAttacker deaths: " + requiredAttackerDeaths);
			Debug.Log ("Defender deaths: " + defenderDeaths);
			Debug.Log ("RDefender deaths: " + requiredDefenderDeaths);
			Debug.Log (simState.ToString ());
			simState = SimulationState.Ending;
			elapsedTime = maxSimulationTime;
			foreach (GameObject unit in AttackingUnits)
				unit.GetComponent<UnitSim> ().StopSim ();
			foreach (GameObject unit in DefendingUnits) 
				unit.GetComponent<UnitSim> ().StopSim ();
		}
		if (elapsedTime >= maxSimulationTime && simState == SimulationState.Playing) {
			simState = SimulationState.Ending;
			Debug.Log ("Switching to Ending");
		}

		if (elapsedTime >= endSimulationTime && simState == SimulationState.Ending) {
			Debug.Log ("Stopping here. ElapsedTime = " + elapsedTime);
			StopSimulation ();
			Debug.Log ("Flushed ElapsedTime = " + elapsedTime);
		}

		elapsedTime += Time.deltaTime;
	}

	void StopSimulation(){
		ResetSimulation ();
		simulationRunning = false;
		game.unitManager.ScanForDeadUnits ();
		game.clickManager.canClick = true;
		game.cameraManager.setCameraState (CameraManager.CameraState.Strategy);


	}

	void SpawnUnitsAt (Vector3 attackingPos, UnitManager.Faction attackerFaction, 
		Vector3 defendingPos, UnitManager.Faction defenderFaction){
		//spawning attackers, grid 6 by x
		SpawnFormation(attackingPos, AttackingUnits, 6, attackerFaction);
		SpawnFormation(defendingPos, DefendingUnits, 6, defenderFaction);

	}


	void ResetSimulation(){
		elapsedTime = 0;
		attackerDeaths = 0;
		defenderDeaths = 0;
		requiredAttackerDeaths = 0;
		requiredDefenderDeaths = 0;
		simulationRunning = false;
		
		foreach (GameObject a in AttackingUnits) 
			Destroy (a.gameObject);
		foreach (GameObject d in DefendingUnits)
			Destroy (d.gameObject);


		AttackingUnits.Clear();
		DefendingUnits.Clear ();
		simState = SimulationState.Ready;
	}

	public void addDeath(UnitSim.UnitSide side){
		if (side == UnitSim.UnitSide.Attacker) {
			attackerDeaths++;
		} else if (side == UnitSim.UnitSide.Defender) {
			defenderDeaths++;
		}
	}

	void SpawnFormation(Vector3 start, List<GameObject> unitList, int numPerLine, UnitManager.Faction faction){
		//start at 499.5
		int mirror = 1;
		if (faction == UnitManager.Faction.Enemy)
			mirror = -1;

		int lines = Mathf.FloorToInt(unitList.Count/numPerLine);
		int unitsSpawned = 0;

		float disHorizontal = 0.6f; //between columns
		float disVertical = 0.6f; //between rows

		//spawn in lines
		float xStartPos = start.x - (numPerLine / 2) * disHorizontal * mirror + (disHorizontal/2)* mirror;
		for (int x = 0; x < lines; x++) {
			for (int y = 0; y < numPerLine; y++) {
				unitList [unitsSpawned].transform.position = 
					new Vector3 (xStartPos + disHorizontal*y * mirror, start.y, start.z - x*disVertical*mirror);
				unitsSpawned++;
			}
		}

		int remaningUnits = unitList.Count - unitsSpawned;
		if (remaningUnits % 2 == 0) {
			//even remaining troops, displace slightly
			xStartPos =  start.x - (remaningUnits / 2) * disHorizontal  * mirror + (disHorizontal/2)* mirror;
			for (int y = 0; y < remaningUnits; y++) {
				unitList [unitsSpawned].transform.position = 
					new Vector3 (xStartPos + disHorizontal * y * mirror, start.y, start.z - lines *disVertical* mirror);
				unitsSpawned++;
			}
		} else {
			//odd remanining troops, displace slightly
			xStartPos = start.x - (Mathf.Floor ((remaningUnits) / 2)) * disHorizontal * mirror;//*mirror;
			for (int y = 0; y < remaningUnits; y++) {
				unitList [unitsSpawned].transform.position = 
					new Vector3 (xStartPos + disHorizontal * y * mirror, start.y, start.z - lines *disVertical* mirror);
				unitsSpawned++;
			}
		}

		//turning around enemy faction
		if (faction == UnitManager.Faction.Enemy) {
			foreach (GameObject unit in unitList) {
				Vector3 pos = unit.transform.position;
				pos.z -= 1;
				unit.transform.LookAt (pos);
			}
		}
	}

	int CalculateDamage(Unit Attacker, Unit Defender, string combatType){
		int totalPool = 0; //expertise added together in melee

		if (combatType == "Melee") //for melee, roll against each other
			totalPool = Defender.getMeleeExpertise () + Attacker.getMeleeExpertise ();
		else if (combatType == "Ranged") { //for ranged, roll against 100 for chance
			totalPool = Attacker.getRangedExpertise ();
			if (Defender.isShielded ()) {
				totalPool -= 25; //-25% chance to hit if shielded
			}
		} else {
			return 0;
		}


		int successfulHits = 0; //amount of hits
		int totalDamage = 0; //damage done this roll
		int rand;

		for (int i = 0; i < Attacker.getUnitSize (); i++) {

			if (combatType == "Melee") {
				rand = Random.Range (0, totalPool + 1); //choose random number in the pool
				//total pool looks like |___DEFENSE___|___ATTACK___|
				if (rand > Defender.getMeleeExpertise ()) { //if random number overcomes them
					successfulHits++;
				}
				totalDamage = successfulHits * (Attacker.getMeleeAttack () - Defender.getDefense ());
			}

			else if (combatType == "Ranged") {
				rand = Random.Range (0, 100 + 1); //rolls against 100 chance
				if (rand < Attacker.getRangedExpertise ()) {
					successfulHits++;
				}
				totalDamage = successfulHits * (Attacker.getRangedAttack () - Defender.getDefense ());
			}

		}

		//modifiers
		if (Defender.isMounted () && Attacker.getMeleeWeaponType () == Unit.MeleeWeaponType.Spear)
			totalDamage *= 2;
		if (Defender.isArmoured ()) {
			if (Attacker.getMeleeWeaponType () == Unit.MeleeWeaponType.Mace || Attacker.getRangedWeaponType () == Unit.RangedWeaponType.Crossbow)
				totalDamage *= 2;
		}


		//Debug.Log ("Successful Hits: " + successfulHits + ". Damage = " + totalDamage);
		return totalDamage;

	}
}
