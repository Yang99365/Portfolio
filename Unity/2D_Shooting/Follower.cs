using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public float maxShotDelay;
    public float curShotDelay;

    public ObjectManager objectManager;

    public Vector3 followPos;
    public int followDelay;
    public Transform parent;
    public Queue<Vector3> parentPos;


    void Awake()
    {
        parentPos = new Queue<Vector3>();
    }
    void Update()
    {
        Watch();
        Follow();
        Fire();       
        Reload();
    }

    void Watch()
    {
        //input pos
        if(!parentPos.Contains(parent.position)) //부모위치가 가만히있으면 저장안함
        {
            parentPos.Enqueue(parent.position);
        }
        

        //output pos
        if(parentPos.Count > followDelay)
        {
            followPos = parentPos.Dequeue();
        }
        else if (parentPos.Count < followDelay) //시작시 채워지기전
        {
            followPos = parent.position;
        }
    }
    void Follow()
    {
        transform.position = followPos;
    }

    void Fire()
    {
        if (!Input.GetButton("Fire1")) // 좌클릭 아닌 상태면 탈출
            return;

        if (curShotDelay < maxShotDelay) //cur샷이 max샷보다 작은상태면 안쏘고 탈출
            return;

        GameObject bullet = objectManager.MakeObj("BulletFollower");
        bullet.transform.position = transform.position;

        Rigidbody2D rigid = bullet.GetComponent<Rigidbody2D>();
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
        

        curShotDelay = 0;

    }
    void Reload()
    {
        curShotDelay += Time.deltaTime;
    }
}

    
