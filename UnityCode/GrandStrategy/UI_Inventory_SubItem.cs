using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Inventory_SubItem : UI_Base, IPointerClickHandler, IBeginDragHandler,
IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int SlotIndex { get; set; } = -1;
    private Vector3 originalPosition;
    private GameObject draggedItemUI;
    
    enum GameObjects
    {
        InventoryItem,
    }


    enum Images
    {
        ItemImage
    }

    enum Texts
    {
        ItemAmountTxt,
        ItemNameTxt,
    }

    public int _ItemInstanceID = -1;
    int _ItemDataID = -1;
    bool isInteractable = true;
    //툴팁 오브젝트 private UI_Inventory_Tooltip _tooltip = null;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        //GetButton((int)Buttons.InventoryItem).gameObject.BindEvent(OnClickInventoryItem);
        OffButton();
        Refresh();

        return true;
    }

    public void SetInfo(int ItemInstanceID, int ItemDataID)
    {
        if(_init == false)
            return;
        if (ItemInstanceID < 0 || ItemDataID < 0)
        {
            OffButton();
            return;
        }
        _ItemInstanceID = ItemInstanceID;
        _ItemDataID = ItemDataID;
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
        GetObject((int)GameObjects.InventoryItem).gameObject.SetActive(true);
        GetImage((int)Images.ItemImage).gameObject.SetActive(true);
        GetImage((int)Images.ItemImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[_ItemDataID].ItemImage);
        GetText((int)Texts.ItemAmountTxt).text = Managers.Inventory.MyItems[SlotIndex].Count.ToString();
        GetText((int)Texts.ItemNameTxt).text = Managers.Data.ItemDic[_ItemDataID].Name;
    }
    public void OffButton() // OffButton인 이유는 Button으로 만들었다가.. 아오
    {
        if (_init == false)
            return;
        GetObject((int)GameObjects.InventoryItem).gameObject.SetActive(false);
        GetImage((int)Images.ItemImage).sprite = null;
        GetImage((int)Images.ItemImage).gameObject.SetActive(false);
        GetText((int)Texts.ItemAmountTxt).text = "";
        GetText((int)Texts.ItemNameTxt).text = "";
        _ItemInstanceID = -1;
        _ItemDataID = -1;
        //isInteractable = false;

    }
    public void OnButton()
    {
        if (_init == false)
            return;
        GetObject((int)GameObjects.InventoryItem).gameObject.SetActive(true);
    }
    public void ClearSubItem()
    {
        if (_init == false)
            return;
        _ItemInstanceID = -1;
        _ItemDataID = -1;
        OffButton();
    }


    public void SetActiveState(bool state)
    {
        if (_init == false)
            return;
        if(state)
        {
            SetIconTransparency(1f); // 완전 불투명
        }
        else
        {
            SetIconTransparency(0.5f); // 반투명
        }
        isInteractable = state;
    }
    private void SetIconTransparency(float alpha)
    {
        if (_init == false)
            return;
        if (GetImage((int)Images.ItemImage) != null)
        {
            Color color = GetImage((int)Images.ItemImage).color;
            color.a = alpha;
            GetImage((int)Images.ItemImage).color = color;
        }
    }
    #region Event
    public void OnPointerClick(PointerEventData eventData)
    {

        if (eventData.button == PointerEventData.InputButton.Left) // 좌클릭으로 슬롯 관련 정보 출력
        {
            Debug.Log("SlotIndex: " + SlotIndex);
            Debug.Log("ItemInstanceID: " + _ItemInstanceID);
            Debug.Log("ItemDataID: " + _ItemDataID);
            // 아이템 정보 출력
            if (_ItemDataID >= 0 && _ItemInstanceID >= 0)
                Debug.Log(Managers.Data.ItemDic[_ItemDataID].Name);

            Debug.Log(isInteractable ? "Interactable" : "Not Interactable");
        }
        if (!isInteractable) return; // 비활성화된 아이템은 클릭 불가
        
        else if (eventData.button == PointerEventData.InputButton.Right) // 우클릭으로 사용
        {
            
            if (_ItemInstanceID < 0 || _ItemDataID < 0)
            {
                return;
            }

            Item item = Managers.Inventory.GetItem(_ItemInstanceID);
            if (item == null) return;

            switch (item.ItemData.ItemGroupType)
            {
                case EItemGroupType.Consumable:
                    UI_GeneralListPopup generalListPopup = Managers.UI.ShowPopupUI<UI_GeneralListPopup>();
                    generalListPopup.SetForItemUse(item);
                    break;
                case EItemGroupType.Equipment:
                    Managers.UI.CloseAllPopupUI();
                    UI_GeneralListPopup equipGeneralListPopup = Managers.UI.ShowPopupUI<UI_GeneralListPopup>();
                    break;
                default:
                    Debug.Log("This item cannot be used or equipped.");
                    break;
            }
        }

    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        if (!isInteractable) return;

        if(_ItemInstanceID < 0 || _ItemDataID < 0)
        {
            return;
        }
        else
        {
            originalPosition = GetImage((int)Images.ItemImage).transform.position;
            GetImage((int)Images.ItemImage).transform.SetParent(transform.parent.parent);

            // 드래그 중인 아이템을 표시하기 위한 UI 생성
            draggedItemUI = new GameObject("DraggedItemUI");
            draggedItemUI.transform.SetParent(transform.parent.parent);
            Image draggedItemImage = draggedItemUI.AddComponent<Image>();
            draggedItemImage.sprite = GetImage((int)Images.ItemImage).sprite;
            draggedItemImage.raycastTarget = false;
            RectTransform rectTransform = draggedItemImage.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 60);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        if (!isInteractable) return;
        if (draggedItemUI != null)
        {
            GetImage((int)Images.ItemImage).transform.position = eventData.position;

            draggedItemUI.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        if (!isInteractable) return;
        GetImage((int)Images.ItemImage).transform.position = originalPosition;
        GetImage((int)Images.ItemImage).transform.SetParent(transform);

        Destroy(draggedItemUI);

        // 드래그 아이템 관련 UI는 예전 프로젝트에 그대로 썻던거라 Destroy쓰고 하는데
        // 개선한다면 새롭게 UI 만들어서 SubItem 생성하고 해야할듯함.
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (!isInteractable) return;
        UI_Inventory_SubItem droppedSubItem = eventData.pointerDrag.GetComponent<UI_Inventory_SubItem>();

        if (droppedSubItem == null)
        {
            Debug.LogError("Dropped subitem is null or not properly initialized.");
            return;
        }
       
        if (droppedSubItem != null && draggedItemUI != this && droppedSubItem.isInteractable) // 드래그한 아이템이 SubItem이고, 현재 슬롯과 다른 슬롯에 드래그된 경우
        {
            //빈공간에 아이템을 드래그한 경우
            if (droppedSubItem._ItemInstanceID < 0 || droppedSubItem._ItemDataID < 0)
            {
                Managers.Inventory.SwapItemForEmptySlot(SlotIndex, droppedSubItem.SlotIndex);
                return;
            }
            // 아이템이 들어있는 공간에 드래그한 경우
            Managers.Inventory.SwapItems(droppedSubItem.SlotIndex, this.SlotIndex);
        }
        // 장비 장착 로직으로 장비장착Subitem 이라면 장비를 장착하게 해주기.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    #endregion
}
