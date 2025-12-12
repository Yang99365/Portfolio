using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralManager : MonoBehaviour
{
    public List<GeneralBase> AllGenerals = new List<GeneralBase>();
    public List<GeneralBase> PlayerFactionGenerals;
    public List<GeneralBase> AnotherFactionGenerals;
    public List<GeneralBase> NoneFactionGenerals; // 중립

    public delegate void OnGeneralChanged();
    public OnGeneralChanged onGeneralChangedCallback;

    public int PlayerGeneralSize = 20;
    public int AnotherGeneralSize = 20;
    public int NoneGeneralSize = 100;

    public bool isFull = false;

    public Transform GeneralContent;


    public static GeneralManager instance;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            GeneralContent = GameObject.Find("GeneralContent").transform;
        }
        else
        {
            GeneralContent = null;
        }
            
    }

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
    }
    public void SlotNumInitialize()
    {
        for (int i = 0; i < GeneralContent.childCount; i++)
        {
            GeneralContent.GetChild(i).GetComponent<GeneralSlot>().slotNum = i;
        }
        //PlayerGeneralSize이 될때까지 null을 추가해준다.
        for (int i = PlayerFactionGenerals.Count; i < PlayerGeneralSize; i++) 
        {
            PlayerFactionGenerals.Add(null);
            
        }
        
    }
    public void InitGeneral()
    {
        //선택한 세력이 가진 장군들을 플레이어 장군으로 넣어준다.
        
        if (FactionManager.instance.playerFactionSelected)
        {
            foreach (var general in AllGenerals)
            {
                if (general.faction == FactionManager.instance.playerFaction.factionName)
                {
                    PlayerFactionGenerals.Add(general);
                }
                else if (general.faction == null)
                {
                    NoneFactionGenerals.Add(general);
                }
                else if (general.faction != FactionManager.instance.playerFaction.factionName && general.faction != null)
                {
                    AnotherFactionGenerals.Add(general);
                }
                
            }
            SlotNumInitialize();
        }
        
    }

    public void SwapGeneral(int index1, int index2)
    {
        GeneralBase general1 = PlayerFactionGenerals[index1];
        GeneralBase general2 = PlayerFactionGenerals[index2];
        
        GeneralBase temp = general1;
        PlayerFactionGenerals[index1] = general2;
        PlayerFactionGenerals[index2] = temp;

        onGeneralChangedCallback?.Invoke();
    }
}