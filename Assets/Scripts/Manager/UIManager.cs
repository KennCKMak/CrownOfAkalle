using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	private GameManager game;
	public GameObject canvas;

    public GameObject panelLeft;
    public GameObject panelRight;
	public Text textCurrentTurn;
	public Text textUnitStats;
	public Text textHoveredUnitStats;

	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();

		canvas = transform.FindChild ("Canvas").gameObject;
		canvas.transform.SetParent(GameObject.Find("CameraStrategy").gameObject.transform);

        panelLeft = canvas.transform.FindChild("PanelLeft").gameObject;
        panelRight = canvas.transform.FindChild("PanelRight").gameObject;
		textCurrentTurn = canvas.transform.FindChild ("textCurrentTurn").gameObject.GetComponent<Text> ();
		textUnitStats = panelLeft.transform.FindChild ("textUnitStats").gameObject.GetComponent<Text> ();
		textHoveredUnitStats = panelRight.transform.FindChild ("textHoveredUnitStats").gameObject.GetComponent<Text> ();
		updateText ();
	}


	
	// Update is called once per frame
	void Update () {

	}

	public void SpawnUnits(){
		//game.unit.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 9, 3, UnitManager.Faction.Player);
		game.unit.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 10, 3, UnitManager.Faction.Player);
		game.unit.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 11, 3, UnitManager.Faction.Player);
		game.unit.CreateUnit (UnitManager.UnitName.ArcherUnit, 10, 2, UnitManager.Faction.Player);
		game.unit.CreateUnit (UnitManager.UnitName.KnightUnit, 12, 3, UnitManager.Faction.Player);

		game.unit.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 10, 7, UnitManager.Faction.Enemy);
		game.unit.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 11, 7, UnitManager.Faction.Enemy);
		//game.unit.CreateUnit (UnitManager.UnitName.ArcherUnit, 10, 8, UnitManager.Faction.Enemy);
		game.unit.CreateUnit (UnitManager.UnitName.ArcherUnit, 11, 8, UnitManager.Faction.Enemy);
		game.unit.CreateUnit (UnitManager.UnitName.SpearmanUnit, 12, 7, UnitManager.Faction.Enemy);
	}

	public void DestroyAllUnits(){
		game.unit.DeleteAllUnits();
	}

	public void DestroyPlayerUnits(){
		game.unit.DeleteAllFactionUnits (UnitManager.Faction.Player);
	}

	public void DestroyAllyUnits(){
		game.unit.DeleteAllFactionUnits (UnitManager.Faction.Ally);
	}

	public void DestroyEnemyUnits(){
		game.unit.DeleteAllFactionUnits (UnitManager.Faction.Enemy);
	}

	public void EndTurn(){
        game.turn.switchTurn (0.0f);
		game.click.Deselect ();
		textCurrentTurn.GetComponent<Text> ().text = "Turn: " + game.turn.getCurrentTurn ().ToString ();
	}

	public void UpdateUnitStatsText(){
        if (game.click.selectedUnit != null)
        {
            panelLeft.SetActive(true);
            Unit unit = game.click.selectedUnit.GetComponent<Unit>();

            string words;

            words = "Unit: " + unit.gameObject.name;
            words.Remove(words.Length - 7 + 6);

            words += "\nHealth: " + unit.getHealth() + "/" + unit.getMaxHealth() + "\n";
            words += "Unit Size: " + unit.getUnitSize() + "/" + unit.getMaxUnitSize() + "\n";
            if (unit.getMeleeWeaponType() != Unit.MeleeWeaponType.None)
            {
                words += "Melee Weapon: " + unit.getMeleeWeaponType().ToString() + "\n";
                words += "Melee Expertise: " + unit.getMeleeExpertise() + "\n";
                words += "Melee Damage: " + unit.getMeleeAttack() + "\n";
            }
            if (unit.getRangedWeaponType() != Unit.RangedWeaponType.None)
            {
                words += "Ranged Weapon: " + unit.getRangedWeaponType().ToString() + "\n";
                words += "Ranged Expertise: " + unit.getRangedExpertise() + "\n";
                words += "Ranged Damage: " + unit.getRangedAttack() + "\n";
            }
            words += "Defense: " + unit.getDefense() + "\n";
            words += "Move Speed: " + unit.getSpeed() + "\n";
            textUnitStats.text = words;
        }
        else
        {
            textUnitStats.text = "";

            panelLeft.SetActive(false);
        }
	}

	public void UpdateHoveredUnitStatsText(){
		textHoveredUnitStats.text = "";
		if (game.click.hoveredUnit != null) {
			if (game.click.hoveredUnit != game.click.selectedUnit) {

                panelRight.SetActive(true);
                Unit unit = game.click.hoveredUnit.GetComponent<Unit> ();

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
                words += "Move Speed: " + unit.getSpeed() + "\n";
                textHoveredUnitStats.text = words;
                return;
			}
		}

        panelRight.SetActive(false);
    }


	public void updateText(){
		UpdateUnitStatsText ();
		UpdateHoveredUnitStatsText ();
		textCurrentTurn.text = game.turn.getCurrentTurnString ();

	}

	public void setSimulation(){
		if (game.combat.isSkippingSimulation ()) {
			game.combat.setSkippingSimulation (false);
			canvas.transform.FindChild ("btnSimulate").transform.GetChild (0).GetComponent<Text> ().text = 
				"Simulation On";
		} else {
			game.combat.setSkippingSimulation (true);
			canvas.transform.FindChild ("btnSimulate").transform.GetChild (0).GetComponent<Text> ().text = 
				"Simulation Off";
		}
	}
}
