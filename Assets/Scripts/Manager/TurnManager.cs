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
        StartCoroutine(swapTurns(1.5f));
	}

    public void switchTurn(float f)
    {
        StartCoroutine(swapTurns(f));
    }


    protected IEnumerator swapTurns(float num){
        yield return new WaitForSeconds(num);
        switch (currentTurn)
        {
            case UnitManager.Faction.Player:
                game.unit.RestoreAllMovement();
                game.AI.AIStart(UnitManager.Faction.Enemy, UnitManager.Faction.Player);
                currentTurn = UnitManager.Faction.Enemy;
                break;
            case UnitManager.Faction.Enemy:
                game.unit.RestoreAllMovement();
                game.AI.AIStop();
                currentTurn = UnitManager.Faction.Player;
                break;
            default:
                break;
        }
        game.ui.updateText();
    }

	public UnitManager.Faction getCurrentTurn(){
		return currentTurn;
	}

	public string getCurrentTurnString(){
		return currentTurn.ToString ();
	}

}
