using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TbsFramework.Units;
using TbsFramework.Cells;
using TbsFramework.Grid;

public class General : Unit
{
    [SerializeField] private GeneralBase _Base;
    public int containerNum; // 가져올 컨테이너 속 무장의 번호


    public string UnitName;
    private Transform Highlighter;
    public Vector3 Offset;

    public override void Initialize()
    {   
        base.Initialize();
        Highlighter = transform.Find("Highlighter");
        UnMark();
        transform.localPosition += Offset;

        (Cell as BattleSquare).isWalkable = false; //흠
        GenerateGeneral();

        
    }
    
    public void GenerateGeneral()
    {
        if(PlayerNumber == 0)// 플레이어라면
        {
            if(BattleContainer.instance.Playergenerals[containerNum] != null)
            {
                _Base = BattleContainer.instance.Playergenerals[containerNum];
                UnitName = _Base.name;
                HitPoints = _Base.hp;
                AttackRange = _Base.rng;
                AttackFactor = _Base.atk;
                DefenceFactor = _Base.def;
                MovementPoints = _Base.mov;
            }
            else
            {
                // 장군이 없다면
                this.OnDestroyed();
            }
        }
        else // 적이라면
        {
            if(BattleContainer.instance.Enemygenerals[containerNum] != null)
            {
                _Base = BattleContainer.instance.Enemygenerals[containerNum];
                UnitName = _Base.name;
                HitPoints = _Base.hp;
                AttackRange = _Base.rng;
                AttackFactor = _Base.atk;
                DefenceFactor = _Base.def;
                MovementPoints = _Base.mov;
            }
            else
            {
                // 장군이 없다면 
                this.OnDestroyed();
            }
        }

    }
    
    //물위걷기, 산넘기, 숲지나기, 일반지형
    public override bool IsCellMovableTo(Cell cell)
    {
        return base.IsCellMovableTo(cell) &&
        (cell as BattleSquare).tileType != BattleSquare.TileType.Water;
    }
    public override bool IsCellTraversable(Cell cell)
    {
        return base.IsCellTraversable(cell) && 
        (cell as BattleSquare).tileType != BattleSquare.TileType.Water;
    }

    //물위걷기, 산넘기, 숲지나기, 일반지형

    
    private void SetHighlighterColor(Color color)
    {
        Highlighter.GetComponent<SpriteRenderer>().color = color;
    }

    public override void SetColor(Color color)
    {
        SetHighlighterColor(color);
    }
}
    /*
    유닛 상태에 따른 하이라이트 변화
    public override void MarkAsDefending(Unit aggressor)
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsAttacking(Unit target)
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsDestroyed()
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsFriendly()
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsReachableEnemy()
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsSelected()
    {
        // 본문을 여기에 작성하세요.
    }

    public override void MarkAsFinished()
    {
        // 본문을 여기에 작성하세요.
    }

    public override void UnMark()
    {
        // 본문을 여기에 작성하세요.
    }
    */

