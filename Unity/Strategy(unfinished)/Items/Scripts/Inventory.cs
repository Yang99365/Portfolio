using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Inventory : MonoBehaviour
{
    public List<GItemSO> items = new List<GItemSO>();
    

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    public int space = 30;
    public bool isFull = false;

    public Transform itemContent;

    public Toggle enableRemove;

    private readonly static Dictionary<ItemType, int> sortWeight = new Dictionary<ItemType, int>()
    {
        { ItemType.Equipment, 1 },
        { ItemType.Consumable, 2 },
        { ItemType.Ownable, 3 }
    };

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            enableRemove = GameObject.Find("EnableRemove").GetComponent<Toggle>();
            itemContent = GameObject.Find("ItemContent").transform;

        }
        else
        {
            enableRemove = null;
            itemContent = null;
        }
            
    }


    #region Singleton
    public static Inventory instance;
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    
    // 모든 슬롯에 SlotNum을 부여한다.
    public void SlotNumInitialize()
    {
        for (int i = 0; i < itemContent.childCount; i++)
        {
            itemContent.GetChild(i).GetComponent<Slot>().slotNum = i;
        }
        for (int i = 0; i < itemContent.childCount; i++)
        {
            items.Add(null);
        }
    }
    public void Start()
    {
        SlotNumInitialize();
        

    }
    public void SpaceFull()
    {
        int emptySlotIndex = items.FindIndex(x => x == null);
        if (emptySlotIndex == -1)
        {
            isFull = true;
        }
        else
        {
            isFull = false;
        }
    }
    public bool Add(GItemSO item)
    {
        bool isAdded;
        if (item.isStackable) // 아이템이 스택 가능한 경우
        {
            isAdded = AddStackableItem(item, item.amount);
        }
        else // 스택 불가능한 경우
        {
                // 빈 슬롯 찾기
            int emptySlotIndex = items.FindIndex(x => x == null);
            if (emptySlotIndex == -1 )//&& items.Count >= space)
            {
                isFull = true;
                Debug.Log("공간이 모자라 아이템 휙득에 실패했습니다.");
                return false;
            }
            if (emptySlotIndex != -1)
            {
                // 빈 슬롯에 아이템 추가
                items[emptySlotIndex] = Instantiate(item);
            }
            else
            {
                // 리스트에 아이템 추가
                items.Add(Instantiate(item));
            }
            isAdded = true;
        }

        if (isAdded)
        {
            Debug.Log("아이템이 추가되었습니다.");
            onItemChangedCallback?.Invoke();
        }
        onItemChangedCallback?.Invoke();
        return isAdded;
    }
    public bool Add(GItemSO item, int amount) // 장비템(스택불가능)일 경우엔 amount만큼 호출
    {
        bool isAdded = false;
        
        if (item.isStackable) // 아이템이 스택 가능한 경우
        {
            isAdded = AddStackableItem(item, amount);
        }
        else // 스택 불가능한 경우
        {
            for (int i = 0; i < amount; i++)
            {
                // 빈 슬롯 찾기
                int emptySlotIndex = items.FindIndex(x => x == null);
                if (emptySlotIndex == -1 )//&& items.Count >= space)
                {
                    isFull = true;
                    Debug.Log("공간이 모자라 아이템 휙득에 실패했습니다.");
                    isAdded = false;
                    //return false;
                }
                else
                {
                    // 빈 슬롯에 아이템 추가
                    items[emptySlotIndex] = Instantiate(item);
                    isAdded = true;
                }           
            }
            
        }

        if (isAdded)
        {
            Debug.Log("아이템이 추가되었습니다.");
            onItemChangedCallback?.Invoke();
        }
        onItemChangedCallback?.Invoke();
        return isAdded;
    }

    private bool AddStackableItem(GItemSO item, int amount)
    {
        int overflowAmount = 0;
        foreach (GItemSO i in items)
        {
            if (i != null && i.Code == item.Code && i.amount < item.MaxStack)
            {
                int amountPossibleToTake = item.MaxStack - i.amount;
                if (amount > amountPossibleToTake)
                {
                    i.amount = item.MaxStack;
                    amount -= amountPossibleToTake;
                    overflowAmount += amount;
                }
                else
                {
                    i.amount += amount;
                    return true;
                }
            }
        }

        while (amount > 0)
        {
            int emptySlotIndex = items.FindIndex(x => x == null);
            if (emptySlotIndex == -1)
            {
                Debug.Log("공간이 모자라 일부 아이템 휙득에 실패했습니다.");
                if (overflowAmount > 0)
                {
                    Debug.Log(overflowAmount + "개의 아이템이 넘쳐서 버려졌습니다."); // 넘친 아이템 수량 출력
                }
                return false;
            }

            GItemSO newItem = Instantiate(item);
            newItem.amount = Mathf.Min(amount, item.MaxStack);
            amount -= newItem.amount;
            overflowAmount += amount;

            // 빈 슬롯에 새 아이템 추가
            items[emptySlotIndex] = newItem;


            /*
            int emptySlotIndex = items.FindIndex(x => x == null);
            GItemSO newItem = Instantiate(item);
            newItem.amount = Mathf.Min(amount, item.MaxStack);
            amount -= newItem.amount;

            if (emptySlotIndex != -1)
            {
                // 빈 슬롯에 새 아이템 추가
                items[emptySlotIndex] = newItem;
            }
            else
            {
                // 새 슬롯 생성
                items.Add(newItem);
            }
            */
        }
        return true;
    }

    /*
    public void Remove(GItemSO item)
    {
        // 해당 슬롯의 아이템을 비운다.
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
    */

    public void EnableItemRemove()
    {
        if (enableRemove.isOn)
        {
            foreach (Transform item in itemContent)
            {
                item.Find("RemoveBtn").gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (Transform item in itemContent)
            {
                item.Find("RemoveBtn").gameObject.SetActive(false);
            }
        }
    }
    public void RemoveItem(int index)
    {
        items[index] = null;
        SpaceFull();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }
    public void SwapItems(int index1, int index2)
    {
        GItemSO item1 = items[index1];
        GItemSO item2 = items[index2];
        if (item1 != null && item2 != null && item1.Code == item2.Code && item1.isStackable)
        {
            // 스택 합치기 로직
            int totalStack = item1.amount + item2.amount;
            if (totalStack <= item1.MaxStack)
            {
                // 합쳐진 스택이 최대 스택을 초과하지 않는 경우
                item2.amount = totalStack; // 스택 합치기
                items[index1] = null; // 원래 슬롯 비우기
            }
            else
            {
                // item 1은 드래그 하는 아이템, item2는 드롭하는 아이템
                // 합쳐진 스택이 최대 스택을 초과하는 경우
                if(item1.amount >= item2.amount)
                {
                    item1.amount = totalStack - item1.MaxStack;
                    item2.amount = item1.MaxStack;
                }
                else
                {
                    item2.amount = totalStack - item1.MaxStack;
                    item1.amount = item1.MaxStack;
                }
                
            }
        }

        else
        {
            // 아이템 교환 로직
            GItemSO tempItem = items[index1];
            items[index1] = items[index2];
            items[index2] = tempItem;
        }

        onItemChangedCallback?.Invoke(); // UI 업데이트 호출
        
    }
    public void OnSortButtonClicked()
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
        
        //InventoryUI.instance.OnToggleChanged(); 왜넣엇지?
    }
}
