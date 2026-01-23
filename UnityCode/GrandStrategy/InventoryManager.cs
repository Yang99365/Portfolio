using Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static InventoryManager;

public class InventoryManager
{
    public delegate void ItemChanged();
    public static event ItemChanged OnItemChanged;

    public readonly int DEFAULT_INVENTORY_SLOT_COUNT = 30;
    public bool isFull = false;

    //public bool SellMode = false;
    public List<Item> MyItems { get; } = new List<Item>();
    public List<Item> ViewItems { get; } = new List<Item>();
    public List<Item> EquipmentItems { get; } = new List<Item>();
    public List<Item> ConsumableItems { get; } = new List<Item>();
    public List<Item> MaterialItems { get; } = new List<Item>();

    public EInventoryGroupType ToggleType = EInventoryGroupType.All;

    #region Toggle
    //public void ToggleSellMode()
    //{
    //    SellMode = !SellMode;
    //}


    
    public void ToggleEquipItem()
    {
        
        ToggleType = EInventoryGroupType.Equipment;
        
    }
    public void ToggleConsumableItem()
    {
        ToggleType = EInventoryGroupType.Consumable;
        
    }
    public void ToggleMaterialItem()
    {
        ToggleType = EInventoryGroupType.Material;
        
    }

    public void ToggleAllItem()
    {
        ToggleType = EInventoryGroupType.All;
        
    }
    #endregion
    public InventoryManager()
    {
        // 생성자에서 슬롯 초기화
        InitializeSlots();
    }
    private void InitializeSlots()
    {
        // 기존 아이템 목록 클리어
        MyItems.Clear();

        // DEFAULT_INVENTORY_SLOT_COUNT만큼 null로 채워서 슬롯 초기화
        for (int i = 0; i < DEFAULT_INVENTORY_SLOT_COUNT; i++)
        {
            MyItems.Add(null);
        }
    }
    public bool AddItem(int templateID) //장비 해제 또는 휙득시에 사용
    {
        bool isAdded;
        Item item = Item.CreateItem(templateID); // 아이템 생성 1개
        if (item.ItemData.MaxStack > 1) //스택이 가능한 아이템
        {
            isAdded = AddStackableItem(templateID, item.Count);
        }
        else // 스택이 불가능한 아이템
        {
            int emptySlotIndex = MyItems.FindIndex(x => x == null);
            if (emptySlotIndex == -1)
            {
                Debug.Log("인벤토리가 가득 찼습니다.");
                return false;
            }
            if (emptySlotIndex != -1)
            {
                // 빈 슬롯에 추가
                MyItems[emptySlotIndex] = item;
            }
            else
            {
                // 리스트에 추가
                MyItems.Add(item);
            }
            isAdded = true;
        }

        Debug.Log("아이템이 추가되었습니다.");
        OnItemChanged?.Invoke();
        // 리스트에 넣고 아이템을 반환
        return isAdded;
    }

    public bool AddItem(int templateID, int count)
    {
        bool isAdded = false;
        Item item = Item.CreateItem(templateID, count); // 아이템 생성 1개
        if (item.ItemData.MaxStack > 1) //스택이 가능한 아이템
        {
            isAdded = AddStackableItem(templateID, count);
        }
        else // 스택이 불가능한 아이템
        {
           for(int i = 0; i < count; i++)
            {
                //빈 슬롯 찾기
                int emptySlotIndex = MyItems.FindIndex(x => x == null);
                if (emptySlotIndex == -1)
                {
                    isFull = true;
                    Debug.Log("공간이 모자라 아이템 휙득에 실패했습니다.");
                    isAdded = false;
                }
                else
                {
                    item.Count = 1;
                    // 빈 슬롯에 아이템 추가
                    MyItems[emptySlotIndex] = item;
                    isAdded = true;
                }
                
            }
        }

        Debug.Log("아이템이 추가되었습니다.");
        OnItemChanged?.Invoke();
        // 리스트에 넣고 아이템을 반환
        return isAdded;
    }

    private bool AddStackableItem(int templateID, int count)
    {
        int overflowCount = 0;
        Item item = Item.CreateItem(templateID, count);
        foreach (Item i in MyItems)
        {
            // 아이템이 존재하고, 템플릿 아이디가 같고, 아이템이 가득차지 않았을 때
            if (i != null && i.DataId == item.DataId && i.Count < item.ItemData.MaxStack)
            {
                int remainCount = item.ItemData.MaxStack - i.Count;
                if (count > remainCount)
                {
                    i.Count = item.ItemData.MaxStack;
                    count -= remainCount;
                    overflowCount += count;
                }
                else
                {
                    i.Count += count;
                    return true;
                }
            }
        }

        while (count > 0)
        {
            // 빈 슬롯 찾기
            int emptySlotIndex = MyItems.FindIndex(x => x == null);
            if (emptySlotIndex == -1)
            {
                Debug.Log(MyItems.Count + " " + DEFAULT_INVENTORY_SLOT_COUNT);
                Debug.Log("공간이 모자라 일부 아이템 휙득에 실패했습니다");
                if (overflowCount > 0)
                {
                    Debug.Log("아이템이 " + overflowCount + "개 넘쳤습니다.");
                }
                return false;
            }
            Item newItem = Item.CreateItem(item.DataId, count);
            newItem.Count = Mathf.Min(count, item.ItemData.MaxStack);
            count -= newItem.Count;
            overflowCount += count;

            MyItems[emptySlotIndex] = newItem;
        }

        Debug.Log("아이템이 추가되었습니다.");
        OnItemChanged?.Invoke();
        return true;

    }

