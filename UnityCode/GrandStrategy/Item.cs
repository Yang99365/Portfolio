using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Item
{
    // SaveData
    private ItemSaveData _saveData;


    #region Properties

    public int InstanceId { get; private set; }
    public int DataId { get; private set; }
    public ItemData ItemData { get; private set; }

    public int Count { get; set; }


    #endregion

    public Item(int dataId, int count = 1)
    {
        InstanceId = Managers.Game.GenerateItemInstanceId();
        DataId = dataId;
        Count = count;
        ItemData = Managers.Data.ItemDic[dataId];
        _saveData = new ItemSaveData(InstanceId, DataId, count);
    }

    public Item(ItemSaveData saveData) // 아이템 로드시(세이브파일) 사용
    {
        InstanceId = saveData.InstanceId;
        DataId = saveData.TemplateId;
        Count = saveData.Count;
        _saveData = saveData;
    }
    public static Item CreateItem(int dataId, int count = 1)
    {
        if (!Managers.Data.ItemDic.TryGetValue(dataId, out var data))
        {
            Debug.LogError($"Invalid DataTemplateID: {dataId}");
            return null;
        }

        switch (data.ItemGroupType)
        {
            case EItemGroupType.Equipment:
                return new EquipmentItem(dataId, count);
            case EItemGroupType.Consumable:
                return new ConsumableItem(dataId, count);
            default:
                Debug.LogError("Unsupported item type");
                return null;
        }
    }

    //private Item Initialize(int instanceID, int dataId, int count)
    //{


    //    if (!Managers.Data.ItemDic.TryGetValue(dataId, out var Data))
    //    {
    //        Debug.LogError($"Invalid DataTemplateID: {dataId}");
    //        return null;
    //    }

    //    Debug.Log($"Item InstanceID: {instanceID}, DataID: {dataId}, Count: {count}");

    //    Item item = null;

    //    switch (Data.ItemGroupType)
    //    {
    //        case EItemGroupType.Equipment:
    //            item = new EquipmentItem(dataId);
    //            break;
    //        case EItemGroupType.Consumable:
    //            item = new ConsumableItem(dataId);
    //            break;
    //        case EItemGroupType.Material:
    //        default:
    //            return null;
    //    }
    //    if(item != null)
    //    {
    //        item.InstanceId = instanceID;
    //        item.DataId = dataId;
    //        item.Count = count;
    //        item.ItemData = Data;
    //    }

    //    Debug.Log($"Item InstanceID: {item.InstanceId}, DataID: {item.DataId}, Count: {item.Count}");

    //    return item;
    //}


    #region SaveData
    public ItemSaveData GetSaveData()
    {
        _saveData.Count = Count;
        return _saveData;
    }
    
    #endregion
    public virtual bool Init()
    {
        return true;
    }

    public virtual bool Use()
    {
        // 아이템 사용
        return false;
    }
    #region Helpers

    #endregion

}

public class EquipmentItem : Item
{
    public int Attack { get; private set; }
    public int Defence { get; private set; }
    public int Intelligence { get; private set; }
    public int Speed { get; private set; }

    protected Data.EquipmentData EquipmentData { get { return (Data.EquipmentData)ItemData; } }

    public EquipmentItem(int templateId, int count) : base(templateId, count)
    {
        Init();
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (ItemData == null)
            return false;

        if (ItemData.Type != EItemType.Armor || ItemData.Type != EItemType.Weapon || ItemData.Type != EItemType.Accessory)
            return false;

        EquipmentData data = (EquipmentData)ItemData;
        {
            Attack = data.Attack;
            Defence = data.Defence;
            Intelligence = data.Intelligence;
            Speed = data.Speed;
        }

        return true;
    }


    public override bool Use() //  장비장착창을 켯으면 Manager.General.GetSelectedGeneral()로 장착할 대상을 가져와서 EquipItem 해주고 그 EquipItem은 인벤토리에서 InstanceID로 찾아서 삭제하고 장착하면될듯
    {
        bool isEquip = false;

        return isEquip;

    }
}

public class ConsumableItem : Item
{
    public EItemStatType EffectType { get; private set; }
    public int Value { get; private set; }

    protected Data.ConsumableData ConsumableData { get { return (Data.ConsumableData)ItemData; } }

    public ConsumableItem(int templateId, int count) : base(templateId, count)
    {
        Init();
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (ItemData == null)
            return false;

        if (ItemData.Type != EItemType.Consumable)
            return false;

        ConsumableData data = (ConsumableData)ItemData;
        {
            EffectType = data.EffectType;
            Value = data.Value;
        }

        return true;
    }

    public override bool Use() // 사용효과 위에 가져왓으니 사용하고 사용할 대상은 Manager.General.GetSelectedGeneral()로 가져와서 사용하면될듯
    {
        bool isUsed = false;
        Debug.Log("Use ConsumableItem");
        // Effect가 리스트일때 사용, 지금은 단일로 구현하였음.
        //foreach (var itemEffect in itemEffects)
        //{
        //    isUsed = itemEffect.ExecuteRole();
        //    수량--;
        //}
        if (ItemData == null)
        {
            Debug.LogError("ItemData is null");
            return isUsed;
        }
        Count--;
        switch (EffectType) // 아이템 사용시 효과 , 무장 지정했을시 대상에게 효과를 주고 UI도 업뎃해야할거같은데..
        {
            
            case EItemStatType.None:
                Debug.Log("None");
                break;
            case EItemStatType.Attack:
                Debug.Log("AttackUpgrade");
                break;
            case EItemStatType.Defence:
                Debug.Log("DefenceUpgrade");
                break;
            case EItemStatType.Intelligence:
                Debug.Log("IntelligenceUpgrade");
                break;
            case EItemStatType.Speed:
                Debug.Log("SpeedUpgrade");
                break;
            case EItemStatType.All:
                Debug.Log("AllUpgrade");
                break;
            case EItemStatType.TroopCount:
                Debug.Log("TroopCountUpgrade");
                break;
            default:
                break;
        }

        return isUsed;
    }
}
