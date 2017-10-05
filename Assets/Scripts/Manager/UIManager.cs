﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	private GameManager game;
	public GameObject canvas;


	public Text textCurrentTurn;
	public Text textUnitStats;
	public Text textHoveredUnitStats;

	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();

		canvas = transform.FindChild ("Canvas").gameObject;
		canvas.transform.parent = transform.FindChild ("CameraStrategy").transform;

		textCurrentTurn = canvas.transform.FindChild ("textCurrentTurn").gameObject.GetComponent<Text> ();
		textUnitStats = canvas.transform.FindChild ("textUnitStats").gameObject.GetComponent<Text> ();
		textHoveredUnitStats = canvas.transform.FindChild ("textHoveredUnitStats").gameObject.GetComponent<Text> ();
		updateText ();
	}


	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {


		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			game.unitManager.DeleteAllUnits ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			game.unitManager.DeleteAllFactionUnits (UnitManager.Faction.Player);
		}




		if (Input.GetKeyDown (KeyCode.Space)) {
			game.unitManager.RestoreMovement (UnitManager.Faction.Player);
		}	
	}

	public void SpawnUnits(){
		game.unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 9, 3, UnitManager.Faction.Player);
		game.unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 10, 3, UnitManager.Faction.Player);
		game.unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 11, 3, UnitManager.Faction.Player);
		game.unitManager.CreateUnit (UnitManager.UnitName.ArcherUnit, 10, 2, UnitManager.Faction.Player);
		game.unitManager.CreateUnit (UnitManager.UnitName.KnightUnit, 12, 3, UnitManager.Faction.Player);

		game.unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 10, 7, UnitManager.Faction.Enemy);
		game.unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 11, 7, UnitManager.Faction.Enemy);
		game.unitManager.CreateUnit (UnitManager.UnitName.ArcherUnit, 10, 8, UnitManager.Faction.Enemy);
		game.unitManager.CreateUnit (UnitManager.UnitName.ArcherUnit, 11, 8, UnitManager.Faction.Enemy);
		game.unitManager.CreateUnit (UnitManager.UnitName.SpearmanUnit, 12, 7, UnitManager.Faction.Enemy);
	}

	public void DestroyAllUnits(){
		game.unitManager.DeleteAllUnits();
	}

	public void DestroyPlayerUnits(){
		game.unitManager.DeleteAllFactionUnits (UnitManager.Faction.Player);
	}

	public void DestroyAllyUnits(){
		game.unitManager.DeleteAllFactionUnits (UnitManager.Faction.Ally);
	}

	public void DestroyEnemyUnits(){
		game.unitManager.DeleteAllFactionUnits (UnitManager.Faction.Enemy);
	}

	public void EndTurn(){
		game.turnManager.switchTurn ();
		game.clickManager.Deselect ();
		textCurrentTurn.GetComponent<Text> ().text = "Turn: " + game.turnManager.getCurrentTurn ().ToString ();
	}

	public void UpdateUnitStatsText(){
		if (game.clickManager.selectedUnit != null) {
			Unit unit = game.clickManager.selectedUnit.GetComponent<Unit> ();
		
			string words;

			words = "Unit: " + unit.gameObject.name;
			words.Remove (words.Length - 7 + 6);

			words += "\nHealth: " + unit.getHealth () + "/" + unit.getMaxHealth () + "\n";
			words += "Unit Size: " + unit.getUnitSize () + "/" + unit.getMaxUnitSize () + "\n";
			if (unit.getMeleeWeaponType () != Unit.MeleeWeaponType.None) {
				words += "Melee Weapon: " + unit.getMeleeWeaponType ().ToString () + "\n";
				words += "Melee Expertise: " + unit.getMeleeExpertise () + "\n";
				words += "Melee Damage: " + unit.getMeleeAttack () + "\n";
			}
			if (unit.getRangedWeaponType () != Unit.RangedWeaponType.None) {
				words += "Ranged Weapon: " + unit.getRangedWeaponType ().ToString () + "\n";
				words += "Ranged Expertise: " + unit.getRangedExpertise () + "\n";
				words += "Ranged Attack: " + unit.getRangedAttack () + "\n";
			}
			words += "Defense: " + unit.getDefense () + "\n";
			textUnitStats.text = words;
		} else
			textUnitStats.text = "";
	}

	public void UpdateHoveredUnitStatsText(){
		textHoveredUnitStats.text = "";
		if (game.clickManager.hoveredUnit != null) {
			if (game.clickManager.hoveredUnit != game.clickManager.selectedUnit) {
				Unit unit = game.clickManager.hoveredUnit.GetComponent<Unit> ();

				string words;

				words = "Unit: " + unit.gameObject.name;
				words.Remove (words.Length - 7 + 6);

				words += "\nHealth: " + unit.getHealth () + "/" + unit.getMaxHealth () + "\n";
				words += "Unit Size: " + unit.getUnitSize () + "/" + unit.getMaxUnitSize () + "\n";
				if (unit.getMeleeWeaponType () != Unit.MeleeWeaponType.None) {
					words += "Melee Weapon: " + unit.getMeleeWeaponType ().ToString () + "\n";
					words += "Melee Expertise: " + unit.getMeleeExpertise () + "\n";
					words += "Melee Damage: " + unit.getMeleeAttack () + "\n";
				}
				if (unit.getRangedWeaponType () != Unit.RangedWeaponType.None) {
					words += "Ranged Weapon: " + unit.getRangedWeaponType ().ToString () + "\n";
					words += "Ranged Expertise: " + unit.getRangedExpertise () + "\n";
					words += "Ranged Attack: " + unit.getRangedAttack () + "\n";
				}
				words += "Defense: " + unit.getDefense () + "\n";
				textHoveredUnitStats.text = words;
			}
		}
	}


	public void updateText(){
		UpdateUnitStatsText ();
		UpdateHoveredUnitStatsText ();
		textCurrentTurn.text = game.turnManager.getCurrentTurnString ();

	}

	public void setSimulation(){
		if (game.combatManager.isSkippingSimulation ()) {
			game.combatManager.setSkippingSimulation (false);
			canvas.transform.FindChild ("btnSimulate").transform.GetChild (0).GetComponent<Text> ().text = 
				"Simulation On";
		} else {
			game.combatManager.setSkippingSimulation (true);
			canvas.transform.FindChild ("btnSimulate").transform.GetChild (0).GetComponent<Text> ().text = 
				"Simulation Off";
		}
	}
}