using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    public float levelTime;
    int level;
    float timer;

    void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
        //최대시간에 몬스터 데이터크기로 나누어 자동으로 몬스터 구간시간 계산
    }
    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1); //소수점 아래 내리고 인트로

        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();  
        }   
    }
    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        //0은 자기자신으로 스포너인 플레이어 위치이므로 1부터해야 자식인 포인트부터 함
        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }
}

//직렬화(개체를 저장,전송을 위해 변환/ 인스펙터창에서 초기화가능
[System.Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}
