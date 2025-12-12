using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class EndBattleState : BattleState 
{
    // This class is responsible for ending the battle and returning to the main menu
	public override void Enter ()
	{
		base.Enter ();
        SceneManager.LoadScene(0);
		//Application.LoadLevel(0);

	}
}