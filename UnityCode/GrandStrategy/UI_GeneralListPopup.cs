using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_GeneralListPopup : UI_Popup
{
    
    enum Buttons
    {
        CloseButton,
        EquipButton,
        FireButton,
    }
    enum GameObjects
    {
        GeneralList,
        GeneralDescArea,
    }

    enum Texts
    {
        GeneralNameText,
        ATKText,
        DEFText,
        INTText,
        SPDText,

        TroopCountText,

    }
    enum Images
    {
        Portrait,
        UnitTypeIcon,
        TroopTypeIcon,
    }
    enum Sliders
    {
        ATK_Slider,
        DEF_Slider,
        INT_Slider,
        SPD_Slider,
    }

    List<UI_General_SubItem> myGenerals = new List<UI_General_SubItem>();
    public UI_GeneralEquipPopup equipPopup;
    public General selectedGeneral = null;
    Item selectedItem = null;
    bool isForItemUse = false;
    bool isForItemEquip = false;

    const int MAX_GENERAL_COUNT = 30;
    private void OnDisable()
    {
        GetObject((int)GameObjects.GeneralDescArea).SetActive(false);
        ResetItemSetting();
        for (int i = 0; i < myGenerals.Count; i++)
        {
            myGenerals[i].SetSelect(false);
        }
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindSliders(typeof(Sliders));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        GetButton((int)Buttons.FireButton).gameObject.BindEvent(OnClickFireButton);

        {
            var parent = GetObject((int)GameObjects.GeneralList).transform;
            for (int i = 0; i < MAX_GENERAL_COUNT; i++)
            {
                UI_General_SubItem item = Managers.UI.MakeSubItem<UI_General_SubItem>(parent);
                myGenerals.Add(item);
            }
        }

        if (Managers.Inventory == null)
        {
            Debug.Log("Inventory is null");
        }
        else
        {
            Debug.Log("Inventory is not null");
            InventoryManager.OnItemChanged -= RefreshAllGenerals;
            InventoryManager.OnItemChanged += RefreshAllGenerals;
        }


        Refresh();
        GetObject((int)GameObjects.GeneralDescArea).SetActive(false);



        return true;
    }

    public void RefreshAllGenerals()
    {
        foreach (var generalSubItem in myGenerals)
        {
            generalSubItem.Refresh();
        }
        if (selectedGeneral != null)
        {
            UpdateGeneralUI();
        }
    }

    public void SetForItemUse(Item item)
    {
        selectedItem = item;
        isForItemUse = true;
        isForItemEquip = false;
        Refresh();
    }

    public void SetForItemEquip(Item item)
    {
        selectedItem = item;
        isForItemUse = false;
        isForItemEquip = true;
        Refresh();
    }
    public void ResetItemSetting()
    {
        selectedGeneral = null;
        selectedItem = null;
        isForItemUse = false;
        isForItemEquip = false;
    }

    public void SetInfo()
    {
        Refresh();
    }
    
    void Refresh()
    {
        if (_init == false)
            return;
        // 장군 리스트를 불러와서 표시
        RefreshGeneral(myGenerals);

    }

    void RefreshGeneral(List<UI_General_SubItem> list)
    {
        List<General> generals = Managers.Game.GetPlayerGeneral();

        for (int i = 0; i < list.Count; i++)
        {
            if (i < generals.Count)
            {
                General general = generals[i];
                list[i].SetInfo(general.GeneralID);
                list[i].gameObject.SetActive(true);
            }
            else
            {
                list[i].gameObject.SetActive(false);
            }
        }

    }
    void OnClickCloseButton(PointerEventData evt)
    {
        GetObject((int)GameObjects.GeneralDescArea).SetActive(false);
        ResetItemSetting();
        for (int i = 0; i < myGenerals.Count; i++)
        {
            myGenerals[i].SetSelect(false);
        }
        //Managers.UI.CloseAllPopupUI();
        if(equipPopup != null)
        {
            Managers.UI.ClosePopupUI(equipPopup);
        }
        Managers.UI.ClosePopupUI(this);
    }
    void OnClickEquipButton(PointerEventData evt)
    {
        if (selectedGeneral == null)
            return;
        equipPopup = Managers.UI.ShowPopupUI<UI_GeneralEquipPopup>();
        equipPopup.SetGeneral(selectedGeneral);
        Refresh();
    }
    
    void OnClickFireButton(PointerEventData evt)
    {
        if (selectedGeneral == null)
            return;

        Managers.Game.FireGeneral(selectedGeneral.GeneralID);
        GetObject((int)GameObjects.GeneralDescArea).SetActive(false);
        selectedGeneral = null;
        for(int i = 0; i < myGenerals.Count; i++)
        {
            myGenerals[i].SetSelect(false);
        }
        Refresh();
    }

    public void SetGeneralInfo(int generalID)
    {
        
        foreach (var button in myGenerals)
        {
            button.SetSelect(false);
        }
        GetObject((int)GameObjects.GeneralDescArea).SetActive(true);

        General general = Managers.Game.Allgenerals.Find(x => x.GeneralID == generalID);

        if (general != null)
        {
            selectedGeneral = general;
            UpdateGeneralUI();

            if (equipPopup != null)
            {
                equipPopup.SetGeneral(selectedGeneral);
            }
            else if (isForItemEquip)
            {
                ShowEquipPopup();
            }
            else if (isForItemUse)
            {
                ShowConfirmPopup();
            }
        }

    }

    void UpdateGeneralUI()
    {
        GetText((int)Texts.GeneralNameText).text = selectedGeneral.GeneralName;
        GetText((int)Texts.ATKText).text = selectedGeneral.ModifiedStats.attack.ToString() + "/ 100";
        GetText((int)Texts.DEFText).text = selectedGeneral.ModifiedStats.defense.ToString() + "/ 100";
        GetText((int)Texts.INTText).text = selectedGeneral.ModifiedStats.intelligence.ToString() + "/ 100";
        GetText((int)Texts.SPDText).text = selectedGeneral.ModifiedStats.speed.ToString() + "/ 100";
        GetText((int)Texts.TroopCountText).text = selectedGeneral.troopCount.ToString() + " / " + selectedGeneral.troopMaxCount.ToString();

        GetImage((int)Images.Portrait).sprite = Managers.Resource.Load<Sprite>(selectedGeneral.SpritePath);
        GetImage((int)Images.UnitTypeIcon).sprite = Managers.Resource.Load<Sprite>(selectedGeneral.unitTypeSpritePath);
        GetImage((int)Images.TroopTypeIcon).sprite = Managers.Resource.Load<Sprite>(selectedGeneral.troopTypeSpritePath);

        GetSlider((int)Sliders.ATK_Slider).value = selectedGeneral.ModifiedStats.attack;
        GetSlider((int)Sliders.DEF_Slider).value = selectedGeneral.ModifiedStats.defense;
        GetSlider((int)Sliders.INT_Slider).value = selectedGeneral.ModifiedStats.intelligence;
        GetSlider((int)Sliders.SPD_Slider).value = selectedGeneral.ModifiedStats.speed;
    }
    void ShowConfirmPopup()
    {
        
        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.GetComponent<Canvas>().sortingOrder = 30;
        confirmPopup.SetInfo($"Use {selectedItem.ItemData.Name} on {selectedGeneral.GeneralName}?", () =>
        {
            if (isForItemUse)
            {
                UseItemOnGeneral();
            }
            else if (isForItemEquip)
            {
                EquipItemOnGeneral();
            }
            Managers.UI.ClosePopupUI(this);
        },selectedItem.ItemData.Name);
        GetObject((int)GameObjects.GeneralDescArea).SetActive(false);
    }
    void ShowEquipPopup()
    {
        equipPopup = Managers.UI.ShowPopupUI<UI_GeneralEquipPopup>();
        equipPopup.SetGeneral(selectedGeneral);
        Managers.UI.ClosePopupUI(this);
    }
    void UseItemOnGeneral()
    {
        if (selectedItem is ConsumableItem consumable)
        {
            selectedGeneral.UseConsumableItem(consumable);
            Managers.Inventory.UseConsum(selectedItem.InstanceId);
            ResetItemSetting();
            for (int i = 0; i < myGenerals.Count; i++)
            {
                myGenerals[i].SetSelect(false);
            }
        }
    }

    void EquipItemOnGeneral()
    {
        if (selectedItem is EquipmentItem equipment)
        {
            selectedGeneral.EquipItem(equipment);
            Managers.Inventory.RemoveItem(selectedItem.InstanceId);
            ResetItemSetting();
            UpdateGeneralUI();
        }
    }
}
