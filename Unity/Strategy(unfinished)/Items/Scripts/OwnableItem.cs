using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ownable Item", menuName = "Inventory/Items/Ownable")]
public class OwnableItem : GItemSO
{
    public int goldIncrease;
    public int GeneralDamageIncrease;

    public void Awake()
    {
        Type = ItemType.Ownable;
        isStackable = false;
    }
    
}