    //public void RemoveItem(int index) // 아이템 판매, 장착, 소비템 고갈 시 사용
    //{
    //    MyItems[index] = null;
    //    IsInventoryFull(); // 인벤이 가득찼는지 확인... 이걸 왜 예전 프로젝트에서 넣었지?
    //    if (OnItemChanged != null)
    //    {
    //        OnItemChanged.Invoke();
    //    }
    //}
    public void UseConsum(int instanceId)
    {
        Item item = MyItems.Find(x => x != null && x.InstanceId == instanceId);
        if (item != null)
        {
            item.Count--;
            if (item.Count <= 0)
            {
                RemoveItem(instanceId);
            }
            OnItemChanged?.Invoke();
        }
    }
    public void RemoveItem(int instanceId)
    {
        int index = MyItems.FindIndex(x => x != null && x.InstanceId == instanceId);
        if (index != -1)
        {
            MyItems[index] = null;
            OnItemChanged?.Invoke();
        }
    }
    public void SwapItems(int index1, int index2 ) // itemSubitme에서 드래그 드랍 사용시 사용
    {
        Item item1 = MyItems[index1]; // drop
        Item item2 = MyItems[index2]; // drag
        if (item1 == null && item2 == null && item1.DataId == item2.DataId && item1.ItemData.MaxStack > 1)
        {
            // 스택 합치기
            int sum = item1.Count + item2.Count;
            if (sum <= item1.ItemData.MaxStack) // 합친 수량이 최대 스택수보다 작을 때
            {
                item2.Count = sum;
                MyItems[index1] = null;
            }
            else
            {
                if(item1.Count >= item2.Count)
                {
                    item1.Count = sum - item1.ItemData.MaxStack;
                    item2.Count = item1.ItemData.MaxStack;
                }
                else
                {
                    item2.Count = sum - item1.ItemData.MaxStack;
                    item1.Count = item1.ItemData.MaxStack;
                }
            }
        }

        else
        {
            Item tempItem = MyItems[index1];
            MyItems[index1] = MyItems[index2];
            MyItems[index2] = tempItem;
        }

        OnItemChanged?.Invoke();
    }
    public void SwapItemForEmptySlot(int index1, int index2)
    {
        Item item1 = MyItems[index1];
        Item item2 = MyItems[index2];
        //어차피 아이템이 들어있는 공간만 Interactable이기 때문에 빈공간을 드래그하는건 스왑안됨.
        if (item1 == null && item2 != null)
        {
            MyItems[index1] = item2;
            MyItems[index2] = null;
        }
        else if (item1 != null && item2 == null)
        {
            MyItems[index2] = item1;
            MyItems[index1] = null;
        }
        OnItemChanged?.Invoke();
    }
    public Item GetItem(int instanceID) // 인벤 속 아이템을 찾아서 반환... instanceID로 찾아야함?
    {
        return MyItems.Find(x => x != null && x.InstanceId == instanceID);
    }
    public void UnEquipItem(Item item)
    {
        int emptySlotIndex = MyItems.FindIndex(x => x == null);
        if (emptySlotIndex != -1)
        {
            MyItems[emptySlotIndex] = item;
            OnItemChanged?.Invoke();
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return;
        }
    }

    /*

    /// 이전 프로젝트 소팅 아이템
    /// public void OnSortButtonClicked()
    {
        // 정렬 로직
        items.Sort((item1, item2) =>
        {
            if (item1 == null) return 1;
            if (item2 == null) return -1;

            // 먼저 아이템 타입에 따른 가중치를 비교
            int weight1 = sortWeight[item1.Type];
            int weight2 = sortWeight[item2.Type];
            int typeComparison = weight1.CompareTo(weight2);

            if (typeComparison != 0)
            {
                return typeComparison;
            }
            else if (item1.Type == ItemType.Consumable && item2.Type == ItemType.Consumable)
            {
                // 두 아이템 모두 Consumable 타입인 경우, 스택 크기를 내림차순으로 비교
                return item2.amount.CompareTo(item1.amount);
            }
            
            return 0; // 타입이 같고 Consumable이 아닌 경우 동일한 순위로 간주
        });
        // UI 업데이트
        onItemChangedCallback?.Invoke();
    }

    */
    public void Clear()
    {
        MyItems.Clear();
        InitializeSlots(); // Clear 후에도 슬롯 재초기화
    }

    #region Helper
    
    public void DebugInventory()
    {
        for (int i = 0; i < MyItems.Count; i++)
        {
            if (MyItems[i] != null)
            {
                Debug.Log(MyItems[i].ItemData.Name + " " + MyItems[i].Count);
            }
            
        }
    }
    
    public void OnItemChangedEvent()
    {
        OnItemChanged?.Invoke();
    }

    public int InventorySlotCount()
    {
        if (MyItems.Count == 0)
        {
            InitializeSlots();
        }
        return DEFAULT_INVENTORY_SLOT_COUNT;
    }

    public bool IsInventoryFull() // 위에 메서드에 써야하는데 그냥 emptySlotIndex == -1으로 다 때려박아서 다른곳에쓰는중
    {
        int emptySlotIndex = MyItems.FindIndex(x => x == null);
        if (emptySlotIndex == -1)
        {
            //isFull = true;
            Debug.Log("공간이 모자랍니다.");
            return true;
        }
        else
        {
            //isFull = false;
            Debug.Log("공간이 있습니다.");
            return false;
        }
    }

    public List<Item> GetInventoryItemsByToggle()
    {
        switch (ToggleType)
        {
            case EInventoryGroupType.All:
                return MyItems;
            case EInventoryGroupType.Equipment:
                return EquipmentItems;
            case EInventoryGroupType.Consumable:
                return ConsumableItems;
            case EInventoryGroupType.Material:
                return MaterialItems;
            default:
                return MyItems;
        }
    }
    

    #endregion
}