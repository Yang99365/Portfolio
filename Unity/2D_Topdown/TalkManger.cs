using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class TalkManger : MonoBehaviour
{
    Dictionary<int, string[]> talkData;
    Dictionary<int, Sprite> portraitData;

    public Sprite[] portraitArr;

    void Awake()
    {
        talkData = new Dictionary<int, string[]>();
        portraitData = new Dictionary<int, Sprite>();
        GenerateDate();
    }

    // Update is called once per frame
    void GenerateDate()
    {
        talkData.Add(2000, new string[] {"안녕?:0", "여기 처음왔니?:1"});
        talkData.Add(1000, new string[] { "안녕2?:0", "여기 처음왔니2?:1"});
        talkData.Add(100, new string[] { "평범한 나무상자다." });

        //Quest Talk
        talkData.Add(10+ 1000, new string[] { "어서와.:0",
                                                "이 마을에 놀라운 전설이 있대:1",
                                                 "오른쪽 호수 쪽에 루도가 알려줄꺼야.:0"});
        talkData.Add(11 + 2000, new string[] { "여어.:1",
                                                "호수 전설을 들으러 왔어?:0",
                                                 "부탁 하나좀 들어줘 내 집근처에서 동전을 주워줘.:1"});


        talkData.Add(20 + 1000, new string[] { "루도의 동전?:1",
        "돈을 흘리고 다니면 못쓰지!:3",
        "나중에 루도에게 한마디 해야곘어.:3",});
        talkData.Add(20 + 2000, new string[] { "찾으면 꼭 좀 가져다 줘.:1",});
        talkData.Add(20 + 5000, new string[] {"근처에서 동전을 찾았다.", });

        talkData.Add(21 + 2000, new string[] { "오, 찾아줘서 고마워.:2", });

        portraitData.Add(2000 + 0, portraitArr[0]);
        portraitData.Add(2000 + 1, portraitArr[1]);
        portraitData.Add(2000 + 2, portraitArr[2]);
        portraitData.Add(2000 + 3, portraitArr[3]);
        portraitData.Add(1000 + 0, portraitArr[4]);
        portraitData.Add(1000 + 1, portraitArr[5]);
        portraitData.Add(1000 + 2, portraitArr[6]);
        portraitData.Add(1000 + 3, portraitArr[7]);
    }

    public string GetTalk(int id, int talkIndex) //대사 반환
    {
        //예외처리
        if(!talkData.ContainsKey(id))
        {
            if (!talkData.ContainsKey(id - id % 10))
            {
                //퀘스트 맨 처음 대사마저 없을 떄
                //기본 대사를 가지고 온다.

                return GetTalk(id-id%100, talkIndex); // get first talk
            }
            else
            {
                //해당 퀘스트 진행 순서 중 대사가 없을때
                //퀘스트 맨 처음대사를 가지고 온다

                return GetTalk(id- id % 10, talkIndex); // get first quest talk

            }


        }

        if(talkIndex == talkData[id].Length)
        {
            return null;
        }
        else
        {
            return talkData[id][talkIndex];
        }
            

    }
    public Sprite GetPortrait(int id, int portraitIndex)
    {
        return portraitData[id + portraitIndex];
    }
}
