using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public SkillBase Base { get; set; }
    public int Cost { get; set; }

    public Skill(SkillBase skillBase)
    {
        Base = skillBase;
        Cost = skillBase.Cost;
    }

}
