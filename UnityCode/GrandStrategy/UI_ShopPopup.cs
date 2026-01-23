using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
public class UI_ShopPopup : UI_Popup
{
    enum Buttons
    {
        BuyButton,
        SellButton,
        CloseButton,
        RerollButton
    }
    enum GameObjects
    {
        ShopContent
    }

    enum Texts
    {
        ItemNameText,
        PriceText,
        SellText,
        BuyText
    }
    enum Images
    {

    }

    private List<UI_ShopItem_SubItem> shopItems = new List<UI_ShopItem_SubItem>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.BuyButton).gameObject.BindEvent(OnClickBuyButton);
        GetButton((int)Buttons.SellButton).gameObject.BindEvent(OnClickSellButton);
        GetButton((int)Buttons.RerollButton).gameObject.BindEvent(OnClickRerollButton);

        if (Managers.Shop == null)
        {
            Debug.Log("shop is null");
        }
        else
        {
            ShopManager.OnShopChanged -= SetInfo;
            ShopManager.OnShopChanged += SetInfo;
        }

        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);

        Managers.Shop.ShopMode = EShopMode.Buy;
        CreateshopItems();

        RefreshUI();

        return true;
    }

    public void SetInfo()
    {
        RefreshUI();
    }
    public void RefreshUI()
    {
        List<Item> currentItems;
        if (Managers.Shop.ShopMode == EShopMode.Buy)
        {
            currentItems = Managers.Shop.GetShopItems();
        }
        else // Sell mode
        {
            currentItems = Managers.Inventory.MyItems.Where(item => item != null).ToList();
        }

        // 모든 슬롯을 비활성화
        foreach (var slot in shopItems)
        {
            slot.gameObject.SetActive(false);
        }

        // 필요한 만큼의 슬롯만 활성화 및 정보 갱신
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (i < shopItems.Count)
            {
                shopItems[i].SetInfo(currentItems[i]);
                shopItems[i].gameObject.SetActive(true);
            }
            else
            {
                UI_ShopItem_SubItem newSlot = Managers.UI.MakeSubItem<UI_ShopItem_SubItem>(GetObject((int)GameObjects.ShopContent).transform);
                newSlot.OnItemClicked = OnItemClicked;
                newSlot.SetInfo(currentItems[i]);
                shopItems.Add(newSlot);
            }
        }

        UpdateModeText();
    }
    public void RefreshItemDesc(int ItemDataID, int Amount) //ui_general_SUbitem처럼 클릭하면 이거호출
    {

    }

    private void CreateshopItems()
    {
        Transform content = GetObject((int)GameObjects.ShopContent)?.transform;
        if (content == null)
        {
            Debug.LogError("Item content transform not found");
            return;
        }

        int initialSlotCount = 30;
        for (int i = 0; i < initialSlotCount; i++)
        {
            UI_ShopItem_SubItem slot = Managers.UI.MakeSubItem<UI_ShopItem_SubItem>(content);
            if (slot != null)
            {
                slot.OnItemClicked = OnItemClicked;
                shopItems.Add(slot);
            }
        }

    }
    //private void ClearshopItems()
    //{
    //    foreach (UI_ShopItem_SubItem slot in shopItems)
    //    {
    //        Managers.Resource.Destroy(slot.gameObject);
    //    }
    //    shopItems.Clear();
    //}

    private void UpdateModeText()
    {
        if(Managers.Shop.ShopMode == EShopMode.Buy)
        {
            GetText((int)Texts.BuyText).text = ">>Buy<<";
            GetText((int)Texts.SellText).text = "Sell";
        }
        else
        {
            GetText((int)Texts.BuyText).text = "Buy";
            GetText((int)Texts.SellText).text = ">>Sell<<";
        }
    }
    #region Event
    void OnClickCloseButton(PointerEventData evt)
    {
        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);
        Managers.UI.ClosePopupUI();
    }
    private void OnClickBuyButton(PointerEventData evt)
    {
        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);
        if (Managers.Shop.ShopMode == EShopMode.Buy)
            return;
        Managers.Shop.SetShopMode(EShopMode.Buy);
        RefreshUI();
    }

    private void OnClickSellButton(PointerEventData evt)
    {
        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);
        if (Managers.Shop.ShopMode == EShopMode.Sell)
            return;
        Managers.Shop.SetShopMode(EShopMode.Sell);
        RefreshUI();
    }

    void OnClickRerollButton(PointerEventData evt)
    {
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
        if (playerFaction.CanSpendGold(Managers.Shop.RerollCost))
        {
            playerFaction.SpendGold(Managers.Shop.RerollCost);
        }
        else
        {
            Debug.Log("Not enough gold to reroll shop");
            return;
        }
        Managers.Shop.RerollShop();
    }

    private void OnItemClicked(UI_ShopItem_SubItem clickedItem)
    {
        Debug.Log("OnItemClicked called in UI_ShopPopup");
        if (clickedItem != null && clickedItem.Item != null)
        {
            GetText((int)Texts.ItemNameText).gameObject.SetActive(true);
            GetText((int)Texts.ItemNameText).text = clickedItem.Item.ItemData.Name;
            GetText((int)Texts.PriceText).gameObject.SetActive(true);
            int totalPrice = clickedItem.Item.ItemData.itemPrice * clickedItem.Item.Count;
            GetText((int)Texts.PriceText).text = $"Price: {totalPrice}";
        }
    }
    #endregion
}
