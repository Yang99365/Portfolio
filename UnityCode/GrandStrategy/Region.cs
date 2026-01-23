using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Region
{
    public Data.RegionData RegionData { get; private set; }


    // Effects 변수, 해당 지역이 받는 버프류의 효과를 저장하는 변수 등 구현해야함.

    #region RegionInfo
    public int RegionID;
    public string RegionName;
    public Color RegionColor;
    public List<ResourceData> resources;
    public int population;
    public List<int> connectedRegionIds; // 연결된 지역 ID 리스트
    public int controllingFactionId; // 소속 세력 ID

    public ERegionState RegionState = ERegionState.Normal;

    public List<Building> Buildings = new List<Building>();


    #endregion


    public Region Init(int RegionID)
    {
        RegionData = Managers.Data.RegionDic[RegionID];

        this.RegionID = RegionData.id;
        this.RegionName = RegionData.regionName;
        this.RegionColor.r = RegionData.regionColor.r / 255f;
        this.RegionColor.g = RegionData.regionColor.g / 255f;
        this.RegionColor.b = RegionData.regionColor.b / 255f;
        this.RegionColor.a = RegionData.regionColor.a / 255f;
        this.resources = RegionData.resources;
        this.population = RegionData.population;
        this.connectedRegionIds = RegionData.connectedRegionIds;
        this.controllingFactionId = RegionData.controllingFactionId;


        InitializeBuildings();


        return this;
    }
    private void InitializeBuildings()
    {
        Buildings.Add(new Building(Building.BuildingType.Farm, 1));
        Buildings.Add(new Building(Building.BuildingType.Mine, 1));
    }

    public void UpgradeBuilding(Building.BuildingType type)
    {
        // 이 지역을 소유한 세력 = controllingFactionId
        Faction controllingFaction = Managers.Game.factions.Find(f => f.FactionID == this.controllingFactionId);
        var building = Buildings.Find(b => b.Type == type);

        if(controllingFaction == null)
        {
            Debug.LogError($"이 지역의 건물 {type} 을 업그레이드를 하려했으나 이 지역을 소유중인 세력이 없어 실패했습니다.");
        }
        if (building != null && CanAffordUpgrade(controllingFaction, building))
        {
            if (controllingFaction.CanSpendGold(building.UpgradeCost))
            {
                // 자원 차감
                controllingFaction.SpendGold(building.UpgradeCost);
                building.Level++;
                Debug.Log($"Upgraded {type} to Level {building.Level}. Cost was {building.UpgradeCost}. Remaining gold: {controllingFaction.GetResourceAmount(EResourceType.Gold)}");
            }
            else
            {
                Debug.LogError($"Not enough resources to upgrade. Current level is {building.Level}. Upgrade cost is {building.UpgradeCost}.");
            }
        }
        else
        {
            Debug.LogError("Cannot afford to upgrade or building not found.");
        }
    }

    private bool CanAffordUpgrade(Faction faction, Building building)
    {
        ResourceData goldResource = faction.hasResources.Find(r => r.resourceType == EResourceType.Gold);
        return goldResource != null && goldResource.amount >= building.UpgradeCost;
    }


    public void SellPopulation(int amount, int pricePerUnit)
    {
        if (population >= amount)
        {
            population -= amount;
            int income = amount * pricePerUnit;
            Faction controllingFaction = Managers.Game.factions.Find(f => f.FactionID == controllingFactionId);
            controllingFaction.ReceiveGold(income);
            Debug.Log($"Sold {amount} population for {income} gold.");
        }
        else
        {
            Debug.LogError("Not enough population to sell.");
        }
    }

    public void BuyPopulation(int amount, int pricePerUnit)
    {
        int cost = amount * pricePerUnit;
        Faction controllingFaction = Managers.Game.factions.Find(f => f.FactionID == controllingFactionId);
        if (controllingFaction.CanSpendGold(cost))
        {
            population += amount;
            controllingFaction.SpendGold(cost);
            Debug.Log($"Bought {amount} population for {cost} gold.");
        }
        else
        {
            Debug.LogError("Not enough gold to buy population.");
        }
    }

    // 건물에 농장과 광산만 구현해놔서 식량, 철광석, 골드만 계산함. 다른 자원들도 필요하면 추가해야함.
    // 해당 지역의 상태를 변경하는 함수, 해당 지역의 자원을 사용하는 함수, 해당 지역의 인구를 얻는 함수, 해당 지역의 인구를 사용하는 함수 등 구현해야함.
    public Dictionary<EResourceType, int> CalculateResourceProduction()
    {
        Dictionary<EResourceType, int> production = new Dictionary<EResourceType, int>
        {
            { EResourceType.Gold, CalculateGoldProduction() },
            { EResourceType.Food, CalculateFoodProduction() },
            { EResourceType.Iron, CalculateIronProduction() }
            // 필요한 다른 자원들도 추가
        };

        return production;
    }

    private int CalculateGoldProduction()
    {
        int baseGold = population / 10; // 인구 10명당 1골드

        // 건물에 따른 추가 골드
        foreach (var building in Buildings)
        {
            if (building.Type == Building.BuildingType.Farm)
                baseGold += building.Level * 5;
            else if (building.Type == Building.BuildingType.Mine)
                baseGold += building.Level * 8;
        }

        return baseGold;
    }

    private int CalculateFoodProduction()
    {
        int baseFood = population / 20; // 인구 20명당 1식량

        // 농장 레벨에 따른 추가 식량
        var farm = Buildings.Find(b => b.Type == Building.BuildingType.Farm);
        if (farm != null)
        {
            baseFood += farm.Level * 10;
        }

        return baseFood;
    }

    private int CalculateIronProduction()
    {
        int baseIron = 0;

        // 광산 레벨에 따른 철광석
        var mine = Buildings.Find(b => b.Type == Building.BuildingType.Mine);
        if (mine != null)
        {
            baseIron += mine.Level * 3;
        }

        return baseIron;
    }
}
