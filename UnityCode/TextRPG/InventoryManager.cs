using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager
{
    #region Properties

    private List<GameManager.ItemSlot> Inventory => Managers.Game.SaveData.inventory.items;

    #endregion

    #region Events

    public event Action<int, int> OnItemChanged; // itemId, newCount

    #endregion

    #region Initialization

    public void Init()
    {
        Debug.Log($"InventoryManager Initialized - {Inventory.Count} items");
    }

    #endregion

    #region Item Management

    public int GetItemCount(int itemId)
    {
        var slot = Inventory.FirstOrDefault(s => s.itemId == itemId);
        return slot?.count ?? 0;
    }

    public bool HasItem(int itemId, int count = 1)
    {
        return GetItemCount(itemId) >= count;
    }

    public bool AddItem(int itemId, int count = 1)
    {
        if (count <= 0)
            return false;

        var itemData = Managers.Data.ItemDict.GetValueOrDefault(itemId);
        if (itemData == null)
        {
            Debug.LogError($"Item {itemId} not found in data!");
            return false;
        }

        var slot = Inventory.FirstOrDefault(s => s.itemId == itemId);

        if (slot != null)
        {
            // Stack existing item
            slot.count = Mathf.Min(slot.count + count, itemData.maxStack);
        }
        else
        {
            // Add new slot
            Inventory.Add(new GameManager.ItemSlot
            {
                itemId = itemId,
                count = Mathf.Min(count, itemData.maxStack)
            });
        }

        OnItemChanged?.Invoke(itemId, GetItemCount(itemId));
        Debug.Log($"Added item {itemData.itemName} x{count}");

        Managers.Game.SaveGame();
        return true;
    }

    public bool ConsumeItem(int itemId, int count = 1)
    {
        if (count <= 0)
            return false;

        var slot = Inventory.FirstOrDefault(s => s.itemId == itemId);

        if (slot == null || slot.count < count)
        {
            Debug.LogWarning($"Not enough item {itemId}! Have: {slot?.count ?? 0}, Need: {count}");
            return false;
        }

        slot.count -= count;

        // Remove empty slot
        if (slot.count <= 0)
        {
            Inventory.Remove(slot);
        }

        OnItemChanged?.Invoke(itemId, GetItemCount(itemId));

        var itemData = Managers.Data.ItemDict.GetValueOrDefault(itemId);
        Debug.Log($"Consumed item {itemData?.itemName ?? itemId.ToString()} x{count}");

        Managers.Game.SaveGame();
        return true;
    }

    public bool UseItem(int itemId)
    {
        var itemData = Managers.Data.ItemDict.GetValueOrDefault(itemId) as Data.ConsumableItemData;

        if (itemData == null)
        {
            Debug.LogWarning($"Item {itemId} is not a consumable!");
            return false;
        }

        if (!HasItem(itemId))
        {
            Debug.LogWarning($"Don't have item {itemId}!");
            return false;
        }

        // Apply effects
        if (itemData.hpRestore > 0)
        {
            Managers.Player.RestoreHp(itemData.hpRestore);
        }

        if (itemData.mpRestore > 0)
        {
            Managers.Player.RestoreMp(itemData.mpRestore);
        }

        // Consume item
        ConsumeItem(itemId);

        return true;
    }

    #endregion

    #region Debug

    public void DebugInventory()
    {
        Debug.Log("========== Inventory ==========");
        foreach (var slot in Inventory)
        {
            var itemData = Managers.Data.ItemDict.GetValueOrDefault(slot.itemId);
            Debug.Log($"[{slot.itemId}] {itemData?.itemName ?? "Unknown"} x{slot.count}");
        }
        Debug.Log("==============================");
    }

    #endregion
}
