using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, 
IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GeneralListUI generalListUI;
    //shop
    public bool isShopMode; // 참이면 판매모드, 거짓이면 구매모드
    public bool isSell=false;
    public GameObject chkSell;


    private Vector3 originalPosition;

    public int slotNum;
    public Image icon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI amountText;
    public GameObject Background;

    public GItemSO item;
    
    public Button removeButton;

    private GameObject draggedItemUI;
    private Canvas canvas;

    private SlotToolTip slotToolTip;
    
    public bool isInteractable = true;

    
    
    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>(); // 캔버스 찾기
        slotToolTip = FindObjectOfType<SlotToolTip>();
        generalListUI = FindObjectOfType<GeneralListUI>();
        DontDestroyOnLoad(gameObject);
    }
    public void UpdateSlotUI(GItemSO newitem)
    {
        if (newitem == null)
        {
            ClearSlot();
            return;
        }
        item = newitem;
        itemNameText.gameObject.SetActive(true);
        itemNameText.text = item.itemName;
        icon.gameObject.SetActive(true);
        icon.sprite = item.ItemImage;
        if (item.Type != ItemType.Ownable && item.Type != ItemType.Equipment)
        { 
            //Debug.Log("아이템 타입이 소지품이거나 장비가 아닙니다.");
            amountText.gameObject.SetActive(true);
            amountText.text = item.amount.ToString();
            Background.SetActive(true);
        }
        else
        {
            amountText.gameObject.SetActive(false);
            Background.SetActive(false);
        }
    }
    public void ClearSlot()
    {
        item = null;
        itemNameText.text = "";
        itemNameText.gameObject.SetActive(false);
        amountText.text = "";
        amountText.gameObject.SetActive(false);
        Background.SetActive(false);
        icon.sprite = null;
        icon.gameObject.SetActive(false);
        
    }
    public void OnRemoveButton()
    {
        if(item != null)
        {
            Inventory.instance.RemoveItem(slotNum);
            ClearSlot();
        }
    }

    public void SetActiveState(bool state)
    {
        
        if (state)
        {
            SetIconTransparency(1f); // 완전 불투명
        }
        else
        {
            SetIconTransparency(0.5f); // 반투명
        }
        isInteractable = state; // 상호작용 가능 여부 설정
    }
    // 아이콘의 투명도를 설정하는 메서드
    private void SetIconTransparency(float alpha)
    {
        if (icon != null)
        {
            Color color = icon.color;
            color.a = alpha; // 알파 값 조정
            icon.color = color;
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 클릭 불가능
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (item != null)
            {
                if(!isShopMode)
                {
                    bool isUsed = item.Use();
                    if (isUsed)
                    {
                        if (item.Type == ItemType.Equipment)
                        {
                            
                            if (generalListUI.SelectedSlot==null)
                            {
                                Debug.Log("장군을 선택해주세요.");
                                return;
                            }
                            if (generalListUI.SelectedSlot.general != null)
                            {
                                GeneralBase general = generalListUI.SelectedSlot.general;
                                
                                if (general.equipment1 == null || general.equipment2 == null)
                                {
                                    general.EquipItem(item.Code);
                                }
                                else if (general.equipment1 != null && general.equipment2 != null)
                                {
                                    Debug.Log("모든 장비 슬롯이 이미 사용 중입니다.");
                                    return;
                                }
                                
                                ClearSlot();
                                Inventory.instance.RemoveItem(slotNum); 
                                Debug.Log("장비를 장착했습니다.");
                                generalListUI.generalDescUI.SetDescription(general.portrait, general.name, general.atk, general.def, general.spd, null, null,null, general.equipment1, general.equipment2);
                                return;
                            }
                            
                        }
                        if (item.Type == ItemType.Consumable)
                        {
                            if (item.amount == 0)//아이템을 모두 사용했을 때(소모품)
                            {
                                ClearSlot();
                                Inventory.instance.RemoveItem(slotNum);
                            }
                            else
                            {
                                UpdateSlotUI(item);
                            }
                        }
                        
                    }
                }
                else
                {
                    // 판매모드일때(상점을 켜놨을때)
                    // 아이템을 판매한다. 우클릭으로 체크한
                    // 판매가격은 아이템의 가격의 50%이다.
                    // 판매가격은 소수점 이하를 버린다.
                    isSell = true;
                    chkSell.SetActive(isSell);
                    
                }
            }
        }
    }
    public void SellItem() // 아이템이 단일 아이템일때
    {
        if(isSell)
        {
            if (item.Type == ItemType.Equipment) // 장비는 단일 아이템
            {
                int sellPrice = (int)(item.price * 0.5f);
                FactionManager.instance.playerFaction.gold += sellPrice;
                Inventory.instance.RemoveItem(slotNum);
                isSell = false;
                chkSell.SetActive(isSell);
                ClearSlot();
            }
            else if (item.Type == ItemType.Consumable) // 소모품은 다중 아이템
            {
                int sellPrice = (int)(item.price * 0.5f);
                FactionManager.instance.playerFaction.gold += sellPrice*item.amount;
                Inventory.instance.RemoveItem(slotNum);
                isSell = false;
                chkSell.SetActive(isSell);
                ClearSlot();
            }
            else if (item.Type == ItemType.Ownable) // 소지품은 단일 아이템
            {
                int sellPrice = (int)(item.price * 0.5f);
                FactionManager.instance.playerFaction.gold += sellPrice;
                Inventory.instance.RemoveItem(slotNum);
                isSell = false;
                chkSell.SetActive(isSell);
                ClearSlot();
            }
            
        }
    }
    

    private void OnDisable() {
        isSell = false;
        chkSell.SetActive(isSell);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능

        if (item != null)
        {
            originalPosition = icon.transform.position; // 원래 위치 저장
            icon.transform.SetParent(transform.parent.parent);



            // 드래그 중인 아이템의 UI 생성
            draggedItemUI = new GameObject("DraggedItemUI");
            draggedItemUI.transform.SetParent(canvas.transform, false); // 캔버스에 추가
            Image image = draggedItemUI.AddComponent<Image>();
            image.sprite = icon.sprite; // 아이콘 설정
            image.raycastTarget = false; // 레이캐스트 타겟 비활성화
            RectTransform rectTransform = draggedItemUI.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100); // 크기 설정
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        if (item != null)
        {
            icon.transform.position = eventData.position;


            // 드래그 위치 업데이트
            draggedItemUI.transform.position = eventData.position;
        }

        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        icon.transform.position = originalPosition; // 아이콘을 원래 위치로 되돌림
        icon.transform.SetParent(transform); // 부모를 원래대로 설정
        

        // 드래그 종료 처리
        Destroy(draggedItemUI); // UI 제거
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드롭 불가능
        Slot droppedSlot = eventData.pointerDrag.GetComponent<Slot>();
        
        if (droppedSlot != null && droppedSlot != this && droppedSlot.isInteractable)
        {   
            if (droppedSlot != this && this.item == null) // 현재 슬롯이 비어있는 경우
            {
                // 드롭된 아이템을 현재 슬롯으로 이동
                this.item = droppedSlot.item;
                droppedSlot.ClearSlot(); // 원래 슬롯 비우기
                UpdateSlotUI(this.item); // 새로운 슬롯 UI 업데이트

                // 'items' 리스트 업데이트
                Inventory.instance.items[this.slotNum] = this.item;
                Inventory.instance.items[droppedSlot.slotNum] = null;
            }
            else if (droppedSlot != this) // 현재 슬롯이 비어있지 않은 경우
            {
                // 아이템 교환 로직
                Inventory.instance.SwapItems(droppedSlot.slotNum, this.slotNum);

            }
            
        }
        // 나중에 드롭하는 위치가 장군 목록창에 있는 장군의
        // 장비창이라면 장비창에 장비를 옮기는 코드를 추가한다.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 시작한 슬롯이면 보여주지 않기
        if (eventData.pointerDrag == null && item != null)
        {
            slotToolTip.ShowToolTip(item ,transform.position);
        }


        /*
        if (item != null)
        {
            slotToolTip.ShowToolTip(item ,transform.position);
        }
        */
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        slotToolTip.HideToolTip();
    }
}
