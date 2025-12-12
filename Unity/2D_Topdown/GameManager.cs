using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TypeEffect talk;
    public Animator TalkPanel;
    public GameObject scanObj;
    public TalkManger talkManger;
    public QuestManager questManager;
    public int talkIndex;
    public bool isAction;

    public GameObject player;
    public GameObject menuSet;
    public Image Portrait;
    public Sprite prevPortrait;
    public Animator PortraitAnim;
    public Text QuestText;
    // Update is called once per frame
     void Start()
    {
        GameLoad();
        QuestText.text = questManager.CheckQuest();
    }
     void Update()
    {
        
        if (Input.GetButtonDown("Cancel"))
        {
            //sub menu
            if (menuSet.activeSelf)
            {
                menuSet.SetActive(false);
            }
            else
            {
                menuSet.SetActive(true);
            }
        }
            
        
    }

    public void Action(GameObject scanObject)
    {
        scanObj = scanObject;
        ObjData objData = scanObj.GetComponent<ObjData>();
        Talk(objData.id, objData.isNpc);


        TalkPanel.SetBool("isShow",isAction);

    }
    void Talk(int id, bool isNpc)
    {
        int questTalkeIndex=0;
        string talkData="";
        //Set Talk Data
        if (talk.isAnim)
        {
            talk.SetMsg("");
            return;
        }
            
        else
        {
            questTalkeIndex = questManager.GetQuestTalkIndex(id);
            talkData = talkManger.GetTalk(id + questTalkeIndex, talkIndex);
        }
        
        //End Talk
        if (talkData == null)
        {
            isAction = false;
            talkIndex = 0;
            QuestText.text = questManager.CheckQuest(id);
       
            return; //강제종료역할 아랫줄이 실행x
        }
        //Continue Talk
        if (isNpc)
        {
            talk.SetMsg(talkData.Split(':')[0]);

            //show portrait
            Portrait.sprite = talkManger.GetPortrait(id,int.Parse(talkData.Split(':')[1]));
            Portrait.color = new Color(1, 1, 1, 1);

            //anim portrait
            if (prevPortrait != Portrait.sprite)
            {
                PortraitAnim.SetTrigger("doEffect");
                prevPortrait = Portrait.sprite;
            }
        }
        else
        {
            talk.SetMsg(talkData);

            Portrait.color = new Color(1, 1, 1, 0);
        }
        isAction = true;
        talkIndex++;
    }

    public void GameEixt()
    {
        Application.Quit();
    }
    public void GameSave()
    {
        PlayerPrefs.SetFloat("PlayerX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerY", player.transform.position.y);
        PlayerPrefs.SetInt("QuestId", questManager.questId);
        PlayerPrefs.SetInt("QuestActionIndex", questManager.questActionIndex);
        PlayerPrefs.Save();

        menuSet.SetActive(false);
        
    }
    public void GameLoad()
    {
        if (!PlayerPrefs.HasKey("PlayerX"))
            return;

        float x = PlayerPrefs.GetFloat("PlayerX");
        float y = PlayerPrefs.GetFloat("PlayerY");
        int questId = PlayerPrefs.GetInt("QuestId");
        int questActionIndex = PlayerPrefs.GetInt("QuestActionIndex");

        player.transform.position = new Vector3(x, y, 0);
        questManager.questId = questId;
        questManager.questActionIndex = questActionIndex;
        questManager.ControlObject();

    }
}
