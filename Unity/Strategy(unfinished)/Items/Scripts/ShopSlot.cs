using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


public class ShopSlot : MonoBehaviour, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int slotNum;
    public GItemSO item;
    public Image icon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI priceText;
    public GameObject Background;
    public InventoryUI inventoryUI;
    public bool SoldOut = false;
    private SlotToolTip slotToolTip;

    public void Init(InventoryUI Iui)
    {
        inventoryUI = Iui;
    }
    public void Awake()
    {
        slotToolTip = FindObjectOfType<SlotToolTip>();
    }
    
    public void UpdateSlotUI()
    {
        if (item == null)
        {
            RemoveSlot();
            return;
        }
        itemNameText.gameObject.SetActive(true);
        itemNameText.text = item.itemName;
        icon.gameObject.SetActive(true);
        icon.sprite = item.ItemImage;
        if (item.Type != ItemType.Ownable && item.Type != ItemType.Equipment)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = item.amount.ToString();
            priceText.gameObject.SetActive(true);
            priceText.text = item.price.ToString();
            Background.SetActive(true);
            if(SoldOut)
            {
                icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
        else
        {
            priceText.gameObject.SetActive(false);
            priceText.text = item.price.ToString();
            amountText.gameObject.SetActive(false);
            Background.SetActive(false);
            if(SoldOut)
            {
                icon.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
        }
    }
    public void RemoveSlot()
    {
        SoldOut = false; // 리셋시 품절 해제?
        item = null;
        itemNameText.text = "";
        itemNameText.gameObject.SetActive(false);
        amountText.text = "";
        amountText.gameObject.SetActive(false);
        priceText.text = "";
        priceText.gameObject.SetActive(false);
        Background.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (item!=null)
        {
            Inventory.instance.SpaceFull(); // 인벤토리가 가득 찼는지 확인
            if (FactionManager.instance.playerFaction.gold >= item.price && !SoldOut && Inventory.instance.isFull == false)
            {
                Inventory.instance.Add(item);
                FactionManager.instance.playerFaction.gold -= item.price;
                SoldOut = true;
                inventoryUI.Buy(slotNum);
                UpdateSlotUI();
            }
            else if (SoldOut)
            {
                Debug.Log("품절된 상품입니다.");
            }
            else if (FactionManager.instance.playerFaction.gold < item.price)
            {
                Debug.Log("골드가 부족합니다.");
            }
            else if (Inventory.instance.isFull == true)
            {
                Debug.Log("인벤토리가 가득 찼습니다.");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null && item != null)
        {
            slotToolTip.ShowToolTip(item ,transform.position);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        slotToolTip.HideToolTip();
    }
}
