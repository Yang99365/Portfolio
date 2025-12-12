using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class GeneralListUI : MonoBehaviour
{ 
    GeneralManager generalManager;

    public bool listActive = false; // 장군 리스트 활성화 여부
    public GeneralSlot[] generalSlots;
    public GeneralSlot SelectedSlot;

    public GameObject generalListUI;
    public GeneralDescUI generalDescUI;

    public TextMeshProUGUI generalListSizeTxt;

    private void Start() {
        
        generalDescUI.ResetDescription();
        generalManager = GeneralManager.instance;
        generalSlots = GetComponentsInChildren<GeneralSlot>();
        generalManager.onGeneralChangedCallback += UpdateUI;
        generalListUI.SetActive(listActive);
        UpdateUI();
    }

    public void ControlGeneralUI()
    {
        if(!FactionManager.instance.playerFactionSelected)
        {
            Debug.Log("플레이어 세력이 선택되지 않았습니다.");
            return;
        }
        listActive = !listActive;
        generalListUI.SetActive(listActive);
        generalDescUI.ResetDescription();
    

        UpdateUI(); // 임시. 나중에 세력이 정해질때 신호를 받아와서 업데이트 해야함.
    }

    void UpdateUI() //SlotUI
    {
        for (int i = 0; i < generalSlots.Length; i++)
        {
            if (i < generalManager.PlayerFactionGenerals.Count)
            {
                generalSlots[i].UpdateSlotUI(generalManager.PlayerFactionGenerals[i]);
            }
            else
            {
                generalSlots[i].ClearSlot();
            }
        }
        generalListSizeTxt.text = generalManager.PlayerFactionGenerals.FindAll(x => x != null).Count + " / " + generalManager.PlayerGeneralSize;
        
    }

    //모든 슬롯 선택해제
    public void DeselectAllSlot()
    {
        foreach (var slot in generalSlots)
        {
            slot.isSelected = false;
            slot.SelectBorder.SetActive(slot.isSelected);
        }
    }
    
}
