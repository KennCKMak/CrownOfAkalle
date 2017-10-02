using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private MapManager mapManager;
	private UnitManager unitManager;
	private CombatManager combatManager;
	private ClickManager clickManager;

	// Use this for initialization
	void Start () {
		mapManager = GetComponent<MapManager> ();
		unitManager = GetComponent<UnitManager> ();
		combatManager = GetComponent<CombatManager> ();
		clickManager = GetComponent<ClickManager> ();
	}



	// Update is called once per frame
	void Awake(){


	}


	void Update () {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {

			unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8, 3, UnitManager.Faction.Ally);
			unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8, 7, UnitManager.Faction.Enemy);
			/*
			for(int i = 0; i < 4; i++){
				unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8 + i, 3, UnitManager.Faction.Ally);
				unitManager.CreateUnit (UnitManager.UnitName.SwordsmanUnit, 8 + i, 17, UnitManager.Faction.Ally);
			}*/
		}

		if (Input.GetKeyDown (KeyCode.Space)) {
			unitManager.RestoreMovement (UnitManager.Faction.Ally);
		}
	}
}
