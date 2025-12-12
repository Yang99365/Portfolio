using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Inventory/Items/Consumable")]
public class ConsumItem : GItemSO
{
    public enum ConsumType
    {
        Potion,
        Doffing
    }
    public ConsumType consumType;
    public int healAmount;
    public int atkBonus;
    public int defBonus;
    public int intBonus;
    public int spdBonus;
    public int actionBonus;


    public void Awake()
    {
        Type = ItemType.Consumable;
        isStackable = true;
    }
}
