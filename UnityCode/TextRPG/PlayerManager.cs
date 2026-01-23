using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

// 플레이어 캐릭터 스탯, 스킬, 장비 관리
public class PlayerManager
{
    #region Properties

    // 캐릭터 기본 정보
    public string PlayerName
    {
        get => Managers.Game.PlayerData.playerName;
        set => Managers.Game.PlayerData.playerName = value;
    }

    public int Level
    {
        get => Managers.Game.PlayerData.level;
        private set => Managers.Game.PlayerData.level = value;
    }

    public int CurrentExp
    {
        get => Managers.Game.PlayerData.currentExp;
        private set => Managers.Game.PlayerData.currentExp = value;
    }

    public int ExpToNextLevel
    {
        get => Managers.Game.PlayerData.expToNextLevel;
        private set => Managers.Game.PlayerData.expToNextLevel = value;
    }

    // 체력/마나
    public int CurrentHp
    {
        get => Managers.Game.PlayerData.currentHp;
        set => Managers.Game.PlayerData.currentHp = Mathf.Clamp(value, 0, MaxHp);
    }

    public int MaxHp
    {
        get => Managers.Game.PlayerData.maxHp;
        private set => Managers.Game.PlayerData.maxHp = value;
    }

    public int CurrentMp
    {
        get => Managers.Game.PlayerData.currentMp;
        set => Managers.Game.PlayerData.currentMp = Mathf.Clamp(value, 0, MaxMp);
    }

    public int MaxMp
    {
        get => Managers.Game.PlayerData.maxMp;
        private set => Managers.Game.PlayerData.maxMp = value;
    }

    // 성장 스탯
    public int Strength
    {
        get => Managers.Game.PlayerData.strength;
        private set => Managers.Game.PlayerData.strength = value;
    }

    public int Intelligence
    {
        get => Managers.Game.PlayerData.intelligence;
        private set => Managers.Game.PlayerData.intelligence = value;
    }

    public int Agility
    {
        get => Managers.Game.PlayerData.agility;
        private set => Managers.Game.PlayerData.agility = value;
    }

    public int Charisma
    {
        get => Managers.Game.PlayerData.charisma;
        private set => Managers.Game.PlayerData.charisma = value;
    }

    // 사용 가능한 포인트
    public int StatPoints
    {
        get => Managers.Game.PlayerData.statPoints;
        private set => Managers.Game.PlayerData.statPoints = value;
    }

    public int SkillPoints
    {
        get => Managers.Game.PlayerData.skillPoints;
        private set => Managers.Game.PlayerData.skillPoints = value;
    }

    // 전투 스탯 (계산됨)
    public int Attack => CalculateAttack();
    public int Defense => CalculateDefense();
    public int Speed => CalculateSpeed();

    #endregion

    #region Events
    public event Action<int> OnLevelUp;
    public event Action<int> OnExpGained;
    public event Action<int, int> OnHpChanged;      // currentHp, maxHp
    public event Action<int, int> OnMpChanged;      // currentMp, maxMp
    public event Action<EStatType, int> OnStatIncreased;
    public event Action<int> OnSkillLearned;
    public event Action OnStatsRecalculated;
    #endregion

    #region Initialization
    public void Init()
    {
        RecalculateStats();
        Debug.Log($"PlayerManager Initialized - Level {Level}, HP {CurrentHp}/{MaxHp}");
    }
    #endregion

    #region Experience & Level

    public void GainExp(int amount)
    {
        if (amount <= 0)
            return;

        CurrentExp += amount;
        OnExpGained?.Invoke(amount);

        Debug.Log($"Exp +{amount} ({CurrentExp}/{ExpToNextLevel})");

        // 레벨업 체크
        while (CurrentExp >= ExpToNextLevel)
        {
            LevelUp();
        }

        Managers.Game.SaveGame();
    }

