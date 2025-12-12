using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class SelectedBattleSlot : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TextMeshProUGUI generalNameText;
    
    public GeneralBase general;
    public SetBattleUI setBattleUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        //무장을 클릭하면 그 무장을 BattleContainer에서 RemoveGeneral로 제거하고, 
        //GeneralBattleSlot의 General을 찾아 isSelected를 false로 바꾸고, 
        //SelectBorder를 비활성화한다.
        // 근데 위의 소유무장 클릭해도 다시 풀리는데
        // 귀찮으니 구현 안해도 될듯?
    }
    public void UpdateSlotUI(GeneralBase newGeneral)
    {
        if (newGeneral == null)
        {
            ClearSlot();
            return;
        }
        general = newGeneral;
        generalNameText.gameObject.SetActive(true);
        generalNameText.text = general.name;
        icon.gameObject.SetActive(true);
        icon.sprite = general.portrait;
    }
    public void ClearSlot()
    {
        general = null;
        generalNameText.text = "";
        generalNameText.gameObject.SetActive(false);
        icon.sprite = null;
        icon.gameObject.SetActive(false);
    }
}
