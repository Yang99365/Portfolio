using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;


public class SetBattleUI : MonoBehaviour
{

    public bool listActive = false;
    public GameObject SetBattlePanel;
    public GameObject MyGeneralPanel;
    public TextMeshProUGUI stagetext;
    public GameObject SelectGeneralPanel;

    public GeneralBattleSlot slotPrefab;
    public SelectedBattleSlot selectedSlotPrefab;

    public GameObject SlotHolder;
    public GameObject SelectedSlotHolder;

    public GeneralBattleSlot[] myGeneralSlots;
    public SelectedBattleSlot[] selectedGeneralSlots;
    

/*
    public void OnEnable()
    {
        SetBattlePanel = GameObject.Find("SetBattle");
        MyGeneralPanel = GameObject.Find("MyGenerals");
        SelectGeneralPanel = GameObject.Find("BattleGenerals");
        stagetext = GameObject.Find("BattleDataTxt").GetComponent<TextMeshProUGUI>();
        myGeneralSlots = GetComponentsInChildren<GeneralBattleSlot>();
        selectedGeneralSlots = GetComponentsInChildren<SelectedBattleSlot>();
        SlotHolder = GameObject.Find("MyGeneralSlotHolder");
        SelectedSlotHolder = GameObject.Find("SelectedSlotHolder");
    }
*/
    
    private void Start()
    {
        
        myGeneralSlots = GetComponentsInChildren<GeneralBattleSlot>();
        selectedGeneralSlots = GetComponentsInChildren<SelectedBattleSlot>();
        GeneralManager.instance.onGeneralChangedCallback += UpdateUI;
        BattleContainer.instance.onContainerChangedCallback += UpdateSelectUI;
        SetBattlePanel.SetActive(listActive);
        InitializeUI();
        UpdateUI();
    }

    public void ControlGeneralUI() //전쟁(유닛배치) 버튼 눌렷을시 작동
    {
        listActive = !listActive;
        SetBattlePanel.SetActive(listActive);
        InitializeUI();
        UpdateUI();
        UpdateSelectUI();
        UpdateStageTxt();
    }

    public void UpdateStageTxt()
    {
        stagetext.text = "Battle Stage : " + RegionManager.instance.selectedRegion.regionName;
    }

    void UpdateUI()
    {
        // GeneralManager에서 null이 아닌 모든 Generals을 가져옴
        List<GeneralBase> nonNullGenerals = GeneralManager.instance.PlayerFactionGenerals.FindAll(x => x != null);

        // nonNullGenerals 리스트의 항목만큼 슬롯 UI를 업데이트
        for (int i = 0; i < myGeneralSlots.Length; i++)
        {
            if (i < nonNullGenerals.Count)
            {
                // 현재 인덱스에 해당하는 General로 UI 업데이트
                myGeneralSlots[i].UpdateSlotUI(nonNullGenerals[i]);
            }
            else
            {
                // 그 외의 슬롯은 Clear 처리
                myGeneralSlots[i].ClearSlot();
            }
        }

    }

    void InitializeUI()
    {
        // General이 null이 아닌 모든 슬롯의 수를 구한다.
        int generalSize = GeneralManager.instance.PlayerFactionGenerals.FindAll(x => x != null).Count;
        
        // GeneralSize가 0이 아니면
        if (generalSize != 0)
        {
            // 기존에 있던 슬롯들을 초기화한다.
            foreach (var slot in myGeneralSlots)
            {
                Destroy(slot.gameObject);
            }

            // GeneralSize만큼 반복하면서 myGeneralSlots를 생성한다.
            myGeneralSlots = new GeneralBattleSlot[generalSize];
            for (int i = 0; i < generalSize; i++)
            {
                // SlotPrefab을 사용하여 새로운 슬롯 인스턴스를 생성한다.
                GeneralBattleSlot newSlot = Instantiate(slotPrefab, SlotHolder.transform);
                // 생성된 슬롯을 배열에 저장한다.
                myGeneralSlots[i] = newSlot;
                
                // 생성된 슬롯에 slotNum을 부여한다.
                newSlot.slotNum = i;
            }
        }   
    }

    // BattleContainer에 들어있는 General들을 보여주는 UI
    // 호출될때마다 BattleContainer의 General들에 맞게 UI를 업데이트
    void UpdateSelectUI()
    {   
        // UI 삭제 및 생성
        // BattleContainer의 PlayerGenerals 배열에서 null이 아닌 요소의 수를 구함
        int nonNullGeneralsCount = BattleContainer.instance.Playergenerals.Count(x => x != null);

        // 기존 슬롯들 제거
        foreach (Transform child in SelectedSlotHolder.transform)
        {
            Destroy(child.gameObject);
        }

        // 새로운 슬롯 배열 생성
        selectedGeneralSlots = new SelectedBattleSlot[nonNullGeneralsCount];

        // nonNullGeneralsCount 만큼 슬롯 생성
        for (int i = 0; i < nonNullGeneralsCount; i++)
        {
            // SlotPrefab을 사용하여 새로운 슬롯 인스턴스를 생성하고, SlotHolder의 자식으로 설정
            SelectedBattleSlot newSlot = Instantiate(selectedSlotPrefab, SelectedSlotHolder.transform);
            // 생성된 슬롯을 배열에 저장
            selectedGeneralSlots[i] = newSlot;
            
            // 생성된 슬롯에 General 정보 업데이트
            if (BattleContainer.instance.Playergenerals[i] != null)
            {
                newSlot.UpdateSlotUI(BattleContainer.instance.Playergenerals[i]);
            }
        }

        // UI 업데이트
        for (int i = 0; i < selectedGeneralSlots.Length; i++)
        {
            // nonNullGeneralsCount 수 만큼 슬롯을 업데이트한다.
            if (i < nonNullGeneralsCount)
            {
                // 현재 인덱스에 해당하는 General이 있다면 UI 업데이트
                if (BattleContainer.instance.Playergenerals[i] != null)
                {
                    selectedGeneralSlots[i].UpdateSlotUI(BattleContainer.instance.Playergenerals[i]);
                }
            }
            else
            {
                // 그 외의 슬롯은 Clear 처리
                selectedGeneralSlots[i].ClearSlot();
            }
        }

    }
}
