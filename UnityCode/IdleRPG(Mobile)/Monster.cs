using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class Monster : Creature
{
    #region Monster Specific Data
    // 몬스터 타입 (일반/보스)
    public bool IsBoss => MonsterData?.isBoss ?? false;

    // 스테이지 정보 (난이도 스케일링용)
    public int StageNumber { get; private set; }
    public float DifficultyMultiplier { get; private set; } = 1.0f;

    // 드롭 관련
    private float _itemDropChance = 0.1f; // 기본 10% 아이템 드롭률
    private float _skillCubeDropChance = 0.05f; // 기본 5% 스킬큐브 드롭률

    // 공격 패턴 (보스용)
    private int _attackPatternIndex = 0;
    private float _specialAttackCooldown = 5f;
    private float _lastSpecialAttackTime = -999f;

    // 이벤트
    public event Action<int, int> OnMonsterKilled; // gold, exp
    public event Action<Monster> OnMonsterSpawned;

    // 디버프 관리
    private Dictionary<int, ActiveBuffInfo> _activeDebuffs = new Dictionary<int, ActiveBuffInfo>();

    // 기본 스탯 (디버프 적용 전 원본 값)
    private float _baseAttack;
    private float _baseDefense;
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;



        return true;
    }

    // Monster 전용 SetInfo
    public void SetMonsterInfo(int monsterTemplateId, int stageNumber, int slotIndex)
    {
        base.SetInfo(monsterTemplateId, false);

        StageNumber = stageNumber;
        SlotIndex = slotIndex;

        // 스테이지에 따른 난이도 조정
        ApplyDifficultyScaling();

        //기본 스탯 저장 (난이도 스케일링 적용 후)
        _baseAttack = Attack;
        _baseDefense = Defense;
  
        // 보스 몬스터 특별 설정
        if (IsBoss)
        {
            ConfigureBossMonster();
            // 보스 버프 적용 후 다시 기본 스탯 저장
            _baseAttack = Attack;
            _baseDefense = Defense;
        }

        // 드롭률 설정
        ConfigureDropRates();

        // 스폰 이벤트
        OnMonsterSpawned?.Invoke(this);

    }

    #region Difficulty Scaling
    // 스테이지 난이도에 따른 스탯 조정
    private void ApplyDifficultyScaling()
    {
        // ConfigData에서 스케일링 값 가져오기
        var balancing = Managers.Data.ConfigData?.gameConfig.balancing;
        if (balancing != null)
        {
            // 스테이지가 올라갈수록 몬스터가 강해짐
            float stageMultiplier = 1 + (StageNumber - 1) * 0.1f; // 스테이지당 10% 증가

            // HP 스케일링
            float hpScaling = Mathf.Pow(balancing.monsterHealthScaling, StageNumber - 1);
            MaxHp = MonsterData.health * hpScaling * stageMultiplier;
            Hp = MaxHp;

            // 공격력 스케일링
            float damageScaling = Mathf.Pow(balancing.monsterDamageScaling, StageNumber - 1);
            Attack = MonsterData.attackPower * damageScaling * stageMultiplier;

            // 방어력도 약간 증가
            Defense = MonsterData.defense * (1 + (StageNumber - 1) * 0.05f);

            DifficultyMultiplier = stageMultiplier;
        }
        else
        {
            // 기본 스케일링
            DifficultyMultiplier = 1 + (StageNumber - 1) * 0.15f;
            MaxHp = MonsterData.health * DifficultyMultiplier;
            Hp = MaxHp;
            Attack = MonsterData.attackPower * DifficultyMultiplier;
            Defense = MonsterData.defense * (1 + (StageNumber - 1) * 0.05f);
        }
    }

    // 보스 몬스터 특별 설정
    private void ConfigureBossMonster()
    {
        // 보스는 추가 스탯 부여
        MaxHp *= 1.5f; // HP 50% 추가
        Hp = MaxHp;
        Attack *= 1.2f; // 공격력 20% 추가
        Defense *= 1.3f; // 방어력 30% 추가

        // 보스는 공격속도가 약간 느림
        AttackSpeed = 0.8f;

        // 보스는 크리티컬 확률이 높음
        CriticalChance = 0.15f;
        CriticalDamage = 2.0f;

        // 보스 전용 드롭률
        _itemDropChance = 0.5f; // 50% 아이템 드롭
        _skillCubeDropChance = 0.3f; // 30% 스킬큐브 드롭
    }

    // 드롭률 설정
    private void ConfigureDropRates()
    {
        var treasureConfig = Managers.Data.ConfigData?.gameConfig.treasureChest;
        if (treasureConfig != null)
        {
            if (IsBoss)
            {
                _itemDropChance = treasureConfig.bossMonsterDropRate;
            }
            else
            {
                _itemDropChance = treasureConfig.normalMonsterDropRate;
            }

            // 스테이지 보너스 드롭률
            _itemDropChance += treasureConfig.stageRarityBonus * StageNumber;
        }

        // 스킬큐브 드롭률
        var skillCubeConfig = Managers.Data.ConfigData?.gameConfig.skillCube;
        if (skillCubeConfig != null)
        {
            _skillCubeDropChance = skillCubeConfig.baseDropRate;
            if (IsBoss)
            {
                _skillCubeDropChance *= 3f; // 보스는 3배
            }
        }
    }
    #endregion

    #region Battle AI Override
    protected override void UpdateIdle()
    {
        if (!IsTargetValid())
        {
            // 타겟이 없으면 새 타겟 찾기
            FindNewTarget();
            return;
        }

        // 보스의 특수 공격 패턴
        if (IsBoss && CanUseSpecialAttack())
        {
            PerformSpecialAttack();
        }
        else if (IsAttackReady())
        {
            PerformBasicAttack();
        }
    }

    // 새 타겟 찾기 (가장 가까운 슬롯의 영웅)
    protected override void FindNewTarget()
    {
        // BattleManager에서 살아있는 영웅 찾기
        var aliveHeroes = Managers.Battle.GetAllAliveHeroes();

        if (aliveHeroes.Count > 0)
        {
            // 같은 슬롯의 영웅 우선
            var sameSlotHero = aliveHeroes.Find(h => h.SlotIndex == SlotIndex);
            if (sameSlotHero != null)
            {
                SetTarget(sameSlotHero);

            }
            else
            {
                // 첫 번째 살아있는 영웅
                SetTarget(aliveHeroes[0]);

            }
        }
    }

    // 보스 특수 공격 가능 여부
    private bool CanUseSpecialAttack()
    {
        return Time.time - _lastSpecialAttackTime >= _specialAttackCooldown;
    }

    // 보스 특수 공격
    private void PerformSpecialAttack()
    {
        _lastSpecialAttackTime = Time.time;
        _attackPatternIndex = (_attackPatternIndex + 1) % 3; // 3가지 패턴 순환

        switch (_attackPatternIndex)
        {
            case 0:
                PerformPowerAttack(); // 강력한 단일 공격
                break;
            case 1:
                PerformAreaAttack(); // 광역 공격
                break;
            case 2:
                PerformDebuffAttack(); // 디버프 공격
                break;
        }
        Debug.Log("boss used specialAttack");
    }

    // 강력한 단일 공격
    private void PerformPowerAttack()
    {
        if (!IsTargetValid())
            return;

        PlayTriggerAnimation("SpecialAttack");

        float damage = Attack * 2.5f; // 250% 데미지
        Creature targetCreature = Target as Creature;
        targetCreature?.TakeDamage(damage, this);

    }

    // 광역 공격
    private void PerformAreaAttack()
    {
        PlayTriggerAnimation("AreaAttack");

        var allHeroes = Managers.Battle.GetAllAliveHeroes();
        float damage = Attack * 0.8f; // 80% 데미지

        foreach (var hero in allHeroes)
        {
            hero.TakeDamage(damage, this);
        }

    }

    // 디버프 공격
    private void PerformDebuffAttack()
    {
        if (!IsTargetValid())
            return;

        PlayTriggerAnimation("DebuffAttack");

        float damage = Attack * 1.5f; // 150% 데미지
        Creature targetCreature = Target as Creature;

        if (targetCreature != null)
        {
            targetCreature.TakeDamage(damage, this);
            // 디버프 효과 (공격력 감소 등) - 추후 구현
            ApplyDebuffToTarget(targetCreature);
        }

    }

    // 디버프 적용
    private void ApplyDebuffToTarget(Creature target)
    {
        // 임시로 공격력 20% 감소 (5초간)
        StartCoroutine(ApplyTemporaryDebuff(target, 0.2f, 5f));
    }

    private IEnumerator ApplyTemporaryDebuff(Creature target, float debuffAmount, float duration)
    {
        float originalAttack = target.Attack;
        target.Attack *= (1 - debuffAmount);

        yield return new WaitForSeconds(duration);

        if (target != null && !target.IsDead)
        {
            target.Attack = originalAttack;
        }
    }
    #endregion

    #region Death and Rewards
    protected override void DropReward()
    {
        if (MonsterData == null)
            return;

        // 골드 보상 (난이도 스케일링 적용)
        int goldReward = Mathf.RoundToInt(MonsterData.goldReward * DifficultyMultiplier);

        // 스테이지 보너스
        var stageData = Managers.Data.StageDataDict.GetValueOrDefault(StageNumber);
        if (stageData != null)
        {
            goldReward = Mathf.RoundToInt(goldReward * stageData.goldMultiplier);
        }

        // 경험치 보상
        int expReward = Mathf.RoundToInt(MonsterData.experienceReward * DifficultyMultiplier);
        if (stageData != null)
        {
            expReward = Mathf.RoundToInt(expReward * stageData.experienceMultiplier);
        }

        // 보상 지급
        Managers.Game.AddCurrency(ECurrencyType.Gold, goldReward);

        // 아이템 드롭 체크
        CheckItemDrop();

        // 스킬큐브 드롭 체크
        CheckSkillCubeDrop();

        // 이벤트 발생
        OnMonsterKilled?.Invoke(goldReward, expReward);

    }

    // 아이템 드롭 체크
    private void CheckItemDrop()
    {
        if (Random.Range(0f, 1f) <= _itemDropChance)
        {
            DropRandomItem();
        }
    }

    // 랜덤 아이템 드롭
    private void DropRandomItem()
    {
        // 레어리티 결정
        var rarityWeights = Managers.Data.ConfigData?.gameConfig.treasureChest.rarityWeights;
        EItemRarity rarity = DetermineItemRarity(rarityWeights);

        // 해당 레어리티의 아이템 중 랜덤 선택
        var possibleItems = Managers.Data.EquipmentDic.Values
            .Where(item => item.itemRairity == rarity)
            .ToList();

        if (possibleItems.Count > 0)
        {
            var randomItem = possibleItems[Random.Range(0, possibleItems.Count)];


            // 임시로 아이템 생성
            var itemSaveData = new ItemSaveData
            {
                instanceId = Managers.Game.SaveData.ItemInstanceGenerator++,
                templateId = randomItem.baseId,
                count = 1,
                equipSlot = -1 // 인벤토리
            };

            Managers.Game.SaveData.Items.Add(itemSaveData);
        }
    }

    // 아이템 레어리티 결정
    private EItemRarity DetermineItemRarity(Data.RarityWeights weights)
    {
        if (weights == null)
        {
            return EItemRarity.Normal;
        }

        float random = Random.Range(0f, 1f);
        float cumulative = 0f;

        // Common
        cumulative += weights.Common;
        if (random <= cumulative) return EItemRarity.Normal;

        // Rare
        cumulative += weights.Rare;
        if (random <= cumulative) return EItemRarity.Rare;

        // Epic
        cumulative += weights.Epic;
        if (random <= cumulative) return EItemRarity.Unique;

        // Legendary
        return EItemRarity.Legend;
    }

    // 스킬큐브 드롭 체크
    private void CheckSkillCubeDrop()
    {
        if (Random.Range(0f, 1f) <= _skillCubeDropChance)
        {
            DropSkillCube();
        }
    }

    // 스킬큐브 드롭
    private void DropSkillCube()
    {
        // 랜덤 스킬 선택
        var allSkills = Managers.Data.SkillDataDict.Values.ToList();
        if (allSkills.Count > 0)
        {
            var randomSkill = allSkills[Random.Range(0, allSkills.Count)];

            // 스킬 인스턴스 생성 (임시)
            var skillSaveData = new SkillSaveData
            {
                instanceId = Managers.Game.SaveData.SkillInstanceGenerator++,
                skillId = randomSkill.skillId,
                level = 1
            };

            Managers.Game.SaveData.Skills.Add(skillSaveData);
        }
    }
    #endregion

    #region Override Methods
    protected override void PerformBasicAttack()
    {
        if (!IsTargetValid())
            return;

        _lastAttackTime = Time.time;

        // 데미지만 적용 (애니메이션 없음)
        float damage = CalculateDamage(Attack);
        Creature targetCreature = Target as Creature;
        if (targetCreature != null)
        {
            targetCreature.TakeDamage(damage, this);
        }
    }
    public override void OnDead(BaseObject attacker)
    {
        ClearAllDebuffs();

        base.OnDead(attacker);

        // 보스 처치 시 특별 이펙트나 연출
        if (IsBoss)
        {
            // 보스 처치 연출
        }

        if (Animator != null && Animator.HasState(0, Animator.StringToHash("Dead")))
        {
            // 죽음 애니메이션이 있으면 2초 대기
            StartCoroutine(RemoveAfterDelay(2f));
        }
        else
        {
            // 애니메이션이 없으면 페이드아웃 효과
            StartCoroutine(FadeOutAndRemove());
        }
    }
    protected override void OnDestroy()
    {
        // 디버프 정리
        if (_activeDebuffs != null && _activeDebuffs.Count > 0)
        {
            foreach (var debuffInfo in _activeDebuffs.Values)
            {
                if (debuffInfo?.coroutine != null)
                    StopCoroutine(debuffInfo.coroutine);
            }
            _activeDebuffs.Clear();
        }

        base.OnDestroy();
    }
    private IEnumerator FadeOutAndRemove()
    {
        // 스프라이트 페이드아웃 효과
        if (Renderer != null)
        {
            float fadeTime = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = Renderer.color;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
                Renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        // 오브젝트 제거
        Managers.Object.Despawn(this);
    }

    private IEnumerator RemoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Managers.Object.Despawn(this);
    }

    // 몬스터는 스테이지 전환 시 자동으로 제거됨
    public override void FullRestore()
    {
        // 몬스터는 회복하지 않음 (새로 생성됨)
    }
    #endregion

    #region Debuff Management

    public void ApplyDebuff(int skillId, EStat statType, float debuffValue, float duration, int skillLevel)
    {
        // Monster에 적용 가능한 스탯만 허용
        if (statType != EStat.Attack && statType != EStat.Defense)
        {
            return;
        }

        if (_activeDebuffs.ContainsKey(skillId))
        {
            var existingDebuff = _activeDebuffs[skillId];

            if (skillLevel > existingDebuff.skillLevel)
            {
                // 더 높은 레벨로 업그레이드
                RemoveDebuff(skillId);
                AddNewDebuff(skillId, skillLevel, statType, debuffValue, duration);
            }
            else if (skillLevel == existingDebuff.skillLevel)
            {
                // 같은 레벨이면 지속시간만 갱신
                RefreshDebuffDuration(skillId, duration);
            }
            else
            {
                // 더 낮은 레벨은 무시
            }
        }
        else
        {
            // 새로운 디버프 추가
            AddNewDebuff(skillId, skillLevel, statType, debuffValue, duration);
        }
    }

    // ==========================================
    // 4. 디버프 관리 Private 메서드들
    // ==========================================

    private void AddNewDebuff(int skillId, int skillLevel, EStat statType, float debuffValue, float duration)
    {
        var debuffInfo = new ActiveBuffInfo(skillId, skillLevel, statType, debuffValue, duration, ESkillTargetType.Single);
        _activeDebuffs[skillId] = debuffInfo;

        debuffInfo.coroutine = StartCoroutine(DebuffDurationCoroutine(skillId, debuffInfo));

        // 스탯 재계산
        RecalculateStats();
    }

    // ✨ 새로 추가: 스탯 재계산 메서드
    private void RecalculateStats()
    {
        // 디버프 보너스 계산
        float debuffAttackBonus = 0;
        float debuffDefenseBonus = 0;

        foreach (var debuffInfo in _activeDebuffs.Values)
        {
            switch (debuffInfo.statType)
            {
                case EStat.Attack:
                    debuffAttackBonus -= _baseAttack * debuffInfo.buffValue;  // 음수로 적용
                    break;
                case EStat.Defense:
                    debuffDefenseBonus -= _baseDefense * debuffInfo.buffValue;
                    break;
            }
        }

        // 최종 스탯 = 기본 스탯 + 디버프 (디버프는 음수)
        Attack = Mathf.Max(1, _baseAttack + debuffAttackBonus);     // 최소 1
        Defense = Mathf.Max(0, _baseDefense + debuffDefenseBonus);  // 최소 0

    }

    private IEnumerator DebuffDurationCoroutine(int skillId, ActiveBuffInfo debuffInfo)
    {
        float elapsed = 0f;

        while (elapsed < debuffInfo.duration)
        {
            elapsed += Time.deltaTime;
            debuffInfo.remainingTime = debuffInfo.duration - elapsed;
            yield return null;
        }

        // 디버프 만료

        _activeDebuffs.Remove(skillId);

        // 스탯 재계산 (디버프 제거 후)
        RecalculateStats();
    }

    private void RemoveDebuff(int skillId)
    {
        if (_activeDebuffs.TryGetValue(skillId, out var debuffInfo))
        {
            if (debuffInfo.coroutine != null)
                StopCoroutine(debuffInfo.coroutine);

            _activeDebuffs.Remove(skillId);

            // 스탯 재계산
            RecalculateStats();
        }
    }

    private void RefreshDebuffDuration(int skillId, float newDuration)
    {
        if (_activeDebuffs.TryGetValue(skillId, out var debuffInfo))
        {
            if (debuffInfo.coroutine != null)
                StopCoroutine(debuffInfo.coroutine);

            debuffInfo.duration = newDuration;
            debuffInfo.remainingTime = newDuration;
            debuffInfo.coroutine = StartCoroutine(DebuffDurationCoroutine(skillId, debuffInfo));
        }
    }

    // ==========================================
    // 5. 모든 디버프 제거 (Public)
    // ==========================================

    /// <summary>
    /// 모든 디버프를 제거 (몬스터 사망 시 또는 스테이지 전환 시 사용)
    /// </summary>
    public void ClearAllDebuffs()
    {
        if (_activeDebuffs.Count == 0)
            return;

        foreach (var debuffInfo in _activeDebuffs.Values)
        {
            if (debuffInfo.coroutine != null)
                StopCoroutine(debuffInfo.coroutine);
        }

        _activeDebuffs.Clear();

        // 스탯 복원
        Attack = _baseAttack;
        Defense = _baseDefense;

    }

    // ==========================================
    // 6. 현재 활성 디버프 목록 가져오기 (디버그/UI용)
    // ==========================================

    public List<ActiveBuffInfo> GetActiveDebuffs()
    {
        return new List<ActiveBuffInfo>(_activeDebuffs.Values);
    }

    #endregion
}