using UnityEngine;
using System.Collections;
public class Driver : MonoBehaviour 
{
	public Drivers normal;
	public Drivers special;
	
	public Drivers Current
	{
		get
		{
			return special != Drivers.None ? special : normal; 
		}
	}
}

/*
일반' 플래그는 유닛이 처음에 로드된 방식을 나타내며, 
'특수' 플래그는 상태 이상 등으로 인해 기본 동작이 재정의되었음을 나타낼 수 있습니다. 
아직 이러한 종류의 기능을 구현하지는 않았지만 처리 방법에 대한 힌트로 코드를 넣었습니다.
*/