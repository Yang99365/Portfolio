using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;
using static ES3;

public class UI_GeneralEquipPopup : UI_Popup
{
    enum Buttons
    {
        GeneralWeapon,
        GeneralArmor,
        GeneralAccessory,
    }
    enum GameObjects
    {
        
        EquipItemContent,
        GeneralWeapon,
        GeneralArmor,
        GeneralAccessory,
    }
    enum Images
    {
        WeaponImage,
        ArmorImage,
        AccessoryImage,
    }

    private General selectedGeneral;
    private List<UI_Equip_SubItem> equipItems = new List<UI_Equip_SubItem>();
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));

        GetButton((int)Buttons.GeneralWeapon).gameObject.BindEvent(OnWeaponSlotRightClick);
        GetButton((int)Buttons.GeneralArmor).gameObject.BindEvent(OnArmorSlotRightClick);
        GetButton((int)Buttons.GeneralAccessory).gameObject.BindEvent(OnAccessorySlotRightClick);


        if (Managers.Inventory == null)
        {
            Debug.Log("Inventory is null");
        }
        else
        {
            Debug.Log("Inventory is not null");
            InventoryManager.OnItemChanged -= RefreshEquipItems;
            InventoryManager.OnItemChanged += RefreshEquipItems;
        }

        CreateEquipItems();

        return true;
    }

    private void RefreshEquipItems()
    {
        ClearEquipItems();
        CreateEquipItems();
        RefreshUI();
    }
    private void ClearEquipItems()
    {
        foreach (var item in equipItems)
        {
            Managers.Resource.Destroy(item.gameObject);
        }
        equipItems.Clear();
    }
    private void CreateEquipItems()
    {
        Transform content = GetObject((int)GameObjects.EquipItemContent)?.transform;
        if (content == null)
        {
            Debug.LogError("Equip item content transform not found");
            return;
        }

        List<Item> equipmentItems = Managers.Inventory.MyItems
            .Where(item => item != null && item.ItemData.ItemGroupType == EItemGroupType.Equipment)
            .ToList();

        foreach (Item item in equipmentItems)
        {
            UI_Equip_SubItem equipItem = Managers.UI.MakeSubItem<UI_Equip_SubItem>(content);
            if (equipItem != null)
            {
                equipItem.SetInfo(item.InstanceId, item.DataId, item);
                equipItems.Add(equipItem);
            }
        }
    }
    private void RefreshUI()
    {
        if (selectedGeneral == null) return;

        // 현재 장착된 장비 표시
        UpdateEquippedItems();

        // 장비 목록 갱신
        foreach (var equipItem in equipItems)
        {
            equipItem.Refresh();
        }
    }

    

    
    public void SetGeneral(General general)
    {
        selectedGeneral = general;
        RefreshUI();
    }
    private void UpdateEquippedItems()
    {
        if (selectedGeneral == null) return;

        // 무기, 방어구, 장신구 이미지 업데이트
        UpdateEquippedItemImage(Images.WeaponImage, selectedGeneral.Equipment.weaponId);
        UpdateEquippedItemImage(Images.ArmorImage, selectedGeneral.Equipment.armorId);
        UpdateEquippedItemImage(Images.AccessoryImage, selectedGeneral.Equipment.accessoryId);
    }
    private void UpdateEquippedItemImage(Images imageType, int itemId)
    {
        Image slotImage = GetImage((int)imageType);
        if (slotImage != null)
        {
            if (itemId != 0 && Managers.Data.ItemDic.TryGetValue(itemId, out ItemData itemData))
            {
                slotImage.sprite = Managers.Resource.Load<Sprite>(itemData.ItemImage);
                slotImage.gameObject.SetActive(true);
            }
            else
            {
                slotImage.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning($"Image not found for {imageType}");
        }

    }
    public void EquipItem(Item item)
    {
        if (selectedGeneral == null || !(item is EquipmentItem equipItem)) return;
        //selectedGeneral.EquipItem(equipItem);
        Managers.Inventory.RemoveItem(item.InstanceId);
        selectedGeneral.EquipItem(equipItem);
        Managers.Inventory.OnItemChangedEvent();
        RefreshEquipItems();
    }

    private void OnWeaponSlotRightClick(PointerEventData data)
    {
        //선택한 무장이 없으면 리턴
        if (selectedGeneral.Equipment.weapon == null) return;
        //선택한 무장이 장비를 장착하고있지 않으면 리턴
        if (selectedGeneral.Equipment.weaponId == 0) return;
        if (data.button == PointerEventData.InputButton.Right)
            UnequipItem(EItemType.Weapon);
    }

    private void OnArmorSlotRightClick(PointerEventData data)
    {
        if (selectedGeneral.Equipment.weapon == null) return;
        if (selectedGeneral.Equipment.armorId == 0) return;
        if (data.button == PointerEventData.InputButton.Right)
            UnequipItem(EItemType.Armor);
    }

    private void OnAccessorySlotRightClick(PointerEventData data)
    {
        if (selectedGeneral.Equipment.weapon == null) return;
        if (selectedGeneral.Equipment.accessoryId == 0) return;
        if (data.button == PointerEventData.InputButton.Right)
            UnequipItem(EItemType.Accessory);
    }
    private void UnequipItem(EItemType itemType)
    {
        if (selectedGeneral == null) return;
        if (Managers.Inventory.IsInventoryFull())
        {
            Debug.Log("Inventory is full");
            return;
        }

        selectedGeneral.UnequipItem(itemType);
        RefreshEquipItems();
    }
}
