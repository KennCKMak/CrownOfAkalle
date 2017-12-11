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

		if (Input.GetKeyDown (KeyCode.Alpha2)) 
			unitManager.DeleteAllUnits ();
		
		if (Input.GetKeyDown (KeyCode.Alpha3)) 
			unitManager.DeleteAllFactionUnits (UnitManager.Faction.Player);

		if (Input.GetKeyDown (KeyCode.Alpha4)) 
			unitManager.DeleteAllFactionUnits (UnitManager.Faction.Enemy);

		if (Input.GetKeyDown (KeyCode.Space)) 
			unitManager.RestoreMovement (UnitManager.Faction.Player);
		

	}
}
