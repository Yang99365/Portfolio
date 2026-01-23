using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using static Define;

public class General
{
    public Data.GeneralData GeneralData { get; private set; }

    #region GeneralInfo

    public int GeneralID;
    public string GeneralName;
    public int FactionID;
    public int ScenarioID;
    public string SpritePath;
    public string SkeletonPath; // Spine
    public int unitTypeID;
    public string unitTypeSpritePath;
    public int troopTypeID;
    public string troopTypeSpritePath;
    public int troopCount; // 장군이 소지한 병사 수
    public int troopMaxCount; // 장군이 소지할 수 있는 최대 병사 수, 최대 5천
    public Stat Stats;
    public List<int> SkillIDs;
    public Equipment Equipment;


    #endregion
    public Data.Stat BaseStats { get; private set; }
    public Data.Stat ModifiedStats { get; private set; }

    public General Init(int GeneralID)
    {
        GeneralData = Managers.Data.GeneralDic[GeneralID];

        this.GeneralID = GeneralData.id;
        this.GeneralName = GeneralData.name;
        this.FactionID = GeneralData.factionId;
        this.ScenarioID = GeneralData.scenarioId;
        this.SpritePath = GeneralData.spriteAddress;
        this.SkeletonPath = GeneralData.skeletonData;
        this.unitTypeID = GeneralData.unitTypeId;
        this.unitTypeSpritePath = GeneralData.unitTypeSprite;
        this.troopTypeID = GeneralData.troopTypeId;
        this.troopTypeSpritePath = GeneralData.troopSprite;
        this.troopCount = GeneralData.troopCount;
        this.troopMaxCount = 5000; // 특수한 장군이나 스킬을 통해 if문으로 최대 병사 수를 늘리기.
        this.Stats = GeneralData.stats;
        this.SkillIDs = GeneralData.skillIds;
        this.Equipment = GeneralData.equipment;


        this.BaseStats = GeneralData.stats;
        this.ModifiedStats = new Data.Stat
        {
            attack = BaseStats.attack,
            defense = BaseStats.defense,
            intelligence = BaseStats.intelligence,
            speed = BaseStats.speed
        };

        return this;
    }
    public void ModifyStat(Define.EItemStatType statType, int value)
    {
        switch (statType)
        {
            case Define.EItemStatType.Attack:
                ModifiedStats.attack += value;
                break;
            case Define.EItemStatType.Defence:
                ModifiedStats.defense += value;
                break;
            case Define.EItemStatType.Intelligence:
                ModifiedStats.intelligence += value;
                break;
            case Define.EItemStatType.Speed:
                ModifiedStats.speed += value;
                break;
            case Define.EItemStatType.All:
                ModifiedStats.attack += value;
                ModifiedStats.defense += value;
                ModifiedStats.intelligence += value;
                ModifiedStats.speed += value;
                break;
            case Define.EItemStatType.TroopCount:
                troopCount += value;
                troopMaxCount += value;
                break;
        }
    }

    // 스탯 초기화 메서드
    public void ResetStats()
    {
        ModifiedStats.attack = BaseStats.attack;
        ModifiedStats.defense = BaseStats.defense;
        ModifiedStats.intelligence = BaseStats.intelligence;
        ModifiedStats.speed = BaseStats.speed;
    }

    // 아이템 효과 적용 메서드
    public void ApplyItemEffect(Item item)
    {
        if (item.ItemData is EquipmentData equipData)
        {
            ModifyStat(Define.EItemStatType.Attack, equipData.Attack);
            ModifyStat(Define.EItemStatType.Defence, equipData.Defence);
            ModifyStat(Define.EItemStatType.Intelligence, equipData.Intelligence);
            ModifyStat(Define.EItemStatType.Speed, equipData.Speed);
            //All을 장비데이터에 빼먹은거같은데
        }
        else if (item.ItemData is ConsumableData consumableData)
        {
            ConsumableItem consumableItem = (ConsumableItem)item;
            switch (consumableItem.EffectType)
            {
                case EItemStatType.Attack:
                    ModifyStat(EItemStatType.Attack, consumableItem.Value);
                    break;
                case EItemStatType.Defence:
                    ModifyStat(EItemStatType.Defence, consumableItem.Value);
                    break;
                case EItemStatType.Intelligence:
                    ModifyStat(EItemStatType.Intelligence, consumableItem.Value);
                    break;
                case EItemStatType.Speed:
                    ModifyStat(EItemStatType.Speed, consumableItem.Value);
                    break;
                case EItemStatType.All:
                    ModifyStat(EItemStatType.All, consumableItem.Value);
                    break;
                case EItemStatType.TroopCount:
                    ModifyStat(EItemStatType.TroopCount, consumableItem.Value);
                    break;
            }
        }
    }

    // 아이템 효과 제거 메서드
    public void RemoveItemEffect(Item item)
    {
        if (item.ItemData is EquipmentData equipData)
        {
            ModifyStat(Define.EItemStatType.Attack, -equipData.Attack);
            ModifyStat(Define.EItemStatType.Defence, -equipData.Defence);
            ModifyStat(Define.EItemStatType.Intelligence, -equipData.Intelligence);
            ModifyStat(Define.EItemStatType.Speed, -equipData.Speed);
        }
    }

    public void EquipItem(EquipmentItem item)
    {
        switch (item.ItemData.Type)
        {
            case EItemType.Weapon:
                if (Equipment.weaponId != 0)
                    UnequipItem(EItemType.Weapon);
                Equipment.weaponId = item.DataId;
                Equipment.weapon = item;
                break;
            case EItemType.Armor:
                if (Equipment.armorId != 0)
                    UnequipItem(EItemType.Armor);
                Equipment.armorId = item.DataId;
                Equipment.armor = item;
                break;
            case EItemType.Accessory:
                if (Equipment.accessoryId != 0)
                    UnequipItem(EItemType.Accessory);
                Equipment.accessoryId = item.DataId;
                Equipment.accessory = item;
                break;
        }
        ApplyItemEffect(item);
    }

    public void UnequipItem(EItemType itemType)
    {
        int itemId = 0;
        Item item = null;
        switch (itemType)
        {
            case EItemType.Weapon:
                itemId = Equipment.weaponId;
                Equipment.weaponId = 0;
                
                item = Equipment.weapon;
                RemoveItemEffect(item);
                Managers.Inventory.UnEquipItem(item);
                break;
            case EItemType.Armor:
                itemId = Equipment.armorId;
                Equipment.armorId = 0;
                
                item = Equipment.armor;
                RemoveItemEffect(item);
                Managers.Inventory.UnEquipItem(item);
                break;
            case EItemType.Accessory:
                itemId = Equipment.accessoryId;
                Equipment.accessoryId = 0;

                item = Equipment.accessory;
                RemoveItemEffect(item);
                Managers.Inventory.UnEquipItem(item);
                break;
        }
    }

    public void UseConsumableItem(ConsumableItem item)
    {
        ApplyItemEffect(item);
       
    }
    
}
