using UnityEngine;
using System.Collections;
public class FacingIndicator : MonoBehaviour 
{
	[SerializeField] Renderer[] directions;
	[SerializeField] Material normal;
	[SerializeField] Material selected;
	
	public void SetDirection (Directions dir)
	{
		int index = (int)dir;
		for (int i = 0; i < 4; ++i)
			directions[i].material = (i == index) ? selected : normal;
	}
}
// 이 스크립트는 캐릭터의 방향을 나타내는 표시를 제어합니다.