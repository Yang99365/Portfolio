using UnityEngine;
using System.Collections;
public class DefeatTargetVictoryCondition : BaseVictoryCondition 
{
	public GeneralUnit target;
	
	protected override void CheckForGameOver ()
	{
		base.CheckForGameOver ();
		if (Victor == Alliances.None && IsDefeated(target))
			Victor = Alliances.Hero;
	}
}
// 이 스크립트는 특정 유닛이 격파되었을 때 영웅이 승리하는 승리 조건을 나타낸다.