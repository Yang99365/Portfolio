using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(fileName = "New General", menuName = "General")]
public class GeneralBase : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] int CharacterID;
    [SerializeField] Sprite Portrait;
    
    [SerializeField] int Level;
    [SerializeField] int Exp;
    [SerializeField] int[] NextLevelExp;



    [SerializeField] bool Status; // true = alive, false = dead
    [SerializeField] string Faction; // 소속
    [SerializeField] Troops TroopType; // 지휘하는 병종
    [SerializeField] Sprite TroopImage; // 병종 이미지
    [SerializeField] GeneralType Type; // 직업
    [SerializeField] Sprite TypeImage; // 직업 이미지

    // base stats
    [SerializeField] int HP;  // Hit Points
    [SerializeField] int MHP; // Max Hit Points
    [SerializeField] int MP;  // Magic Points
    [SerializeField] int MMP; // Max Magic Points

    [SerializeField] int ATK; // Physical Attack
    [SerializeField] int DEF; // Physical Defense
    [SerializeField] int MAT; // Magic Attack
    [SerializeField] int MDF; // Magic Defense
    [SerializeField] int RNG; // Range
    [SerializeField] float EVD; // Evade
    [SerializeField] float RES; // Status Resistance
    [SerializeField] int SPD; // Speed
    [SerializeField] int MOV; // Move Range
    [SerializeField] float CRT; // Critical Rate
    [SerializeField] bool CTR; // Counter - 공격받았을때 공격하는지 여부

	
    
    [SerializeField] EquipItem Equipment1; // 장착한 장비들
    [SerializeField] EquipItem Equipment2;
    [SerializeField] List<ASkill> ActiveSkills; // 사용하는 액티브 스킬들
    [SerializeField] List<PSkill> PassiveSkills; // 가진 패시브 스킬들

    public void EquipItem(string itemCode)
    {
        EquipItem item = ItemController.instance.GetItem(itemCode) as EquipItem;
        if (Equipment1 == null)
        {
            Equipment1 = item;
            ApplyEquipmentEffects(item);
        }
        else if (Equipment2 == null)
        {
            Equipment2 = item;
            ApplyEquipmentEffects(item);
        }
    }

    // 장비 해제 메소드
    public void UnequipItem(string itemCode)
    {
        EquipItem item = ItemController.instance.GetItem(itemCode) as EquipItem;
        if (Equipment1 == item)
        {
            
            RemoveEquipmentEffects(item);
            Equipment1 = null;
        }
        else if (Equipment2 == item)
        {
            RemoveEquipmentEffects(item);
            Equipment2 = null;
        }
    }

    // 장비 효과 적용 메소드
    private void ApplyEquipmentEffects(EquipItem item)
    {
        ATK += (int)item.atkBonus;
        DEF += (int)item.defBonus;
        MAT += (int)item.intBonus;
        SPD += (int)item.spdBonus;
        // 기타 스탯에 대한 효과도 여기에 추가...
    }

    // 장비 효과 제거 메소드
    private void RemoveEquipmentEffects(EquipItem item)
    {
        ATK -= (int)item.atkBonus;
        DEF -= (int)item.defBonus;
        MAT -= (int)item.intBonus;
        SPD -= (int)item.spdBonus;
        // 기타 스탯에 대한 효과도 여기에 추가...
    }

    # region Getters
    public string Name
    {
        get { return name; }
    }
    public int ID
    {
        get { return CharacterID; }
    }
    public Sprite portrait
    {
        get { return Portrait; }
    }
    public int level
    {
        get { return Level; }
    }
    public int exp
    {
        get { return Exp; }
    }
    public int nextLevelExp
    {
        get { return NextLevelExp[Level]; }
    }

    public bool status
    {
        get { return Status; }
    }

    public string faction
    {
        get { return Faction; }
    }

    public Troops troopType
    {
        get { return TroopType; }
    }

    public Sprite troopImage
    {
        get { return TroopImage; }
    }

    public GeneralType type
    {
        get { return Type; }
    }

    public int hp
    {
        get { return HP; }
    }

    public int mhp
    {
        get { return MHP; }
    }

    public int mp
    {
        get { return MP; }
    }

    public int mmp
    {
        get { return MMP; }
    }

    public int atk
    {
        get { return ATK; }
    }

    public int def
    {
        get { return DEF; }
    }

    public int mat
    {
        get { return MAT; }
    }

    public int mdf
    {
        get { return MDF; }
    }
    public int rng
    {
        get { return RNG; }
    }

    public float evd
    {
        get { return EVD; }
    }

    public float res
    {
        get { return RES; }
    }

    public int spd
    {
        get { return SPD; }
    }

    public int mov
    {
        get { return MOV; }
    }

    public float crt
    {
        get { return CRT; }
    }

    public bool ctr
    {
        get { return CTR; }
    }
    public EquipItem equipment1
    {
        get { return Equipment1; } set { Equipment1 = value;}
    }
    public EquipItem equipment2
    {
        get { return Equipment2; } set { Equipment2 = value; }
    }
    public List<ASkill> activeSkills
    {
        get { return ActiveSkills; }
    }
    public List<PSkill> passiveSkills
    {
        get { return PassiveSkills; }
    }
    # endregion
}
// UI로 표시할떈 이걸쓰고 전투에선 그냥 이름과 ID만 체크해서 유닛레시피를 통해 불러온다...