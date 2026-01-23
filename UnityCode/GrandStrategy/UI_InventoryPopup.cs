using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_InventoryPopup : UI_Popup
{
    enum Buttons
    {
        EquipButton,
        ConsumButton,
        EnvButton,
        CloseButton,
        AllButton,
        ItemButton_Test,
    }
    enum GameObjects
    {
        InventoryContent
    }

    enum Texts
    {
        

    }
    enum Images
    {
        
    }

    private List<UI_Inventory_SubItem> myItems = new List<UI_Inventory_SubItem>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        GetButton((int)Buttons.ConsumButton).gameObject.BindEvent(OnClickConsumButton);
        GetButton((int)Buttons.EnvButton).gameObject.BindEvent(OnClickEnvButton);
        GetButton((int)Buttons.AllButton).gameObject.BindEvent(OnClickAllButton);
        GetButton((int)Buttons.ItemButton_Test).gameObject.BindEvent(OnClickTestButton);

        // InventoryContent에 InventoryItem을 생성
        {
            var parent = GetObject((int)GameObjects.InventoryContent).transform;
            int slotCount = Managers.Inventory.InventorySlotCount();
            for (int i = 0; i < slotCount; i++)
            {
                UI_Inventory_SubItem item = Managers.UI.MakeSubItem<UI_Inventory_SubItem>(parent);
                item.SlotIndex = i;
                //Managers.Inventory.MyItems.Add(null);
                myItems.Add(item);
            }
        }

        if(Managers.Inventory == null)
        {
            Debug.Log("Inventory is null");
        }
        else
        {
            Debug.Log("Inventory is not null");
            InventoryManager.OnItemChanged -= SetInfo;
            InventoryManager.OnItemChanged += SetInfo;
        }
        

        // RefreshAllInventory(myItems); 로 바꿔야함. 테스트용으로 Refresh사용
        Refresh();

        return true;
    }

    public void SetInfo()
    {
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;

        RefreshInventory(myItems);
        

    }

    void RefreshInventory(List<UI_Inventory_SubItem> list)
    {
        List<Item> items = Managers.Inventory.MyItems;
        int count = Mathf.Min(items.Count, list.Count);

        for (int i = 0; i < count; i++)
        {
            if (items[i] == null)
            {
                list[i].SetInfo(-1, -1);
            }
            else
            {
                Item item = items[i];
                list[i].SetInfo(item.InstanceId, item.DataId);
                switch (Managers.Inventory.ToggleType)
                {
                    case EInventoryGroupType.All:
                        list[i].SetActiveState(true);
                        break;
                    case EInventoryGroupType.Equipment:
                        if(item.ItemData.ItemGroupType == EItemGroupType.Equipment)
                            list[i].SetActiveState(true);
                        else
                            list[i].SetActiveState(false);
                        break;
                    case EInventoryGroupType.Consumable:
                        if (item.ItemData.ItemGroupType == EItemGroupType.Consumable)
                            list[i].SetActiveState(true);
                        else
                            list[i].SetActiveState(false);
                        break;
                    case EInventoryGroupType.Material:
                        if (item.ItemData.ItemGroupType == EItemGroupType.Material)
                            list[i].SetActiveState(true);
                        else
                            list[i].SetActiveState(false);
                        break;
                }
            }
            //if (Managers.Inventory.MyItems[i] != null)
            //    Debug.Log(Managers.Inventory.MyItems[i].ItemData.Name);
            //else
            //    Debug.Log("[" + i + "] is null");
        }
        for (int i = count; i < list.Count; i++)
        {
            list[i].SetInfo(-1, -1);
        }
        Debug.Log("RefreshInventory");
        
    }
    
    
    void OnClickCloseButton(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI();
    }

    void OnClickEquipButton(PointerEventData evt)
    {
        Managers.Inventory.ToggleEquipItem();
        Refresh();
    }

    void OnClickConsumButton(PointerEventData evt)
    {
        Managers.Inventory.ToggleConsumableItem();
        Refresh();
    }

    void OnClickEnvButton(PointerEventData evt)
    {
        Managers.Inventory.ToggleMaterialItem();
        Refresh();
    }

    void OnClickAllButton(PointerEventData evt)
    {
        Managers.Inventory.ToggleAllItem();
        Refresh();
    }
    void OnClickTestButton(PointerEventData evt)
    {
        Managers.Inventory.AddItem(1001, 95); // test
        Managers.Inventory.AddItem(101);
        Managers.Inventory.AddItem(1002, 95); // test
        Managers.Inventory.DebugInventory();
    }
}
