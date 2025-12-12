using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public abstract class BaseAbilityPower : MonoBehaviour
{
	protected abstract int GetBaseAttack ();
	protected abstract int GetBaseDefense (GeneralUnit target);
	protected abstract int GetPower ();
	void OnEnable ()
	{
		this.AddObserver(OnGetBaseAttack, BaseAbilityEffect.GetAttackNotification);
		this.AddObserver(OnGetBaseDefense, BaseAbilityEffect.GetDefenseNotification);
		this.AddObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
	}
	void OnDisable ()
	{
		this.RemoveObserver(OnGetBaseAttack, BaseAbilityEffect.GetAttackNotification);
		this.RemoveObserver(OnGetBaseDefense, BaseAbilityEffect.GetDefenseNotification);
		this.RemoveObserver(OnGetPower, BaseAbilityEffect.GetPowerNotification);
	}
	void OnGetBaseAttack (object sender, object args)
	{
		if (IsMyEffect(sender))
		{
			var info = args as Info<GeneralUnit, GeneralUnit, List<ValueModifier>>;
			info.arg2.Add( new AddValueModifier(0, GetBaseAttack()) );
		}
	}
	void OnGetBaseDefense (object sender, object args)
	{
		if (IsMyEffect(sender))
		{
			var info = args as Info<GeneralUnit, GeneralUnit, List<ValueModifier>>;
			info.arg2.Add( new AddValueModifier(0, GetBaseDefense(info.arg1)) );
		}
	}
	void OnGetPower (object sender, object args)
	{
		if (IsMyEffect(sender))
		{
			var info = args as Info<GeneralUnit, GeneralUnit, List<ValueModifier>>;
			info.arg2.Add( new AddValueModifier(0, GetPower()) );
		}
	}
    bool IsMyEffect (object sender)
	{
		MonoBehaviour obj = sender as MonoBehaviour;
		return (obj != null && obj.transform.parent == transform);
	}
}
// 이 코드는 다음과 같은 목적을 위해 사용됩니다.
// 데미지 계산을 위한 기본 공격력, 방어력 및 힘을 제공합니다.