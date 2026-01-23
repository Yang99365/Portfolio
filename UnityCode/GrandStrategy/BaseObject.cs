using ES3Types;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;
using static Define;

public class BaseObject : InitBase
{
    public EObjectType ObjectType { get; protected set; } = EObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    public Rigidbody2D RigidBody { get; private set; }
    public Animator Animator { get; private set; }
    public RectTransform RectTransform { get; private set; }
    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }
    public int DataTemplateID { get; set; }
    bool _lookLeft = true;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        RigidBody = gameObject.GetOrAddComponent<Rigidbody2D>();
        Animator = gameObject.GetComponentInChildren<Animator>();
        RectTransform = gameObject.GetOrAddComponent<RectTransform>();


        return true;
    }

    public void LookAtTarget(BaseObject target)
    {
        Vector2 dir = target.transform.position - transform.position;
        if (dir.x > 0)
            LookLeft = true;
        else
            LookLeft = false;
    }
    public static Vector3 GetLookAtRotation(Vector3 dir)
    {
        // Mathf.Atan2를 사용해 각도를 계산하고, 라디안에서 도로 변환
        float angle = Mathf.Atan2(-dir.x, dir.y) * Mathf.Rad2Deg;

        // Z축을 기준으로 회전하는 Vector3 값을 리턴
        return new Vector3(0, 0, angle);
    }

    public bool LookLeft
    {
        get { return _lookLeft; }
        set
        {
            _lookLeft = value;
            Flip(!value);
        }
    }
    #region Battle
    //public virtual void OnDamaged(BaseObject attacker, SkillBase skill)
    //{

    //}

    //public virtual void OnDead(BaseObject attacker, SkillBase skill)
    //{

    //}
    #endregion

    #region Animation
    protected virtual void UpdateAnimation()
    {
    }
    public void PlayTriggerAnimation(string name)//attack,damaged,death
    {
        if (Animator == null)
            return;

        Animator.SetTrigger(name);

    }
    public void PlayBoolAnimation(string name, bool flag)//move,debuff,isdead
    {
        if (Animator == null)
            return;

        Animator.SetBool(name, flag);
    }
    public void Flip(bool flag)
    {
        if (Animator == null)
            return;

        RectTransform.localScale = new Vector3(flag ? 1 : -1, 1, 1);

    }

    #endregion
    #region Battle
    public virtual void OnDamaged(BaseObject attacker)
    {

    }

    public virtual void OnDead(BaseObject attacker)
    {

    }
    //public virtual void OnDamaged(BaseObject attacker, SkillBase skill)
    //{

    //}

    //public virtual void OnDead(BaseObject attacker, SkillBase skill)
    //{

    //}
    #endregion
}
