using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true); //비활성 되어있는 핸드를 true로 하면 가져올수있음
    }

    void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        //inputVec.x = Input.GetAxisRaw("Horizontal"); Pc가 아닌 모바일을 위해 잠시 주석처리
        //inputVec.y = Input.GetAxisRaw("Vertical"); //GetAxis는 부드럽게 Raw는 딱딱끊어이동
    }
    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        // 1. 힘을 주다
        //rigid.AddForce(inputVec);

        // 2. 속도를 제어
        //rigid.velocity = inputVec;

        // 3. 위치를 제어
        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        anim.SetFloat("Speed", inputVec.magnitude);

        if(inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }    
    }
    /*
    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
        //유니티인풋시스템 사용
    }
    */

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;

        GameManager.instance.health -= Time.deltaTime * 10;
        if (GameManager.instance.health < 0)
        {
            for (int index=2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }
    //모바일을 위한 온무브
    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
}
