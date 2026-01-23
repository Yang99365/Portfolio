using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Equip_SubItem : UI_Base, IPointerClickHandler
{
    enum GameObjects
    {
        EquipItem,
    }

    enum Images
    {
        ItemImage
    }

    enum Texts
    {
        ItemNameTxt,
    }

    public int _ItemInstanceID = -1;
    public int _ItemDataID = -1;
    public Item _item = null;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        OffButton();

        return true;
    }

    public void SetInfo(int ItemInstanceID, int ItemDataID, Item item)
    {
        _ItemInstanceID = ItemInstanceID;
        _ItemDataID = ItemDataID;
        _item = item;
        Refresh();
    }

    public void Refresh()
    {
        if (_init == false)
            return;

        if (_ItemInstanceID < 0 || _ItemDataID < 0)
        {
            OffButton();
            return;
        }

        OnButton();
        GetObject((int)GameObjects.EquipItem).gameObject.SetActive(true);
        GetImage((int)Images.ItemImage).gameObject.SetActive(true);
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[_ItemDataID].ItemImage);
        GetText((int)Texts.ItemNameTxt).text = Managers.Data.ItemDic[_ItemDataID].Name;
    }

    public void OffButton()
    {
        if (_init == false)
            return;
        GetObject((int)GameObjects.EquipItem).gameObject.SetActive(false);
        GetImage((int)Images.ItemImage).sprite = null;
        GetImage((int)Images.ItemImage).gameObject.SetActive(false);
        GetText((int)Texts.ItemNameTxt).text = "";
        _ItemInstanceID = -1;
        _ItemDataID = -1;
    }

    public void OnButton()
    {
        if (_init == false)
            return;
        GetObject((int)GameObjects.EquipItem).gameObject.SetActive(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_ItemInstanceID < 0 || _ItemDataID < 0)
            {
                return;
            }

            Item item = Managers.Inventory.GetItem(_ItemInstanceID);
            if (item == null) return;

            UI_GeneralEquipPopup equipPopup = GetComponentInParent<UI_GeneralEquipPopup>();
            UI_GeneralListPopup generalListPopup = GetComponentInParent<UI_GeneralListPopup>();
            if (equipPopup != null)
            {
                equipPopup.EquipItem(item);
                
            }
        }
    }
}