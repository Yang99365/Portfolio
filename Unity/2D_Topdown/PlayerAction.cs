using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAction : MonoBehaviour
{
    public GameManager GM;
    float h;
    float v;
    public float speed;
    bool isHorizonMove; //대각선제한
    Vector3 dirVec;

    GameObject scanObject;

    Rigidbody2D rigid;
    Animator anim;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        h = GM.isAction ? 0 : Input.GetAxisRaw("Horizontal");
        v = GM.isAction ? 0 : Input.GetAxisRaw("Vertical");

        bool hDown = GM.isAction ? false : Input.GetButtonDown("Horizontal");
        bool vDown = GM.isAction ? false : Input.GetButtonDown("Vertical");
        bool hUp = GM.isAction ? false : Input.GetButtonUp("Horizontal");
        bool vUp = GM.isAction ? false : Input.GetButtonUp("Vertical");
        //GM.isAction ? false : 액션중엔 움직임 제한

        //수평이동 체크

        if (hDown)  // 오른쪽으로 가다 위로가면 오른쪽눌러있는상태여도 안갈때 방지
            isHorizonMove = true;
        else if(vDown)
            isHorizonMove=false;
        else if(hUp || vUp)
            isHorizonMove= h != 0;

        //Anim
        if(anim.GetInteger("hAxisRaw") != h)
        {
            anim.SetBool("IsChange",true);
            anim.SetInteger("hAxisRaw", (int)h);
        }
        else if (anim.GetInteger("vAxisRaw") != v)
        {
            anim.SetBool("IsChange", true);
            anim.SetInteger("vAxisRaw", (int)v);
        }
        else
        {
            anim.SetBool("IsChange", false);
        }

        //Direction
        if (vDown && v == 1)// 윗키 눌럿을때
        {
            dirVec = Vector3.up;
        }
        else if (vDown && v == -1)
            dirVec = Vector3.down;
        else if (hDown && h == 1) // 오른쪽
            dirVec = Vector3.right;
        else if (hDown && h == -1) //왼쪽
            dirVec = Vector3.left;

        //Scan Obj
        if(Input.GetButtonDown("Jump")&&scanObject !=null)
        {
            GM.Action(scanObject);
        }

    }
    void FixedUpdate()
    {
        Vector2 moveVec = isHorizonMove ? new Vector2(h,0): new Vector2(0,v);
        rigid.velocity = moveVec *speed;

        //Ray
        Debug.DrawRay(rigid.position,dirVec * 0.7f, new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, dirVec, 0.7f, LayerMask.GetMask("Object"));

        if(rayHit.collider != null)
        {
            scanObject = rayHit.collider.gameObject; //힛 한 오브젝트 담기
        }
        else { scanObject = null; }
    
    }
}
