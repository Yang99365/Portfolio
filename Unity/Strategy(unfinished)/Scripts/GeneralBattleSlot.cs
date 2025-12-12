using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GeneralBattleSlot : MonoBehaviour, IPointerClickHandler 
/*
, IBeginDragHandler, 
IDragHandler, IEndDragHandler, IDropHandler
*/
{
    private Vector3 originalPosition;

    public int slotNum;
    public Image icon;
    public TextMeshProUGUI generalNameText;
    //public TextMeshProUGUI levelText;

    public GeneralBase general;

    //나중에 마우스 갖다대면 그 무장 툴팁띄우기 언젠간 구현..

    //public bool isInteractable = true; 토글용(나중에 병종으로 토글)
    public bool isSelected = false;
    public GameObject SelectBorder;

    // Start is called before the first frame update
    void Start()
    {
        //setBattleUI = FindObjectOfType<SetBattleUI>();
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
        
    }

    public void ClearSlot()
    {
        general = null;
        generalNameText.text = "";
        generalNameText.gameObject.SetActive(false);
        //levelText.gameObject.SetActive(false);
        icon.sprite = null;
        icon.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //if (!isInteractable) return;
        if (general != null)
        {
            if (general.hp <= 0)
            {
                // 체력이 0이면 그냥 비활성화처럼 처리하는게 나을지도?(초상화에 회색필터같은거 씌우기)
                Debug.Log("체력이 0인 장군입니다.");
                return;
            }
            if (isSelected) // 클릭했을때, 이미 선택된 상태라면 선택풀고, 컨테이너에서 넣은거 꺼냄
            {
                isSelected = false;
                SelectBorder.SetActive(isSelected);
                BattleContainer.instance.RemoveGeneral(general, true);        
            }
            else
            {
                if (BattleContainer.instance.playerGeneral >= 3) return; // 장군이 꽉 찼다면 리턴
                // 이 슬롯을 선택하고 SelectedSlot을 업데이트
                isSelected = true;
                SelectBorder.SetActive(isSelected);
                BattleContainer.instance.AddGeneral(general, true);
            }
        }
    }

    /* 
    //엄.. 왜 드래그해서 레이어에 집어넣어도 BattleContainer에 갱신도안되고
    //드래그하면 했던 슬롯이 먹통이 되는거지?
    // 굳이 안만들어도 클릭으로 기능구현은 됬으니 패스

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (general != null && isSelected == false)
        {
            originalPosition = icon.transform.position; // 원래 위치 저장
            icon.transform.SetParent(transform.parent.parent);

            draggedBattleGeneralUI = new GameObject("DraggedBattleGeneralUI");
            draggedBattleGeneralUI.transform.SetParent(canvas.transform, false); // 캔버스에 추가
            Image image = draggedBattleGeneralUI.AddComponent<Image>();
            image.sprite = icon.sprite; // 아이콘 설정
            image.raycastTarget = false; // 레이캐스트 타겟 비활성화
            RectTransform rectTransform = draggedBattleGeneralUI.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(230, 230); // 크기 설정
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        if (general != null && isSelected == false)
        {
            icon.transform.position = eventData.position;


            // 드래그 위치 업데이트
            draggedBattleGeneralUI.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드래그 불가능
        icon.transform.position = originalPosition; // 아이콘을 원래 위치로 되돌림
        icon.transform.SetParent(transform); // 부모를 원래대로 설정
        

        // 드래그 종료 처리
        Destroy(draggedBattleGeneralUI); // UI 제거
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("OnDrop");
        //if (!isInteractable) return; // 상호작용 불가능한 슬롯이면 드롭 불가능
        // 드롭된 곳이 선택장군 슬롯이라면
        int battleGeneralsLayer = LayerMask.NameToLayer("BattleGenerals");
        GameObject droppedPanel = eventData.pointerDrag;
        if (droppedPanel != null && droppedPanel.layer == battleGeneralsLayer && isSelected == false)
        {   
            isSelected = true;
            SelectBorder.SetActive(isSelected);
            BattleContainer.instance.AddGeneral(general, true);
        }
    }
    */
}
