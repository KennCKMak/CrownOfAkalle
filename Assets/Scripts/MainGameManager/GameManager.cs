﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	[HideInInspector] public MapManager map;
	[HideInInspector] public UnitManager unit;
	[HideInInspector] public CombatManager combat;
	[HideInInspector] public ClickManager click;
	[HideInInspector] public CameraManager camManager;
	[HideInInspector] public TurnManager turn;
	[HideInInspector] public UIManager ui;
    [HideInInspector] public AIManager AI;
    public List<Unit> tempUnitsList = new List<Unit>();


	[HideInInspector] public AudioManager audioManager;

    public bool paused;


	// Use this for initialization
	void Awake () {
		map = GetComponent<MapManager> ();
		unit = GetComponent<UnitManager> ();
		combat = GetComponent<CombatManager> ();
		click = GetComponent<ClickManager> ();
		camManager = GetComponent<CameraManager> ();
		turn = GetComponent<TurnManager> ();
		ui = GetComponent<UIManager> ();
        AI = GetComponent<AIManager>();
		
	}

    void Start()
    {
        Time.timeScale = 1.0f;

        //Invoke("SpawnUnits", 0.5f);
    }

    void SpawnUnits()
    {
        ui.SpawnUnits();
    }

    void Update() {
        

        if (Input.GetMouseButtonDown(1))
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

    public void Pause()
    {
        if (paused)
        {
            StopPause();

        }
        else
            StartPause();
        paused = !paused;


    }
   
    public void StartPause()
    {
        Time.timeScale = 0.0f;
    }

    public void StopPause()
    {
        Time.timeScale = 1.0f;
    }

}