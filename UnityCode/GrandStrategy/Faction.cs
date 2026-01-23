using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;
using System;
using static Define;
using Random = UnityEngine.Random;

public class Faction
{
    public event Action OnResourceChanged;
    public Data.FactionData FactionData { get; private set; }

    // Effects 변수, 해당 세력츼 버프류(패시브)의 효과를 저장하는 변수 와 능력을 사용하는 함수 등 구현해야함

    #region FactionInfo

    public int FactionID;
    public string FactionName;
    public Color FactionColor;
    public List<Region> controlledRegions; // 세력이 점령한 지역 ID 리스트
    public List<ResourceData> hasResources; // 세력이 소유한 자원 리스트
    public List<Faction> allies; // 동맹 세력 ID 리스트
    public List<Faction> enemies; // 적대 세력 ID 리스트
    public List<Faction> neutrals; // 중립 세력 ID 리스트
    public List<General> generals; 
    public List<int> factionSkillIds; // 세력 특수 능력 ID 리스트(미구현)

    #endregion

    public Faction Init(int FactionID)
    {
        FactionData = Managers.Data.FactionDic[FactionID];

        this.FactionID = FactionData.id;
        this.FactionName = FactionData.factionName;
        this.FactionColor.r = FactionData.factionColor.r / 255f;
        this.FactionColor.g = FactionData.factionColor.g / 255f;
        this.FactionColor.b = FactionData.factionColor.b / 255f;
        this.FactionColor.a = FactionData.factionColor.a / 255f;
        this.hasResources = FactionData.hasResources;
        this.controlledRegions = new List<Region>();
        this.allies = new List<Faction>();
        this.enemies = new List<Faction>();
        this.neutrals = new List<Faction>();
        this.generals = new List<General>();



        this.factionSkillIds = FactionData.factionSkillIds;

        return this;
    }
    // 세력간의 관계 설정, 지역 교환에 쓸 함수들
    public void ListAdd<T>(List<T> list, T item)
    {
        list.Add(item);
    }

    public void ListRemove<T>(List<T> list, T item)
    {
        list.Remove(item);
    }

    // 자원 관련 메소드
    // 가진 자원 가져오기
    public int GetResourceAmount(EResourceType resourceType)
    {
        ResourceData resource = hasResources.Find(r => r.resourceType == resourceType);
        return resource != null ? resource.amount : 0;
    }

    public void ReceiveGold(int amount)
    {
        // 가정: Gold는 Faction의 자원 리스트에 포함되어 있음
        ResourceData gold = hasResources.Find(r => r.resourceType == EResourceType.Gold);
        if (gold != null)
        {
            gold.amount += amount;
            OnResourceChanged?.Invoke();
        }
    }

    public void SpendGold(int amount)
    {
        ResourceData gold = hasResources.Find(r => r.resourceType == EResourceType.Gold);
        if (gold != null && gold.amount >= amount)
        {
            gold.amount -= amount;
            OnResourceChanged?.Invoke();
        }
        else
        {
            Debug.LogError("Not enough gold to spend.");
        }
    }

    public bool CanSpendGold(int amount)
    {
        ResourceData gold = hasResources.Find(r => r.resourceType == EResourceType.Gold);
        return gold != null && gold.amount >= amount;
    }

    // 장군 고용 메소드
    public void HireGeneral()
    {
        if(Managers.Game.noneFactionGenerals.Count > 0 && CanSpendGold(1000)) // 무소속 무장이 있고 골드가 충분한 경우
        {
            int randomIndex = Random.Range(0, Managers.Game.noneFactionGenerals.Count);
            General selectedGeneral = Managers.Game.noneFactionGenerals[randomIndex];

            // 골드 차감
            SpendGold(1000);
            
            // 세력 ID 업데이트
            selectedGeneral.FactionID = FactionID;
            generals.Add(selectedGeneral); // 이렇게 추가하고 빼기보단 게임매니저의 AllGeneral을 세력에 따라 갱신하는게..
            Debug.Log($"Hired {selectedGeneral.GeneralName} for 1000 gold.");

            // 무소속 리스트에서 제거
            Managers.Game.noneFactionGenerals.RemoveAt(randomIndex);
        }
        else
        {
            Debug.LogError("Not enough gold or no generals available for hire.");
        }
        
    }

