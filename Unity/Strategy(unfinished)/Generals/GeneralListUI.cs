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
    public GameObject DescPanel;
    public GameObject generalDescUI; // stat
    public GameObject generalDescUI2; // skill
    public GameObject generalDescUI3; // equipment 텍스트만

    public TextMeshProUGUI generalListSizeTxt;

    private void Start() {
        generalManager = GeneralManager.instance;
        generalSlots = GetComponentsInChildren<GeneralSlot>();
        generalManager.onGeneralChangedCallback += UpdateUI;
        generalListUI.SetActive(listActive);
        UpdateUI();
    }

    public void ControlGeneralUI()
    {
        listActive = !listActive;
        generalListUI.SetActive(listActive);
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

    void UpdateDescUI() //DescUI
    {
        /*
        if (SelectedSlot != null)
        {
            generalDescUI.SetActive(true);
            generalDescUI2.SetActive(true);
            generalDescUI3.SetActive(true);
            generalDescUI.GetComponent<GeneralDescUI>().UpdateDescUI(SelectedSlot.general);
            generalDescUI2.GetComponent<GeneralDescUI2>().UpdateDescUI(SelectedSlot.general);
            generalDescUI3.GetComponent<GeneralDescUI3>().UpdateDescUI(SelectedSlot.general);
        }
        else
        {
            generalDescUI.SetActive(false);
            generalDescUI2.SetActive(false);
            generalDescUI3.SetActive(false);
        }
        */
    }   
    
}
