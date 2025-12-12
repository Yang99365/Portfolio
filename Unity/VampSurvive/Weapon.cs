using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    float timer;
    Player player;

    void Awake()
    {
        player = GameManager.instance.player;
        
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        switch (id)
        {
            case 0:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;
                if(timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }

        //..Test Code..
        if(Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }
    }
    public void LevelUp(float damage, int count)
    {
        this.damage = damage * Character.Damage;
        this.count += count;

        if (id == 0)
            Batch();

        player.BroadcastMessage("ApplyGear",SendMessageOptions.DontRequireReceiver); //이미 생성된 무기도 Gear를 적용하기
    }

    public void Init(ItemData data)
    {
        // Basic Set
        name = "Weapon " + data.itemId;
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero; //플레이어 안에서 로컬포지션 0,0,0
        // Property Set
        id = data.itemId;
        damage = data.baseDamage * Character.Damage;
        count = data.baseCount + Character.Count;

        for (int index =0; index < GameManager.instance.pool.prefabs.Length; index++)
        {
            if(data.projectile == GameManager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }           

        }

        switch (id)
        {
            case 0:
                speed = 150 * Character.WeaponSpeed;
                Batch();
                break;
            default:
                speed = 0.5f*Character.WeaponRate;
                break;
        }
        // Hand Set
        Hand hand = player.hands[(int)data.itemType]; //근접은0 원거리는1가져옴
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver); //이미 생성된 무기도 Gear를 적용하기
    }


    void Batch()
    {
        for (int index=0; index<count; index++)
        {
            Transform bullet;
                
            if(index<transform.childCount) //childCount 자식오브젝트 개수확인
            {
                bullet = transform.GetChild(index); //기존오브젝트 먼저 활용하고
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;//모자라면 풀링으로 가져오기
                bullet.parent = transform; //부모를 풀매니저에서 플레이어weapon0으로
                //레벨업시 index가 childCount를 넘기에 else문으로 들어가서 풀링으로 가져옴
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity; //플레이어 위치로 초기화

            Vector3 rotVec = Vector3.forward * 360 * index / count; //4개면 0,360*1/4,,360*2/4,360*3/4
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero); // -100 은 근거리
        }
    }

    void Fire()
    {
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}
