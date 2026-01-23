using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Building
{
    public enum BuildingType
    {
        Farm,
        Mine,
        
    }

    public BuildingType Type;
    public int Level;
    public int UpgradeCost => Level * 100;

    public Building(BuildingType type, int level)
    {
        Type = type;
        Level = level;
    }
}