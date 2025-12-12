using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float speed;

    public int startIndex;
    public int endIndex;
    public Transform[] sprites;

    float viewHeight;

    // Update is called once per frame

    private void Awake()
    {
        viewHeight = Camera.main.orthographicSize * 2; //view높이
    }
    void Update()
    {
        //Move
        Vector3 curPos = transform.position;
        Vector3 nextPos = Vector3.down * speed * Time.deltaTime;
        transform.position = curPos + nextPos;


        //background scrolling
        if (sprites[endIndex].position.y < viewHeight*(-1))  //-10이하로 내려가면
        {
            //Sprite reUse
            Vector3 backSpritePos = sprites[startIndex].localPosition; //맨위 스프라이트의 위치 가져오기
            Vector3 frontSpritePos = sprites[endIndex].localPosition; //맨아래 위치 가져오기
            sprites[endIndex].transform.localPosition = backSpritePos + Vector3.up * viewHeight;
            //-10 이하로 내려간 endindex스프라이트의 지역위치를 맨위 스프라이트의 지역위치에 +10을 해준다.

            //Cursor Indexs Change 인덱스돌리기
            int startIndexSave = startIndex; //맨위 스프라이트의 인덱스를 저장
            startIndex = endIndex; //위치 옮겼으니 맨아래 스프라이트를 맨위 스프라이트로
            endIndex = (startIndexSave - 1 == -1) ? sprites.Length - 1 : startIndexSave - 1;
            //시작인덱스에 1뺀 값이 -1이면 index가 2 아니면 1빼준다.

        }

    }

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Background")
        {
            if (collision.gameObject.transform.GetSiblingIndex() == startIndex)
            {
                sprites[endIndex].position = new Vector3(0, sprites[startIndex].position.y + 10, 0);
                sprites[startIndex].SetSiblingIndex(endIndex);
                int temp = startIndex;
                startIndex = endIndex;
                endIndex = temp;
            }
        }
    }

}
