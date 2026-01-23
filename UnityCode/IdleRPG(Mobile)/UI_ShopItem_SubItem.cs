using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using static Define;
using static Data;

public class UI_ShopItem_SubItem : UI_Base, IPointerClickHandler
{
    enum Texts
    {
        ItemAmountText
    }

    enum Images
    {
        ItemImage,
        SelectedFrame
    }

    public Item Item { get; private set; }
    public System.Action<UI_ShopItem_SubItem> OnItemClicked;

    private bool _isSelected = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

       
        GetImage((int)Images.SelectedFrame).gameObject.SetActive(false);

        return true;
    }

    public void SetInfo(Item item)
    {
        Item = item;

        if (Item != null && Item.ItemData != null)
        {
            
            if (!string.IsNullOrEmpty(Item.ItemData.ItemImage))
            {
                Sprite itemSprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[item.DataId].ItemImage);
                if (itemSprite != null)
                {
                    GetImage((int)Images.ItemImage).sprite = itemSprite;
                }
                else
                {
                    GetImage((int)Images.ItemImage).sprite = null;

                }
            }

            
            GetText((int)Texts.ItemAmountText).text = Item.Count > 1 ? Item.Count.ToString() : "";

            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    
    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        GetImage((int)Images.SelectedFrame).gameObject.SetActive(selected);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Item == null)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnItemClicked?.Invoke(this);
        }
    }
}