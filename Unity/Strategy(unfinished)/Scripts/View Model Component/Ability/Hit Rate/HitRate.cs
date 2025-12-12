using UnityEngine;
using System.Collections;
public abstract class HitRate : MonoBehaviour 
{
	#region Notifications
	/// <summary>
	/// 기본값이 false인 토글 가능한 MatchException 인수를 포함합니다.
	/// </summary>
	public const string AutomaticHitCheckNotification = "HitRate.AutomaticHitCheckNotification";
	/// <summary>
	/// 기본값이 false인 토글 가능한 MatchException 인수를 포함합니다.
	/// </summary>
	public const string AutomaticMissCheckNotification = "HitRate.AutomaticMissCheckNotification";
	/// <summary>
	/// 세 가지 매개변수가 있는 Info 인수를 포함합니다: Attacker(유닛), Defender(유닛),
	/// 및 방어자의 계산된 회피/저항(int)입니다. 적중률을 수정하는 상태 효과
	/// arg2 매개변수를 수정해야 합니다.
	/// </summary>
	public const string StatusCheckNotification = "HitRate.StatusCheckNotification";
	#endregion

	#region Fields
	public virtual bool IsAngleBased { get { return true; }}
	protected GeneralUnit attacker;
	#endregion
	#region MonoBehaviour
	protected virtual void Start ()
	{
		attacker = GetComponentInParent<GeneralUnit>();
	}

	#endregion
	#region Public
	/// <summary>
	/// 0~100 범위의 값을 확률 백분율로 반환합니다.
	/// 적중 성공 능력
	/// </summary>
	public abstract int Calculate (Tile target); // 타일 위 유닛에 대한 적중률을 계산합니다.
	// 빈공간에 함정을 설치하는 등의 경우에는 적중률을 계산할 필요가 없습니다. (미구현)

	public virtual bool RollForHit (Tile target)
	{
		int roll = UnityEngine.Random.Range(0, 101);
		int chance = Calculate(target);
		return roll <= chance;
	}
	#endregion

	

	#region Protected
	protected virtual bool AutomaticHit (GeneralUnit target)
	{
		MatchException exc = new MatchException(attacker, target);
		this.PostNotification(AutomaticHitCheckNotification, exc);
		return exc.toggle;
	}
	protected virtual bool AutomaticMiss (GeneralUnit target)
	{
		MatchException exc = new MatchException(attacker, target);
		this.PostNotification(AutomaticMissCheckNotification, exc);
		return exc.toggle;
	}
	protected virtual int AdjustForStatusEffects (GeneralUnit target, int rate)
	{
		Info<GeneralUnit, GeneralUnit, int> args = new Info<GeneralUnit, GeneralUnit, int>(attacker, target, rate);
		this.PostNotification(StatusCheckNotification, args);
		return args.arg2;
	}
	protected virtual int Final (int evade)
	{
		return 100 - evade;
	}
	#endregion
}
