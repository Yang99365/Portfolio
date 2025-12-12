using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.VisualScripting;

public class GeneralSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, 
IDragHandler, IEndDragHandler, IDropHandler
{
    private Vector3 originalPosition;

    public int slotNum;
    public Image icon;
    public Image troopIcon;
    public TextMeshProUGUI generalNameText;
    //public TextMeshProUGUI levelText;
    public GameObject Background; // 없어도..될듯?

    public GeneralBase general;
    public GeneralListUI generalListUI;
    //public GeneralDescUI generalDesc; 이변수 쓰지말고 slotNum을 보내거나 가져와서 해당 장군의 정보를 보여주는 함수를 만들어야함

    private GameObject draggedGeneralUI;
    private Canvas canvas;
    

    //public bool isInteractable = true; 토글용(나중에 병종으로 토글)
    public bool isSelected = false;
    public GameObject SelectBorder;

    
    private void OnDisable() {
        isSelected = false;
        SelectBorder.SetActive(isSelected);
    }
    

    private void Awake()
    {
        canvas = FindObjectOfType<Canvas>(); // 캔버스 찾기
        generalListUI = FindObjectOfType<GeneralListUI>();
    }

    public void UpdateSlotUI(GeneralBase newGeneral)
    {
        if (newGeneral == null)
        {
            ClearSlot();
            return;
        }
        if (isSelected)
        {
            SelectBorder.SetActive(isSelected);
        }
        general = newGeneral;
        generalNameText.gameObject.SetActive(true);
        generalNameText.text = general.name;
        //levelText.gameObject.SetActive(true);
        //levelText.text = "Lv." + general.level.ToString();
        icon.gameObject.SetActive(true);
        icon.sprite = general.portrait;
        troopIcon.gameObject.SetActive(true);
        troopIcon.sprite = general.troopImage;
        Background.SetActive(true);
    }

    public void ClearSlot()
    {
        general = null;
        generalNameText.text = "";
        generalNameText.gameObject.SetActive(false);
        //levelText.gameObject.SetActive(false);
        icon.sprite = null;
        icon.gameObject.SetActive(false);
        troopIcon.sprite = null;
        troopIcon.gameObject.SetActive(false);
        Background.SetActive(false);
    }
    /*
    generalListUI에서 토글을 누르면 해당 병종만 보이게 하기 위해 추가한 함수
    아니면 그냥 업데이트 슬롯으로 해당 병종이 있는 슬롯만 보이게 하면 될 듯
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
    private void SetIconTransparency(float alpha)
    {
        if (icon != null)
        {
            Color color = icon.color;
            color.a = alpha; // 알파 값 조정
            icon.color = color;
        }
    }
    */
    public void OnPointerClick(PointerEventData eventData)
    {
        //우클릭을 하면 해당 장군의 아이템을 모두 해제하는 기능
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (general != null)
            {
                Inventory.instance.SpaceFull();
                if(Inventory.instance.isFull)
                {
                    Debug.Log("인벤토리 공간이 모자랍니다.");
                    return;
                }
                //장군의 장비를 모두 해제하고 해제한건 인벤토리로, Null은 제외
                if (general.equipment1 != null && !Inventory.instance.isFull)
                {
                    Inventory.instance.Add(general.equipment1);
                    general.UnequipItem(general.equipment1.Code);
                    general.equipment1 = null;
                    Inventory.instance.SpaceFull();
                }
                if (general.equipment2 != null && !Inventory.instance.isFull)
                {
                    Inventory.instance.Add(general.equipment2);
                    general.UnequipItem(general.equipment2.Code);
                    general.equipment2 = null;
                    Inventory.instance.SpaceFull();
                }
                generalListUI.generalDescUI.SetDescription(general.portrait, general.name, general.atk, general.def, general.spd, null, null,null, general.equipment1, general.equipment2);
                return; // 장비 해제 후 리턴
            }
        }
        //if (!isInteractable) return;
        if (general != null)
        {
            if (isSelected) // 클릭했을때, 이미 선택된 상태라면
            {
                generalListUI.generalDescUI.ResetDescription();
                isSelected = false;
                SelectBorder.SetActive(isSelected);
                // GeneralDesc도 해제 아직 안만듬
                // 선택이 해제된 경우 SelectedSlot을 null로 설정
                if (generalListUI.SelectedSlot == this)
                {
                    generalListUI.SelectedSlot = null;
                }
            }
            else
            {
                // GeneralDescUI를 업데이트 Null 부분은 스킬인데 스킬을 넘겨서 그 스킬의 Desc를 호출하도록 해야함
                generalListUI.generalDescUI.SetDescription(general.portrait, general.name, general.atk, general.def, general.spd, null, null,null, general.equipment1, general.equipment2);
                // 다른 슬롯이 이미 선택되어 있는 경우 그 슬롯의 선택을 해제
                if (generalListUI.SelectedSlot != null)
                {
                    generalListUI.SelectedSlot.isSelected = false;
                    generalListUI.SelectedSlot.SelectBorder.SetActive(false);
                    // GeneralDesc도 해제 아직 안만듬
                }

                // 이 슬롯을 선택하고 SelectedSlot을 업데이트
                isSelected = true;
                SelectBorder.SetActive(isSelected);
                // GeneralDesc도 활성화 아직 안만듬 UpdateDescUI로
                generalListUI.SelectedSlot = this;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (general != null)
        {
            originalPosition = icon.transform.position; // 원래 위치 저장
            icon.transform.SetParent(transform.parent.parent);

            draggedGeneralUI = new GameObject("DraggedGeneralUI");
            draggedGeneralUI.transform.SetParent(canvas.transform, false); // 캔버스에 추가
            Image image = draggedGeneralUI.AddComponent<Image>();
            image.sprite = icon.sprite; // 아이콘 설정
            image.raycastTarget = false; // 레이캐스트 타겟 비활성화
            RectTransform rectTransform = draggedGeneralUI.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100); // 크기 설정
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        if (general != null)
        {
            icon.transform.position = eventData.position;


            // 드래그 위치 업데이트
            draggedGeneralUI.transform.position = eventData.position;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        icon.transform.position = originalPosition; // 아이콘을 원래 위치로 되돌림
        icon.transform.SetParent(transform); // 부모를 원래대로 설정
        

        // 드래그 종료 처리
        Destroy(draggedGeneralUI); // UI 제거
    }
    public void OnDrop(PointerEventData eventData)
    {
        generalListUI.DeselectAllSlot();
        
        generalListUI.generalDescUI.ResetDescription();
        Debug.Log("OnDrop");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드롭 불가능
        // 드롭된 곳이 슬롯이라면 GeneralSlot으로 캐스팅
        GeneralSlot droppedSlot = eventData.pointerDrag.GetComponent<GeneralSlot>();
        
        if (droppedSlot != null && droppedSlot != this /*&& droppedSlot.isInteractable*/)
        {   
            if (droppedSlot != this && this.general == null) // 현재 슬롯이 비어있는 경우
            {
                // 드롭된 아이템을 현재 슬롯으로 이동
                this.general = droppedSlot.general;
                droppedSlot.ClearSlot(); // 원래 슬롯 비우기
                UpdateSlotUI(this.general); // 새로운 슬롯 UI 업데이트

                // 'generals' 리스트 업데이트
                GeneralManager.instance.PlayerFactionGenerals[this.slotNum] = this.general;
                GeneralManager.instance.PlayerFactionGenerals[droppedSlot.slotNum] = null;
                

                generalListUI.generalDescUI.ResetDescription();
                isSelected = true;
                SelectBorder.SetActive(isSelected);
                generalListUI.SelectedSlot = this;
                generalListUI.generalDescUI.SetDescription(general.portrait, general.name, general.atk, general.def, general.spd, null, null,null, general.equipment1, general.equipment2);
                
            }
            else if (droppedSlot != this) // 현재 슬롯이 비어있지 않은 경우
            {
                GeneralManager.instance.SwapGeneral(droppedSlot.slotNum, this.slotNum);
                // 아이템 교환 로직
            }
            
        }
        // 나중에 드롭하는 위치가 장군 목록창에 있는 장군의
        // 장비창이라면 장비창에 장비를 옮기는 코드를 추가한다.
    }
    public void DeactiveSelectBorder()
    {
        SelectBorder.SetActive(false);
    }

}
