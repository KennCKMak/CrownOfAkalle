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
		Debug.Log ("Called");
        yield return new WaitForSeconds(num);
        switch (currentTurn)
        {
        case UnitManager.Faction.Player:
            game.unit.RestoreAllMovement();
			game.AI.AIStart(UnitManager.Faction.Enemy, UnitManager.Faction.Player);
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
            currentTurn = UnitManager.Faction.Enemy;
            break;
        case UnitManager.Faction.Enemy:
            game.unit.RestoreAllMovement();
			game.AI.AIStop();
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseMove);
			currentTurn = UnitManager.Faction.Player;
			//currentTurn = UnitManager.Faction.Ally;
			//game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
			break;
		case UnitManager.Faction.Ally:
			game.unit.RestoreAllMovement ();
			game.AI.AIStop ();
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
			currentTurn = UnitManager.Faction.Neutral;
			break;
		case UnitManager.Faction.Neutral:
			game.unit.RestoreAllMovement ();
			game.AI.AIStop ();
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseMove);
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
