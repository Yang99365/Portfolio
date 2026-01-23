using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class ShopManager
{
    public Data.ShopTableData CurrentShopTable { get; private set; }
    public int RerollCost = 50;

    public delegate void ShopChanged();
    public event ShopChanged OnShopChanged;

    private List<Item> _shopItems = new List<Item>();
    private bool _isInitialized = false;

    public void Initialize()
    {
        if (_isInitialized)
            return;

        UpdateShopTableByStage();
        RerollShop();
        _isInitialized = true;
    }


    public bool RerollShop()
    {
        UpdateShopTableByStage();
        _shopItems.Clear();

        if (CurrentShopTable == null || CurrentShopTable.items == null)
        {
            Debug.LogError("Shop table is null or has no items!");
            return false;
        }

        // 확률 기반으로 아이템 생성
        foreach (var shopItemData in CurrentShopTable.items)
        {
            if (Random.Range(0f, 100f) < shopItemData.probability)
            {
                // ItemSaveData 생성
                ItemSaveData itemSaveData = new ItemSaveData
                {
                    instanceId = Managers.Game.GenerateItemInstanceId(),
                    templateId = shopItemData.itemId,
                    count = shopItemData.amount,
                    equipSlot = -1 // 상점 아이템은 장착 안 된 상태
                };

                Item item = Item.CreateItem(itemSaveData);
                if (item != null)
                {
                    _shopItems.Add(item);
                }
            }
        }

        OnShopChanged?.Invoke();
        return true;
    }


    private void UpdateShopTableByStage()
    {
        int currentStage = Managers.Battle.CurrentStageNumber;

        // 현재 스테이지에 맞는 테이블 찾기
        Data.ShopTableData selectedTable = null;
        foreach (var table in Managers.Data.ShopTableDic.Values)
        {
            if (currentStage >= table.minStage &&
                (table.maxStage == -1 || currentStage <= table.maxStage))
            {
                selectedTable = table;
                break;
            }
        }

        if (selectedTable == null)
        {
            if (Managers.Data.ShopTableDic.TryGetValue(1, out Data.ShopTableData defaultTable))
            {
                selectedTable = defaultTable;
            }
            else
            {
                Debug.LogError($"No shop table found for stage: {currentStage}");
                return;
            }
        }

        CurrentShopTable = selectedTable;
    }

    public bool BuyItem(int instanceId)
    {
        Item item = _shopItems.Find(i => i.InstanceId == instanceId);
        if (item == null)
        {
            Debug.LogError("Item not found in shop");
            return false;
        }

        int totalPrice = item.ItemData.itemPrice * item.Count;
        if (Managers.Game.Gold < totalPrice)
        {
            Debug.Log("Not enough gold to buy the item");
            return false;
        }

        // 골드 차감
        Managers.Game.Gold -= totalPrice;

        // 인벤토리에 추가
        Managers.Inventory.MakeItem(item.DataId, item.Count);

        // 상점 목록에서 제거
        _shopItems.Remove(item);

        OnShopChanged?.Invoke();
        return true;
    }

    public List<Item> GetShopItems()
    {
        return new List<Item>(_shopItems);
    }
}
