using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	
	public MapManager mapManager;
	public UnitManager unitManager;
	public CombatManager combatManager;
	public ClickManager clickManager;
	public CameraManager cameraManager;
	public TurnManager turnManager;
	public UIManager uiManager;


	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();
		unitManager = GetComponent<UnitManager> ();
		combatManager = GetComponent<CombatManager> ();
		clickManager = GetComponent<ClickManager> ();
		cameraManager = GetComponent<CameraManager> ();
		turnManager = GetComponent<TurnManager> ();
		uiManager = GetComponent<UIManager> ();
	}



	// Update is called once per frame
	void Awake(){


	}


	void Update () {
		if (Input.GetMouseButtonDown (1))
			clickManager.Deselect ();
		/*
		if (Input.GetKeyDown (KeyCode.Alpha1)) {

			unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8, 3, UnitManager.Faction.Player);
			unitManager.CreateUnit (UnitManager.UnitName.ArcherUnit, 9, 3, UnitManager.Faction.Player);


			unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8, 7, UnitManager.Faction.Enemy);
			unitManager.CreateUnit (UnitManager.UnitName.ArcherUnit, 9, 7, UnitManager.Faction.Enemy);

			for(int i = 0; i < 4; i++){
				unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8 + i, 3, UnitManager.Faction.Ally);
				unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8 + i, 17, UnitManager.Faction.Ally);
			}
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			unitManager.DeleteAllUnits ();
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			unitManager.DeleteAllFactionUnits (UnitManager.Faction.Player);
		}




		if (Input.GetKeyDown (KeyCode.Space)) {
			unitManager.RestoreMovement (UnitManager.Faction.Player);
		}
	*/

	}
}
