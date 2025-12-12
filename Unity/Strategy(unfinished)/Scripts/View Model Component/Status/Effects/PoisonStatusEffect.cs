using UnityEngine;
using System.Collections;
public class PoisonStatusEffect : StatusEffect 
{
    // 원하는대로 수정하라곤 하는데..
    // 이 코드는 일단 턴이 시작될 때마다 10%의 체력을 깎는다.
	GeneralUnit owner;
	void OnEnable ()
	{
		owner = GetComponentInParent<GeneralUnit>();
		if (owner)
			this.AddObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
	}
	void OnDisable ()
	{
		this.RemoveObserver(OnNewTurn, TurnOrderController.TurnBeganNotification, owner);
	}
	void OnNewTurn (object sender, object args)
	{
		Stats s = GetComponentInParent<Stats>();
		int currentHP = s[StatTypes.HP];
		int maxHP = s[StatTypes.MHP];
		int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
		s.SetValue(StatTypes.HP, (currentHP - reduce), false);
	}
}