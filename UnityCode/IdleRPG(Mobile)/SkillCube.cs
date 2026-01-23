using System;
using UnityEngine;
using static Data;
using static Define;

[Serializable]
public class SkillCube
{
    public SkillSaveData SaveData { get; private set; } = new SkillSaveData();
    #region Properties
    public int InstanceId
    {
        get { return SaveData.instanceId; }
        set { SaveData.instanceId = value; }
    }
    public int DataId
    {
        get { return SaveData.skillId; }
        set { SaveData.skillId = value; }
    }
    public int EquipSlot
    {
        get { return SaveData.equipSlot; }
        set { SaveData.equipSlot = value; }
    }
    public int Level
    {
        get { return SaveData.level; }
        set { SaveData.level = value; }
    }
    public Data.SkillData SkillData
    {
        get
        {
            return Managers.Data.SkillDataDict[DataId];
        }
    }
    public ESkillType SkillType { get; private set; } // Active, Passive
    public ESkillRairity Rarity { get; private set; }

    #endregion

    #region Battle Properties
    public float LastUsedTime { get; set; } = -999f;
    public bool IsReady => Time.time - LastUsedTime >= (SkillData?.cooldown ?? 0);
    public float RemainingCooldown => Mathf.Max(0, (SkillData?.cooldown ?? 0) - (Time.time - LastUsedTime));
    #endregion
    public int EquippedHeroId { get; set; } = -1; // hmm...

    #region Constructors
    public SkillCube(int dataId, int level = 1)
    {
        DataId = dataId;
        SkillType = SkillData.skillType;
        Rarity = SkillData.rarity;
        Level = level;
        EquipSlot = -1; // 인벤토리
        EquippedHeroId = -1;
    }
    public virtual bool Init()
    {
        return true;
    }
    public SkillCube(SkillSaveData saveData)
    {
        SaveData = saveData;
        InstanceId = saveData.instanceId;
        DataId = saveData.skillId;
        Level = saveData.level;
        EquipSlot = saveData.equipSlot;
        EquippedHeroId = -1;
    }
    #endregion
    #region Factory Method
    public static SkillCube CreateSkillCube(SkillSaveData CubeInfo)
    {
        if (!Managers.Data.SkillDataDict.TryGetValue(CubeInfo.skillId, out var skillData))
        {
            Debug.Log($"[SkillCube] CreateSkillCube Failed! Not Found SkillData. Id : {CubeInfo.skillId}");
            return null;
        }
        SkillCube skillCube = null;

        skillCube = new SkillCube(CubeInfo.skillId);

        if (skillCube != null)
        {
            skillCube.SaveData = CubeInfo;
            skillCube.DataId = CubeInfo.skillId;
            skillCube.InstanceId = CubeInfo.instanceId;
            skillCube.Level = CubeInfo.level;
        }
        return skillCube;
    }
    #endregion
    #region Save/Load
    public SkillSaveData GetSaveData()
    {
        return new SkillSaveData
        {
            instanceId = this.InstanceId,
            skillId = this.DataId,
            level = this.Level,
            equipSlot = this.EquipSlot
        };
    }
    #endregion

    #region Methods
    public void Use()
    {
        LastUsedTime = Time.time;
        Debug.Log($"Used skill: {SkillData?.skillName} (Cooldown: {SkillData?.cooldown}s)");
    }

    // 쿨다운 초기화
    public void ResetCooldown()
    {
        LastUsedTime = -999f;
    }
    public bool Enhance(SkillCube skillCube)
    {
        if (skillCube == null || skillCube.DataId != DataId)
        {
            Debug.LogWarning("Cannot enhance: Different skill or null material");
            return false;
        }

        // 강화 로직 (예: 3개 모으면 레벨업)
        // 일단 간단하게 레벨만 올리기
        Level++;
        Debug.Log($"Skill {SkillData.skillName} enhanced to Lv.{Level}");

        return true;
    }
    public bool CanEquipToHero(Hero hero)
    {
        if (hero == null || SkillData == null)
            return false;

        // 캐릭터 제한 확인
        if (SkillData.requiredCharacterId > 0 &&
            hero.DataTemplateID != SkillData.requiredCharacterId)
        {
            Debug.Log($"Skill {SkillData.skillName} requires specific character");
            return false;
        }

        // 패시브 스킬은 장착 불가 (이미 기본으로 적용됨)
        // 아 씁. 이거때문이엿구나
        //if (SkillData.skillType == ESkillType.Passive)
        //{
        //    Debug.Log($"Passive skills cannot be equipped from cube");
        //    return false;
        //}

        return true;
    }
    #endregion

    #region Helpers
    public ESkillType GetSkillType()
    {
        SkillCube skillCube = this;
        switch (skillCube.SkillData.skillType)
        {
            case ESkillType.Active:
                return ESkillType.Active;
            case ESkillType.Passive:
                return ESkillType.Passive;
            default:
                return ESkillType.None;
        }
    }
    #endregion
    #region Display Info
    public string GetName()
    {
        return SkillData?.skillName ?? "Unknown Skill";
    }

    public string GetDescription()
    {
        if (SkillData == null)
            return "Unknown skill cube";

        string desc = $"{SkillData.skillName} Lv.{Level}\n";
        desc += $"Type: {SkillData.skillType}\n";
        desc += $"Rarity: {SkillData.rarity}\n";
        desc += $"Cooldown: {SkillData.cooldown}s\n";
        desc += $"\n{SkillData.description}";

        return desc;
    }

    public Color GetRarityColor()
    {
        switch (Rarity)
        {
            case ESkillRairity.Common:
                return Color.white;
            case ESkillRairity.Rare:
                return new Color(0.3f, 0.6f, 1f); // Blue
            case ESkillRairity.Unique:
                return new Color(0.6f, 0.3f, 1f); // Purple
            case ESkillRairity.Epic:
                return new Color(1f, 0.5f, 0f); // Orange
            case ESkillRairity.Legend:
                return new Color(1f, 0.9f, 0f); // Gold
            default:
                return Color.gray;
        }
    }
    #endregion
    #region Battle Methods
    
    #endregion
}
