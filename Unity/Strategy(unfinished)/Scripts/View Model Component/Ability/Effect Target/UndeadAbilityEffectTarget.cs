using UnityEngine;
using System.Collections;
public class UndeadAbilityEffectTarget : AbilityEffectTarget 
{
	/// <요약>
	/// Undead 구성 요소가 있어야 하는지 여부를 나타냅니다(true).
	/// 대상이 유효하려면 존재하지 않아야 합니다(false).
	/// </summary>
	public bool toggle;
	public override bool IsTarget (Tile tile)
	{
		if (tile == null || tile.content == null)
			return false;
		bool hasComponent = tile.content.GetComponent<Undead>() != null;
		if (hasComponent != toggle)
			return false;
		
		Stats s = tile.content.GetComponent<Stats>();
		return s != null && s[StatTypes.HP] > 0;
	}
}