    private void LevelUp()
    {
        CurrentExp -= ExpToNextLevel;
        Level++;

        // 다음 레벨 경험치 계산
        var config = Managers.Data.ConfigData?.progression;
        if (config != null)
        {
            ExpToNextLevel = Mathf.RoundToInt(config.baseExpToLevel * Mathf.Pow(config.expScalingPerLevel, Level - 1));

            // 포인트 획득
            StatPoints += config.statPointsPerLevel;
            SkillPoints += config.skillPointsPerLevel;
        }
        else
        {
            // 기본값
            ExpToNextLevel = 100 * Level;
            StatPoints += 3;
            SkillPoints += 1;
        }

        // 스탯 재계산
        RecalculateStats();

        // 체력/마나 전체 회복
        CurrentHp = MaxHp;
        CurrentMp = MaxMp;

        OnLevelUp?.Invoke(Level);
        Debug.Log($" Level Up! Level {Level}");
        Debug.Log($" Stat Points: {StatPoints}, Skill Points: {SkillPoints}");

        Managers.Game.SaveGame();
    }

    #endregion

    #region Stat Management


    public bool IncreaseStat(EStatType statType, int amount = 1)
    {
        if (amount <= 0 || StatPoints < amount)
        {
            Debug.LogWarning($"Not enough stat points! Have: {StatPoints}, Need: {amount}");
            return false;
        }

        switch (statType)
        {
            case EStatType.Strength:
                Strength += amount;
                break;
            case EStatType.Intelligence:
                Intelligence += amount;
                break;
            case EStatType.Agility:
                Agility += amount;
                break;
            case EStatType.Charisma:
                Charisma += amount;
                break;
            default:
                Debug.LogWarning($"Cannot increase stat: {statType}");
                return false;
        }

        StatPoints -= amount;
        RecalculateStats();

        OnStatIncreased?.Invoke(statType, amount);
        Debug.Log($"{statType} +{amount} (Total: {GetStat(statType)})");

        Managers.Game.SaveGame();
        return true;
    }

 
    public int GetStat(EStatType statType)
    {
        return statType switch
        {
            EStatType.Strength => Strength,
            EStatType.Intelligence => Intelligence,
            EStatType.Agility => Agility,
            EStatType.Charisma => Charisma,
            EStatType.MaxHp => MaxHp,
            EStatType.MaxMp => MaxMp,
            _ => 0
        };
    }


    public void RecalculateStats()
    {
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(Managers.Game.PlayerData.characterId);
        if (characterData == null)
        {
            Debug.LogWarning("Character data not found!");
            return;
        }

        // 기본 HP/MP 계산 (레벨 기반)
        int baseHp = characterData.baseHp + (Level - 1) * 10;  // 레벨당 +10 HP
        int baseMp = characterData.baseMp + (Level - 1) * 5;   // 레벨당 +5 MP

        // 장비 보너스 추가
        int equipmentHpBonus = GetEquipmentStatBonus("hp");
        int equipmentMpBonus = GetEquipmentStatBonus("mp");

        MaxHp = baseHp + equipmentHpBonus;
        MaxMp = baseMp + equipmentMpBonus;

        // 현재 HP/MP가 최대치를 넘지 않도록
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        CurrentMp = Mathf.Min(CurrentMp, MaxMp);

        OnStatsRecalculated?.Invoke();
        Debug.Log($"Stats Recalculated - HP: {MaxHp}, MP: {MaxMp}, ATK: {Attack}, DEF: {Defense}");
    }

    private int CalculateAttack()
    {
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(Managers.Game.PlayerData.characterId);
        if (characterData == null)
            return 10;

        // 기본 공격력 + 힘 보너스 + 장비 보너스
        int baseAttack = characterData.baseAttack + Strength;
        int equipmentBonus = GetEquipmentStatBonus("attack");

        return baseAttack + equipmentBonus;
    }

    private int CalculateDefense()
    {
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(Managers.Game.PlayerData.characterId);
        if (characterData == null)
            return 5;

        // 기본 방어력 + 민첩 보너스 + 장비 보너스
        int baseDefense = characterData.baseDefense + (Agility / 2);
        int equipmentBonus = GetEquipmentStatBonus("defense");

        return baseDefense + equipmentBonus;
    }


    private int CalculateSpeed()
    {
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(Managers.Game.PlayerData.characterId);
        if (characterData == null)
            return 10;

        // 기본 속도 + 민첩 보너스 + 장비 보너스
        int baseSpeed = characterData.baseSpeed + Agility;
        int equipmentBonus = GetEquipmentStatBonus("speed");

        return baseSpeed + equipmentBonus;
    }

