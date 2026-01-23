using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class BattleManager
{
    #region Battle Participant Classes
    // 전투 참가자 기본 클래스
    public abstract class BattleParticipant
    {
        public string Name { get; set; }
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int CurrentMp { get; set; }
        public int MaxMp { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public bool IsDead => CurrentHp <= 0;
        public bool IsDefending { get; set; }
        public ECombatantType CombatantType { get; set; }

        public virtual void TakeDamage(int damage)
        {
            // 방어 중이면 피해 감소
            if (IsDefending)
            {
                var config = Managers.Data.ConfigData?.battle;
                float reduction = config?.defendDamageReduction ?? 0.5f;
                damage = Mathf.RoundToInt(damage * (1f - reduction));
            }

            CurrentHp = Mathf.Max(CurrentHp - damage, 0);
        }

        public virtual void Heal(int amount)
        {
            int healed = Mathf.Min(amount, MaxHp - CurrentHp);
            CurrentHp += healed;
        }


        public virtual bool ConsumeMp(int amount)
        {
            if (CurrentMp < amount)
                return false;

            CurrentMp -= amount;
            return true;
        }
    }

    // 플레이어 전투 정보
    public class PlayerCombatant : BattleParticipant
    {
        public List<int> EquippedSkillIds { get; private set; }

        public PlayerCombatant()
        {
            CombatantType = ECombatantType.Player;
            Name = Managers.Player.PlayerName;
            CurrentHp = Managers.Player.CurrentHp;
            MaxHp = Managers.Player.MaxHp;
            CurrentMp = Managers.Player.CurrentMp;
            MaxMp = Managers.Player.MaxMp;
            Attack = Managers.Player.Attack;
            Defense = Managers.Player.Defense;
            Speed = Managers.Player.Speed;
            EquippedSkillIds = Managers.Player.GetEquippedSkills();
        }

        public void SyncToPlayer()
        {
            Managers.Player.CurrentHp = CurrentHp;
            Managers.Player.CurrentMp = CurrentMp;
        }
    }

    // 동료 전투 정보
    public class CompanionCombatant : BattleParticipant
    {
        public PartyManager.Companion CompanionData { get; private set; }
        public List<int> SkillIds { get; private set; }

        public CompanionCombatant(PartyManager.Companion companion)
        {
            CompanionData = companion;
            CombatantType = ECombatantType.Ally;

            Name = companion.CharacterData.characterName;
            CurrentHp = companion.CurrentHp;
            MaxHp = companion.MaxHp;
            CurrentMp = companion.CurrentMp;
            MaxMp = companion.MaxMp;
            Attack = companion.Attack;
            Defense = companion.Defense;
            Speed = companion.Speed;
            SkillIds = new List<int>(companion.SkillIds);
        }

        public void SyncToCompanion()
        {
            CompanionData.CurrentHp = CurrentHp;
            CompanionData.CurrentMp = CurrentMp;
        }
    }

    // 적 전투 정보
    public class EnemyCombatant : BattleParticipant
    {
        public Data.EnemyData EnemyData { get; private set; }
        public List<int> SkillIds { get; private set; }
        public EEnemyAI AIType { get; private set; }

        public EnemyCombatant(Data.EnemyData enemyData)
        {
            EnemyData = enemyData;
            CombatantType = ECombatantType.Enemy;

            Name = enemyData.enemyName;
            CurrentHp = enemyData.hp;
            MaxHp = enemyData.hp;
            CurrentMp = enemyData.mp;
            MaxMp = enemyData.mp;
            Attack = enemyData.attack;
            Defense = enemyData.defense;
            Speed = enemyData.speed;
            SkillIds = new List<int>(enemyData.skillIds);
            AIType = enemyData.aiType;
        }
    }

    #endregion

    #region Battle Action Class

    // 전투 행동
    public class BattleAction
    {
        public BattleParticipant Actor { get; set; }
        public EBattleActionType ActionType { get; set; }
        public BattleParticipant Target { get; set; }
        public int SkillId { get; set; }
        public string Description { get; set; }
    }

    #endregion

    #region Properties

    private EBattleState _battleState = EBattleState.None;
    public EBattleState BattleState
    {
        get => _battleState;
        private set
        {
            if (_battleState != value)
            {
                EBattleState prev = _battleState;
                _battleState = value;
                OnBattleStateChanged?.Invoke(prev, value);
            }
        }
    }

    // 전투 참가자
    private PlayerCombatant _player;
    private List<CompanionCombatant> _companions = new List<CompanionCombatant>();
    private List<EnemyCombatant> _enemies = new List<EnemyCombatant>();

    // 턴 순서
    private List<BattleParticipant> _turnOrder = new List<BattleParticipant>();
    private int _currentTurnIndex = 0;
    private int _turnCount = 1;

    // 전투 결과
    private List<Data.RewardData> _battleRewards = new List<Data.RewardData>();

    // 전투 로그
    private List<string> _battleLog = new List<string>();

    // Properties
    public PlayerCombatant Player => _player;
    public List<CompanionCombatant> Companions => _companions;
    public List<EnemyCombatant> Enemies => _enemies;
    public BattleParticipant CurrentTurnParticipant => _turnOrder.ElementAtOrDefault(_currentTurnIndex);
    public int TurnCount => _turnCount;
    public List<string> BattleLog => _battleLog;

    // 전투 속도 설정
    public float BattleSpeed { get; set; } = 1.0f;  // 1.0 = 보통, 2.0 = 2배속, 0.5 = 느리게

    #endregion

    #region Events
    public event Action<EBattleState, EBattleState> OnBattleStateChanged;
    public event Action OnBattleStarted;
    public event Action<BattleParticipant> OnTurnStart;
    public event Action<BattleAction> OnActionExecuted;
    public event Action<BattleParticipant, int> OnDamageDealt;
    public event Action<BattleParticipant, int> OnHealingDone;
    public event Action<BattleParticipant> OnParticipantDied;
    public event Action<bool, List<Data.RewardData>> OnBattleEnded;  // victory, rewards
    public event Action<string> OnBattleLogAdded;
    #endregion

    #region Initialization

    public void Init()
    {
        Debug.Log("BattleManager Initialized (Auto-Battle Mode)");
    }

    #endregion

    #region Battle Start
    /// 전투 시작 (자동 진행)
    public void StartBattle(int enemyId)
    {
        Debug.Log($"========== Battle Start: Enemy {enemyId} ==========");

        // 전투 로그 초기화
        _battleLog.Clear();
        AddLog("=== 전투 시작 ===");

        // 전투 참가자 초기화
        InitializeBattleParticipants(enemyId);

        // 턴 순서 결정
        DetermineTurnOrder();

        // 전투 시작
        BattleState = EBattleState.Start;
        _turnCount = 1;
        _currentTurnIndex = 0;

        OnBattleStarted?.Invoke();

        // 자동으로 전투 시작 (코루틴이 필요하므로 외부에서 호출)
        // StartAutoBattle()는 MonoBehaviour에서 코루틴으로 실행
    }

    // 전투 참가자 초기화
    private void InitializeBattleParticipants(int enemyId)
    {
        // 플레이어
        _player = new PlayerCombatant();
        AddLog($"{_player.Name} 참전!");

        // 동료들
        _companions.Clear();
        foreach (var companion in Managers.Party.AliveCompanions)
        {
            var combatant = new CompanionCombatant(companion);
            _companions.Add(combatant);
            AddLog($"{combatant.Name} 참전!");
        }

        // 적 생성
        _enemies.Clear();
        var enemyData = Managers.Data.EnemyDict.GetValueOrDefault(enemyId);
        if (enemyData != null)
        {
            var enemy = new EnemyCombatant(enemyData);
            _enemies.Add(enemy);
            AddLog($"{enemy.Name} 등장!");
        }
        else
        {
            Debug.LogError($"Enemy data not found: {enemyId}");
        }

        // 보상 초기화
        _battleRewards.Clear();
        if (enemyData != null)
        {
            _battleRewards.Add(new Data.RewardData
            {
                rewardType = ERewardType.Exp,
                amount = enemyData.expReward
            });
            _battleRewards.Add(new Data.RewardData
            {
                rewardType = ERewardType.Gold,
                amount = enemyData.goldReward
            });
        }
    }

    // 턴 순서 결정 (속도 기반)
    private void DetermineTurnOrder()
    {
        _turnOrder.Clear();

        // 모든 참가자 추가
        _turnOrder.Add(_player);
        _turnOrder.AddRange(_companions);
        _turnOrder.AddRange(_enemies);

        // 속도 순으로 정렬 (내림차순)
        _turnOrder = _turnOrder.OrderByDescending(p => p.Speed).ToList();

        AddLog("--- 턴 순서 ---");
        for (int i = 0; i < _turnOrder.Count; i++)
        {
            AddLog($"{i + 1}. {_turnOrder[i].Name} (속도: {_turnOrder[i].Speed})");
        }
    }

    #endregion

    #region Auto Battle Control

    // 자동 전투 진행 (외부에서 코루틴으로 호출)
    public IEnumerator AutoBattleCoroutine()
    {
        while (BattleState != EBattleState.Victory && BattleState != EBattleState.Defeat)
        {
            // 전투 종료 체크
            if (CheckBattleEnd())
                break;

            // 다음 턴 실행
            ExecuteNextTurn();

            // 전투 속도에 따른 대기
            yield return new WaitForSeconds(0.5f / BattleSpeed);

            // 다시 한 번 승패 확인
            if (CheckBattleEnd())
                break;
        }

        Debug.Log("Battle coroutine ended");
    }

    // 다음 턴 실행 (자동)
    private void ExecuteNextTurn()
    {
        // CRITICAL: 전투 종료 체크를 가장 먼저!
        if (CheckBattleEnd())
            return;

        // 살아있는 참가자가 있는지 확인
        var aliveParticipants = _turnOrder.Where(p => !p.IsDead).ToList();
        if (aliveParticipants.Count == 0)
        {
            Debug.LogError("No alive participants! This shouldn't happen.");
            return;
        }

        // 죽은 참가자 건너뛰기
        while (_currentTurnIndex < _turnOrder.Count && _turnOrder[_currentTurnIndex].IsDead)
        {
            _currentTurnIndex++;
        }

        // 모든 턴 종료 시 다음 라운드
        if (_currentTurnIndex >= _turnOrder.Count)
        {
            _currentTurnIndex = 0;
            _turnCount++;
            AddLog($"\n===== Turn {_turnCount} =====");

            // 재귀 호출 제거 - 코루틴이 다음 프레임에 처리
            return;
        }

        var participant = CurrentTurnParticipant;
        if (participant == null)
        {
            Debug.LogWarning("Current participant is null!");
            _currentTurnIndex++;
            return;
        }

        if (participant.IsDead)
        {
            Debug.LogWarning($"{participant.Name} is dead but still in turn order!");
            _currentTurnIndex++;
            return;
        }

        // 방어 상태 해제
        participant.IsDefending = false;

        // 턴 시작
        OnTurnStart?.Invoke(participant);

        // AI 행동 결정 및 실행
        var action = DecideAction(participant);
        if (action != null)
        {
            ExecuteAction(action);
        }
        else
        {
            AddLog($"{participant.Name}은(는) 행동하지 못했습니다.");
        }

        _currentTurnIndex++;
    }

    #endregion

    #region AI Decision (모든 참가자 공통)

    // 행동 결정 (플레이어, 동료, 적 모두 동일한 로직)
    private BattleAction DecideAction(BattleParticipant participant)
    {
        List<int> skillIds = new List<int>();

        // 스킬 리스트 가져오기
        if (participant is PlayerCombatant player)
        {
            skillIds = player.EquippedSkillIds;
        }
        else if (participant is CompanionCombatant companion)
        {
            skillIds = companion.SkillIds;
        }
        else if (participant is EnemyCombatant enemy)
        {
            skillIds = enemy.SkillIds;
        }

        // 1순위: HP 30% 이하면 회복 스킬 사용
        if (participant.CurrentHp < participant.MaxHp * 0.3f)
        {
            var healSkill = GetUsableSkill(participant, skillIds, ESkillType.Heal);
            if (healSkill != null)
            {
                return new BattleAction
                {
                    Actor = participant,
                    ActionType = EBattleActionType.Skill,
                    Target = participant,  // 자신에게 힐
                    SkillId = healSkill.skillId
                };
            }
        }

        // 2순위: 사용 가능한 공격 스킬
        var attackSkill = GetUsableSkill(participant, skillIds, ESkillType.Attack);
        if (attackSkill != null)
        {
            var target = SelectTarget(participant);
            return new BattleAction
            {
                Actor = participant,
                ActionType = EBattleActionType.Skill,
                Target = target,
                SkillId = attackSkill.skillId
            };
        }

        // 3순위: 버프 스킬
        var buffSkill = GetUsableSkill(participant, skillIds, ESkillType.Buff);
        if (buffSkill != null)
        {
            return new BattleAction
            {
                Actor = participant,
                ActionType = EBattleActionType.Skill,
                Target = participant,
                SkillId = buffSkill.skillId
            };
        }

        // 4순위: 기본 공격
        return new BattleAction
        {
            Actor = participant,
            ActionType = EBattleActionType.Attack,
            Target = SelectTarget(participant)
        };
    }

    private Data.SkillData GetUsableSkill(BattleParticipant participant, List<int> skillIds, ESkillType skillType)
    {
        if (skillIds == null || skillIds.Count == 0)
            return null;

        foreach (int skillId in skillIds)
        {
            var skillData = Managers.Data.SkillDict.GetValueOrDefault(skillId);
            if (skillData != null &&
                skillData.skillType == skillType &&
                participant.CurrentMp >= skillData.mpCost)
            {
                return skillData;
            }
        }
        return null;
    }

    private BattleParticipant SelectTarget(BattleParticipant actor)
    {
        // 아군이면 적 선택
        if (actor.CombatantType == ECombatantType.Player || actor.CombatantType == ECombatantType.Ally)
        {
            // 가장 HP가 낮은 적 선택
            return _enemies.Where(e => !e.IsDead).OrderBy(e => e.CurrentHp).FirstOrDefault();
        }
        // 적이면 아군 선택
        else
        {
            var allies = GetAllAllies();

            // AI 타입에 따라 다른 타겟 선택
            if (actor is EnemyCombatant enemy)
            {
                switch (enemy.AIType)
                {
                    case EEnemyAI.Aggressive:
                        // 가장 HP 낮은 아군
                        return allies.OrderBy(a => a.CurrentHp).FirstOrDefault();

                    case EEnemyAI.Defensive:
                        // 항상 플레이어 공격
                        return _player;

                    case EEnemyAI.Balanced:
                    default:
                        // 랜덤
                        return allies[UnityEngine.Random.Range(0, allies.Count)];
                }
            }

            // 기본: 플레이어
            return _player;
        }
    }

    #endregion

    #region Action Execution

    private void ExecuteAction(BattleAction action)
    {
        if (action == null || action.Actor == null || action.Target == null)
            return;

        switch (action.ActionType)
        {
            case EBattleActionType.Attack:
                ExecuteAttack(action.Actor, action.Target);
                break;

            case EBattleActionType.Skill:
                ExecuteSkill(action.Actor, action.Target, action.SkillId);
                break;
        }

        OnActionExecuted?.Invoke(action);
    }

    private void ExecuteAttack(BattleParticipant attacker, BattleParticipant target)
    {
        if (target == null || target.IsDead)
            return;

        int damage = CalculateDamage(attacker.Attack, target.Defense);

        // 크리티컬 판정
        bool isCritical = RollCritical();
        if (isCritical)
        {
            var config = Managers.Data.ConfigData?.battle;
            float critMultiplier = config?.criticalDamageMultiplier ?? 1.5f;
            damage = Mathf.RoundToInt(damage * critMultiplier);
        }

        target.TakeDamage(damage);
        OnDamageDealt?.Invoke(target, damage);

        // 로그 추가
        string critText = isCritical ? " [크리티컬!]" : "";
        AddLog($"{attacker.Name}의 공격! → {target.Name}에게 {damage} 데미지{critText}");

        // 사망 처리
        if (target.IsDead)
        {
            HandleParticipantDeath(target);
        }
    }

    private void ExecuteSkill(BattleParticipant caster, BattleParticipant target, int skillId)
    {
        var skillData = Managers.Data.SkillDict.GetValueOrDefault(skillId);
        if (skillData == null)
            return;

        // MP 소모
        if (!caster.ConsumeMp(skillData.mpCost))
        {
            AddLog($"{caster.Name}의 MP가 부족합니다!");
            return;
        }

        AddLog($"{caster.Name}이(가) {skillData.skillName} 사용!");

        // 스킬 타입별 처리
        switch (skillData.skillType)
        {
            case ESkillType.Attack:
                ExecuteAttackSkill(caster, target, skillData);
                break;

            case ESkillType.Heal:
                ExecuteHealSkill(caster, target, skillData);
                break;

            case ESkillType.Buff:
                AddLog($"{target.Name}의 능력이 상승했다! (버프 시스템 미구현)");
                break;

            case ESkillType.Debuff:
                AddLog($"{target.Name}의 능력이 하락했다! (디버프 시스템 미구현)");
                break;
        }
    }


    private void ExecuteAttackSkill(BattleParticipant caster, BattleParticipant target, Data.SkillData skillData)
    {
        if (target == null || target.IsDead)
            return;

        // 스킬 데미지 계산
        int baseDamage = skillData.power;

        // 스탯 스케일링
        if (skillData.scalingStat != EStatType.None)
        {
            int statValue = GetStatValue(caster, skillData.scalingStat);
            baseDamage += Mathf.RoundToInt(statValue * skillData.scalingRatio);
        }

        int damage = CalculateDamage(baseDamage, target.Defense);

        target.TakeDamage(damage);
        OnDamageDealt?.Invoke(target, damage);

        AddLog($"→ {target.Name}에게 {damage} 데미지!");

        if (target.IsDead)
        {
            HandleParticipantDeath(target);
        }
    }

 
    private void ExecuteHealSkill(BattleParticipant caster, BattleParticipant target, Data.SkillData skillData)
    {
        if (target == null || target.IsDead)
            return;

        int healAmount = skillData.power;

        // 스탯 스케일링
        if (skillData.scalingStat != EStatType.None)
        {
            int statValue = GetStatValue(caster, skillData.scalingStat);
            healAmount += Mathf.RoundToInt(statValue * skillData.scalingRatio);
        }

        int beforeHp = target.CurrentHp;
        target.Heal(healAmount);
        int actualHeal = target.CurrentHp - beforeHp;

        OnHealingDone?.Invoke(target, actualHeal);
        AddLog($"→ {target.Name}의 HP {actualHeal} 회복! ({target.CurrentHp}/{target.MaxHp})");
    }

    #endregion

    #region Damage Calculation


    private int CalculateDamage(int attack, int defense)
    {
        var config = Managers.Data.ConfigData?.battle;
        int minDamage = config?.minDamage ?? 1;

        int damage = attack - Mathf.RoundToInt(defense * 0.5f);
        return Mathf.Max(damage, minDamage);
    }

    private bool RollCritical()
    {
        var config = Managers.Data.ConfigData?.battle;
        float critChance = config?.baseCriticalChance ?? 0.1f;
        return UnityEngine.Random.value < critChance;
    }


    private int GetStatValue(BattleParticipant participant, EStatType statType)
    {
        // 플레이어는 PlayerManager에서
        if (participant is PlayerCombatant)
        {
            return Managers.Player.GetStat(statType);
        }

        // 나머지는 기본 스탯
        return statType switch
        {
            EStatType.Strength => participant.Attack,
            EStatType.Intelligence => participant.Attack,
            EStatType.Agility => participant.Speed,
            _ => 0
        };
    }

    #endregion

    #region Death & Battle End


    private void HandleParticipantDeath(BattleParticipant participant)
    {
        AddLog($"{participant.Name}이(가) 쓰러졌다!");
        OnParticipantDied?.Invoke(participant);
    }

    private bool CheckBattleEnd()
    {
        bool allEnemiesDead = _enemies.All(e => e.IsDead);
        bool playerDead = _player.IsDead;

        if (allEnemiesDead)
        {
            EndBattle(true);
            return true;
        }

        if (playerDead)
        {
            EndBattle(false);
            return true;
        }

        return false;
    }


    private void EndBattle(bool victory)
    {
        BattleState = victory ? EBattleState.Victory : EBattleState.Defeat;

        // 플레이어/동료 상태 동기화
        _player?.SyncToPlayer();
        foreach (var companion in _companions)
        {
            companion?.SyncToCompanion();
        }

        if (victory)
        {
            AddLog("\n=== 승리! ===");

            // 보상 로그
            foreach (var reward in _battleRewards)
            {
                if (reward.rewardType == ERewardType.Exp)
                    AddLog($"경험치 +{reward.amount}");
                else if (reward.rewardType == ERewardType.Gold)
                    AddLog($"골드 +{reward.amount}");
            }

            OnBattleEnded?.Invoke(true, _battleRewards);

            // StoryManager 콜백은 StoryScene이 준비된 후 호출하도록 변경
            // Managers.Story.OnBattleEnd(true, _battleRewards); ← 제거!
        }
        else
        {
            AddLog("\n=== 패배... ===");
            OnBattleEnded?.Invoke(false, null);

            // 패배도 마찬가지
            // Managers.Story.OnBattleEnd(false, null); ← 제거!
        }
    }

    #endregion

    #region Battle Log

    private void AddLog(string message)
    {
        _battleLog.Add(message);
        OnBattleLogAdded?.Invoke(message);
        Debug.Log($"[Battle] {message}");
    }

    public string GetFullBattleLog()
    {
        return string.Join("\n", _battleLog);
    }

    #endregion

    #region Helper Methods


    private List<BattleParticipant> GetAllAllies()
    {
        var allies = new List<BattleParticipant> { _player };
        allies.AddRange(_companions.Where(c => !c.IsDead));
        return allies;
    }

    #endregion

    #region Battle Speed Control

    // 전투 속도 변경
    public void SetBattleSpeed(float speed)
    {
        BattleSpeed = Mathf.Clamp(speed, 0.5f, 3.0f);
        Debug.Log($"Battle speed set to {BattleSpeed}x");
    }

    // 전투 스킵 (즉시 결과만)
    public void SkipBattle()
    {
        // TODO: 즉시 전투 결과 계산하여 승패 결정
        AddLog("전투를 빠르게 진행합니다...");

        // 간단한 승패 판정 (총 스탯 비교)
        int allyPower = _player.Attack + _player.Defense;
        foreach (var companion in _companions)
        {
            allyPower += companion.Attack + companion.Defense;
        }

        int enemyPower = 0;
        foreach (var enemy in _enemies)
        {
            enemyPower += enemy.Attack + enemy.Defense;
        }

        bool victory = allyPower > enemyPower;
        EndBattle(victory);
    }

    #endregion
}
