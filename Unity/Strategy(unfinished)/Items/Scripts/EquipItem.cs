using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equip Item", menuName = "Inventory/Items/Equip Item")]
public class EquipItem : GItemSO
{
    public enum EquipType
    {
        Weapon,
        Armor,
        Accessory
    }
    public enum EquipSlot
    {
        Head = 0,
        Body = 1,
        Hand = 2,
        Foot = 3,
        Accessory = 4,
        MainWeapon = 5,
        SubWeapon = 6
    }
    
    public EquipType equipType;
    public EquipSlot equipSlot;
    public float atkBonus;
    public float defBonus;
    public float intBonus;
    public float spdBonus;
    public float actionBonus;
    
    public void Awake()
    {
        Type = ItemType.Equipment;
        isStackable = false;
    }
}