    private int GetEquipmentStatBonus(string statName)
    {
        int bonus = 0;
        var playerData = Managers.Game.PlayerData;

        // 무기
        if (playerData.equippedWeapon > 0)
        {
            var weaponData = Managers.Data.ItemDict.GetValueOrDefault(playerData.equippedWeapon) as Data.EquipmentItemData;
            if (weaponData != null)
            {
                bonus += GetStatFromEquipment(weaponData, statName);
            }
        }

        // 방어구
        if (playerData.equippedArmor > 0)
        {
            var armorData = Managers.Data.ItemDict.GetValueOrDefault(playerData.equippedArmor) as Data.EquipmentItemData;
            if (armorData != null)
            {
                bonus += GetStatFromEquipment(armorData, statName);
            }
        }

        // 악세사리
        if (playerData.equippedAccessory > 0)
        {
            var accessoryData = Managers.Data.ItemDict.GetValueOrDefault(playerData.equippedAccessory) as Data.EquipmentItemData;
            if (accessoryData != null)
            {
                bonus += GetStatFromEquipment(accessoryData, statName);
            }
        }

        return bonus;
    }


    private int GetStatFromEquipment(Data.EquipmentItemData equipment, string statName)
    {
        return statName.ToLower() switch
        {
            "hp" => equipment.bonusHp,
            "mp" => equipment.bonusMp,
            "attack" => equipment.bonusAttack,
            "defense" => equipment.bonusDefense,
            "speed" => equipment.bonusSpeed,
            _ => 0
        };
    }

    #endregion

    #region HP/MP Management

    public void RestoreHp(int amount)
    {
        int oldHp = CurrentHp;
        CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
        int restored = CurrentHp - oldHp;

        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        Debug.Log($"HP +{restored} ({CurrentHp}/{MaxHp})");
    }

    public void RestoreMp(int amount)
    {
        int oldMp = CurrentMp;
        CurrentMp = Mathf.Min(CurrentMp + amount, MaxMp);
        int restored = CurrentMp - oldMp;

        OnMpChanged?.Invoke(CurrentMp, MaxMp);
        Debug.Log($"MP +{restored} ({CurrentMp}/{MaxMp})");
    }


