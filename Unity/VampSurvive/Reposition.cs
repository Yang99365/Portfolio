using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Reposition : MonoBehaviour
{
    Collider2D coll;

    void Awake()
    {
        coll = GetComponent<Collider2D>();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(!collision.CompareTag("Area")) //충돌한 태그가 Area일때만 실행(탈출안함)
            return;

        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 myPos = transform.position;
        
        //input시스템
        //Vector3 playerDir = GameManager.instance.player.inputVec;

        
        switch (transform.tag)
        {
            case "Ground":
                float diffX = playerPos.x - myPos.x;
                float diffY = playerPos.y - myPos.y;
                float dirX = diffX < 0 ? -1 : 1;
                float dirY = diffY < 0 ? -1 : 1;
                diffX = Mathf.Abs(diffX);
                diffY = Mathf.Abs(diffY);
                //두 오브젝트 위치차이를 활용한 로직으로 변경 (16로직보환)

                if (diffX > diffY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                else if (diffX < diffY)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }
                break;
            case "Enemy":
                if(coll.enabled)
                {
                    Vector3 dist = playerPos - myPos;
                    Vector3 ran = new Vector3(Random.Range(-3,3),Random.Range(-3,3),0);
                    transform.Translate(ran + dist * 2); //랜덤벡터를 더하여 퍼져있는 몬스터 재배치
                }

                break;
            
        }
    }
}
