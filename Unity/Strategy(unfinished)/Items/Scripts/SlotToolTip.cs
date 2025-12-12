using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotToolTip : MonoBehaviour
{
    [SerializeField] 
    private GameObject toolTip;
    [SerializeField]
    private Text itemNameText;
    [SerializeField]
    private Text itemDescriptionText;
    [SerializeField]
    private Text itemUseText;

    public void ShowToolTip(GItemSO item, Vector3 pos)
    {
        toolTip.SetActive(true);
        pos += new Vector3(toolTip.GetComponent<RectTransform>().rect.width *0.6f, -toolTip.GetComponent<RectTransform>().rect.height * 0.6f, 0f);
        toolTip.transform.position = pos;
        itemNameText.text = item.itemName;
        if(item.Type == ItemType.Equipment)
        { // 설명텍스트에 임시로 일단 장비타입과 장비슬롯을 추가
          // 나중에 장비타입과 장비슬롯의 설명은 다른 텍스트로 분리해서 추가해야함(오른쪽위 구석이나 아래쪽 구석에)
            EquipItem equipItem = item as EquipItem; // Cast item to EquipItem
            itemDescriptionText.text = item.Description + "\n" + "희귀도" + item.rarityType + 
            "\n" + "장비 유형: " + equipItem.equipType + "\n" + "장비 슬롯: " + equipItem.equipSlot;
        }
        else if(item.Type == ItemType.Consumable)
        {
            itemDescriptionText.text = item.Description;
        }
        else if(item.Type == ItemType.Ownable)
        {
            itemDescriptionText.text = item.Description;
        }
        else
        {
            itemDescriptionText.text = "알수없음";
        }

        if (item.Type == ItemType.Consumable)
        {
            itemUseText.text = "우클릭해서 사용할 무장 선택";
        }
        else if (item.Type == ItemType.Equipment)
        {
            itemUseText.text = "우클릭으로 장착할 무장 선택";
        }
        else if (item.Type == ItemType.Ownable)
        {
            itemUseText.text = "인벤토리에 보유 시 효과 발동";
        }
        else
        {
            itemUseText.text = "알수없음";
        }
    }
    public void HideToolTip()
    {
        toolTip.SetActive(false);
    }
}
