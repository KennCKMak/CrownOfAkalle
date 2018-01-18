using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour {
	public float timeScale = 1.0f;
	private float oldTimeScale = 0.0f;
	[HideInInspector] public MapManager map;
	[HideInInspector] public UnitManager unit;
	[HideInInspector] public CombatManager combat;
	[HideInInspector] public ClickManager click;
	[HideInInspector] public CameraManager camManager;
	[HideInInspector] public TurnManager turn;
	[HideInInspector] public UIManager ui;
    [HideInInspector] public AIManager AI;
	[HideInInspector] public SimulationPlane simPlane;


	[HideInInspector] public AudioManager audioManager;

    public static bool paused;
	public static bool running = true;

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
		if(!audioManager)
			audioManager = AudioManager.instance;
	}

    void Start()
    {
		running = true;
       // Time.timeScale = 20.0f;

        //Invoke("SpawnUnits", 0.5f);
    }


    void Update() {
		if (oldTimeScale != timeScale) {
			oldTimeScale = timeScale;
			Time.timeScale = timeScale;
		}
		//Time.timeScale = timeScale;

        if (Input.GetMouseButtonDown(1))
            click.DeselectUnit();
		/*
        if (Input.GetKeyDown (KeyCode.Alpha2)) 
			unit.DeleteAllUnits ();
		
		if (Input.GetKeyDown (KeyCode.Alpha3)) 
			unit.DeleteAllFactionUnits (UnitManager.Faction.Player);

		if (Input.GetKeyDown (KeyCode.Alpha4)) 
			unit.DeleteAllFactionUnits (UnitManager.Faction.Enemy);

		if (Input.GetKeyDown (KeyCode.Space)) 
			unit.RestoreMovement (UnitManager.Faction.Player);

        if (Input.GetKeyDown(KeyCode.M))
            audioManager.SwitchBGM();*/

            
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


	public void RequestSFX(string s){
		audioManager.PlaySFX (s);
	}
}
