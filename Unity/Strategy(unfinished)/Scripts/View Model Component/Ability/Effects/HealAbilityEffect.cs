using UnityEngine;
using System.Collections;
public class HealAbilityEffect : BaseAbilityEffect 
{
	public override int Predict (Tile target)
	{
		GeneralUnit attacker = GetComponentInParent<GeneralUnit>();
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		return GetStat(attacker, defender, GetPowerNotification, 0);
	}
	protected override int OnApply (Tile target)
	{
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		
		// 예측값으로 시작
		int value = Predict(target);
		
		// 임의의 분산을 추가합니다.
		value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));
		
		// 금액을 범위로 고정
		value = Mathf.Clamp(value, minDamage, maxDamage);
		
		// 대상에 금액을 적용합니다.
		Stats s = defender.GetComponent<Stats>();
		s[StatTypes.HP] += value;
		return value;
	}
}