using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ShopManager
{
    public Data.ShopTableData shopTableData;
    public EShopMode ShopMode;
    public int RerollCost = 100;

    public delegate void ShopChanged();
    public static event ShopChanged OnShopChanged;

    private List<Item> shopItems = new List<Item>();

    public void RerollShop()
    {
        ChangeShopTable();
        shopItems.Clear();
        foreach (var itemData in shopTableData.Items)
        {
            if (Random.Range(0f, 100f) < itemData.Probability)
            {
                Item item = Item.CreateItem(itemData.ItemID, itemData.Amount);
                if (item != null)
                {
                    shopItems.Add(item);
                }
            }
        }
        //Managers.UI.ShopUI.SetShop(shopList);
        OnShopChanged?.Invoke();
    }

    public void ChangeShopTable()
    {
        EGamePhase gamePhase = Managers.Turn.gamePhase;
        int tableId = (int)gamePhase + 1;
        if (Managers.Data.ShopTableDic.TryGetValue(tableId, out Data.ShopTableData newShopTableData))
        {
            shopTableData = newShopTableData;
        }
        else
        {
            Debug.LogError($"Shop table not found for game phase: {gamePhase}");
        }
    }

    // 아래는 아직 시험안함
    public bool BuyItem(int instanceId)
    {
        Item item = shopItems.Find(i => i.InstanceId == instanceId);
        if (item == null)
        {
            Debug.LogError("Item not found in shop");
            return false;
        }

        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);

        if (playerFaction != null && playerFaction.CanSpendGold(item.ItemData.itemPrice * item.Count))
        {
            playerFaction.SpendGold(item.ItemData.itemPrice * item.Count);
            Managers.Inventory.AddItem(item.DataId, item.Count);
            shopItems.Remove(item);

            OnShopChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.Log("Not enough gold to buy the item");
            return false;
        }
    }

    public bool SellItem(int instanceId)
    {
        Item item = Managers.Inventory.GetItem(instanceId);
        if (item == null)
        {
            Debug.LogError($"Item with instanceId {instanceId} not found in inventory");
            return false;
        }

        int sellPrice = (int)(item.ItemData.itemPrice * item.Count * 0.5f);
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);

        if (playerFaction != null)
        {
            playerFaction.ReceiveGold(sellPrice);
            Managers.Inventory.RemoveItem(instanceId);

            OnShopChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.LogError("Player faction not found");
            return false;
        }
    }
    public List<Item> GetShopItems()
    {
        return new List<Item>(shopItems);
    }

    public void SetShopMode(EShopMode mode)
    {
        ShopMode = mode;
        OnShopChanged?.Invoke();
    }

}
