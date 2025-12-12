using UnityEngine;
using System.Collections;
public class BlindStatusEffect : StatusEffect
{
	void OnEnable ()
	{
		this.AddObserver( OnHitRateStatusCheck, HitRate.StatusCheckNotification );
	}
	
	void OnDisable ()
	{
		this.RemoveObserver( OnHitRateStatusCheck, HitRate.StatusCheckNotification );
	}
	void OnHitRateStatusCheck (object sender, object args)
	{
		Info<GeneralUnit, GeneralUnit, int> info = args as Info<GeneralUnit, GeneralUnit, int>;
		GeneralUnit owner = GetComponentInParent<GeneralUnit>();
		if (owner == info.arg0)
		{
			// 공격자는 눈이 멀었습니다.
			info.arg2 += 50;
		}
		else if (owner == info.arg1)
		{
			// 방어자는 눈이 멀었습니다.
			info.arg2 -= 20;
		}
	}
}