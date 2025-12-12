using UnityEngine;
using System.Collections;
public class ATypeHitRate : HitRate 
{
	// 회피율에 따른 적중률을 계산합니다.
	public override int Calculate (Tile target)
	{
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		if (AutomaticHit(defender))
		    return Final(0);

		if (AutomaticMiss(defender))
			return Final(100);

		int evade = GetEvade(defender);
		evade = AdjustForRelativeFacing(defender, evade);
		evade = AdjustForStatusEffects(defender, evade);
		evade = Mathf.Clamp(evade, 5, 95);
		return Final(evade);
	}
	int GetEvade (GeneralUnit target)
	{
		Stats s = target.GetComponentInParent<Stats>();
		return Mathf.Clamp(s[StatTypes.EVD], 0, 100);
	}
	int AdjustForRelativeFacing (GeneralUnit target, int rate)
	{
		switch (attacker.GetFacing(target))
		{
		case Facings.Front:
			return rate;
		case Facings.Side:
			return rate / 2;
		default:
			return rate / 4;
		}
	}
}