    // 외교 조작 메소드
    public void DeclareWar(Faction targetFaction)
    {
        if (!enemies.Contains(targetFaction))
        {
            enemies.Add(targetFaction);
            targetFaction.enemies.Add(this);
            neutrals.Remove(targetFaction);
            targetFaction.neutrals.Remove(this);
            Debug.Log($"{FactionName} has declared war on {targetFaction.FactionName}");
        }
    }

    public void MakeTruce(Faction targetFaction)
    {
        if (enemies.Contains(targetFaction))
        {
            enemies.Remove(targetFaction);
            targetFaction.enemies.Remove(this);
            neutrals.Add(targetFaction);
            targetFaction.neutrals.Add(this);
            Debug.Log($"{FactionName} has made truce with {targetFaction.FactionName}");
        }
    }

    public void FormAlliance(Faction targetFaction)
    {
        if (!allies.Contains(targetFaction) && !enemies.Contains(targetFaction))
        {
            allies.Add(targetFaction);
            targetFaction.allies.Add(this);
            neutrals.Remove(targetFaction);
            targetFaction.neutrals.Remove(this);
            Debug.Log($"{FactionName} has formed an alliance with {targetFaction.FactionName}");
        }
    }

    public void BreakAlliance(Faction targetFaction)
    {
        if (allies.Contains(targetFaction))
        {
            allies.Remove(targetFaction);
            targetFaction.allies.Remove(this);
            neutrals.Add(targetFaction);
            targetFaction.neutrals.Add(this);
            Debug.Log($"{FactionName} has broken the alliance with {targetFaction.FactionName}");
        }
    }

    public void CollectResources()
    {
        foreach (Region region in controlledRegions)
        {
            var production = region.CalculateResourceProduction();
            float stateMultiplier = GetRegionStateMultiplier(region.RegionState);

            foreach (var resourcePair in production)
            {
                // 현재 자원 찾기
                ResourceData resource = hasResources.Find(r => r.resourceType == resourcePair.Key);

                // 지역 상태에 따른 생산량 보정 적용
                int adjustedValue = Mathf.RoundToInt(resourcePair.Value * stateMultiplier);

                if (resource != null)
                {
                    // 자원 추가 (보정된 값 사용)
                    resource.amount += adjustedValue;
                }
                else
                {
                    // 새로운 자원 타입 추가 (보정된 값 사용)
                    hasResources.Add(new ResourceData
                    {
                        resourceType = resourcePair.Key,
                        amount = adjustedValue
                    });
                }

                // 디버그 로그로 자원 획득 정보 출력
                Debug.Log($"{FactionName}이(가) {region.RegionName}에서 " +
                    $"{resourcePair.Key} {adjustedValue}(기본: {resourcePair.Value}, " +
                    $"배율: {stateMultiplier})를 얻었습니다.");
            }
        }

        // 자원 변경 이벤트 발생
        OnResourceChanged?.Invoke();
    }

    public General GetGeneral(int generalID)
    {
        return generals.Find(g => g.GeneralID == generalID);
    }

    // 지역 상태에 따른 생산량 보정 메서드
    private float GetRegionStateMultiplier(ERegionState state)
    {
        switch (state)
        {
            case ERegionState.Confusion:
                return 0.7f;  // 30% 감소
            case ERegionState.Nice:
                return 1.3f;  // 30% 증가
            default:
                return 1.0f;
        }
    }

    public void RemoveControlledRegion(Region region)
    {
        controlledRegions.Remove(region);
    }
    public void AddControlledRegion(Region region)
    {
        controlledRegions.Add(region);
    }

    // 세력간의 지역 교환, 자원 교환,    세력간의 관계 확인 등 구현해야함
}
