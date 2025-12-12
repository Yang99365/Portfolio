using System.Collections.Generic;

[System.Serializable]
public class Generals
{
    public string Name;
    public string Code;
    public string Portrait;
    public int Attack;
    public int Defense;
    public int Intelligence;
    public int Speed;
    public int ActionPoint;
    public int Cost;
    public bool Status;
    public string Faction;
    public List<string> LeadableUnits;
    public string LeadingUnit;
    public List<string> Items;
    public List<string> OffensiveSkills;
    public List<string> PassiveSkills;
}