using UnityEngine;
using System.Collections;
public class DefeatAllEnemiesVictoryCondition : BaseVictoryCondition 
{
	protected override void CheckForGameOver ()
	{
		base.CheckForGameOver();
		if (Victor == Alliances.None && PartyDefeated(Alliances.Enemy))
			Victor = Alliances.Hero;
	}
}
// 이 스크립트는 적군이 중립군이 승리가 아니고 적군이 모두 격파되었을 때 영웅이 승리하는 승리 조건을 나타낸다.