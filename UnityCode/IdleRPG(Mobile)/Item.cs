using System;
using Unity.VisualScripting;
using UnityEngine;
using static Data;
using static Define;

[Serializable]
public class Item
{
    public ItemSaveData SaveData { get; private set; } = new ItemSaveData();
    #region Properties
    public int InstanceId
    {
        get { return SaveData.instanceId; }
        set { SaveData.instanceId = value; }
    }
    public int DataId
    {
        get { return SaveData.templateId; }
        set { SaveData.templateId = value; }
    }
    public int EquipSlot
    {
        get { return SaveData.equipSlot; }
        set { SaveData.equipSlot = value; }
    }
    public Data.ItemData ItemData
    {
        get
        {
            return Managers.Data.ItemDic[DataId];
        }
    }
    public int Count
    {
        get { return SaveData.count; }
        set { SaveData.count = value; }
    }
    public EItemType ItemType { get; private set; } //equip, consum
    public EEquipmentType equipmentType { get; private set; } // weapon, armor, accessory

    #endregion

    #region Constructors
    // 새 아이템 생성
    public Item(int dataId)
    {
        DataId = dataId;
        ItemType = ItemData.itemType;
        
    }


    public virtual bool Init()
    {
        return true;
    }
    // 세이브 데이터에서 로드
    public Item(ItemSaveData saveData)
    {
        SaveData = saveData;
        InstanceId = saveData.instanceId;
        DataId = saveData.templateId;
        Count = saveData.count;
    }
    #endregion

    #region Factory Method
    public static Item CreateItem(ItemSaveData itemInfo)
    {
        if (!Managers.Data.ItemDic.TryGetValue(itemInfo.templateId, out var data))
        {
            Debug.LogError($"Invalid Item ID: {itemInfo.templateId}");
            return null;
        }

        Item item = null;

        // 아이템 타입에 따라 적절한 클래스 생성
        switch (data.itemType)
        {
            case EItemType.Equipment:
                item = new EquipmentItem(itemInfo.templateId);
                item.equipmentType = (item as EquipmentItem).EquipmentData.equipmentType;
                break;
            case EItemType.Consumable:
                item = new ConsumableItem(itemInfo.templateId);
                break;
            case EItemType.Material:
                item = new MaterialItem(itemInfo.templateId);
                break;
            default:
                item = new Item(itemInfo.templateId);
                break;
        }

        if (item != null)
        {
            item.SaveData = itemInfo;
            item.DataId = itemInfo.templateId;
            item.InstanceId = itemInfo.instanceId;
            item.Count = itemInfo.count;

            item.Init();
        }

        return item;
    }
    #endregion

    #region Save/Load
    public ItemSaveData GetSaveData()
    {
        return new ItemSaveData
        {
            instanceId = InstanceId,
            templateId = DataId,
            count = Count,
            equipSlot = EquipSlot
        };
    }
    #endregion

    #region Virtual Methods
    public virtual bool Use()
    {
        // 기본 아이템은 사용 불가
        Debug.Log($"Cannot use {ItemData.baseName}");
        return false;
    }

    public virtual string GetDescription()
    {
        return $"{ItemData.baseName} x{Count}";
    }
    #endregion
}

// 장비 아이템
public class EquipmentItem : Item
{
    public Data.EquipmentData EquipmentData => ItemData as Data.EquipmentData;
    public float attack { get; private set; }
    public float attackSpeed { get; private set; }
    public float maxHealth { get; private set; }
    public float defense { get; private set; }
    public float criticalChance { get; private set; }
    public float criticalDamage { get; private set; }

    public EquipmentItem(int templateId) : base(templateId)
    {
        if (EquipmentData == null)
        {
            Debug.LogError($"Item {templateId} is not equipment!");
        }
        Init();
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (ItemData == null)
            return false;

        if (ItemData.itemType != EItemType.Equipment)
            return false;

        EquipmentData data = (EquipmentData)ItemData;
        {
            attack = data.stats.attack;
            attackSpeed = data.stats.attackSpeed;
            maxHealth = data.stats.maxHealth;
            defense = data.stats.defense;
            criticalChance = data.stats.criticalChance;
            criticalDamage = data.stats.criticalDamage;
        }

        return true;
    }
    public override bool Use()
    {
        // 장비는 장착 UI를 열어야 함
        Debug.Log($"Open equip UI for {ItemData.baseName}");
        // TODO: UI 매니저를 통해 장비 장착 UI 열기
        return false;
    }

