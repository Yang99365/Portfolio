using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public int questId;
    public int questActionIndex;
    public GameObject[] questObject;

    Dictionary<int, QuestData> questList;

    // Start is called before the first frame update
    void Awake()
    {
        questList = new Dictionary<int, QuestData>();
        GenerateData();
    }

    // Update is called once per frame
    void GenerateData()
    {
        questList.Add(10,new QuestData("마을 사람들과 대화하기"
                                        ,new int[] { 1000,2000 }));
        questList.Add(20, new QuestData("루도의 동전 찾아주기"
                                        , new int[] { 5000, 2000 }));
        questList.Add(30, new QuestData("퀘스트 클리어!"
                                        , new int[] { 0 }));
    }

    public int GetQuestTalkIndex(int id) //npc 번호를 받고 퀘스트 번호 반환하는 함수
    {
        return questId + questActionIndex;
    }
    public string CheckQuest(int id)
    {
        
        if (id == questList[questId].npcId[questActionIndex])
        {
            questActionIndex++;
        }
        // 순서에 맞게 대화했을때만 대화순서 상승

        ControlObject();

        if (questActionIndex == questList[questId].npcId.Length) // 퀘스트 대화순서가 끝에 도달시 번호증가
        {
            NextQuest();
        }

        return questList[questId].questName;
    }

    public string CheckQuest()
    {
        return questList[questId].questName;
    }

    void NextQuest()
    {
        questId += 10;
        questActionIndex = 0;
    }

    public void ControlObject()
    {
        switch(questId)
        {
            case 10:
                if(questActionIndex == 2)
                {
                    questObject[0].SetActive(true);
                }
                break;

            case 20:
                if(questActionIndex == 0)
                    questObject[0].SetActive(true);
                else if (questActionIndex == 1)
                {
                    questObject[0].SetActive(false);
                }
                break;
            
        }
    }
}
