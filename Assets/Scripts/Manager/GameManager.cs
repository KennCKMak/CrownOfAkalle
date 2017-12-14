using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	[HideInInspector]
	public MapManager map;
	[HideInInspector]
	public UnitManager unit;
	[HideInInspector]
	public CombatManager combat;
	[HideInInspector]
	public ClickManager click;
	[HideInInspector]
	public CameraManager camManager;
	[HideInInspector]
	public TurnManager turn;
	[HideInInspector]
	public UIManager ui;

	[HideInInspector]
	public AudioManager audioManager;


	// Use this for initialization
	void Awake () {
		map = GetComponent<MapManager> ();
		unit = GetComponent<UnitManager> ();
		combat = GetComponent<CombatManager> ();
		click = GetComponent<ClickManager> ();
		camManager = GetComponent<CameraManager> ();
		turn = GetComponent<TurnManager> ();
		ui = GetComponent<UIManager> ();

		audioManager = FindObjectOfType<AudioManager> ();
	}



	void Update () {

        if (Input.GetMouseButtonDown(1))
            click.DeselectUnit();

        if (Input.GetKeyDown(KeyCode.Escape))
            click.DeselectUnit();


        if (Input.GetKeyDown (KeyCode.Alpha2)) 
			unit.DeleteAllUnits ();
		
		if (Input.GetKeyDown (KeyCode.Alpha3)) 
			unit.DeleteAllFactionUnits (UnitManager.Faction.Player);

		if (Input.GetKeyDown (KeyCode.Alpha4)) 
			unit.DeleteAllFactionUnits (UnitManager.Faction.Enemy);

		if (Input.GetKeyDown (KeyCode.Space)) 
			unit.RestoreMovement (UnitManager.Faction.Player);

        if (Input.GetKeyDown(KeyCode.M))
            audioManager.SwitchBGM();

    }
}
