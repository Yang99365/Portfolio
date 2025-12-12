using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDopingEft", menuName = "ItemEffect/ItemDopingEft")]
public class ItemDopingEft : ItemEffect
{
    [SerializeField] private int _dopingValue = 0;
    public override bool ExecuteRole()
    {
        // 장군의 특정 스텟을 도핑벨류만큼 올린다.
        Debug.Log("도핑효과를 받았습니다.");

        //Player.instance.IncreaseHealth(_dopingValue);//임시로 만든 효과

        
        return true;
    }
    
}
