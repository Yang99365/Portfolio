using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;


// 동료 캐릭터 관리
public class PartyManager
{
    #region Companion Class

    public class Companion
    {
        // 기본 정보
        public int CharacterId { get; private set; }
        public Data.CharacterData CharacterData { get; private set; }

        // 전투 스탯
        public int CurrentHp { get; set; }
        public int MaxHp { get; private set; }
        public int CurrentMp { get; set; }
        public int MaxMp { get; private set; }
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public int Speed { get; private set; }

        // 스킬
        public List<int> SkillIds { get; private set; }

        // 상태
        public bool IsDead => CurrentHp <= 0;

        public Companion(int characterId)
        {
            CharacterId = characterId;
            CharacterData = Managers.Data.CharacterDict.GetValueOrDefault(characterId);

            if (CharacterData == null)
            {
                Debug.LogError($"Character data not found: {characterId}");
                return;
            }

            // 스탯 초기화
            MaxHp = CharacterData.baseHp;
            MaxMp = CharacterData.baseMp;
            Attack = CharacterData.baseAttack;
            Defense = CharacterData.baseDefense;
            Speed = CharacterData.baseSpeed;

            CurrentHp = MaxHp;
            CurrentMp = MaxMp;

            // 기본 스킬
            SkillIds = new List<int>(CharacterData.defaultSkillIds);
        }

        public void RestoreHp(int amount)
        {
            CurrentHp = Mathf.Min(CurrentHp + amount, MaxHp);
        }


        public void RestoreMp(int amount)
        {
            CurrentMp = Mathf.Min(CurrentMp + amount, MaxMp);
        }


        public void TakeDamage(int damage)
        {
            CurrentHp = Mathf.Max(CurrentHp - damage, 0);
        }

        public bool ConsumeMp(int amount)
        {
            if (CurrentMp < amount)
                return false;

            CurrentMp -= amount;
            return true;
        }

        /// <summary>
        /// 전체 회복
        /// </summary>
        public void FullRestore()
        {
            CurrentHp = MaxHp;
            CurrentMp = MaxMp;
        }
    }
    #endregion

    #region Properties
    private List<Companion> _companions = new List<Companion>();


    public List<Companion> Companions => _companions;


    public List<Companion> AliveCompanions => _companions.Where(c => !c.IsDead).ToList();


    public int PartySize => _companions.Count;


    public const int MAX_PARTY_SIZE = 3;


    public bool IsPartyFull => _companions.Count >= MAX_PARTY_SIZE;
    #endregion

    #region Events
    public event Action<Companion> OnCompanionJoined;
    public event Action<Companion> OnCompanionLeft;
    public event Action<Companion> OnCompanionDied;
    #endregion

    #region Initialization
    public void Init()
    {
        LoadCompanionsFromSaveData();
        Debug.Log($"PartyManager Initialized - Party Size: {PartySize}");
    }

    private void LoadCompanionsFromSaveData()
    {
        _companions.Clear();

        var companionIds = Managers.Game.SaveData.party.companionIds;
        foreach (int companionId in companionIds)
        {
            var companion = new Companion(companionId);
            _companions.Add(companion);
            Debug.Log($"Loaded companion: {companion.CharacterData.characterName}");
        }
    }
    #endregion

    #region Companion Management


    public bool AddCompanion(int characterId)
    {
        if (IsPartyFull)
        {
            Debug.LogWarning("Party is full! Cannot add more companions.");
            return false;
        }

        // 이미 파티에 있는지 확인
        if (_companions.Any(c => c.CharacterId == characterId))
        {
            Debug.LogWarning($"Companion {characterId} already in party!");
            return false;
        }

        // 캐릭터 데이터 확인
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(characterId);
        if (characterData == null)
        {
            Debug.LogError($"Character data not found: {characterId}");
            return false;
        }

        // 동료 생성 및 추가
        var companion = new Companion(characterId);
        _companions.Add(companion);

        // 세이브 데이터에 추가
        Managers.Game.SaveData.party.companionIds.Add(characterId);

        OnCompanionJoined?.Invoke(companion);
        Debug.Log($" {companion.CharacterData.characterName} joined the party!");

        Managers.Game.SaveGame();
        return true;
    }


    public bool RemoveCompanion(int characterId)
    {
        var companion = _companions.FirstOrDefault(c => c.CharacterId == characterId);
        if (companion == null)
        {
            Debug.LogWarning($"Companion {characterId} not found in party!");
            return false;
        }

        _companions.Remove(companion);

        // 세이브 데이터에서 제거
        Managers.Game.SaveData.party.companionIds.Remove(characterId);

        OnCompanionLeft?.Invoke(companion);
        Debug.Log($"{companion.CharacterData.characterName} left the party.");

        Managers.Game.SaveGame();
        return true;
    }


