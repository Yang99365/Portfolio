using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using static HeroManager;


public class Hero : Creature
{
    #region Hero Specific Data
    // 영웅 인스턴스 ID (세이브 데이터용)
    public int HeroInstanceId { get; private set; }

    // 레벨 시스템
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; } = 0;
    public int ExperienceToNextLevel => Level * 100; // 임시 공식

    // 장비 시스템
    private Dictionary<EEquipmentType, Data.EquipmentData> _equippedItems = new Dictionary<EEquipmentType, Data.EquipmentData>();

    // 스킬 시스템 (스킬큐브에서 나온 스킬 중 장착해서 낀 스킬들)
    private Dictionary<int, SkillCube> _equippedSkillCubes = new Dictionary<int, SkillCube>(); // slotIndex, SkillCube
    public const int MAX_SKILL_SLOTS = 4; // 최대 스킬 슬롯

    private List<SkillCube> _activeSkills = new List<SkillCube>();   // 액티브 스킬만
    private List<SkillCube> _passiveSkills = new List<SkillCube>();  // 패시브 스킬만

    // 버프 관리
    private Dictionary<int, ActiveBuffInfo> _activeBuffs = new Dictionary<int, ActiveBuffInfo>();

    #region Base Stats
    private float _baseAttack;
    private float _baseDefense;
    private float _baseMaxHp;
    private float _baseAttackSpeed;
    private float _baseCriticalChance;
    private float _baseCriticalDamage;
    #endregion
    #region Bonus Stats
    // 레벨 보너스
    private float _levelBonusAttack;
    private float _levelBonusDefense;
    private float _levelBonusMaxHp;

    // 장비 보너스
    private float _equipmentBonusAttack;
    private float _equipmentBonusDefense;
    private float _equipmentBonusMaxHp;
    private float _equipmentBonusAttackSpeed;
    private float _equipmentBonusCriticalChance;
    private float _equipmentBonusCriticalDamage;

    // 패시브 스킬 보너스
    private float _passiveBonusAttack;
    private float _passiveBonusDefense;
    private float _passiveBonusMaxHp;

    // 마스터리 보너스
    private float _masteryBonusAttackPercent;
    private float _masteryBonusDefensePercent;
    private float _masteryBonusMaxHpPercent;
    private float _masteryBonusAttackSpeedPercent;
    private float _masteryBonusCriticalChancePercent;
    private float _masteryBonusCriticalDamagePercent;

    // 스텟 보너스
    private float _buffBonusAttack;
    private float _buffBonusDefense;
    private float _buffBonusMaxHp;
    private float _buffBonusAttackSpeed;
    private float _buffBonusCriticalChance;
    private float _buffBonusCriticalDamage;
    #endregion

    // 이벤트
    public event Action<Hero, int> OnLevelUp;
    public event Action<Data.EquipmentData, EEquipmentType> OnEquipmentChanged;
    public event Action<SkillCube, int> OnSkillEquipped; // 스킬, 슬롯 인덱스
    public event Action<SkillCube> OnSkillUsed;
    public event Action<List<ActiveBuffInfo>> OnBuffsChanged; // buff ui 업데이트용 이벤트
    #endregion


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    // Hero 전용 SetInfo
    public void SetHeroInfo(int heroTemplateId, int instanceId, HeroSaveData saveData = null)
    {
        base.SetInfo(heroTemplateId, true);

        HeroInstanceId = instanceId;

        // Animator Override Controller 로드
        LoadAnimatorOverride();

        InitializeBaseStats();

        if (saveData != null)
        {
            LoadFromSaveData(saveData);
        }
        else
        {
            InitializeAsNewHero();
        }

        CalculateFinalStats();

    }
    private void LoadAnimatorOverride()
    {
        if (HeroData == null || string.IsNullOrEmpty(HeroData.animatorAddress))
        {
            Debug.LogWarning($"No animator address for hero {HeroData?.characterName}");
            return;
        }

        Managers.Resource.LoadAsync<AnimatorOverrideController>(
            HeroData.animatorAddress,
            (overrideController) =>
            {
                if (Animator != null && overrideController != null)
                {
                    Animator.runtimeAnimatorController = overrideController;
                    Debug.Log($"Loaded animator for {HeroData.characterName}");
                }
            }
        );
    }
    private void InitializeBaseStats()
    {
        if (HeroData == null)
        {
            Debug.LogError("HeroData is null in SetHeroInfo");
            return;
        }

        _baseAttack = HeroData.stats.attack;
        _baseDefense = HeroData.stats.defense;
        _baseMaxHp = HeroData.stats.maxHealth;
        _baseAttackSpeed = HeroData.stats.attackSpeed;
        _baseCriticalChance = HeroData.stats.criticalChance;
        _baseCriticalDamage = HeroData.stats.criticalDamage;
    }

    private void LoadFromSaveData(HeroSaveData saveData)
    {
        Level = saveData.level;
        Experience = saveData.exp;
        SlotIndex = saveData.slotIndex;

        // 장비, 스킬큐브는 매니저에서 로드
    }


    private void InitializeAsNewHero()
    {
        Level = 1;
        Experience = 0;
        EquipDefaultSkills();
    }

    #region Stat Calculation
    // 최종 스탯 계산 (기본 + 장비 + 마스터리)
    public void CalculateFinalStats()
    {
        // 1. 레벨 보너스 계산
        CalculateLevelBonus();

        // 2. 장비 보너스 계산
        CalculateEquipmentBonus();

        // 3. 패시브 스킬 보너스 계산
        CalculatePassiveSkillBonus();

        // 4. 버프 보너스 계산
        CalculateBuffBonus();

        // 5. 최종 스탯 적용
        ApplyFinalStats();

        Managers.Hero?.NotifyStatsChanged(DataTemplateID);
    }
    private void CalculateLevelBonus()
    {
        // 레벨 보너스는 단순 계산
        _levelBonusAttack = (Level - 1) * 2;
        _levelBonusDefense = (Level - 1) * 1;
        _levelBonusMaxHp = (Level - 1) * 10;
    }

    private void CalculateEquipmentBonus()
    {
        // 장비 보너스 초기화
        _equipmentBonusAttack = 0;
        _equipmentBonusDefense = 0;
        _equipmentBonusMaxHp = 0;
        _equipmentBonusAttackSpeed = 0;
        _equipmentBonusCriticalChance = 0;
        _equipmentBonusCriticalDamage = 0;

        // 현재 장착된 장비만 계산
        foreach (var equipment in _equippedItems.Values)
        {
            if (equipment != null)
            {
                _equipmentBonusAttack += equipment.stats.attack;
                _equipmentBonusDefense += equipment.stats.defense;
                _equipmentBonusMaxHp += equipment.stats.maxHealth;
                _equipmentBonusAttackSpeed += equipment.stats.attackSpeed;
                _equipmentBonusCriticalChance += equipment.stats.criticalChance;
                _equipmentBonusCriticalDamage += equipment.stats.criticalDamage;
            }
        }
    }

    private void CalculatePassiveSkillBonus()
    {
        // 패시브 보너스 초기화
        _passiveBonusAttack = 0;
        _passiveBonusDefense = 0;
        _passiveBonusMaxHp = 0;

        // 현재 장착된 패시브 스킬만 계산
        foreach (var skillCube in _passiveSkills)
        {
            if (skillCube?.SkillData == null) continue;

            foreach (var effect in skillCube.SkillData.effects)
            {
                if (effect.effectType == ESkillEffectType.Buff &&
                    effect.skillTargetType == ESkillTargetType.Self)
                {
                    // 패시브는 기본값의 %로 계산
                    float buffValue = effect.value * (1 + (skillCube.Level - 1) * 0.1f);

                    switch (effect.statType)
                    {
                        case EStat.Attack:
                            _passiveBonusAttack += _baseAttack * buffValue;
                            break;
                        case EStat.Defense:
                            _passiveBonusDefense += _baseDefense * buffValue;
                            break;
                        case EStat.MaxHealth:
                            _passiveBonusMaxHp += _baseMaxHp * buffValue;
                            break;
                    }
                }
            }
        }
    }
    private void CalculateBuffBonus()
    {
        _buffBonusAttack = 0;
        _buffBonusDefense = 0;
        _buffBonusMaxHp = 0;
        _buffBonusAttackSpeed = 0;
        _buffBonusCriticalChance = 0;
        _buffBonusCriticalDamage= 0;

        // 모든 활성 버프의 효과를 합산
        foreach (var buffInfo in _activeBuffs.Values)
        {
            if (buffInfo.targetType == ESkillTargetType.Self)
            {
                // 스탯 타입에 따라 적절한 보너스에 추가
                switch (buffInfo.statType)
                {
                    case EStat.Attack:
                        _buffBonusAttack += _baseAttack * buffInfo.buffValue;
                        break;
                    case EStat.Defense:
                        _buffBonusDefense += _baseDefense * buffInfo.buffValue;
                        break;
                    case EStat.MaxHealth:
                        _buffBonusMaxHp += _baseMaxHp * buffInfo.buffValue;
                        break;
                    case EStat.AttackSpeed:
                        _buffBonusAttackSpeed += _baseAttackSpeed * buffInfo.buffValue;
                        break;
                    case EStat.criticalChance:
                        _buffBonusCriticalChance += _baseCriticalChance * buffInfo.buffValue;
                        break;
                    case EStat.criticalDamage:
                        _buffBonusCriticalDamage += _baseCriticalDamage * buffInfo.buffValue;
                        break;
                }
            }
        }
    }
    //상점에서 마스터리 구매 시 호출
    public void ApplyMasteryBonus(float attackPercent, float defensePercent, float maxHpPercent,
                                   float attackSpeedPercent, float critChancePercent, float critDamagePercent)
    {
        // 퍼센트 값 저장
        _masteryBonusAttackPercent = attackPercent;
        _masteryBonusDefensePercent = defensePercent;
        _masteryBonusMaxHpPercent = maxHpPercent;
        _masteryBonusAttackSpeedPercent = attackSpeedPercent;
        _masteryBonusCriticalChancePercent = critChancePercent;
        _masteryBonusCriticalDamagePercent = critDamagePercent;

        // 마스터리 보너스 재계산
        ApplyFinalStats();
    }


    private void ApplyFinalStats()
    {
        // 마스터리 보너스는 여기서 직접 계산
        float masteryAttackBonus = _baseAttack * _masteryBonusAttackPercent / 100f;
        float masteryDefenseBonus = _baseDefense * _masteryBonusDefensePercent / 100f;
        float masteryMaxHpBonus = _baseMaxHp * _masteryBonusMaxHpPercent / 100f;
        float masteryAttackSpeedBonus = _baseAttackSpeed * _masteryBonusAttackSpeedPercent / 100f;
        float masteryCritChanceBonus = _baseCriticalChance * _masteryBonusCriticalChancePercent / 100f;
        float masteryCritDamageBonus = _baseCriticalDamage * _masteryBonusCriticalDamagePercent / 100f;

        // 최종 스탯 = 기본 + 레벨 + 장비 + 패시브 + 마스터리
        Attack = _baseAttack +
                 _levelBonusAttack +
                 _equipmentBonusAttack +
                 _passiveBonusAttack +
                 _buffBonusAttack +
                 masteryAttackBonus;

        Defense = _baseDefense +
                  _levelBonusDefense +
                  _equipmentBonusDefense +
                  _passiveBonusDefense +
                  _buffBonusDefense +
                  masteryDefenseBonus;

        MaxHp = _baseMaxHp +
                _levelBonusMaxHp +
                _equipmentBonusMaxHp +
                _passiveBonusMaxHp +
                _buffBonusMaxHp +
                masteryMaxHpBonus;

        AttackSpeed = _baseAttackSpeed +
                      _equipmentBonusAttackSpeed +
                      _buffBonusAttackSpeed +
                      masteryAttackSpeedBonus;

        CriticalChance = _baseCriticalChance +
                         _equipmentBonusCriticalChance +
                         _buffBonusCriticalChance +
                         masteryCritChanceBonus;

        CriticalDamage = _baseCriticalDamage +
                         _equipmentBonusCriticalDamage +
                         _buffBonusCriticalDamage +
                         masteryCritDamageBonus;

        // HP 조정
        if (Hp > MaxHp)
            Hp = MaxHp;
    }


    #endregion

    #region Equipment System
    public bool EquipItem(Data.EquipmentData equipmentData)
    {
        if (equipmentData == null)
            return false;

        // 클래스 제한 체크
        if (equipmentData.classRestriction > 0 &&
            equipmentData.classRestriction != (int)HeroData.characterClass)
        {
            return false;
        }

        // 장비 장착
        _equippedItems[equipmentData.equipmentType] = equipmentData;

        // 장비 보너스만 재계산
        CalculateEquipmentBonus();
        ApplyFinalStats();

        OnEquipmentChanged?.Invoke(equipmentData, equipmentData.equipmentType);
        return true;
    }

    // 장비 해제 시
    public Data.EquipmentData UnequipItem(EEquipmentType type)
    {
        if (_equippedItems.TryGetValue(type, out var equipment))
        {
            _equippedItems.Remove(type);

            // 장비 보너스만 재계산
            CalculateEquipmentBonus();
            ApplyFinalStats();

            OnEquipmentChanged?.Invoke(null, type);
            return equipment;
        }

        return null;
    }

    

    // 장착된 장비 가져오기
    public Data.EquipmentData GetEquippedItem(EEquipmentType type)
    {
        _equippedItems.TryGetValue(type, out var equipment);
        return equipment;
    }

    // 모든 장착 장비 가져오기
    public Dictionary<EEquipmentType, Data.EquipmentData> GetAllEquippedItems()
    {
        return new Dictionary<EEquipmentType, Data.EquipmentData>(_equippedItems);
    }
    #endregion

    #region Skill System
    // 스킬 장착
    public bool EquipSkillCube(SkillCube skillCube, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SKILL_SLOTS)
        {
            Debug.LogError($"Invalid skill slot index: {slotIndex}");
            return false;
        }

        if (skillCube == null)
        {
            Debug.LogError("SkillCube is null");
            return false;
        }

        // 스킬큐브가 이 영웅에게 장착 가능한지 확인
        if (!skillCube.CanEquipToHero(this))
        {
            Debug.Log($"SkillCube {skillCube.GetName()} cannot be equipped to {HeroData.characterName}");
            return false;
        }

        // 기존 슬롯에 스킬이 있으면 제거
        if (_equippedSkillCubes.ContainsKey(slotIndex))
        {
            UnequipSkillCube(slotIndex);
        }

        // 스킬큐브 장착
        _equippedSkillCubes[slotIndex] = skillCube;

        // 타입별 리스트에 추가
        if (skillCube.SkillType == ESkillType.Passive)
        {
            _passiveSkills.Add(skillCube);
            CalculatePassiveSkillBonus();
            ApplyFinalStats();
            Debug.Log($"{HeroData.characterName} equipped passive skill: {skillCube.GetName()}");
        }
        else if (skillCube.SkillType == ESkillType.Active)
        {
            _activeSkills.Add(skillCube);
            Debug.Log($"{HeroData.characterName} equipped active skill: {skillCube.GetName()}");
        }

        // 이벤트 발생
        OnSkillEquipped?.Invoke(skillCube, slotIndex);

        return true;
    }
    public bool HasSkill(int skillId)
    {
        return _equippedSkillCubes.Values.Any(s => s != null && s.SkillData != null && s.SkillData.skillId == skillId);
    }
    public SkillCube UnequipSkillCube(int slotIndex)
    {
        if (_equippedSkillCubes.TryGetValue(slotIndex, out var skillCube))
        {
            _equippedSkillCubes.Remove(slotIndex);

            // 타입별 리스트에서도 제거
            if (skillCube.SkillType == ESkillType.Passive)
            {
                _passiveSkills.Remove(skillCube);
                CalculatePassiveSkillBonus();
                ApplyFinalStats();
            }
            else if (skillCube.SkillType == ESkillType.Active)
            {
                _activeSkills.Remove(skillCube);
            }

            OnSkillEquipped?.Invoke(null, slotIndex);

            Debug.Log($"{HeroData.characterName} unequipped skill {skillCube.GetName()}");
            return skillCube;
        }

        return null;
    }
    // 기본 스킬 장착 (세이브 데이터가 없을 때)
    private void EquipDefaultSkills()
    {
        if (HeroData == null || HeroData.skillIds == null)
            return;

        int slotIndex = 0;
        foreach (int skillId in HeroData.skillIds)
        {
            if (slotIndex >= MAX_SKILL_SLOTS)
                break;

            if (Managers.Data.SkillDataDict.TryGetValue(skillId, out var skillData))
            {
                // 임시 스킬큐브 생성 (세이브 없을 때 기본 제공용)
                var defaultCube = new SkillCube(skillId, 1);
                defaultCube.InstanceId = skillId * 10000 + HeroInstanceId; // 임시 고유 ID

                // 내부적으로 장착 (이벤트 발생 없이)
                _equippedSkillCubes[slotIndex] = defaultCube;

                // 타입별 리스트에 추가
                if (skillData.skillType == ESkillType.Passive)
                {
                    _passiveSkills.Add(defaultCube);
                    Debug.Log($"{HeroData.characterName} equipped default passive: {skillData.skillName}");
                }
                else if (skillData.skillType == ESkillType.Active)
                {
                    _activeSkills.Add(defaultCube);
                    Debug.Log($"{HeroData.characterName} equipped default active: {skillData.skillName}");
                }

                slotIndex++;
            }
        }
        Debug.Log($"[{HeroData.characterName}] Total equipped: {_equippedSkillCubes.Count} skills");
        Debug.Log($"[{HeroData.characterName}] Active skills: {_activeSkills.Count}");
        Debug.Log($"[{HeroData.characterName}] Passive skills: {_passiveSkills.Count}");
    }

    // 사용 가능한 스킬 찾기 (쿨타임이 끝난 스킬)
    private SkillCube GetReadySkill()
    {
        // 액티브 스킬 리스트만 검색
        return _activeSkills
            .Where(s => s != null && s.IsReady)
            .FirstOrDefault();
    }

    // 모든 스킬 쿨다운 초기화 (스테이지 전환 시)
    public void ResetAllSkillCooldowns()
    {
        // 액티브 스킬만 초기화하면 됨
        foreach (var skillCube in _activeSkills)
        {
            skillCube?.ResetCooldown();
        }
    }
    public Dictionary<int, SkillCube> GetEquippedSkillCubes()
    {
        return new Dictionary<int, SkillCube>(_equippedSkillCubes);
    }

    // 특정 슬롯의 스킬큐브 가져오기
    public SkillCube GetSkillCubeAtSlot(int slotIndex)
    {
        _equippedSkillCubes.TryGetValue(slotIndex, out var skillCube);
        return skillCube;
    }
    public List<SkillCube> GetActiveSkills()
    {
        return new List<SkillCube>(_activeSkills);
    }

    // 패시브 스킬 목록 가져오기
    public List<SkillCube> GetPassiveSkills()
    {
        return new List<SkillCube>(_passiveSkills);
    }
    #endregion

    #region Battle AI Override
    protected override void UpdateIdle()
    {
        // 타겟 유효성 체크 - 매번 확인
        if (!IsTargetValid())
        {
            FindNewTarget();

            // 타겟을 못 찾으면 대기
            if (!IsTargetValid())
            {
                Animator.ResetTrigger("Attack");
                return;
            }
        }

        // 액티브 스킬 우선 체크
        var readySkill = GetReadySkill();
        if (readySkill != null)
        {
            UseSkill(readySkill);
        }
        else if (IsAttackReady())
        {
            // 모든 스킬이 쿨다운이면 기본 공격
            PerformBasicAttack();
        }
    }

    // 스킬 사용
    private void UseSkill(SkillCube skillCube)
    {
        if (!IsTargetValid() || skillCube == null)
            return;

        State = EObjectState.Skill;
        skillCube.Use(); // 쿨다운 시작

        // 스킬 애니메이션
        //PlayTriggerAnimation("Skill");

        // 스킬 효과 적용
        ApplySkillEffects(skillCube, skillCube.SkillData.skillId);

        // 이벤트 발생
        OnSkillUsed?.Invoke(skillCube);

        Debug.Log($"{HeroData.characterName} used skill: {skillCube.GetName()}");

        // 스킬 사용 후 Idle 상태로 복귀
        StartCoroutine(ReturnToIdleAfterSkill(0.1f));
    }

    // 스킬 효과 적용
    private void ApplySkillEffects(SkillCube skillCube, int skillId)
    {
        if (skillCube?.SkillData == null) return;

        foreach (var effect in skillCube.SkillData.effects)
        {
            switch (effect.effectType)
            {
                case ESkillEffectType.Damage:
                    ApplyDamageEffect(effect, skillCube.Level);
                    break;
                case ESkillEffectType.Heal:
                    ApplyHealEffect(effect, skillCube.Level);
                    break;
                case ESkillEffectType.Buff:
                    ApplyBuffEffect(effect, skillCube.Level, skillId);
                    break;
                case ESkillEffectType.DeBuff:
                    ApplyDebuffEffect(effect, skillCube.Level, skillId);
                    break;
                case ESkillEffectType.Summon:
                    // 소환은 추후 구현
                    break;
            }
        }
    }

    // 데미지 스킬 효과 (스킬 레벨 반영)
    private void ApplyDamageEffect(Data.SkillEffect effect, int skillLevel)
    {
        if (!IsTargetValid())
            return;

        // 스킬 레벨에 따른 데미지 증가 (레벨당 10% 증가)
        float damage = Attack * effect.value * (1 + (skillLevel - 1) * 0.1f);
        damage = CalculateDamage(damage);

        Creature targetCreature = Target as Creature;
        if (targetCreature != null)
        {
            // 타겟 타입에 따른 처리
            if (effect.skillTargetType == ESkillTargetType.Single)
            {
                targetCreature.TakeDamage(damage, this);
            }
            else if (effect.skillTargetType == ESkillTargetType.Multi)
            {
                // 멀티 타겟은 BattleManager에서 처리
                var allEnemies = Managers.Battle.GetAllAliveMonsters();
                foreach (var enemy in allEnemies)
                {
                    enemy.TakeDamage(damage * 0.7f, this); // 광역은 70% 데미지
                }
            }
        }
    }

    // 힐 스킬 효과 (스킬 레벨 반영)
    private void ApplyHealEffect(Data.SkillEffect effect, int skillLevel)
    {
        if (effect.skillTargetType == ESkillTargetType.Self)
        {
            // 스킬 레벨에 따른 힐량 증가
            float healAmount = MaxHp * effect.value * (1 + (skillLevel - 1) * 0.1f);
            Heal(healAmount);

            Debug.Log($"{HeroData.characterName} healed for {healAmount}");
        }
        // 파티 힐은 추후 구현
    }

    // 버프 효과 (스킬 레벨 반영)
    private void ApplyBuffEffect(Data.SkillEffect effect, int skillLevel, int skillId)
    {
        ApplyTemporaryBuff(skillId, effect, skillLevel);
    }

    // 디버프 효과 (스킬 레벨 반영)
    private void ApplyDebuffEffect(Data.SkillEffect effect, int skillLevel, int skillId)
    {
        if (!IsTargetValid())
            return;

        // 디버프 값 계산 (레벨에 따른 효과 증가)
        float debuffValue = effect.value * (1 + (skillLevel - 1) * 0.05f);

        // 타겟 타입에 따른 처리
        if (effect.skillTargetType == ESkillTargetType.Single)
        {
            // 단일 대상
            Monster targetMonster = Target as Monster;
            if (targetMonster != null)
            {
                targetMonster.ApplyDebuff(skillId, effect.statType, debuffValue, effect.duration, skillLevel);
                Debug.Log($"{HeroData.characterName} applied debuff to {targetMonster.MonsterData.monsterName}");
            }
        }
        else if (effect.skillTargetType == ESkillTargetType.Multi)
        {
            // 다중 대상
            var allEnemies = Managers.Battle.GetAllAliveMonsters();
            foreach (var enemy in allEnemies)
            {
                enemy.ApplyDebuff(skillId, effect.statType, debuffValue * 0.7f, effect.duration, skillLevel);  // 광역은 70%
            }
            Debug.Log($"{HeroData.characterName} applied AoE debuff to {allEnemies.Count} enemies");
        }
    }
    private void ApplyTemporaryBuff(int skillId, Data.SkillEffect effect, int skillLevel)
    {
        if (skillId == 0)
        {
            Debug.LogWarning("Invalid skill ID for buff!");
            return;
        }

        float buffValue = effect.value * (1 + (skillLevel - 1) * 0.05f);

        if (_activeBuffs.ContainsKey(skillId))
        {
            var existingBuff = _activeBuffs[skillId];

            if (skillLevel > existingBuff.skillLevel)
            {
                Debug.Log($"[{HeroData.characterName}] Buff upgraded: Level {existingBuff.skillLevel} -> {skillLevel}");
                RemoveBuff(skillId);
                AddNewBuff(skillId, skillLevel, effect.statType, buffValue, effect.duration, effect.skillTargetType);  // statType 추가!
            }
            else if (skillLevel == existingBuff.skillLevel)
            {
                Debug.Log($"[{HeroData.characterName}] Buff duration refreshed: {effect.duration} seconds");
                RefreshBuffDuration(skillId, effect.duration);
            }
            else
            {
                Debug.Log($"[{HeroData.characterName}] Lower level buff ignored");
            }
        }
        else
        {
            AddNewBuff(skillId, skillLevel, effect.statType, buffValue, effect.duration, effect.skillTargetType);  // statType 추가!
        }
    }
    private void AddNewBuff(int skillId, int skillLevel, EStat statType, float buffValue, float duration, ESkillTargetType targetType)
    {
        var buffInfo = new ActiveBuffInfo(skillId, skillLevel, statType, buffValue, duration, targetType);
        _activeBuffs[skillId] = buffInfo;
        buffInfo.coroutine = StartCoroutine(BuffDurationCoroutine(skillId, buffInfo));

        CalculateFinalStats();
        OnBuffsChanged?.Invoke(GetActiveBuffs());
    }
    private void RefreshBuffDuration(int skillId, float newDuration)
    {
        if (_activeBuffs.TryGetValue(skillId, out var buffInfo))
        {
            if (buffInfo.coroutine != null)
                StopCoroutine(buffInfo.coroutine);

            buffInfo.duration = newDuration;
            buffInfo.remainingTime = newDuration;
            buffInfo.coroutine = StartCoroutine(BuffDurationCoroutine(skillId, buffInfo));

        }
        OnBuffsChanged?.Invoke(GetActiveBuffs());
    }
    private void RemoveBuff(int skillId)
    {
        if (_activeBuffs.TryGetValue(skillId, out var buffInfo))
        {
            if (buffInfo.coroutine != null)
                StopCoroutine(buffInfo.coroutine);

            _activeBuffs.Remove(skillId);
            Debug.Log($"[{HeroData.characterName}] Buff removed: Skill ID {skillId}");
            CalculateFinalStats();
        }
        OnBuffsChanged?.Invoke(GetActiveBuffs());
    }
    private IEnumerator BuffDurationCoroutine(int skillId, ActiveBuffInfo buffInfo)
    {
        float elapsed = 0f;

        while (elapsed < buffInfo.duration)
        {
            elapsed += Time.deltaTime;
            buffInfo.remainingTime = buffInfo.duration - elapsed;
            yield return null;
        }

        Debug.Log($"[{HeroData.characterName}] Buff expired: Skill ID {skillId}");
        _activeBuffs.Remove(skillId);
        CalculateFinalStats();
    }
    public void ClearAllBuffs()
    {
        foreach (var buffInfo in _activeBuffs.Values)
        {
            if (buffInfo.coroutine != null)
                StopCoroutine(buffInfo.coroutine);
        }

        _activeBuffs.Clear();
        CalculateFinalStats();
        Managers.Hero?.NotifyStatsChanged(DataTemplateID);
        OnBuffsChanged?.Invoke(GetActiveBuffs());
    }
    public List<ActiveBuffInfo> GetActiveBuffs()
    {
        return new List<ActiveBuffInfo>(_activeBuffs.Values);
    }
    // 스킬 사용 후 Idle 상태로 복귀
    private IEnumerator ReturnToIdleAfterSkill(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (State == EObjectState.Skill)
        {
            State = EObjectState.Idle;
        }
    }
    protected override void FindNewTarget()
    {
        // 기존 타겟이 유효하면 유지
        if (IsTargetValid())
            return;

        // BattleManager에서 살아있는 몬스터 찾기
        var aliveMonsters = Managers.Battle.GetAllAliveMonsters();

        if (aliveMonsters.Count > 0)
        {
            // 같은 슬롯의 몬스터 우선
            var sameSlotMonster = aliveMonsters.Find(m => m.SlotIndex == SlotIndex);
            if (sameSlotMonster != null)
            {
                SetTarget(sameSlotMonster);
                Debug.Log($"{HeroData.characterName} targets {sameSlotMonster.MonsterData.monsterName} (same slot)");
            }
            else
            {
                // 가장 가까운 몬스터 (첫 번째)
                SetTarget(aliveMonsters[0]);
                Debug.Log($"{HeroData.characterName} targets {aliveMonsters[0].MonsterData.monsterName}");
            }
        }
        else
        {
            // 타겟이 없으면 null로 설정
            Target = null;
        }
    }

    #endregion

    #region Level System
    // 경험치 획득
    public void GainExperience(int amount)
    {
        Experience += amount;

        // 레벨업 체크
        while (Experience >= ExperienceToNextLevel)
        {
            Experience -= ExperienceToNextLevel;
            LevelUp();
        }
    }

    // 레벨업
    private void LevelUp()
    {
        Level++;

        // 스탯 재계산
        CalculateLevelBonus();
        ApplyFinalStats();

        // HP 완전 회복
        Hp = MaxHp;

        // 이벤트 발생
        OnLevelUp?.Invoke(this, Level);

        Debug.Log($"{HeroData.characterName} leveled up to {Level}!");
    }
    #endregion

    #region Override Methods
    // 영웅 전용 완전 회복 (스킬 쿨타임도 초기화)
    public override void FullRestore()
    {
        base.FullRestore(); // 부모 클래스의 FullRestore 호출 (AI 재시작 포함)
        ResetAllSkillCooldowns(); // 스킬 쿨다운 초기화
        ClearAllBuffs();
        Debug.Log($"Hero {HeroData.characterName} fully restored");
    }

    // 영웅은 죽어도 보상을 드롭하지 않음
    protected override void DropReward()
    {
        // 영웅은 보상 없음
    }

    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);

        // 영웅 사망 시 특별 처리
        Debug.Log($"Hero {HeroData.characterName} has fallen!");
    }
    #endregion

    #region Save Data
    // 세이브 데이터 생성
    public HeroSaveData CreateSaveData()
    {
        var saveData = new HeroSaveData
        {
            templateId = DataTemplateID,
            level = Level,
            exp = Experience,
            slotIndex = SlotIndex,
            isUnlocked = true,
            weaponId = GetEquippedItem(EEquipmentType.Weapon)?.baseId ?? 0,
            armorId = GetEquippedItem(EEquipmentType.Armor)?.baseId ?? 0,
            accessoryId = GetEquippedItem(EEquipmentType.Accessory)?.baseId ?? 0,
            skills = new List<SkillSaveData>()
        };

        // 스킬 저장
        foreach (var kvp in _equippedSkillCubes)
        {
            if (kvp.Value != null)
            {
                saveData.skills.Add(new SkillSaveData
                {
                    instanceId = kvp.Value.InstanceId,
                    skillId = kvp.Value.DataId,
                    level = kvp.Value.Level,
                    equipSlot = kvp.Key // 슬롯 인덱스
                });
            }
        }

        return saveData;
    }
    #endregion
    #region Cleanup

    protected override void OnDestroy()
    {
        // ClearAllBuffs()를 호출하면 코루틴 정리와 딕셔너리 클리어를 한 번에 처리
        ClearAllBuffs();

        // 리스트 정리
        _activeSkills.Clear();
        _passiveSkills.Clear();
        _equippedSkillCubes.Clear();

        base.OnDestroy();
    }

    #endregion
}



[Serializable]
public class ActiveBuffInfo
{
    public int skillId;
    public int skillLevel;
    public EStat statType;
    public float buffValue;
    public float duration;
    public float remainingTime;
    public ESkillTargetType targetType;
    public Coroutine coroutine;

    public ActiveBuffInfo(int skillId, int skillLevel, EStat statType, float buffValue, float duration, ESkillTargetType targetType)
    {
        this.skillId = skillId;
        this.skillLevel = skillLevel;
        this.statType = statType;
        this.buffValue = buffValue;
        this.duration = duration;
        this.remainingTime = duration;
        this.targetType = targetType;
    }
}

