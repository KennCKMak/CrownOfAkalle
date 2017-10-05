using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

	public UnitManager.Faction currentTurn;
	private GameManager game;

	void Start () {
		game = GetComponent<GameManager> ();
		currentTurn = UnitManager.Faction.Player;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void switchTurn(){
		switch (currentTurn){
		case UnitManager.Faction.Player:
			setTurn (UnitManager.Faction.Enemy);
			game.unitManager.RestoreMovement (UnitManager.Faction.Enemy);
			break;
		case UnitManager.Faction.Enemy:
			setTurn (UnitManager.Faction.Player);
			game.unitManager.RestoreMovement (UnitManager.Faction.Player);
			break;
		default:
			break;
		}
			


		game.uiManager.updateText ();
	}

	public void setTurn(UnitManager.Faction newTurn){
		currentTurn = newTurn;
	}

	public UnitManager.Faction getCurrentTurn(){
		return currentTurn;
	}

	public string getCurrentTurnString(){
		return currentTurn.ToString ();
	}

}
