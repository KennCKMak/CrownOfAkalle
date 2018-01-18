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
		checkEndTurn ();
		if (!GameManager.running) {
			game.ui.SetEndTurnButton (false);
			game.ui.SetPauseButton (false);
			yield break;
		}


		yield return new WaitForSeconds(num);

        switch (currentTurn)
        {
		case UnitManager.Faction.Player:
			game.AI.AIStart (UnitManager.Faction.Enemy, UnitManager.Faction.Player);
			game.unit.RestoreAllMovement ();

			game.ui.SwitchHelpTextState (UIManager.HelpTextState.None);
			game.ui.SetEndTurnButton (false);

			currentTurn = UnitManager.Faction.Enemy;
			AudioManager.instance.PlaySFX ("OtherTurnSFX");
            break;


		case UnitManager.Faction.Enemy:
			game.AI.AIStop();
            game.unit.RestoreAllMovement();

			game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseUnit);
			//game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
			game.ui.SetEndTurnButton (true);

			currentTurn = UnitManager.Faction.Player;
			AudioManager.instance.PlaySFX ("PlayerTurnSFX");
			//currentTurn = UnitManager.Faction.Ally;
			//AudioManager.instance.PlaySFX ("OtherTurnSFX");
			break;


		case UnitManager.Faction.Ally:
			game.AI.AIStop ();
			game.unit.RestoreAllMovement ();
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.None);
			currentTurn = UnitManager.Faction.Neutral;
			AudioManager.instance.PlaySFX ("OtherTurnSFX");
			break;


		case UnitManager.Faction.Neutral:
			game.AI.AIStop ();
			game.unit.RestoreAllMovement ();
			game.ui.SwitchHelpTextState(UIManager.HelpTextState.ChooseUnit);
			currentTurn = UnitManager.Faction.Player;
			AudioManager.instance.PlaySFX ("PlayerTurnSFX");
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


	public void checkEndTurn(){
//		Debug.Log ("Checking end turn");
		if (!game.unit.FactionHasUnits (UnitManager.Faction.Player))
			PlayerWasDefeated ();
		else if (!game.unit.FactionHasUnits (UnitManager.Faction.Enemy))
			PlayerWasVictorious ();
			
	}

	public void PlayerWasDefeated(){
		Debug.Log ("Defeated");
		GameManager.paused = true;
		GameManager.running = false;
		game.Pause ();
		game.ui.SetMiddlePanel (true, "Defeat!");
		game.ui.SetEndTurnButton (false);
		game.ui.SetPauseButton (false);
		game.ui.SetHelpText (false);

		AudioManager.instance.PlaySFX ("DefeatSFX");
		AudioManager.instance.PlayBGM ("DefeatBGM");
	}

	public void PlayerWasVictorious(){
		Debug.Log ("Victorious");
		GameManager.paused = true;
		GameManager.running = false;
		game.Pause ();
		game.ui.SetMiddlePanel (true, "Victory!");
		game.ui.SetEndTurnButton (false);
		game.ui.SetPauseButton (false);
		game.ui.SetHelpText (false);

		AudioManager.instance.PlaySFX ("VictorySFX");
		AudioManager.instance.PlayBGM ("VictoryBGM");


	}
}
