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
	private int distance;

	public float elapsedTime;
	protected float introSimulationTime = 1.0f;
	protected float minSimulationTime = 8.0f;
	protected float maxSimulationTime = 16.0f;
	protected float endSimulationTime = 22.0f;
	SimulationState simState;
	public enum SimulationState { Ready, Intro, Playing, Ending, End}

	protected bool skipSimulation;

	protected GameObject camHolder;

	[SerializeField] protected Unit attacker; //this is for animation purposes, the last swing at the end
	[SerializeField] protected Unit defender;

	private Transform SimulationPlane;
	private float planeX, planeY, planeZ;
	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();
		AttackingUnits = new List<GameObject> ();
		DefendingUnits = new List<GameObject> ();
		simulationRunning = false;
		skipSimulation = false;

		SimulationPlane = GameObject.Find ("SimulationPlane").transform;
		planeX = SimulationPlane.position.x;
		planeY = SimulationPlane.position.y;
		planeZ = SimulationPlane.position.z;
	}


	public void Update()
	{
		if (simulationRunning) {
			Simulate ();
		}
	}



	public void RequestCombatResolve(Unit initiator, Unit target, int dist){
		
		attacker = initiator;
		defender = target;

		//Debug.Log ("Starting combat between " + initiator.transform.name + " and " + target.transform.name);

        attacker.setOutline(false);
        defender.setOutline(false);

		game.click.canClick = false;

		attacker.getAnimator ().SetTrigger ("Attack");
		defender.DelayAnimation("TakeDamage", 0.4f);

		distance = dist;

		Invoke ("passInformation", 0.5f);
	}

	void passInformation(){
		game.camManager.setCameraState (CameraManager.CameraState.Simulation);

		if (distance == 1) {
			ResolveCombatMelee (attacker, defender);
		} else if (distance >= 2) {
			ResolveCombatRanged (attacker, defender);
		}
	}

	//sends the appropriate details to the simulation setup
	void ResolveCombatMelee(Unit Attacker, Unit Defender){


		initialAttackerTroopCount = Attacker.getUnitSize();
		initialDefenderTroopCount = Defender.getUnitSize();

		//whoever is ranged attacks first. 
		if (Attacker.isRanged ()) {
			//Attacker deals some ranged damage, slightly nerfed
			Defender.takeDamage (CalculateDamage (Attacker, Defender, "Melee") * 8 /10 ); //80% dmg if you initiate in melee distnace
			requiredDefenderDeaths = initialDefenderTroopCount - Defender.getUnitSize ();
			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Melee"));//halve ranged fire if in melee distance
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();


		} else if (Defender.isRanged ()) {
			//Defender deals some ranged damage when attacking
			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Melee") /2);  //50% dmg dealt to enemy if they charge you
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();
			Defender.takeDamage (CalculateDamage (Attacker, Defender, "Melee"));
			requiredDefenderDeaths = initialDefenderTroopCount - Defender.getUnitSize ();



		} else {

			Defender.takeDamage (CalculateDamage (Attacker, Defender, "Melee"));
			requiredDefenderDeaths = initialDefenderTroopCount - Defender.getUnitSize ();

			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Melee"));
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();

		}
		/*if (Defender.isMelee ()) {
			Attacker.takeDamage (CalculateDamage (Defender, Attacker, "Melee"));
			requiredAttackerDeaths = initialAttackerTroopCount - Attacker.getUnitSize ();
		}*/


		//Debug.Log ("AttackerDeaths: " + attackerDeaths + ". DefenderDeaths: " + Defender);
		if (!skipSimulation) 
			SetupSimulation ("Melee");
		else
			StopSimulation ();
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
		if (!skipSimulation)
			SetupSimulation ("Ranged");
		else
			StopSimulation ();

	}

	//sets up all the simulated prefabs - values, locations
	void SetupSimulation(string combatType){
		if(attacker.faction == UnitManager.Faction.Enemy)
			game.simPlane.GenerateTerrain (defender, attacker, distance);
		else
			game.simPlane.GenerateTerrain (attacker, defender, distance);
			

		for (int i = 0; i < initialAttackerTroopCount; i++) {
			GameObject newUnit = Instantiate (attacker.getSimPrefab()) as GameObject;	
			newUnit.GetComponent<UnitSim> ().setHealth(attacker.getHealthPerUnit ());
			newUnit.GetComponent<UnitSim> ().setUnitSide (UnitSim.UnitSide.Attacker);
			newUnit.GetComponent<UnitSim> ().combatManager = this;
			newUnit.GetComponent<UnitSim> ().game = game;
			newUnit.GetComponent<UnitSim> ().setCombatType (combatType);
			newUnit.GetComponent<UnitSim> ().factionColour = attacker.factionColour;

			newUnit.GetComponent<UnitSim> ().setIsMounted (attacker.isMounted ());

			if (combatType == "Melee") {
				if (attacker.isMelee ()) {
					newUnit.GetComponent<UnitSim> ().setDamage (attacker.getMeleeAttack () / 2);
					newUnit.GetComponent<UnitSim> ().setRange (1);
					newUnit.GetComponent<UnitSim> ().setMeleeWeaponType (attacker.getMeleeWeaponType ());
				} else if (attacker.isRanged ()) {
					newUnit.GetComponent<UnitSim> ().setDamage (attacker.getRangedAttack () / 2);
					newUnit.GetComponent<UnitSim> ().setRange (attacker.getWeaponRange());
					newUnit.GetComponent<UnitSim> ().setRangedWeaponType (attacker.getRangedWeaponType ());
				}
			} else if (combatType == "Ranged") {
				newUnit.GetComponent<UnitSim> ().setDamage(attacker.getRangedAttack ()/2);
				newUnit.GetComponent<UnitSim> ().setRange(attacker.getWeaponRange());
				newUnit.GetComponent<UnitSim> ().setRangedWeaponType (attacker.getRangedWeaponType ());
			}
			newUnit.GetComponent<UnitSim>().canAttack = true;

			newUnit.GetComponent<UnitSim>().setDefense(attacker.getDefense ()/2);
			newUnit.GetComponent<UnitSim>().setSpeed(attacker.getSpeed ());

			//newUnit.transform.GetChild (0).gameObject.GetComponent<Renderer>().material.shader = attacker.getShaderOutline();
			newUnit.GetComponent<UnitSim>().animator = newUnit.GetComponent<Animator>();
			newUnit.GetComponent<UnitSim> ().otherAllies = initialAttackerTroopCount;

			AttackingUnits.Add (newUnit);
		}

		for (int i = 0; i < initialDefenderTroopCount; i++) {
			GameObject newUnit = Instantiate (defender.getSimPrefab()) as GameObject;
			newUnit.GetComponent<UnitSim> ().setHealth(defender.getHealthPerUnit ());
			newUnit.GetComponent<UnitSim> ().setUnitSide (UnitSim.UnitSide.Defender);
			newUnit.GetComponent<UnitSim> ().combatManager = this;
			newUnit.GetComponent<UnitSim> ().game = game;
			newUnit.GetComponent<UnitSim> ().setCombatType (combatType);
			newUnit.GetComponent<UnitSim> ().factionColour = defender.factionColour;

			newUnit.GetComponent<UnitSim> ().setIsMounted (defender.isMounted ());

			//setting damage and whether we can attack
			if (combatType == "Melee" && defender.isMelee ()) {
				newUnit.GetComponent<UnitSim> ().setDamage(defender.getMeleeAttack ()/2);
				newUnit.GetComponent<UnitSim> ().setRange(1);
				newUnit.GetComponent<UnitSim> ().setMeleeWeaponType (defender.getMeleeWeaponType ());
				newUnit.GetComponent<UnitSim> ().canAttack = true;
			} else if (combatType == "Melee" && defender.isRanged ()) {
				newUnit.GetComponent<UnitSim> ().setDamage(defender.getRangedAttack ()/2);
				newUnit.GetComponent<UnitSim> ().setRange(defender.getWeaponRange());
				newUnit.GetComponent<UnitSim> ().setRangedWeaponType (defender.getRangedWeaponType ());
				newUnit.GetComponent<UnitSim> ().canAttack = true;
			} else if(combatType == "Ranged" && defender.isRanged ()) {
				if (defender.getWeaponRange () >= attacker.getWeaponRange ()) {
					newUnit.GetComponent<UnitSim> ().setDamage(defender.getRangedAttack ()/2);
					newUnit.GetComponent<UnitSim> ().setRange(defender.getWeaponRange());
					newUnit.GetComponent<UnitSim> ().setRangedWeaponType (defender.getRangedWeaponType ());
					newUnit.GetComponent<UnitSim> ().canAttack = true;
				}
			} else {
				newUnit.GetComponent<UnitSim> ().canAttack = false;
			}

			newUnit.GetComponent<UnitSim>().setDefense(defender.getDefense ()/2);
			newUnit.GetComponent<UnitSim>().setSpeed(defender.getSpeed ());

			//newUnit.transform.GetChild (0).gameObject.GetComponent<Renderer> ().material.shader = defender.getShaderOutline();
			newUnit.GetComponent<UnitSim>().animator = newUnit.GetComponent<Animator>();
			newUnit.GetComponent<UnitSim> ().otherAllies = initialDefenderTroopCount;

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

		case "Melee": 
			if (attacker.faction == UnitManager.Faction.Player) {
				AttackerPos = new Vector3 (planeX, planeY - 0.405f, planeZ - 8);
				DefenderPos = new Vector3 (planeX, planeY - 0.405f, planeZ + 8);
			} else {
				AttackerPos = new Vector3 (planeX, planeY - 0.405f, planeZ + 8);
				DefenderPos = new Vector3 (planeX, planeY - 0.405f, planeZ - 8);
			}
			SpawnUnitsAt (AttackerPos, attacker.faction, DefenderPos, defender.faction);

			break;
		case "Ranged":
			if (attacker.faction == UnitManager.Faction.Player) {
				AttackerPos = new Vector3 (planeX, planeY - 0.405f, planeZ - 8);
				DefenderPos = new Vector3 (planeX, planeY - 0.405f, planeZ + 8);
			} else { 
				AttackerPos = new Vector3 (planeX, planeY - 0.405f, planeZ + 8);
				DefenderPos = new Vector3 (planeX, planeY - 0.405f, planeZ - 8);
			}
			SpawnUnitsAt (AttackerPos, attacker.faction, DefenderPos, defender.faction);
			break;
		default:
			break;
		}



			

		StartSimulation ();
	}

	void StartSimulation(){

		if (camHolder == null) {
			try {
				camHolder = GameObject.Find ("CameraHolder").gameObject;
			} catch {
				if (!camHolder) {
					camHolder = game.simPlane.gameObject;
				}
			}
		}
		game.camManager.CameraSimulation.GetComponent<CameraControl> ().FollowTarget (camHolder);
		Vector3 newLoc = game.camManager.CameraSimulation.transform.position;
		newLoc.x = game.simPlane.transform.position.x;
		game.camManager.CameraSimulation.transform.position = newLoc;

		elapsedTime = 0.0f;
		simState = SimulationState.Intro;
		AudioManager.instance.SwitchBGMTo (AudioManager.bgmSongVersion.Rage);
		AudioManager.instance.PlaySFX ("StartCombatSFX");
		simulationRunning = true;
	}

	void Simulate(){
		if (elapsedTime > introSimulationTime && simState == SimulationState.Intro) {
			//Debug.Log ("Started");
			foreach (GameObject unit in AttackingUnits) {
				unit.GetComponent<UnitSim> ().StartSimDelayed ();
			}

			foreach (GameObject unit in DefendingUnits) {
				unit.GetComponent<UnitSim> ().StartSimDelayed ();
			}

			simState = SimulationState.Playing;
			//Debug.Log ("Sim: Starting");
		}

		if (attackerDeaths >= requiredAttackerDeaths) {
			foreach (GameObject unit in AttackingUnits) {
				unit.GetComponent<UnitSim> ().becomeInvuln ();
			}
		}
		if (defenderDeaths >= requiredDefenderDeaths) {
			foreach (GameObject unit in DefendingUnits) {
				unit.GetComponent<UnitSim> ().becomeInvuln ();
			}
		}

		//if entirety of oen side is wiped out
		if (attackerDeaths >= AttackingUnits.Count || defenderDeaths >= DefendingUnits.Count){
			if (simState == SimulationState.Playing && elapsedTime > minSimulationTime) {
				simState = SimulationState.Ending;
				elapsedTime = endSimulationTime - 2.0f;
			}
		}

		if (attackerDeaths >= requiredAttackerDeaths && defenderDeaths >= requiredDefenderDeaths 
			&& simState == SimulationState.Playing  && elapsedTime > minSimulationTime) {

			simState = SimulationState.Ending;
			elapsedTime = endSimulationTime - 2.0f;
		}
		if (elapsedTime >= maxSimulationTime && simState == SimulationState.Playing) {
			simState = SimulationState.Ending;
			elapsedTime = endSimulationTime - 2.0f;
		}

		if (elapsedTime >= endSimulationTime && simState == SimulationState.Ending) {
			StopSimulation ();
		}

		elapsedTime += Time.deltaTime;
	}

	void StopSimulation(){
		foreach (GameObject unit in AttackingUnits)
			unit.GetComponent<UnitSim> ().StopSim ();
		foreach (GameObject unit in DefendingUnits) 
			unit.GetComponent<UnitSim> ().StopSim ();
		
		simulationRunning = false;

		//checks the actual game itself for killed units
		game.unit.ScanForDeadUnits ();

		game.click.canClick = true;
		if (game.turn.getCurrentTurn () != UnitManager.Faction.Player && !game.AI.AIStopFlagged) {
			game.AI.InvokeSetCanClick (2.0f);
		}

		game.camManager.CameraSimulation.GetComponent<CameraControl> ().StopFollow ();
		game.camManager.CameraSimulation.GetComponent<CameraControl> ().ResetHeight ();
		game.camManager.CameraSimulation.GetComponent<CameraControl> ().ResetRotation ();
		game.camManager.setCameraState (CameraManager.CameraState.Strategy);


		//attacker.getAnimator ().SetTrigger ("Attack");
		attacker.GetComponent<Unit> ().setState (Unit.State.Done);
        //defender.DelayAnimation("TakeDamage", 0.5f);
		//defender.getAnimator ().SetTrigger ("TakeDamage");
		if(game.turn.getCurrentTurn() == UnitManager.Faction.Player)
			game.unit.checkPlayerEndTurn ();


		game.audioManager.SwitchBGMTo (AudioManager.bgmSongVersion.Stream);
		if (!skipSimulation) {
			AudioManager.instance.PlaySFX ("EndCombatSFX");
		}

		ResetSimulation ();

	}

	void SpawnUnitsAt (Vector3 attackingPos, UnitManager.Faction attackerFaction, 
		Vector3 defendingPos, UnitManager.Faction defenderFaction){
		//spawning attackers, 
		SpawnFormation(attackingPos, AttackingUnits, attackerFaction);
		SpawnFormation(defendingPos, DefendingUnits, defenderFaction);

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
		camHolder = null;

		AttackingUnits.Clear();
		DefendingUnits.Clear ();
		simState = SimulationState.Ready;
	}

	public void addDeath(UnitSim.UnitSide side){
		if (side == UnitSim.UnitSide.Attacker) {
			attackerDeaths++;
		}
        if (side == UnitSim.UnitSide.Defender) {
			defenderDeaths++;
		}
	}

    //used by unitsim to prevent going over
    public bool RequestDeathPermission(UnitSim.UnitSide side)
    {
        if (side == UnitSim.UnitSide.Attacker)
        {
            if (attackerDeaths <= requiredAttackerDeaths)
                return true;
        }
        if (side == UnitSim.UnitSide.Defender)
        {
            if (defenderDeaths <= requiredDefenderDeaths)
                return true;
        }
        return false;
    }

	void SpawnFormation(Vector3 start, List<GameObject> unitList, UnitManager.Faction faction){
		//start at 499.5
		int mirror = 1;
		if (faction == UnitManager.Faction.Enemy)
			mirror = -1;



		//0.6 for melee
		int numPerLine = 8;
		float disHorizontal = 0.55f; //between columns
		float disVertical = 0.55f; //between rows
		if (unitList [0].GetComponent<UnitSim> ().isMounted ()) {
			disHorizontal = 0.6f;
			disVertical = 0.85f;
			numPerLine = 6;
		}
		int lines = Mathf.FloorToInt(unitList.Count/numPerLine);
		int unitsSpawned = 0;

		//spawn in lines
		float xStartPos = start.x - (numPerLine / 2) * disHorizontal * mirror + (disHorizontal/2)* mirror;
		for (int x = 0; x < lines; x++) {
			for (int y = 0; y < numPerLine; y++) {
				unitList [unitsSpawned].transform.position = 
					new Vector3 (xStartPos + disHorizontal*y * mirror, start.y, start.z - x*disVertical*mirror);
				unitsSpawned++;
				if (faction != UnitManager.Faction.Enemy && x == 0 && y == 0) {
					camHolder = unitList [unitsSpawned].gameObject;
				}
					
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

	public int CalculateDamage(Unit AttackingUnit, Unit DefendingUnit, string combatType){
		int totalPool = 0; //expertise added together in melee
		if (combatType == "Melee") {//for melee, roll against each other
			if (AttackingUnit.isMelee ()) {
				totalPool = DefendingUnit.getMeleeExpertise () + AttackingUnit.getMeleeExpertise ();
			}else if (AttackingUnit.isRanged ()) {
				totalPool = DefendingUnit.getMeleeExpertise () + AttackingUnit.getRangedExpertise () / 2;
			}
		} else if (combatType == "Ranged") { //for ranged, roll against 100 for chance
			totalPool = AttackingUnit.getRangedExpertise ();
			if (DefendingUnit.isShielded ()) {
				totalPool -= 25; //-25% chance to hit if shielded
			}
		} else {
			return 0;
		}


		int successfulHits = 0; //amount of hits
		int totalDamage = 0; //damage done this roll
		int rand;

		for (int i = 0; i < AttackingUnit.getUnitSize (); i++) {
			
			if (combatType == "Melee") {
				rand = Random.Range (0, totalPool + 1); //choose random number in the pool
				//total pool looks like |___DEFENSE___|___ATTACK___|
				if (rand > DefendingUnit.getMeleeExpertise ()) { //if random number overcomes them
					successfulHits++;
				}
				if(AttackingUnit.isMelee())
					totalDamage = successfulHits * Mathf.Max(AttackingUnit.getMeleeAttack () - DefendingUnit.getDefense (), 1);
				else if(AttackingUnit.isRanged())
					totalDamage = successfulHits * Mathf.Max(AttackingUnit.getRangedAttack () - DefendingUnit.getDefense (), 1);
			}

			else if (combatType == "Ranged") {
				rand = Random.Range (0, 100 + 1); //rolls against 100 chance
				if (rand < AttackingUnit.getRangedExpertise ()) {
					successfulHits++;
				}
				totalDamage = successfulHits * Mathf.Max(AttackingUnit.getRangedAttack () - DefendingUnit.getDefense (), 1);
			}

		}

		//modifiers
		if (AttackingUnit.isMounted () && DefendingUnit.getMeleeWeaponType () != Unit.MeleeWeaponType.Spear)
			totalDamage *= 2;
		if (AttackingUnit.isMounted () && DefendingUnit.getMeleeWeaponType () == Unit.MeleeWeaponType.Spear)
			totalDamage /= 3;

		if (DefendingUnit.isMounted () && AttackingUnit.getMeleeWeaponType () == Unit.MeleeWeaponType.Spear)
			totalDamage *= 3;
		if (DefendingUnit.isArmoured ()) {
			if (AttackingUnit.getMeleeWeaponType () == Unit.MeleeWeaponType.Mace || AttackingUnit.getRangedWeaponType () == Unit.RangedWeaponType.Crossbow)
				totalDamage *= 2;
		}


		//Debug.Log ("Successful Hits: " + successfulHits + ". Damage = " + totalDamage);
		return totalDamage;
	}



	public void setSkippingSimulation(bool b){
		skipSimulation = b;
	}

	public bool isSkippingSimulation(){
		return skipSimulation;
	}

	public float randTimer(){
		float rand = Random.value;
		return rand;
	}
}
