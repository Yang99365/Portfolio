using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using sound.playerSound;
public class DamageAbilityEffect : BaseAbilityEffect 
{
	
	#region Public
	public override int Predict (Tile target)
	{
		GeneralUnit attacker = GetComponentInParent<GeneralUnit>();
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		// 공격자의 기본 공격 통계를 고려하여 가져옵니다.
		// 미션 아이템, 지원 확인, 상태 확인, 장비 등
		int attack = GetStat(attacker, defender, GetAttackNotification, 0);
		// 고려한 대상의 기본 방어 통계를 가져옵니다.
		// 미션 아이템, 지원 확인, 상태 확인, 장비 등
		int defense = GetStat(attacker, defender, GetDefenseNotification, 0);
		// 기본 데미지 계산
		int damage = attack - (defense / 2);
		damage = Mathf.Max(damage, 1);
		// 가능한 변형을 고려하여 능력치 전력 통계를 가져옵니다.
		int power = GetStat(attacker, defender, GetPowerNotification, 0);
		// 파워 보너스 적용
		damage = power * damage / 100;
		damage = Mathf.Max(damage, 1);
		// 다음과 같은 다양한 검사를 기반으로 피해를 조정합니다.
		// 원소 피해, 치명타, 피해 승수 등.
		damage = GetStat(attacker, defender, TweakDamageNotification, damage);
		// 데미지를 일정 범위로 제한
		damage = Mathf.Clamp(damage, minDamage, maxDamage);
		return -damage;
	}
	
	protected override int OnApply (Tile target)
	{
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		// 예상 손상 값으로 시작
		int value = Predict(target);
		// 임의의 분산을 추가합니다.
		value = Mathf.FloorToInt(value * UnityEngine.Random.Range(0.9f, 1.1f));
		// 데미지를 일정 범위로 제한
		value = Mathf.Clamp(value, minDamage, maxDamage);
		//타겟에 데미지를 적용한다
		Stats s = defender.GetComponent<Stats>();
		_PlayerSound.instance.PlayPlayerSFX(_PlayerSound.PlayerSfx.Hit);
		s[StatTypes.HP] += value;
		return value;
	}
	#endregion
}