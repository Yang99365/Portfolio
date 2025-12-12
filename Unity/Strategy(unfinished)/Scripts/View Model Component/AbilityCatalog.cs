using UnityEngine;
using System.Collections;
/// <요약>
/// 모든 직계 자식이 카테고리라고 가정합니다.
/// 그리고 카테고리의 직계 자식은
/// 능력입니다
/// </summary>
public class AbilityCatalog : MonoBehaviour 
{
	public int CategoryCount ()
	{
		return transform.childCount;
	}
	public GameObject GetCategory (int index)
	{
		if (index < 0 || index >= transform.childCount)
			return null;
		return transform.GetChild(index).gameObject;
	}
	public int AbilityCount (GameObject category)
	{
		return category != null ? category.transform.childCount : 0;
	}
	public Ability GetAbility (int categoryIndex, int abilityIndex)
	{
		GameObject category = GetCategory(categoryIndex);
		if (category == null || abilityIndex < 0 || abilityIndex >= category.transform.childCount)
			return null;
		return category.transform.GetChild(abilityIndex).GetComponent<Ability>();
	}
}