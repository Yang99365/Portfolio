using Pathfinding;
using Pathfinding.RVO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class GeneralUnit : Creature
{
    public bool NeedArrange { get; set; }

    public bool isSelect = false;
    public bool hasVelocityCheck = false;

    private bool isAttacking = false;
    private AnimationEventHandler animEventHandler;

    public override ECreatureState State
    {
        get { return _state; }
        set
        {
            if (_state != value)
            {
               
                base.State = value;
            }
        }
    }

    ECreatureMoveState _creatureMoveState = ECreatureMoveState.None;
    public ECreatureMoveState CreatureMoveState
    {
        get { return _creatureMoveState; }
        private set
        {
            _creatureMoveState = value;
            switch (value)
            {
                case ECreatureMoveState.Chase:
                    NeedArrange = true;
                    break;
                case ECreatureMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.General;

        Transform animatorChild = transform.GetChild(0); // Animator를 가진 자식 찾기
        if (animatorChild != null)
        {
            animEventHandler = animatorChild.gameObject.AddComponent<AnimationEventHandler>();
        }

        //Collider.isTrigger = true;
        //RigidBody.simulated = false;

        ai = GetComponent<AIPath>();
        rvo = GetComponent<RVOController>();

        ai.enableRotation = false;
        ai.radius = 0.2f;  // 충돌 반경을 0으로 설정

        ai.onSearchPath += OnPathRecalculated;


        StartCoroutine(CoUpdateAI());

        return true;
    }
    
    
    public override void SetInfo(int templateID, bool isGeneral = true)
    {
        base.SetInfo(templateID, isGeneral);

        State = ECreatureState.Idle;

        //AiPath.maxSpeed = GeneralData.stats.speed/10; // 군대유닛은 장군유닛따라 가도록 생성할떄 설정
        ai.maxSpeed = GeneralData.stats.speed / 10;

        
        Managers.Battle.AddUnit(this.gameObject);
        
    }

    #region AI
    protected override void UpdateIdle()
    {
        if (CreatureMoveState == ECreatureMoveState.ForceMove)
        {
            State = ECreatureState.Move;
            return;
        }
        // 이미 Target이 있고 유효하다면, 바로 Chase 상태로 전환
        if (Target != null && Target.IsValid())
        {
            State = ECreatureState.Move;
            CreatureMoveState = ECreatureMoveState.Chase;
            isAttacking = false;
            return;
        }

        // 범위 내 적 탐지
        Creature enemyTarget = FindClosestInRange(
        SEARCH_DISTANCE,
        Managers.Object.Generals,
        obj => obj != null &&
               obj is Creature creature &&
               IsEnemy(creature) &&
               creature.State != ECreatureState.Dead
    ) as Creature;

        if (enemyTarget != null) // 선택된 유닛이 아닐 때만 자동 추격
        {
            Target = enemyTarget;
            State = ECreatureState.Move;
            CreatureMoveState = ECreatureMoveState.Chase;
            isAttacking = false;
            return;
        }

        


    }
    protected override void UpdateMove()
    {
        if(isMoving)
        {
            CheckMoveDirection();
        }

        if (CreatureMoveState == ECreatureMoveState.ForceMove)
        {
            State = ECreatureState.Move;
            if (ai.reachedDestination)
            {
                HandleDestinationReached();
            }
            return;
        }

        if (CreatureMoveState == ECreatureMoveState.Chase)
        {
            if (Target.IsValid() == false)
            {
                State = ECreatureState.Idle;
                CreatureMoveState = ECreatureMoveState.None;
                isAttacking = false;
                return;
            }

            LookAtTarget(Target);
            ChaseOrAttackTarget(SEARCH_DISTANCE, AttackDistance);
            return;
        }

        // 쫒아가고나서 Skill이 풀리고 Idle이 되면 원래 위치로 복귀해야..

    }
    
    
    protected override void UpdateDead()
    {
        base.UpdateDead();
        ai.isStopped = true; // A* 이동 중지
        rvo.enabled = false; // RVO 컨트롤러 비활성화
    }
    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        if (CreatureMoveState == ECreatureMoveState.ForceMove)
        {
            State = ECreatureState.Move;
            return;
        }

        if (Target.IsValid() == false)
        {
            State = ECreatureState.Idle;
            CreatureMoveState = ECreatureMoveState.None;
            return;
        }
        
        
        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = AttackDistance * AttackDistance;

        if (distToTargetSqr > attackDistanceSqr)
        {
            // 공격 범위를 벗어났을 때 Move 상태로 전환하고 Chase 모드로 설정
            State = ECreatureState.Move;
            CreatureMoveState = ECreatureMoveState.Chase;
            
            return;
        }

        if (!isAttacking)
        {
            StartAttack();
        }

    }
    public void OnAttackAnimationPoint()
    {
        if (!Target.IsValid()) return;
        Target.OnDamaged(this);
    }

    public void OnAttackAnimationComplete()
    {
        isAttacking = false;

        if (CreatureMoveState == ECreatureMoveState.ForceMove)
            return;

        // 타겟이 유효하지 않거나 현재 Skill 상태라면 Idle로 전환
        if (!Target.IsValid() || State == ECreatureState.Skill)
        {
            State = ECreatureState.Idle;
            CreatureMoveState = ECreatureMoveState.None;
        }
        // Move 상태라면 추적 계속
        else if (State == ECreatureState.Move)
        {
            CreatureMoveState = ECreatureMoveState.Chase;
        }
    }

    private void StartAttack()
    {
        if (!Target.IsValid()) return;

        isAttacking = true;
        LookAtTarget(Target);
        PlayTriggerAnimation("Attack");
    }

    
    
    //protected override void UpdateOnDamaged()
    //{
    //    base.UpdateOnDamaged();
    //}
    #endregion


    private void CheckMoveDirection()
    {
        Vector3 destination = ai.destination;
        // X축 방향으로의 이동 방향 확인
        float moveDirection = destination.x - transform.position.x;

        // 왼쪽으로 이동하면 true, 오른쪽으로 이동하면 false
        LookLeft = (moveDirection > 0);
    }

    public void MoveToPosition(Vector2 destination)
    {
        if (!isSelect) return;

        Target = null;
        isAttacking = false;  // 명시적으로 공격 상태 초기화
        CreatureMoveState = ECreatureMoveState.ForceMove;
        State = ECreatureState.Move;  // 상태 변경
        isMoving = true;

        

        Vector3 moveDirection = ((Vector3)destination - transform.position).normalized;

        ai.destination = destination;
        ai.SearchPath();

    }

    
    private void OnPathRecalculated()
    {
         //경로 재계산시에는 방향만 체크
        CheckMoveDirection();
    }
    
    private void HandleDestinationReached()
    {
        isMoving = false;
        State = ECreatureState.Idle;
        CreatureMoveState = ECreatureMoveState.None;
    }
    

    private void OnDestroy()
    {
        
        StopAllCoroutines();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public override void OnDead(BaseObject attacker)
    {
        isAttacking = false;
        base.OnDead(attacker); 
    }
}
