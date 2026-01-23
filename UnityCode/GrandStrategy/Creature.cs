using Pathfinding;
using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
public class Creature : BaseObject
{
    public BaseObject Target { get; protected set; }
    //public SkillComponent Skills { get; protected set; }

    public Data.GeneralData GeneralData { get; private set; }

    //public EffectComponent Effects { get; set; }
   
    public bool isMoving = false;
    public ETeamType Team { get; set; } = ETeamType.None;
    public AIPath ai;
    public RVOController rvo;
    public float AttackAnimationLength = 0.3f;
    protected float DistToTargetSqr
    {
        get
        {
            Vector3 dir = (Target.transform.position - transform.position);
            float distToTarget = dir.sqrMagnitude;
            return distToTarget;
        }
    }

    #region Stats
    public float Hp { get; set; }
    public float MaxHp;
    public float Attack;
    public float Defence;
    public float Intelligence;
    public float Speed;
    // for General
    public int TroopCount;

    #endregion

    protected ECreatureState _state = ECreatureState.None;
    public virtual ECreatureState State
    {
        get { return _state; }
        set
        {
            _state = value;
            UpdateAnimation();
        }
    }

    // 병사유닛도 따로 Data만들어서 햇어야했는데 그냥 소환한 장군따라가기로
    protected float AttackDistance
    {
        get
        {
            float env = 2.2f;
            if (Target != null && Target.ObjectType == EObjectType.Env)
                return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);

            float baseValue = GeneralData.stats.attackRange;
            return baseValue;
        }
        set
        {
            GeneralData.stats.attackRange = value;
        }
    }
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        
        return true;
    }

    public virtual void SetInfo(int templateID, bool isGeneral)
    {
        DataTemplateID = templateID;

        Data.GeneralData tempData;
        if (!Managers.Data.GeneralDic.TryGetValue(templateID, out tempData))
        {
            Debug.LogError($"Failed to find GeneralData for ID: {templateID}");
            return;
        }
        GeneralData = tempData;  // 프로퍼티에 할당

        if (isGeneral)
        {
            Team = Managers.Battle.myGenerals.Any(x => x.GeneralID == templateID) ?
                   ETeamType.My : ETeamType.Enemy;
            if (ObjectType != EObjectType.General)
            {
                Debug.LogError($"Invalid ObjectType for General: {ObjectType}");
                return;
            }
        }
        else
        {
            if (ObjectType != EObjectType.Army)
            {
                Debug.LogError($"Invalid ObjectType for Army: {ObjectType}");
                return;
            }
        }

        if (!isGeneral)
            AttackDistance = AttackDistance * 0.8f;


        //Collider
        Collider.offset = new Vector2(0, 0.4f);
        if(isGeneral)
            Collider.radius = 0.4f;
        else
            Collider.radius = 0.3f;

        // RigidBody
        //RigidBody.mass = 0;

        // Skills 병사마다 스킬 주려했으나.. 미리 설계안해서 ㅈㅈ
        if (isGeneral)
        {
            //Skills = gameObject.GetOrAddComponent<SkillComponent>();
            //Skills.SetInfo(this.GeneralData);
        }

        
        TroopCount = GeneralData.troopCount;
        Hp = TroopCount;
        MaxHp = TroopCount;
        Attack = GeneralData.stats.attack;
        Defence = GeneralData.stats.defense;
        Intelligence = GeneralData.stats.intelligence;
        Speed = GeneralData.stats.speed;

        // State
        State = ECreatureState.Idle;

        // Effect
        //Effects = gameObject.AddComponent<EffectComponent>();
        //Effects.SetInfo(this);
    }

    protected override void UpdateAnimation()
    {
        switch (State)
        {
            case ECreatureState.Idle:
                PlayBoolAnimation("Move", false);
                break;
            case ECreatureState.Move:
                PlayBoolAnimation("Move", true);
                break;
            //case ECreatureState.OnDamaged:
            //    PlayTriggerAnimation("Damaged");
            //    break;
            case ECreatureState.Skill:
                PlayBoolAnimation("Move", false); //임시
                //PlayTriggerAnimation("Attack");
                break;
            case ECreatureState.Dead:
                PlayBoolAnimation("Move", false);
                PlayTriggerAnimation("Dead");
                PlayBoolAnimation("isDead", true);
                break;
            default:
                break;
        }
    }
    public bool IsEnemy(Creature other)
    {
        if (other == null) return false;
        return Team != other.Team;
    }

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (State)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case ECreatureState.Move:
                    UpdateMove();
                    break;
                //case ECreatureState.OnDamaged
                //
                //    break;
                case ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case ECreatureState.Dead:
                    UpdateDead();
                    break;
                default:
                    break;
            }
            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            else
                yield return null;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    //protected virtual void UpdateAttack() { }
    protected virtual void UpdateSkill()
    {
        if (_coWait != null)
            return;
    }
    
    
    protected virtual void UpdateDead() { }

    protected Coroutine _coWait;

    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion

    protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject, bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude;

            // 서치 범위보다 멀리 있으면 스킵.
            if (distToTargetSqr > searchDistanceSqr)
                continue;

            // 이미 더 좋은 후보를 찾았으면 스킵.
            if (distToTargetSqr > bestDistanceSqr)
                continue;

            // 추가 조건
            if (func != null && func.Invoke(obj) == false)
                continue;

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
    {

        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = attackRange * attackRange;

        if (distToTargetSqr <= attackDistanceSqr)
        {
            //범위안에 들어오면 즉시 이동을 멈추고 공격.
            ai.destination = transform.position;
            // 공격 범위 이내로 들어왔다면 공격.
            State = ECreatureState.Skill;
            //skill.DoSkill();
            return;
        }
        else
        {
            // 공격 범위 밖이라면 추적.
            ai.destination = Target.transform.position;
            ai.SearchPath();
            State = ECreatureState.Move;


            // 너무 멀어지면 포기.
            float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                Target = null;
                State = ECreatureState.Idle;
            }
            return;
        }
    }

    #region battle
    public override void OnDamaged(BaseObject attacker)
    {
        base.OnDamaged(attacker);

        if (attacker.IsValid() == false)
            return;

        Creature creature = attacker as Creature;
        if (creature == null)
            return;


        float damage = creature.Attack - Defence;
        Hp = Mathf.Clamp(Hp - damage, 0, MaxHp);

        //데미지 폰트 출력

        if (Hp <= 0)
        {
            OnDead(creature);
            State = ECreatureState.Dead;
            return;
        }
        else
        {
            //PlayTriggerAnimation("Damaged");
        }
            
    }
    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);

        // 즉시 타겟 무효화
        if (attacker is Creature attackerCreature)
        {
            attackerCreature.Target = null;
        }
        Managers.Battle.UnitDeath(this);
        State = ECreatureState.Dead;
        StartCoroutine(CoDieEffect());
    }
    IEnumerator CoDieEffect()
    {
        yield return new WaitForSeconds(1f);
        Managers.Object.Despawn(this);
    }
    #endregion

}