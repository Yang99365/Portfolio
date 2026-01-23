using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;
using static Define;

public class BaseObject : InitBase
{
    public EGameObjectType ObjectType { get; protected set; } = EGameObjectType.None;
    public CircleCollider2D Collider { get; private set; }
    public SpriteRenderer Renderer { get; private set; }
    public Rigidbody2D RigidBody { get; private set; }
    public Animator Animator { get; private set; }
    //private HurtFlashEffect HurtFlash;

    public float ColliderRadius { get { return Collider != null ? Collider.radius : 0.0f; } }
    public Vector3 CenterPosition { get { return transform.position + Vector3.up * ColliderRadius; } }
    public int DataTemplateID { get; set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Collider = gameObject.GetOrAddComponent<CircleCollider2D>();
        Renderer = gameObject.GetComponent<SpriteRenderer>();
        RigidBody = gameObject.GetOrAddComponent<Rigidbody2D>();
        Animator = gameObject.GetComponentInChildren<Animator>();
        //HurtFlash = gameObject.GetOrAddComponent<HurtFlashEffect>();


        return true;
    }

    #region Animation & Renderer

    protected virtual void SetRenderer(int DataID, bool isHero, bool isItem)
    {
        if(Renderer == null)
            return;
        if(isHero && !isItem)
            Renderer.sprite = Managers.Resource.Load<Sprite>(Managers.Data.HeroDataDict[DataID].spriteAddress);
        else if (!isHero && !isItem)
            Renderer.sprite = Managers.Resource.Load<Sprite>(Managers.Data.MonsterDataDict[DataID].spriteAddress);
        //else if (isItem)
        //    Renderer.sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDataDict[DataID].spriteAddress);
    }
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

    #endregion
    #region Battle
    public virtual void OnDamaged(BaseObject attacker)
    {
        //HurtFlash.Flash();
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