    public Companion GetCompanion(int characterId)
    {
        return _companions.FirstOrDefault(c => c.CharacterId == characterId);
    }


    public Companion GetCompanionByIndex(int index)
    {
        if (index < 0 || index >= _companions.Count)
            return null;

        return _companions[index];
    }

    #endregion

    #region Party State Management

    public void RestoreParty()
    {
        foreach (var companion in _companions)
        {
            companion.FullRestore();
        }

        Debug.Log("Party fully restored!");
    }

    public void HandleCompanionDeath(Companion companion)
    {
        if (companion == null || !companion.IsDead)
            return;

        OnCompanionDied?.Invoke(companion);
        Debug.Log($"{companion.CharacterData.characterName} has fallen!");
    }

    public bool IsPartyWiped()
    {
        return AliveCompanions.Count == 0;
    }

    #endregion

    #region AI Behavior (전투용)


    public BattleAction DecideCompanionAction(Companion companion, List<object> allies, List<object> enemies)
    {
        if (companion == null || companion.IsDead)
            return null;

        var characterData = companion.CharacterData;
        if (characterData == null)
            return null;

        // AI 타입별 행동 결정
        // 여기서는 간단하게 구현 (나중에 더 복잡하게 만들 수 있음)

        // 1. HP가 낮으면 회복 스킬 우선
        if (companion.CurrentHp < companion.MaxHp * 0.3f)
        {
            var healSkill = GetHealSkill(companion);
            if (healSkill != null && companion.CurrentMp >= healSkill.mpCost)
            {
                return new BattleAction
                {
                    actionType = EBattleActionType.Skill,
                    skillId = healSkill.skillId,
                    target = companion  // 자신에게 힐
                };
            }
        }

        // 2. 스킬 사용 가능하면 공격 스킬
        var attackSkill = GetAttackSkill(companion);
        if (attackSkill != null && companion.CurrentMp >= attackSkill.mpCost)
        {
            var target = SelectEnemyTarget(enemies);
            return new BattleAction
            {
                actionType = EBattleActionType.Skill,
                skillId = attackSkill.skillId,
                target = target
            };
        }

        // 3. 기본 공격
        var enemyTarget = SelectEnemyTarget(enemies);
        return new BattleAction
        {
            actionType = EBattleActionType.Attack,
            target = enemyTarget
        };
    }

    private Data.SkillData GetHealSkill(Companion companion)
    {
        foreach (int skillId in companion.SkillIds)
        {
            var skillData = Managers.Data.SkillDict.GetValueOrDefault(skillId);
            if (skillData != null && skillData.skillType == ESkillType.Heal)
            {
                return skillData;
            }
        }
        return null;
    }

    private Data.SkillData GetAttackSkill(Companion companion)
    {
        foreach (int skillId in companion.SkillIds)
        {
            var skillData = Managers.Data.SkillDict.GetValueOrDefault(skillId);
            if (skillData != null && skillData.skillType == ESkillType.Attack)
            {
                return skillData;
            }
        }
        return null;
    }


    private object SelectEnemyTarget(List<object> enemies)
    {
        if (enemies == null || enemies.Count == 0)
            return null;

        // 임시: 첫 번째 적 선택 (나중에 더 스마트하게)
        return enemies[0];
    }

    #endregion

    #region Helper Classes

    public class BattleAction
    {
        public EBattleActionType actionType;
        public int skillId;
        public object target;       // Companion 또는 Enemy
    }

    #endregion

    #region Debug
    public void DebugPartyInfo()
    {
        Debug.Log("========== Party Info ==========");
        Debug.Log($"Party Size: {PartySize}/{MAX_PARTY_SIZE}");

        for (int i = 0; i < _companions.Count; i++)
        {
            var companion = _companions[i];
            Debug.Log($"[{i}] {companion.CharacterData.characterName}");
            Debug.Log($"    HP: {companion.CurrentHp}/{companion.MaxHp}");
            Debug.Log($"    MP: {companion.CurrentMp}/{companion.MaxMp}");
            Debug.Log($"    ATK: {companion.Attack}, DEF: {companion.Defense}, SPD: {companion.Speed}");
            Debug.Log($"    Skills: {string.Join(", ", companion.SkillIds)}");
        }

        Debug.Log("================================");
    }

    #endregion
}
