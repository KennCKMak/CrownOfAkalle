using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {

	private GameManager game;
	public GameObject canvas;

    public GameObject panelLeft;
    public GameObject panelRight;
    public GameObject pausePanel;
	public GameObject helpPanel;
	public enum HelpTextState { None, ChooseUnit, ChooseMove, ChooseAction };
	public HelpTextState helpTextState = HelpTextState.ChooseUnit;
	public bool helpTextEnabled = true;

    public GameObject pauseButton;
	public GameObject endTurnButton;

	public Text textCurrentTurn;
	public Text textUnitStats;
	public Text textHoveredUnitStats;
	public Text textHelp;
	public Text textSimToggle;

	public GameObject panelMiddle;
	public Text textPanelMiddle;
	// Use this for initialization
	void Start () {
		game = GetComponent<GameManager> ();

		canvas = transform.FindChild ("Canvas").gameObject;
		canvas.transform.SetParent(GameObject.Find("CameraStrategy").gameObject.transform);

		updateText ();

	}

   
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape) && GameManager.running) {
			if (game.click.selectedUnit) {
				game.click.DeselectUnit ();
				return;
			}
			if (game.camManager.getCameraState () == CameraManager.CameraState.Simulation)
				return;
			PauseButton ();
		}
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

				words = "Unit: <b>" + unit.gameObject.name + "</b>";

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
		UpdateHelpText ();
		textCurrentTurn.text = "Turn: " + game.turn.getCurrentTurnString ();

	}

	public void SwitchHelpTextState(HelpTextState state){
		helpTextState = state;
		UpdateHelpText ();
	}

	public void UpdateHelpText(){
		helpPanel.SetActive (helpTextEnabled);
		string text = "";
		switch (helpTextState) {
		case HelpTextState.None:
			break;
		case HelpTextState.ChooseUnit:
			text = "Contol the Camera with WASD & QE \n" +
			"Zoom in and out using the mouse wheel \n\n" +
			"Left-click on a blue soldier to select it \n" +
			"End your turn with the End Turn button";

			break;
		case HelpTextState.ChooseMove:
			text = "\n\nLeft-Click on a blue tile to move your unit \n\n" + "Right-click to deselect your unit";
			break;
		case HelpTextState.ChooseAction:	
			text = "Left-Click on a red tile to attack that unit \n" +
				"Left-Click on your unit again to move the unit without attacking \n\n" +
				"Right-Click to cancel and rechoose movement tile";
			break;
		default:
			break;
		}
		textHelp.text = text;
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

    public void PauseButton()
    {
        game.Pause();
        pausePanel.SetActive(GameManager.paused);
		pauseButton.SetActive(!GameManager.paused);
		SetEndTurnButton (!GameManager.paused);
    }

	public void SetPauseButton(bool b){
		pauseButton.SetActive (b);
	}

    public void SwitchLevel(string s)
    {
        try
        {
            SceneManager.LoadScene(s, LoadSceneMode.Single);
			Time.timeScale = 1.0f;
			if(s == SceneManager.GetActiveScene().name){
				AudioManager.instance.StopBGM();
				AudioManager.instance.PlayBGM("E Pluribus Unum", AudioManager.bgmSongVersion.Stream);
			}
			GameManager.paused = false;
			GameManager.running = true;
        }catch
        {
            Debug.LogWarning("Failed to load scene " + s);
        }
    }

	public void ToggleHelp(){
		helpTextEnabled = !helpTextEnabled;
		SetHelpText (helpTextEnabled);
	}

	public void SetHelpText(bool b){
		helpPanel.SetActive(b);
		UpdateHelpText ();
	}

	public void ToggleBGM(){
		game.audioManager.ToggleMuteBGM ();
	}

	public void ToggleSFX(){
		game.audioManager.ToggleMuteSFX ();
	}

	public void ToggleSimulation(){
		if (game.combat.isSkippingSimulation ()) {
			game.combat.setSkippingSimulation (false);
			textSimToggle.text = "Simulation:OFF";
		}else{
			game.combat.setSkippingSimulation (true);
			textSimToggle.text = "Simulation:ON";
		}
	}

	public void SetEndTurnButton(bool b){
		endTurnButton.SetActive (b);
	}

	public void SetMiddlePanel(bool b){
		panelMiddle.SetActive (b);
	}

	public void SetMiddlePanel(bool b, string s){
		panelMiddle.SetActive (b);
		textPanelMiddle.text = s;
	}
}