    public void TakeDamage(int damage)
    {
        CurrentHp -= damage;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0)
        {
            Debug.Log("Player defeated!");
            // TODO: 게임오버 처리
        }
    }

    public bool ConsumeMp(int amount)
    {
        if (CurrentMp < amount)
            return false;

        CurrentMp -= amount;
        OnMpChanged?.Invoke(CurrentMp, MaxMp);
        return true;
    }


    public void FullRestore()
    {
        CurrentHp = MaxHp;
        CurrentMp = MaxMp;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
        OnMpChanged?.Invoke(CurrentMp, MaxMp);
        Debug.Log("Full restore!");
    }

    #endregion

    #region Equipment Management
    public bool EquipItem(int itemId)
    {
        var itemData = Managers.Data.ItemDict.GetValueOrDefault(itemId) as Data.EquipmentItemData;
        if (itemData == null)
        {
            Debug.LogWarning($"Item {itemId} is not equipment!");
            return false;
        }

        // 기존 장비 해제 (인벤토리로 반환)
        int oldEquipmentId = 0;
        switch (itemData.equipSlot)
        {
            case EEquipmentSlot.Weapon:
                oldEquipmentId = Managers.Game.PlayerData.equippedWeapon;
                Managers.Game.PlayerData.equippedWeapon = itemId;
                break;
            case EEquipmentSlot.Armor:
                oldEquipmentId = Managers.Game.PlayerData.equippedArmor;
                Managers.Game.PlayerData.equippedArmor = itemId;
                break;
            case EEquipmentSlot.Accessory:
                oldEquipmentId = Managers.Game.PlayerData.equippedAccessory;
                Managers.Game.PlayerData.equippedAccessory = itemId;
                break;
        }

        // TODO: 인벤토리에서 제거, 기존 장비 인벤토리로 반환

        RecalculateStats();
        Debug.Log($"Equipped: {itemData.itemName}");

        Managers.Game.SaveGame();
        return true;
    }

    public bool UnequipItem(EEquipmentSlot slot)
    {
        int equipmentId = 0;
        switch (slot)
        {
            case EEquipmentSlot.Weapon:
                equipmentId = Managers.Game.PlayerData.equippedWeapon;
                Managers.Game.PlayerData.equippedWeapon = 0;
                break;
            case EEquipmentSlot.Armor:
                equipmentId = Managers.Game.PlayerData.equippedArmor;
                Managers.Game.PlayerData.equippedArmor = 0;
                break;
            case EEquipmentSlot.Accessory:
                equipmentId = Managers.Game.PlayerData.equippedAccessory;
                Managers.Game.PlayerData.equippedAccessory = 0;
                break;
        }

        if (equipmentId == 0)
            return false;

        // TODO: 인벤토리로 반환

        RecalculateStats();
        Debug.Log($"Unequipped: {slot}");

        Managers.Game.SaveGame();
        return true;
    }

    #endregion

    #region Skill Management

    public bool LearnSkill(int skillId)
    {
        var skillData = Managers.Data.SkillDict.GetValueOrDefault(skillId);
        if (skillData == null)
        {
            Debug.LogWarning($"Skill {skillId} not found!");
            return false;
        }

        // 이미 배운 스킬인지 확인
        if (Managers.Game.PlayerData.learnedSkills.Contains(skillId))
        {
            Debug.LogWarning($"Skill {skillData.skillName} already learned!");
            return false;
        }

        // 조건 확인
        if (!CanLearnSkill(skillData))
        {
            Debug.LogWarning($"Cannot learn {skillData.skillName} - requirements not met");
            return false;
        }

        // 스킬 포인트 소모
        if (skillData.learnCost > 0)
        {
            if (SkillPoints < skillData.learnCost)
            {
                Debug.LogWarning($"Not enough skill points! Have: {SkillPoints}, Need: {skillData.learnCost}");
                return false;
            }
            SkillPoints -= skillData.learnCost;
        }

        // 스킬 배우기
        Managers.Game.PlayerData.learnedSkills.Add(skillId);
        OnSkillLearned?.Invoke(skillId);
        Debug.Log($"★ Learned Skill: {skillData.skillName}");

        Managers.Game.SaveGame();
        return true;
    }


    private bool CanLearnSkill(Data.SkillData skillData)
    {
        // 레벨 확인
        if (Level < skillData.requiredLevel)
            return false;

        // 스탯 확인
        if (Strength < skillData.requiredStrength)
            return false;
        if (Intelligence < skillData.requiredIntelligence)
            return false;
        if (Agility < skillData.requiredAgility)
            return false;

        return true;
    }


    public bool EquipSkill(int skillId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogWarning($"Invalid skill slot: {slotIndex}");
            return false;
        }

        // 배운 스킬인지 확인
        if (!Managers.Game.PlayerData.learnedSkills.Contains(skillId))
        {
            Debug.LogWarning("Skill not learned!");
            return false;
        }

        // 슬롯 확장
        while (Managers.Game.PlayerData.equippedSkills.Count <= slotIndex)
        {
            Managers.Game.PlayerData.equippedSkills.Add(0);
        }

        Managers.Game.PlayerData.equippedSkills[slotIndex] = skillId;
        Debug.Log($"Skill {skillId} equipped to slot {slotIndex}");

        Managers.Game.SaveGame();
        return true;
    }

    public bool UnequipSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= Managers.Game.PlayerData.equippedSkills.Count)
            return false;

        Managers.Game.PlayerData.equippedSkills[slotIndex] = 0;
        Debug.Log($"Skill unequipped from slot {slotIndex}");

        Managers.Game.SaveGame();
        return true;
    }


    public List<int> GetEquippedSkills()
    {
        return Managers.Game.PlayerData.equippedSkills.Where(id => id > 0).ToList();
    }

    #endregion

    #region Helpers

    public bool IsDead => CurrentHp <= 0;

    public bool CheckStat(EStatType statType, int requiredValue)
    {
        int currentValue = GetStat(statType);
        return currentValue >= requiredValue;
    }

    #endregion
}
