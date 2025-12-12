using UnityEngine;
using System.Collections;
public static class FacingsExtensions
{
	public static Facings GetFacing (this GeneralUnit attacker, GeneralUnit target)
	{
        // 이 코드는 두 유닛의 방향을 비교하여 공격자가 어느 방향을 향하고 있는지를 판단합니다.
		Vector2 targetDirection = target.dir.GetNormal();
		Vector2 approachDirection = ((Vector2)(target.tile.pos - attacker.tile.pos)).normalized;
		float dot = Vector2.Dot( approachDirection, targetDirection );
		if (dot >= 0.45f)
			return Facings.Back;
		if (dot <= -0.45f)
			return Facings.Front;
		return Facings.Side;
        
	}
}