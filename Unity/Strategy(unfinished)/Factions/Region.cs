using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class Region
{
    public string regionName;
    public Color regionColor;
    public int population;
    public string landmarkBuilding; // 랜드마크 건물 이름
    public int landmarkBuildingLevel; // 랜드마크 건물 레벨
    public int landmarkBuildingMaxLevel = 1; // 랜드마크 건물 최대 레벨
    public bool canBuildLandmark;  // 랜드마크 건물을 지을 수 있는지 여부
    

    public int barracksLevel;  // 병영 레벨
    public int mineLevel;      // 광산 레벨
    public int maxBarracksLevel = 3;  // 최대 병영 레벨
    public int maxMineLevel = 5;  // 최대 광산 레벨

    public Region(Color color, string name, int initialPopulation, string landmarkBuildingName = null)
    {
        regionName = name;
        regionColor = color;
        population = initialPopulation;
        landmarkBuilding = landmarkBuildingName;
        canBuildLandmark = landmarkBuilding != null; // 랜드마크가 null이 아니면 true
        barracksLevel = 0;
        mineLevel = 0;
        landmarkBuildingLevel = 0;
    }
    // 특별한 기능 +
}
