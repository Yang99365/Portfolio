using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Equipment,
    Consumable,
    Ownable
}
public enum rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
public class GItemSO : ScriptableObject
{

    



    public string itemName;
    public string Code;
    public ItemType Type;
    public Sprite ItemImage;

    public bool isStackable;

    public int MaxStack;
    public int amount;
    public int price;
    public int level; // 착용 레벨 , 소모품의 경우 사용 레벨 , 소지품의 경우 0
    public rarity rarityType;

    public List<ItemEffect> itemEffects;
    

    
    
    [TextArea(15, 20)]
    public string Description;

    public bool Use()
    {
        bool isUsed = false;
        switch (Type)
        {
            case ItemType.Equipment:
                isUsed = true;
                
                // 장비는 장착하면 인벤토리 속 장비를 장군의 장비창에 옮겨서 장착한다.
                // 장비창에 장비가 이미 있다면 장비창의 장비를 인벤토리로 옮기고 장비창에 장비를 장착한다.
                // 장비를 해제할땐 장비창의 장비를 인벤토리로 옮긴다. 해제는 Use()가 아닌 다른 함수로 구현한다.
                //Debug.Log("장비를 장착했습니다.");
                return isUsed;

            case ItemType.Consumable:
                Debug.Log("소모품을 사용했습니다.");
                foreach (var itemEffect in itemEffects)
                {
                    isUsed = itemEffect.ExecuteRole();
                    amount--;
                }
                return isUsed;

            case ItemType.Ownable:
                Debug.Log("소지품을 사용했습니다.");
                return isUsed;
            default:
                return isUsed;
        }
    }
}
