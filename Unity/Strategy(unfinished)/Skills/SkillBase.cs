using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Skills/SkillBase")]
public class SkillBase : ScriptableObject
{
    [SerializeField] string skillName;

    [TextArea]
    [SerializeField] string skillDescription;

    [SerializeField] GeneralType type;

    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int range;
    [SerializeField] int cost;

    public string SkillName { get => skillName; }
    public string SkillDescription { get => skillDescription; }
    public GeneralType Type { get => type; }
    public int Power { get => power; }
    public int Accuracy { get => accuracy; }
    public int Range { get => range; }
    public int Cost { get => cost; }

}
