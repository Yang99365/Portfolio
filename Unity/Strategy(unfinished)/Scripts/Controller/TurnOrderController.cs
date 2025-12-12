using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class TurnOrderController : MonoBehaviour 
{
	#region Constants
	const int turnActivation = 1000; //턴을 할당받기위한 위한 최소임계값
	const int turnCost = 500; // 차례가 끝나면 CTR을 감소시키는데 필요한 비용
	const int moveCost = 300; // 이동하는데 필요한 비용
	const int actionCost = 200; // 행동하는데 필요한 비용, 넘길시 턴이 빨리 돌아옴
	#endregion
	#region Notifications
	public const string RoundBeganNotification = "TurnOrderController.roundBegan";
	public const string TurnCheckNotification = "TurnOrderController.turnCheck";
	public const string TurnCompletedNotification = "TurnOrderController.turnCompleted";
	public const string TurnBeganNotification = "TurnOrderController.TurnBeganNotification";
	public const string RoundEndedNotification = "TurnOrderController.roundEnded";
	#endregion
	#region Public
	public IEnumerator Round ()
	{
        // Round는 무한루프로 돌아가며, 각 유닛의 CTR을 증가시킨다.
        // CTR이 증가된 후, CTR이 가장 높은 유닛을 찾아서 턴을 넘긴다.
        // 턴을 넘길 때마다 CTR을 감소시킨다.
        // CTR이 0이 되면, 유닛의 턴이 끝난다.
		BattleController bc = GetComponent<BattleController>();;
		while (true)
		{
			this.PostNotification(RoundBeganNotification);
			List<GeneralUnit> units = new List<GeneralUnit>( bc.units );
			for (int i = 0; i < units.Count; ++i)
			{
				Stats s = units[i].GetComponent<Stats>();
				s[StatTypes.CTR] += s[StatTypes.SPD];
			}
            
            // 유닛들을 최종 CTR에 따라 정렬한다.
			units.Sort( (a,b) => GetCounter(a).CompareTo(GetCounter(b)) );
			for (int i = units.Count - 1; i >= 0; --i)
			{
				if (CanTakeTurn(units[i]))
				{
					bc.turn.Change(units[i]);
					units[i].PostNotification(TurnBeganNotification);
					yield return units[i];
					int cost = turnCost;
					if (bc.turn.hasUnitMoved)
						cost += moveCost;
					if (bc.turn.hasUnitActed)
						cost += actionCost;
					Stats s = units[i].GetComponent<Stats>();
					s.SetValue(StatTypes.CTR, s[StatTypes.CTR] - cost, false);
					units[i].PostNotification(TurnCompletedNotification);
				}
			}
			
			this.PostNotification(RoundEndedNotification);
		}
	}
	#endregion
	#region Private
	bool CanTakeTurn (GeneralUnit target)
	{
		/*
		// 선택사항 === 플레이어 턴을 건너뛰고 시청만 할 수 있도록 이 비트를 추가하세요.
		Alliance a = target.GetComponentInChildren<Alliance>();
		if (a.type == Alliances.Hero)
			return false;
		// === 선택사항 종료 
		*/

		BaseException exc = new BaseException( GetCounter(target) >= turnActivation );
		target.PostNotification( TurnCheckNotification, exc );
		return exc.toggle;
	}
	int GetCounter (GeneralUnit target)
	{
		return target.GetComponent<Stats>()[StatTypes.CTR];
	}
	#endregion
}