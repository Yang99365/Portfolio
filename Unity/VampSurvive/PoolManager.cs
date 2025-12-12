using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // .. 프리펩 보관할 변수
    public GameObject[] prefabs;

    // .. 풀 담당을 하는 리스트들
    List<GameObject>[] pools;

    // .. 풀의 초기화를 담당하는 함수
    void Awake()
    {
        pools = new List<GameObject>[prefabs.Length]; //배열 초기화
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<GameObject>(); //배열 속 리스트 초기화
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;
        
        // ... 선택한 풀의 놀고 (비활성화 된) 있는 게임오브젝트 접근 
        foreach (GameObject item in pools[index]) // 배열, 리스트들의 데이터를 순차적으로 접근하는 반복문
        {
            if(!item.activeSelf)
            {
                // ... 발견하면 select 변수에 할당
                select = item;
                select.SetActive(true);
                break;
            }
        }   
        // ... 못찾았으면?
        if(!select)
        {
            // ... 새롭게 생성하고 select 변수에 할당
            select = Instantiate(prefabs[index], transform);
            pools[index].Add(select);
        }

        return select;
    }
}
