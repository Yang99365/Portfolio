using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Data;
using static Define;

public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; }

    // 데이터
    public Data.HeroData HeroData { get; private set; }
    public Data.MonsterData MonsterData { get; private set; }

    // 배치 슬롯 정보 추가
    public int SlotIndex { get; set; } = -1; // -1은 배치되지 않은 상태

    private Coroutine _aiCoroutine;
    #region Stats
    // 통합 스탯 시스템으로 개선
    protected float _hp;
    public float Hp
    {
        get => _hp;
        set
        {
            _hp = Mathf.Clamp(value, 0, MaxHp);
            OnHpChanged?.Invoke(_hp, MaxHp);
        }
    }

    public float MaxHp { get; set; }
    public float Attack { get; set; }
    public float AttackSpeed { get; set; } = 1.0f; // 공격 속도 (초당 공격 횟수)
    public float Defense { get; set; }
    public float CriticalChance { get; set; }
    public float CriticalDamage { get; set; }

    // 이벤트 추가
    public event Action<float, float> OnHpChanged; // current, max
    public event Action<Creature> OnDeath;
    public event Action<float> OnDealDamage; // 데미지 딜링 이벤트
    public event Action<float> OnTakeDamage; // 데미지 받기 이벤트
    #endregion

    #region State
    protected EObjectState _state = EObjectState.None;
    public virtual EObjectState State
    {
        get { return _state; }
        set
        {
            if (_state == value)
                return;

            _state = value;
            UpdateAnimation();
        }
    }

    public bool IsDead => State == EObjectState.Dead;
    public bool CanAttack => State == EObjectState.Idle && !IsDead;
    #endregion

    #region Attack System
    protected float _lastAttackTime = 0f;
    protected float _attackCooldown => 1f / AttackSpeed; // 공격 간격 계산

    // 기본 공격 가능 여부 체크
    public bool IsAttackReady()
    {
        return Time.time - _lastAttackTime >= _attackCooldown;
    }

    // 타겟 설정 (Idle RPG는 자동 타겟팅)
    public virtual void SetTarget(BaseObject target)
    {
        Target = target;
    }

    // 타겟 유효성 체크
    public bool IsTargetValid()
    {
        if (Target == null)
            return false;

        Creature targetCreature = Target as Creature;
        if (targetCreature == null)
            return false;

        return !targetCreature.IsDead;
    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public virtual void SetInfo(int templateID, bool isHero)
    {
        DataTemplateID = templateID;

        if (isHero)
        {
            ObjectType = EGameObjectType.Hero;

            // HeroData 로드 및 검증
            if (!Managers.Data.HeroDataDict.TryGetValue(templateID, out HeroData heroData))
            {
                Debug.LogError($"HeroData not found for ID: {templateID}");
                return;
            }
            HeroData = heroData;

            // Hero 스탯 설정
            MaxHp = HeroData.stats.maxHealth;
            Hp = MaxHp;
            Attack = HeroData.stats.attack;
            AttackSpeed = HeroData.stats.attackSpeed;
            Defense = HeroData.stats.defense;
            CriticalChance = HeroData.stats.criticalChance;
            CriticalDamage = HeroData.stats.criticalDamage;
        }
        else
        {
            ObjectType = EGameObjectType.Monster;

            // MonsterData 로드 및 검증
            if (!Managers.Data.MonsterDataDict.TryGetValue(templateID, out MonsterData monsterData))
            {
                Debug.LogError($"MonsterData not found for ID: {templateID}");
                return;
            }
            MonsterData = monsterData;

            // Monster 스탯 설정
            MaxHp = MonsterData.health;
            Hp = MaxHp;
            Attack = MonsterData.attackPower;
            AttackSpeed = 1.0f; // 몬스터 기본 공격속도
            Defense = MonsterData.defense;
            CriticalChance = 0.05f; // 몬스터 기본 크리티컬
            CriticalDamage = 1.5f; // 몬스터 기본 크리티컬 데미지
        }

        gameObject.name = $"{ObjectType}_{templateID}";

        SetRenderer(templateID, isHero, false);

        // State 초기화
        State = EObjectState.Idle;

        // AI 시작
        //StartAI();
    }

    #region AI System for Idle RPG
    // AI 업데이트 간격 (최적화를 위해 매 프레임이 아닌 일정 간격으로)
    public float UpdateAITick { get; protected set; } = 0.1f;

    public void StartAI()
    {
        // 기존 AI Coroutine이 있다면 중지
        StopAI();

        // 새로운 AI Coroutine 시작
        if (!IsDead)
        {
            _aiCoroutine = StartCoroutine(CoUpdateAI());
            Debug.Log($"{gameObject.name} AI Started");
        }
    }
    // AI 중지 메서드
    public void StopAI()
    {
        if (_aiCoroutine != null)
        {
            StopCoroutine(_aiCoroutine);
            _aiCoroutine = null;
            Debug.Log($"{gameObject.name} AI Stopped");
        }
    }
    protected IEnumerator CoUpdateAI()
    {
        while (!IsDead)
        {
            //if (Managers.Battle != null && Managers.Battle.BattleState == EBattleState.Battle)
            //{
                
            //}
            switch (State)
            {
                case EObjectState.Idle:
                    UpdateIdle();
                    break;
                case EObjectState.Skill:
                    UpdateSkill();
                    break;
                case EObjectState.Dead:
                    yield break; // 죽으면 AI 중지
            }

            yield return new WaitForSeconds(UpdateAITick);
        }

        // Coroutine 종료 시 참조 제거
        _aiCoroutine = null;
    }

    protected virtual void UpdateIdle()
    {
        // 타겟 유효성 체크
        if (!IsTargetValid())
        {
            FindNewTarget();
        }

        // 타겟이 유효하고 공격 가능하면 공격
        if (IsTargetValid() && IsAttackReady())
        {
            PerformBasicAttack();
        }
    }
    protected virtual void FindNewTarget()
    {
        // Hero와 Monster에서 각각 override
    }

    protected virtual void UpdateSkill()
    {
        // 스킬 사용 중 상태 (Hero에서 구현)
    }

    // 기본 공격 수행
    protected virtual void PerformBasicAttack()
    {
        if (!IsTargetValid())
            return;

        _lastAttackTime = Time.time;

        // 애니메이션 트리거
        PlayTriggerAnimation("Attack");

        //OnAttackHit() 애니메이션 이벤트로 이동
        //// 데미지 계산
        //float damage = CalculateDamage(Attack);

        //// 타겟에게 데미지 적용
        //Creature targetCreature = Target as Creature;
        //if (targetCreature != null)
        //{
        //    targetCreature.TakeDamage(damage, this);
        //    OnDealDamage?.Invoke(damage);
        //}
    }
    public void OnAttackHit()
    {
        if (!IsTargetValid())
            return;

        float damage = CalculateDamage(Attack);
        Creature targetCreature = Target as Creature;
        if (targetCreature != null)
        {
            targetCreature.TakeDamage(damage, this);
            OnDealDamage?.Invoke(damage);
        }
    }
    // 데미지 계산 (크리티컬 포함)
    protected float CalculateDamage(float baseDamage)
    {
        float finalDamage = baseDamage;

        // 크리티컬 판정
        if (UnityEngine.Random.Range(0f, 1f) < CriticalChance)
        {
            finalDamage *= CriticalDamage;

            // 크리티컬 이펙트나 UI 표시를 위한 처리 가능
            Debug.Log($"{gameObject.name} Critical Hit!");
        }

        return finalDamage;
    }

    // 데미지 받기
    public virtual void TakeDamage(float damage, Creature attacker)
    {
        if (IsDead)
            return;

        float finalDamage = Mathf.Max(1, damage - Defense);

        Hp -= finalDamage;
        OnTakeDamage?.Invoke(finalDamage);
        if (Hp > 0)
        {
            PlayTriggerAnimation("Damaged");
            OnDamaged(attacker);

            // 타겟이 없으면 공격자를 타겟으로 설정
            if (Target == null)
            {
                SetTarget(attacker);
            }
        }
        else
        {
            Hp = 0;
            State = EObjectState.Dead;
            StopAI(); // AI 중지
            OnDead(attacker);
        }
    }

    // 회복
    public virtual void Heal(float amount)
    {
        if (IsDead)
            return;

        Hp += amount;
    }

    // 완전 회복 - AI 재시작 포함
    public virtual void FullRestore()
    {
        Hp = MaxHp;
        State = EObjectState.Idle;
        _lastAttackTime = 0f;
        Target = null; // 타겟 초기화

        //Animator를 Idle State로 강제 리셋
        if (Animator != null)
        {
            Animator.ResetTrigger("Attack");
            Animator.ResetTrigger("Death");
            Animator.Play("Hero_Idle", 0, 0f);
        }

        // AI 재시작
        //StartAI();

        Debug.Log($"{gameObject.name} fully restored with AI");
    }
    #endregion

    #region Animation
    protected override void UpdateAnimation()
    {
        switch (State)
        {
            case EObjectState.Idle:
                PlayBoolAnimation("Move", false);
                PlayBoolAnimation("Idle", true);
                break;
            case EObjectState.Skill:
                PlayBoolAnimation("Move", false);
                // 스킬 애니메이션은 각 스킬에서 처리
                break;
            case EObjectState.Dead:
                PlayBoolAnimation("Move", false);
                PlayTriggerAnimation("Death");
                PlayBoolAnimation("isDead", true);
                break;
        }
    }
    #endregion

    #region Battle Events
    public override void OnDamaged(BaseObject attacker)
    {
        base.OnDamaged(attacker);
        // 추가 피격 처리
    }

    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);

        // AI 중지
        StopAllCoroutines();

        // 죽음 이벤트 발생
        OnDeath?.Invoke(this);

        // 일정 시간 후 제거 (또는 다음 스테이지를 위해 유지)
        if (ObjectType == EGameObjectType.Monster)
        {
            // 몬스터는 죽으면 보상 드롭
            DropReward();
        }
    }

    // 보상 드롭 (몬스터 전용)
    protected virtual void DropReward()
    {
        if (MonsterData == null)
            return;

        // GameManager에 보상 전달
        // 경험치나 아이템 드롭 처리도 여기서

    }
    #endregion

    protected virtual void OnDestroy()
    {
        StopAI();

        // 이벤트 정리
        OnHpChanged = null;
        OnDeath = null;
        OnDealDamage = null;
        OnTakeDamage = null;
    }
}