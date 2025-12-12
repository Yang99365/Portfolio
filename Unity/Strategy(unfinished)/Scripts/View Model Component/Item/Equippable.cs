using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Equippable : MonoBehaviour 
{
	#region Fields
	/// <summary>
	/// 기본값인 EquipSlots 플래그
	/// 이 아이템의 위치를 ​​장착합니다.
	/// 예를 들어 일반 무기는 다음을 지정합니다.
	/// 기본이지만 양손 무기는 지정합니다.
	/// 기본 및 보조 모두.
	/// </summary>
	public EquipSlots defaultSlots;
	/// <summary>
	/// 일부 장비는 장착이 허용될 수 있습니다.
	/// 둘 이상의 슬롯 위치(예:
	/// 이중 휘두르는 검.
	/// </summary>
	public EquipSlots secondarySlots;
	/// <summary>
	/// 현재 아이템이 장착되어 있는 슬롯
	/// </summary>
	public EquipSlots slots;
	bool _isEquipped;
	#endregion
	#region Public
	public void OnEquip ()
	{
		if (_isEquipped)
			return;
		_isEquipped = true;
		Feature[] features = GetComponentsInChildren<Feature>();
		for (int i = 0; i < features.Length; ++i)
			features[i].Activate(gameObject);
	}
	public void OnUnEquip ()
	{
		if (!_isEquipped)
			return;
		_isEquipped = false;
		Feature[] features = GetComponentsInChildren<Feature>();
		for (int i = 0; i < features.Length; ++i)
			features[i].Deactivate();
	}
	#endregion
}