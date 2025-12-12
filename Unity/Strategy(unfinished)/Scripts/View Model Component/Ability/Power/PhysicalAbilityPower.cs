using UnityEngine;
using System.Collections;
public class PhysicalAbilityPower : BaseAbilityPower 
{
	public int level;
	
	protected override int GetBaseAttack ()
	{
		return GetComponentInParent<Stats>()[StatTypes.ATK];
	}
	protected override int GetBaseDefense (GeneralUnit target)
	{
		return target.GetComponent<Stats>()[StatTypes.DEF];
	}
	
    // 유닛은 공격 능력치가 변경되지 않았더라도 
    //전력 "레벨" 덕분에 서로 다른 양의 피해를 입힐 수 있는 여러 능력을 가질 수 있습니다.
	protected override int GetPower () 
	{
		return level;
	}
}