    public override string GetDescription()
    {
        if (EquipmentData == null) return base.GetDescription();

        string desc = $"{ItemData.baseName}\n";
        desc += $"Type: {EquipmentData.equipmentType}\n";

        if (EquipmentData.stats.attack > 0)
            desc += $"Attack: +{EquipmentData.stats.attack}\n";
        if (EquipmentData.stats.defense > 0)
            desc += $"Defense: +{EquipmentData.stats.defense}\n";
        if (EquipmentData.stats.maxHealth > 0)
            desc += $"HP: +{EquipmentData.stats.maxHealth}\n";

        return desc;
    }
}

// 소비 아이템
public class ConsumableItem : Item
{
    public float Value { get; private set; }
    public Data.ConsumableData ConsumableData => ItemData as Data.ConsumableData;

    public ConsumableItem(int templateId) : base(templateId)
    {
        if (ConsumableData == null)
        {
            Debug.LogError($"Item {templateId} is not consumable!");
        }
        Init();
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        if (ItemData == null)
            return false;

        if (ItemData.itemType != EItemType.Consumable)
            return false;

        ConsumableData data = (ConsumableData)ItemData;
        {
            Value = data.consumableEffectValue;
        }

        return true;
    }

    public override bool Use()
    {
        if (ConsumableData == null) return false;

        bool used = false;

        // 효과 적용
        switch (ConsumableData.consumableEffectType)
        {
            case EConsumableEffectType.Heal:
                // 모든 영웅 회복
                var heroes = Managers.Battle.GetAllAliveHeroes();
                if (heroes.Count > 0)
                {
                    foreach (var hero in heroes)
                    {
                        hero.Heal(ConsumableData.consumableEffectValue);
                    }
                    Debug.Log($"Healed all heroes for {ConsumableData.consumableEffectValue}");
                    used = true;
                }
                break;

            case EConsumableEffectType.Buff:
                // 버프 적용
                Debug.Log($"Applied buff: {ConsumableData.baseName}");
                used = true;
                break;

            default:
                Debug.Log($"Effect {ConsumableData.consumableEffectType} not implemented");
                break;
        }

        if (used)
        {
            Count--; // 사용 시 수량 감소
        }

        return used;
    }

    public override string GetDescription()
    {
        if (ConsumableData == null) return base.GetDescription();

        string desc = $"{ItemData.baseName} x{Count}\n";
        desc += $"Effect: {ConsumableData.consumableEffectType}\n";
        desc += $"Value: {ConsumableData.consumableEffectValue}";

        return desc;
    }
}

// 재료 아이템
public class MaterialItem : Item
{
    public Data.MaterialData MaterialData => ItemData as Data.MaterialData;

    public MaterialItem(int templateId) : base(templateId)
    {
        if (MaterialData == null)
        {
            Debug.LogError($"Item {templateId} is not material!");
        }
    }

    public override bool Use()
    {
        // 재료는 직접 사용 불가
        Debug.Log($"Materials cannot be used directly: {ItemData.baseName}");
        return false;
    }

    public override string GetDescription()
    {
        if (MaterialData == null) return base.GetDescription();

        string desc = $"{ItemData.baseName} x{Count}\n";
        desc += $"{MaterialData.materialDescription}";

        return desc;
    }

    #region Helpers
    public EEquipmentType GetEquipmentType()
    {
        Item item = this;
        switch (item.equipmentType)
        {
            case EEquipmentType.Weapon:
                return EEquipmentType.Weapon;
            case EEquipmentType.Armor:
                return EEquipmentType.Armor;
            case EEquipmentType.Accessory:
                return EEquipmentType.Accessory;
        }
        return EEquipmentType.None;
    }
    #endregion
}
