using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class EsunaAbilityEffect : BaseAbilityEffect 
{
	static HashSet<Type> CurableTypes
	{
		get
		{
			if (_curableTypes == null)
			{
				_curableTypes = new HashSet<Type>();
				_curableTypes.Add( typeof(PoisonStatusEffect) );
				_curableTypes.Add( typeof(BlindStatusEffect) );
			}
			return _curableTypes;
		}
	}
	static HashSet<Type> _curableTypes;
	public override int Predict (Tile target)
	{
		return 0;
	}
	protected override int OnApply (Tile target)
	{
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		Status status = defender.GetComponentInChildren<Status>();
		DurationStatusCondition[] candidates = status.GetComponentsInChildren<DurationStatusCondition>();
		for (int i = candidates.Length - 1; i >= 0; --i)
		{
			StatusEffect effect = candidates[i].GetComponentInParent<StatusEffect>();
			if ( CurableTypes.Contains( effect.GetType() ))
				candidates[i].Remove();
		}
		return 0;
	}
}
// 이 스크립트는 상태이상을 해제하는 능력 효과를 정의합니다. 
//이 효과는 대상의 상태를 확인하고, 그 중 독이나 실명과 같은 상태이상을 제거합니다.