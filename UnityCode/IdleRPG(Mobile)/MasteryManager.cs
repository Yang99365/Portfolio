using System;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MasteryManager
{
    #region Events
    public event Action OnMasteryChanged;
    #endregion

    #region Properties
    public int AttackLevel
    {
        get => Managers.Game.SaveData.masteryAttackLevel;
        private set
        {
            Managers.Game.SaveData.masteryAttackLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }

    public int DefenseLevel
    {
        get => Managers.Game.SaveData.masteryDefenseLevel;
        private set
        {
            Managers.Game.SaveData.masteryDefenseLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }

    public int MaxHpLevel
    {
        get => Managers.Game.SaveData.masteryMaxHpLevel;
        private set
        {
            Managers.Game.SaveData.masteryMaxHpLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }

    public int AttackSpeedLevel
    {
        get => Managers.Game.SaveData.masteryAttackSpeedLevel;
        private set
        {
            Managers.Game.SaveData.masteryAttackSpeedLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }

    public int CritChanceLevel
    {
        get => Managers.Game.SaveData.masteryCritChanceLevel;
        private set
        {
            Managers.Game.SaveData.masteryCritChanceLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }

    public int CritDamageLevel
    {
        get => Managers.Game.SaveData.masteryCritDamageLevel;
        private set
        {
            Managers.Game.SaveData.masteryCritDamageLevel = value;
            OnMasteryChanged?.Invoke();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 마스터리 레벨 가져오기
    /// </summary>
    public int GetMasteryLevel(int masteryId)
    {
        if (!Managers.Data.MasteryDic.TryGetValue(masteryId, out Data.MasteryData masteryData))
        {
            Debug.LogError($"Invalid mastery ID: {masteryId}");
            return 0;
        }

        switch (masteryData.statType)
        {
            case "Attack": return AttackLevel;
            case "Defense": return DefenseLevel;
            case "MaxHealth": return MaxHpLevel;
            case "AttackSpeed": return AttackSpeedLevel;
            case "criticalChance": return CritChanceLevel;
            case "criticalDamage": return CritDamageLevel;
            default:
                Debug.LogError($"Unknown stat type: {masteryData.statType}");
                return 0;
        }
    }

    /// <summary>
    /// 마스터리 업그레이드 가능 여부 확인
    /// </summary>
    public bool CanUpgrade(int masteryId)
    {
        if (!Managers.Data.MasteryDic.TryGetValue(masteryId, out Data.MasteryData masteryData))
            return false;

        int currentLevel = GetMasteryLevel(masteryId);

        // 최대 레벨 체크
        if (currentLevel >= masteryData.maxLevel)
            return false;

        // 골드 체크
        int cost = masteryData.GetCostForLevel(currentLevel + 1);
        if (Managers.Game.Gold < cost)
            return false;

        return true;
    }

    /// <summary>
    /// 마스터리 업그레이드
    /// </summary>
    public bool UpgradeMastery(int masteryId)
    {
        if (!Managers.Data.MasteryDic.TryGetValue(masteryId, out Data.MasteryData masteryData))
        {
            Debug.LogError($"Invalid mastery ID: {masteryId}");
            return false;
        }

        int currentLevel = GetMasteryLevel(masteryId);

        // 최대 레벨 체크
        if (currentLevel >= masteryData.maxLevel)
        {
            Debug.Log($"Mastery {masteryData.masteryName} is already max level");
            return false;
        }

        // 골드 체크
        int cost = masteryData.GetCostForLevel(currentLevel + 1);
        if (Managers.Game.Gold < cost)
        {
            Debug.Log($"Not enough gold to upgrade {masteryData.masteryName}");
            return false;
        }

        // 골드 차감
        Managers.Game.Gold -= cost;

        // 레벨 업
        switch (masteryData.statType)
        {
            case "Attack": AttackLevel++; break;
            case "Defense": DefenseLevel++; break;
            case "MaxHealth": MaxHpLevel++; break;
            case "AttackSpeed": AttackSpeedLevel++; break;
            case "criticalChance": CritChanceLevel++; break;
            case "criticalDamage": CritDamageLevel++; break;
            default:
                Debug.LogError($"Unknown stat type: {masteryData.statType}");
                return false;
        }

        Debug.Log($"Upgraded {masteryData.masteryName} to level {GetMasteryLevel(masteryId)}");

        // 모든 영웅에게 마스터리 보너스 적용
        ApplyMasteryToAllHeroes();

        return true;
    }

    /// <summary>
    /// 현재 마스터리 보너스 계산 (%)
    /// </summary>
    public float GetMasteryBonus(int masteryId)
    {
        if (!Managers.Data.MasteryDic.TryGetValue(masteryId, out Data.MasteryData masteryData))
            return 0f;

        int currentLevel = GetMasteryLevel(masteryId);
        return masteryData.GetTotalBonusForLevel(currentLevel);
    }

    /// <summary>
    /// 모든 영웅에게 마스터리 적용
    /// </summary>
    public void ApplyMasteryToAllHeroes()
    {
        float attackBonus = GetMasteryBonus(1);      // Attack
        float defenseBonus = GetMasteryBonus(2);     // Defense
        float maxHpBonus = GetMasteryBonus(3);       // MaxHP
        float attackSpeedBonus = GetMasteryBonus(4); // AttackSpeed
        float critChanceBonus = GetMasteryBonus(5);  // CritChance
        float critDamageBonus = GetMasteryBonus(6);  // CritDamage

        // HeroManager에 마스터리 값 업데이트 (배치/미배치 영웅 모두 적용)
        Managers.Hero.UpdateMastery(
            attackBonus,
            defenseBonus,
            maxHpBonus,
            attackSpeedBonus,
            critChanceBonus,
            critDamageBonus
        );

        Debug.Log($"Applied mastery to all heroes - Attack: +{attackBonus}%, Defense: +{defenseBonus}%, MaxHP: +{maxHpBonus}%");
    }
    #endregion
}