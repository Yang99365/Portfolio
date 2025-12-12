using UnityEngine;
using System.Collections;
public class FullTypeHitRate : HitRate 
{
	public override bool IsAngleBased { get { return false; }} // 무조건 맞는 능력이므로 각도에 의한 명중률을 사용하지 않습니다.
	public override int Calculate (Tile target)
	{
		GeneralUnit defender = target.content.GetComponent<GeneralUnit>();
		if (AutomaticMiss(defender))
			return Final(100);
		return Final (0);
	}
}
// 이 스크립트는 능력의 명중률을 계산하는 데 사용됩니다.
// FullTypeHitRate은 능력이 항상 명중하는 것으로 가정합니다.