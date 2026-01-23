using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class InventoryManager
{
    #region Events
    public event Action OnInventoryChanged;
    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;
    public event Action<SkillCube> OnSkillCubeAdded;
    public event Action<SkillCube> OnSkillCubeRemoved;
    #endregion

    #region Properties
    public const int DEFAULT_INVENTORY_SIZE = 50;
    public const int DEFAULT_SKILLCUBE_SIZE = 30;

    // 아이템 인벤토리 (슬롯 기반)
    private List<Item> _items = new List<Item>();

    // 스킬큐브 인벤토리 (별도 관리)
    private List<SkillCube> _skillCubes = new List<SkillCube>();
    // 장착한 장비 아이템
    private Dictionary<int,/*EquipmentType*/ Item> EquippedItems = new Dictionary<int, Item>();
    // 장착한 스킬큐브
    private Dictionary<int,/*SloIndex*/ SkillCube> EquippedSkillCubes = new Dictionary<int, SkillCube>();
    // 창고
    // 미구현 굳이..

    // 현재 선택된 탭
    public EInventoryGroupType CurrentTab { get; set; } = EInventoryGroupType.All;

    // Properties
    public List<Item> Items => _items;
    public List<SkillCube> SkillCubes => _skillCubes;
    public int ItemCount => _items.Count(item => item != null);
    public int SkillCubeCount => _skillCubes.Count(skillcube => skillcube != null);
    public bool IsItemInventoryFull => ItemCount >= DEFAULT_INVENTORY_SIZE;
    public bool IsSkillInventoryFull => SkillCubeCount >= DEFAULT_SKILLCUBE_SIZE;
    #endregion

    #region Initialization
    public void Init()
    {
        // 슬롯 초기화
        InitializeSlots();

        // 세이브 데이터에서 로드
        LoadFromSaveData();

        Debug.Log($"InventoryManager initialized - Items: {ItemCount}, SkillCubes: {SkillCubeCount}");
    }

    private void InitializeSlots()
    {
        _items.Clear();
        // 빈 슬롯으로 초기화
        for (int i = 0; i < DEFAULT_INVENTORY_SIZE; i++)
        {
            _items.Add(null);
        }
    }

    private void LoadFromSaveData()
    {

        // 아이템 로드
        if (Managers.Game.SaveData.Items != null)
        {
            Debug.Log($"Total items in save data: {Managers.Game.SaveData.Items.Count}");

            foreach (var itemData in Managers.Game.SaveData.Items)
            {
                if (itemData.equipSlot == -1) // -1 = 인벤토리
                {
                    var item = Item.CreateItem(itemData);

                    if (item != null)
                    {
                        int emptySlot = FindEmptyItemSlot();
                        if (emptySlot != -1)
                        {
                            _items[emptySlot] = item;
                            Debug.Log($"Loaded item: {item.ItemData.baseName} (Type: {item.ItemType}, Slot: {emptySlot})");

                            // 장비 아이템인 경우 stats 확인
                            if (item is EquipmentItem equipItem)
                            {
                                Debug.Log($"  - Attack: {equipItem.attack}, Defense: {equipItem.defense}, HP: {equipItem.maxHealth}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"✗ No empty slot for item: {item.ItemData.baseName}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"✗ Failed to create item from templateId: {itemData.templateId}");
                    }
                }
            }
        }

        // 스킬큐브 로드
        if (Managers.Game.SaveData.Skills != null)
        {
            Debug.Log($"Total skills in save data: {Managers.Game.SaveData.Skills.Count}");

            foreach (var skillData in Managers.Game.SaveData.Skills)
            {
                if (skillData.equipSlot == -1) // -1 = 인벤토리
                {
                    var cube = SkillCube.CreateSkillCube(skillData);

                    if (cube != null)
                    {
                        _skillCubes.Add(cube);
                    }
                    else
                    {
                        Debug.LogError($"✗ Failed to create skill cube from skillId: {skillData.skillId}");
                    }
                }
            }
        }

       
    }
    #endregion

    #region Item Management
    // 아이템 추가 (몬스터 드롭, 보상 등)
    public Item MakeItem(int templateId, int count = 1)
    {
        if (Managers.Data.ItemDic.TryGetValue(templateId, out var itemData) == false)
        {
            Debug.LogError($"Invalid Item ID: {templateId}");
            return null;
        }

        // 장비 아이템인 경우 개별 생성
        if (itemData.itemType == EItemType.Equipment)
        {
            Item lastItem = null;
            for (int i = 0; i < count; i++)
            {
                int instanceId = Managers.Game.GenerateItemInstanceId();
                ItemSaveData saveData = new ItemSaveData
                {
                    instanceId = instanceId,
                    templateId = templateId,
                    count = 1, // 장비는 항상 1개
                    equipSlot = -1 // inventory
                };

                lastItem = AddItem(saveData);
            }
            return lastItem; // 마지막에 생성된 아이템 반환
        }
        else
        {
            // 소비/재료 아이템은 스택 가능
            int instanceId = Managers.Game.GenerateItemInstanceId();
            ItemSaveData saveData = new ItemSaveData
            {
                instanceId = instanceId,
                templateId = templateId,
                count = count,
                equipSlot = -1 // inventory
            };

            return AddItem(saveData);
        }
    }

    public Item AddItem(ItemSaveData itemInfo)
    {
        Item item = Item.CreateItem(itemInfo);
        if (item == null)
            return null;

        // 장비 아이템은 항상 개별 슬롯에 추가 (스택 불가)
        if (item.ItemType == EItemType.Equipment)
        {
            int emptySlot = FindEmptyItemSlot();
            if (emptySlot == -1)
            {
                Debug.Log("Inventory is full!");
                OnInventoryChanged?.Invoke();
                SaveToGameData();
                return null;
            }

            _items[emptySlot] = item;
            OnItemAdded?.Invoke(item);

            Debug.Log($"Added Equipment: {item.ItemData.baseName} (Unique ID: {item.InstanceId})");

            OnInventoryChanged?.Invoke();
            SaveToGameData();
            return item;
        }
        // 소비/재료 아이템은 스택 가능
        else if (item.ItemData.maxStack > 1)
        {
            AddStackableItem(item, item.Count);

            OnInventoryChanged?.Invoke();
            SaveToGameData();
            return item;
        }
        // maxStack이 1인 특수 아이템 (스택 불가)
        else
        {
            int emptySlot = FindEmptyItemSlot();
            if (emptySlot == -1)
            {
                Debug.Log("Inventory is full!");
                OnInventoryChanged?.Invoke();
                SaveToGameData();
                return null;
            }

            _items[emptySlot] = item;
            OnItemAdded?.Invoke(item);

            Debug.Log($"Added {item.ItemData.baseName}");

            OnInventoryChanged?.Invoke();
            SaveToGameData();
            return item;
        }
    }

    // 스택 가능한 아이템 추가
    private void AddStackableItem(Item Addeditem, int count)
    {
        int remainingCount = count;

        // 먼저 기존 스택에 추가 시도
        foreach (Item item in _items)
        {
            if (item != null && item.DataId == Addeditem.DataId && item.Count < Addeditem.ItemData.maxStack)
            {
                int addCount = Math.Min(remainingCount, Addeditem.ItemData.maxStack - item.Count);
                item.Count += addCount;
                remainingCount -= addCount;

                if (remainingCount <= 0)
                {
                    OnItemAdded?.Invoke(item);
                    Debug.Log($"Added {Addeditem.ItemData.baseName} x{count} to existing stack");
                    return;
                }
            }
        }

        // 남은 수량을 새 슬롯에 추가
        while (remainingCount > 0)
        {
            int emptySlot = FindEmptyItemSlot();
            if (emptySlot == -1)
            {
                Debug.Log($"Inventory full! {remainingCount} items couldn't be added");
                return;
            }

            int stackCount = Math.Min(remainingCount, Addeditem.ItemData.maxStack);
            Item newItem = Item.CreateItem(new ItemSaveData
            {
                instanceId = Managers.Game.GenerateItemInstanceId(),
                templateId = Addeditem.DataId,
                count = stackCount,
                equipSlot = -1
            });
            if (newItem != null)
            {
                _items[emptySlot] = newItem;
                remainingCount -= stackCount;
                OnItemAdded?.Invoke(newItem);
            }
        }

        Debug.Log($"Added {Addeditem.ItemData.baseName} x{count}");
        return;
    }

    // 아이템 제거
    public void RemoveItem(int instanceId)
    {
        Item item = _items.Find(i => i != null && i.InstanceId == instanceId);
        if (item == null)
            return;
        else
        {
            _items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }

        OnInventoryChanged?.Invoke();
        SaveToGameData();
        return;
    }

    // 아이템 사용 (소비 아이템)
    public bool UseItem(int instanceId)
    {
        Item item = _items.Find(i => i != null && i.InstanceId == instanceId);
        if (item == null)
        {
            Debug.Log("Item not found!");
            return false;
        }

        if (item.ItemData.itemType != EItemType.Consumable)
        {
            Debug.Log("This item cannot be used!");
            return false;
        }

        // ConsumableItem으로 캐스팅
        var consumable = item as ConsumableItem;
        if (consumable != null && consumable.Use())
        {
            // 사용 성공, 수량 감소는 Use() 내부에서 처리
            if (item.Count <= 0)
            {
                _items.Remove(item);
                OnItemRemoved?.Invoke(item);
            }

            OnInventoryChanged?.Invoke();
            SaveToGameData();
        }

        return true;
    }
    // 아이템 장착 (장비 아이템)
    public void EquipItem(int instanceId)
    {
        Item item = _items.Find(i => i != null && i.InstanceId == instanceId);
        if (item == null)
        {
            Debug.Log("Item not found!");
            return;
        }

        EEquipmentType equipType = item.equipmentType;
        if (equipType == EEquipmentType.None)
        {
            Debug.Log("This item cannot be equipped!");
            return;
        }
        // 이미 장착된 아이템이 있으면 인벤토리로 반환
        if (EquippedItems.TryGetValue((int)equipType, out Item prev))
        {
            UnEquipItem(prev.InstanceId);
        }

        // 장착
        item.EquipSlot = (int)equipType;
        EquippedItems[(int)equipType] = item;
        _items.Remove(item);
        // 영웅한테 장착은 반영이 안된듯함 UI로 하나?

    }
    public void UnEquipItem(int instanceId, bool checkFull = true)
    {
        var item = _items.Find(i => i != null && i.InstanceId == instanceId);
        if (item == null)
            return;

        if (checkFull && IsItemInventoryFull)
            return;

        // 장착 해제
        item.EquipSlot = -1;//inventory
        EquippedItems.Remove((int)item.equipmentType);
        _items.Add(item);
        OnInventoryChanged?.Invoke();
        SaveToGameData();

        
    }

    // 아이템 교체 (드래그 앤 드롭) //이건아직안고쳣음. 인벤토리 UI에서 실제로 써봐야함
    public void SwapItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= _items.Count || index2 < 0 || index2 >= _items.Count)
            return;

        var item1 = _items[index1];
        var item2 = _items[index2];

        // 같은 아이템이면 스택 시도 (장비가 아닌 경우만)
        if (item1 != null && item2 != null &&
            item1.DataId == item2.DataId &&
            item1.ItemType != EItemType.Equipment && // 장비는 스택 불가
            item1.ItemData.maxStack > 1)
        {
            int totalCount = item1.Count + item2.Count;
            if (totalCount <= item1.ItemData.maxStack)
            {
                // 합치기 가능
                item2.Count = totalCount;
                _items[index1] = null;
            }
            else
            {
                // 일부만 합치기
                item2.Count = item1.ItemData.maxStack;
                item1.Count = totalCount - item1.ItemData.maxStack;
            }
        }
        else
        {
            // 위치 교체
            _items[index1] = item2;
            _items[index2] = item1;
        }

        OnInventoryChanged?.Invoke();
        SaveToGameData();
    }

    // 빈 슬롯 찾기
    private int FindEmptyItemSlot()
    {
        return _items.FindIndex(x => x == null);
    }
    private int FindEmptySkillSlot()
    {
        return _skillCubes.FindIndex(x => x == null);
    }

    // 아이템 획득 (ItemHolder에서 호출)
    public Item AcquireItem(int templateId, int count = 1)
    {
        //// 아이템 획득 이펙트나 사운드 재생 >> bool 일떄의 코드 주석처리
        //Managers.Sound.Play(ESound.Effect, "ItemPickup");

        //bool success = AddItem(templateId, count);

        //if (success)
        //{
        //    // UI 알림
        //    var itemData = Managers.Data.ItemDic[templateId];
        //    Debug.Log($"Acquired {itemData.baseName} x{count}!");
        //}

        //return success;

        // 아이템 휙득 이펙트, 사운드 재생
        //Managers.Sound.Play(ESound.Effect, "ItemPickup");

        return MakeItem(templateId, count);
    }
    #endregion

    #region SkillCube Management
    // 스킬큐브 추가 (몬스터 드롭,보상)
    public SkillCube MakeSkillCube(int templateId, int level = 1)
    {
        int instanceId = Managers.Game.GenerateSkillInstanceId();

        if (Managers.Data.SkillDataDict.TryGetValue(templateId, out var skillData) == false)
        {
            Debug.LogError($"Invalid Skill ID: {templateId}");
            return null;
        }

        SkillSaveData saveData = new SkillSaveData
        {
            instanceId = instanceId,
            skillId = templateId,
            level = level,
            equipSlot = -1 // inventory
        };

        return AddSkillCube(saveData, level);
    }
    
    public SkillCube AddSkillCube(SkillSaveData cubeInfo, int level =1)
    {
        if (IsSkillInventoryFull)
        {
            Debug.Log("SkillCube inventory is full!");
            return null;
        }

        SkillCube cube = SkillCube.CreateSkillCube(cubeInfo);
        if (cube == null)
            return null;

        _skillCubes.Add(cube);
        OnSkillCubeAdded?.Invoke(cube);

        OnInventoryChanged?.Invoke();
        SaveToGameData();

        return cube;
    }

    // 스킬큐브 제거
    public void RemoveSkillCube(int instanceId)
    {
        SkillCube cube = _skillCubes.FirstOrDefault(s => s.InstanceId == instanceId);
        if (cube == null)
            return;
        else
        {
            _skillCubes.Remove(cube);
            OnSkillCubeRemoved?.Invoke(cube);
        }
        OnInventoryChanged?.Invoke();
        SaveToGameData();
        return;
    }

    // 스킬큐브를 장착
    public void EquipSkillCube(int cubeInstanceId, int slotIndex)
    {
        SkillCube cube = _skillCubes.FirstOrDefault(s => s.InstanceId == cubeInstanceId);
        if (cube == null)
        {
            Debug.Log("SkillCube not found!");
            return;
        }
        // 이미 장착된 스킬이 있으면 인벤토리로 반환
        if (EquippedSkillCubes.TryGetValue(slotIndex, out SkillCube prev))
            UnEquipSkill(prev.InstanceId);

        //장착
        cube.EquipSlot = slotIndex; // 영웅이 낀 슬롯 인덱스
        EquippedSkillCubes[slotIndex] = cube;
        _skillCubes.Remove(cube);
        // 영웅에게 장착 반영은 UI에서 반영해야할듯함.
        // ui에서 보일땐 슬롯인덱스가 -1(인벤토리)인거만 조회하면될듯
        //hero.EquipSkill(skillInstance, slotIndex);

        OnInventoryChanged?.Invoke();
        SaveToGameData();
        return;
    }
    public void UnEquipSkill(int cubeInstanceId, bool checkFull = true)
    {
        var cube = _skillCubes.FirstOrDefault(s => s.InstanceId == cubeInstanceId);
        if (cube == null)
            return;

        if (checkFull && IsSkillInventoryFull)
            return;

        // 장착 해제
        cube.EquipSlot = -1;//inventory
        EquippedSkillCubes.Remove(cube.EquipSlot);
        _skillCubes.Add(cube);
        OnInventoryChanged?.Invoke();
        SaveToGameData();
    }

    public void SwapSkillCubes(int index1, int index2)
    {
        if (index1 < 0 || index1 >= _skillCubes.Count || index2 < 0 || index2 >= _skillCubes.Count)
            return;

        var cube1 = _skillCubes[index1];
        var cube2 = _skillCubes[index2];

        // 위치 교체
        _skillCubes[index1] = cube2;
        _skillCubes[index2] = cube1;

        OnInventoryChanged?.Invoke();
        SaveToGameData();
    }
    #endregion

    #region Filtering & Display
    public enum EInventoryGroupType
    {
        All,
        Equipment,
        Consumable,
        Material,
        SkillCube
    }

    // 현재 탭에 따른 아이템 필터링
    public List<Item> GetFilteredItems()
    {
        switch (CurrentTab)
        {
            case EInventoryGroupType.Equipment:
                return _items.Where(i => i != null && i.ItemData.itemType == EItemType.Equipment).ToList();
            case EInventoryGroupType.Consumable:
                return _items.Where(i => i != null && i.ItemData.itemType == EItemType.Consumable).ToList();
            case EInventoryGroupType.Material:
                return _items.Where(i => i != null && i.ItemData.itemType == EItemType.Material).ToList();
            case EInventoryGroupType.All:
            default:
                return _items.Where(i => i != null).ToList();
        }
    }

    // 스킬큐브는 따로 UI에 탭 만들어서 보일것.

    #endregion

    #region Save & Load
    private void SaveToGameData()
    {
        Debug.Log("Saving inventory to game data...");

        // ★ 중요: 장착된 아이템 유지 (equipSlot != -1)
        var equippedItems = Managers.Game.SaveData.Items.Where(i => i.equipSlot != -1).ToList();

        // 인벤토리에 있는 아이템 추가 (equipSlot == -1)
        foreach (var item in _items)
        {
            if (item != null)
            {
                equippedItems.Add(item.GetSaveData());
            }
        }

        Managers.Game.SaveData.Items = equippedItems;

        // ★ 중요: 장착된 스킬큐브 유지 (equipSlot != -1)
        var equippedCubes = Managers.Game.SaveData.Skills.Where(s => s.equipSlot != -1).ToList();

        // 인벤토리에 있는 스킬큐브 추가 (equipSlot == -1)
        foreach (var cube in _skillCubes)
        {
            if (cube != null)
            {
                equippedCubes.Add(cube.GetSaveData());
            }
        }

        Managers.Game.SaveData.Skills = equippedCubes;

        Debug.Log($"Inventory saved - Total Items: {Managers.Game.SaveData.Items.Count}, Total Skills: {Managers.Game.SaveData.Skills.Count}");

        // 실제 파일 저장은 GameManager에서
        Managers.Game.SaveGame();
    }
    #endregion

    #region Debug
    public void DebugInventory()
    {
        Debug.Log("=== Inventory Debug ===");
        Debug.Log($"Items: {ItemCount}/{DEFAULT_INVENTORY_SIZE}");

        int slotIndex = 0;
        foreach (var item in _items)
        {
            if (item != null)
            {
                Debug.Log($"[Slot {slotIndex}] {item.ItemData.baseName} x{item.Count} (ID: {item.InstanceId})");
            }
            slotIndex++;
        }

        Debug.Log($"\nSkillCubes: {SkillCubeCount}/{DEFAULT_SKILLCUBE_SIZE}");
        foreach (var cube in _skillCubes)
        {
            var data = cube.SkillData;
            Debug.Log($"- {data.skillName} Lv.{cube.Level} (ID: {cube.InstanceId})");
        }
    }
    public void Clear()
    {
        _items.Clear();
        _skillCubes.Clear();
        EquippedItems.Clear();
    }

    // 테스트용 랜덤 아이템 추가
    public void AddTestItem()
    {
        MakeItem(400, 2);
        MakeItem(100, 2);
        MakeItem(101, 1);
    }

    // 테스트용 랜덤 스킬큐브 추가
    public void AddRandomSkillCube()
    {
        MakeSkillCube(3001, 1);
        MakeSkillCube(3001, 2);
    }
    #endregion
    #region Helper
    public Item GetItem(int instanceId)
    {
        return _items.Find(i => i != null && i.InstanceId == instanceId);
    }
    public int GetItemCount(int instanceId)
    {
        return _items.Where(i => i != null && i.InstanceId == instanceId).Sum(i => i.Count);
    }
    public SkillCube GetSkillCube(int instanceId)
    {
        return _skillCubes.Find(s => s.InstanceId == instanceId);
    }
    public List<SkillCube> GetSkillCubes() => _skillCubes;

    public Item GetItemAtSlot(int index)
    {
        if (index >= 0 && index < _items.Count)
            return _items[index];
        return null;
    }
    #endregion
}