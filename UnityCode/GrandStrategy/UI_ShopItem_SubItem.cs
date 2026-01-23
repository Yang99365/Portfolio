using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using static Define;

public class UI_ShopItem_SubItem : UI_Base, IPointerClickHandler
{
    
    enum Texts
    {
        ItemAmountText
    }

    enum Images
    {
        ItemImage
    }

    public Item Item { get; private set; }
    public System.Action<UI_ShopItem_SubItem> OnItemClicked;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        return true;
    }

    public void SetInfo(Item item)
    {
        Item = item;

        if (Item != null)
        {
            GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[item.DataId].ItemImage);
            GetText((int)Texts.ItemAmountText).text = Item.Count.ToString();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Left click on item: " + Item.ItemData.Name);
            OnItemClicked?.Invoke(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Managers.Shop.ShopMode == EShopMode.Buy)
            {
                BuyItem();
            }
            else
            {
                SellItem();
            }
        }
    }

    private void BuyItem()
    {
        if (Item != null)
        {
            Debug.Log("Buying item: " + Item.ItemData.Name);
            Managers.Shop.BuyItem(Item.InstanceId);
        }
    }

    private void SellItem()
    {
        if (Item != null)
        {
            Debug.Log("Selling item: " + Item.ItemData.Name);
            Managers.Shop.SellItem(Item.InstanceId);
        }
    }
}