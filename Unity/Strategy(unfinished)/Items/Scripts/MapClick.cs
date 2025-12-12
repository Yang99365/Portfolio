using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class MapClick : MonoBehaviour
{
    // 이벤트 트리거, 클릭시 RegionManager의 OnClickRegion 함수를 호출한다.
    public void OnClick()
    {
        RegionManager.instance.OnClickRegion();
    }

}
