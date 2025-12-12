using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Faction
{
    public string factionName; // 세력의 이름
    public Color factionColor; // 세력의 고유 색깔
    public FactionAbility[] factionAbilities;
    public List<string> controlledRegions; // 세력이 소유한 지역 리스트
    public List<string> generalsCodes;
    public int gold;

    public Faction(string name, Color color, int initialGold)
    {
        factionName = name;
        factionColor = color;
        controlledRegions = new List<string>();
        gold = initialGold;
    }
}
    