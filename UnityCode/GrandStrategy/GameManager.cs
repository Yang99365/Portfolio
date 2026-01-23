using Data;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[Serializable]
public class GameSaveData
{
    public int Turn = 1;

    public int Gold = 0;
    public int Wood = 0;
    public int Stone = 0;
    public int Iron = 0;
    public int Food = 0;
    public int Horse = 0;

    public List<GeneralSaveData> Generals = new List<GeneralSaveData>();
    public List<RegionSaveData> Regions = new List<RegionSaveData>();
    public List<FactionSaveData> Factions = new List<FactionSaveData>();

    public int ItemInstanceGenerator = 1;
    public List<ItemSaveData> Items = new List<ItemSaveData>();
}
public class GeneralSaveData
{
    public int DataId = 0;
    public int factionId = 0;
    // 가진 장비도 추가해야함
}
public class RegionSaveData
{
    public int DataId = 0;
    public int factionId = 0;
    // 건물, 인구 등의 정보 추가필요
}
public class FactionSaveData
{
    public int DataId = 0;
    public List<int> generals = new List<int>();
    public List<int> regions = new List<int>();
    public List<int> allies = new List<int>();
    public List<int> enemies = new List<int>();
    public List<int> neutrals = new List<int>();

    // 세력의 자원, 지역 등의 정보 추가필요
}

public class ItemSaveData
{
    public int InstanceId; // 쌩DB ID의 사용을 막기위한 인스턴스 ID
    public int TemplateId; // 데이터 시트 ID
    public int Count;

    public ItemSaveData(int instanceId, int templateId, int count)
    {
        InstanceId = instanceId;
        TemplateId = templateId;
        Count = count;
    }
}

public class GameManager
{
    GameSaveData _saveData = new GameSaveData();
    public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }

    // Region 클래스랑 Faction 클래스 작성하고, 맵을 불러오면서
    // Region, Faction 리스트를 만들어서 시나리오 맵 데이터와 펙션데이터, 지역데이터를 통해 리스트를 크기에 맞게 만들고
    // 이렇게 만든 내용들로 게임을 진행시키기
    // 게임 시작 시에 플레이어 펙션은 플레이어가 선택한 펙션으로 설정
    // *자원관리, 동맹관리, 세이브, 로드, 턴 관리, 이벤트 관리, 전투 관리, 승리 조건 관리, 패배 조건 관리*
    public ScenarioMapData selectedScenario;
    public int selectedFactionId;

    public event Action OnActionPointChanged;
    private int _actionPoint = Define.MaxActionPoint;

    public bool isGameOver = false;
    public event Action<bool> OnGameEnd;
    public int ActionPoint
    {
        get => _actionPoint;
        private set
        {
            if (_actionPoint != value)
            {
                _actionPoint = value;
                OnActionPointChanged?.Invoke();
            }
        }
    }

    public List<Region> regions = new List<Region>();
    public List<Region> noneFactionRegions = new List<Region>();
    public List<Faction> factions = new List<Faction>();
    public List<General> Allgenerals = new List<General>();
    public List<General> noneFactionGenerals = new List<General>();

    public int GenerateItemInstanceId()
    {
        int itemInstanceId = _saveData.ItemInstanceGenerator;
        _saveData.ItemInstanceGenerator++;
        return itemInstanceId;
    }

    public void CreateGame() // 시나리오씬 스크립트에서 맵 만들면서 호출하기
    {
        if(Managers.Turn == null)
        {
            Debug.LogError("TurnManager가 없습니다.");
            return;
        }
        else
        {
            //Manager.Turn.OnTurnChanged 로 했었으나.. 인벤에서 쓰던대로 바꿈
            TurnManager.OnTurnChanged -= OnTurnEnd;
            TurnManager.OnTurnChanged += OnTurnEnd;
        }
        // 맵 속 지역 생성
        foreach (var region in selectedScenario.regionIds)
        {
            regions.Add(new Region().Init(region));
        }
        // 모든 무장들 생성
        foreach (var general in selectedScenario.generalsIds)
        {
            Allgenerals.Add(new General().Init(general));
        }
        // 무소속 무장들 생성
        foreach (var general in Allgenerals)
        {
            if (general.FactionID == 0)
            {
                noneFactionGenerals.Add(general);
                Debug.Log("무소속 무장 생성");
                Debug.Log(general.GeneralName);
            }
        }

        // 맵 속 세력 생성
        foreach (var faction in selectedScenario.playableFactionIds)
        {
            factions.Add(new Faction().Init(faction));
        }

        //foreach (var region in regions)
        //{
        //    if (Managers.Game.factions.Find(x => x.FactionID == region.controllingFactionId) == null)
        //    {
        //        noneFactionRegions.Add(region);
        //    }
        //    Debug.Log(region.RegionData.regionName);
        //}
        // 무소속 지역 작동이 안됨. 왜지? if문의 조건이 Popup스크립트에선 잘되는데;

        // 각 세력의 기본적인 지역, 동맹, 적대, 중립 관계 설정 , 무장배치
        foreach (Faction faction in factions)
        {
            // 각 세력에 지역 할당, 세력이 소유한 지역을 찾아서 할당
            faction.controlledRegions = regions.FindAll(x => faction.FactionData.controlledRegionIds.Contains(x.RegionID));

            // 각 세력에 동맹 할당
            faction.allies = factions.FindAll(x => faction.FactionData.allies.Contains(x.FactionID));
            // 각 세력에 적대 할당
            faction.enemies = factions.FindAll(x => faction.FactionData.enemies.Contains(x.FactionID));
            // 각 세력에 중립 할당
            faction.neutrals = factions.FindAll(x => faction.FactionData.neutrals.Contains(x.FactionID));

            // 소속이 있는 무장은 각 세력에 할당
            faction.generals = Allgenerals.FindAll(x => x.FactionID == faction.FactionID);
        }

        

        foreach (var faction in factions)
        {
            //테스트용, 각 세력의 무장들 출력
            Debug.Log($"{faction.FactionName}의 무장들");
            foreach (var general in faction.generals)
            {
                Debug.Log(general.GeneralName);
            }
            

        }
    }

    public Region FindRegionByColor(Color color)
    {
        float tolerance = 0.001f; // 색상 비교에 사용할 허용 오차
        foreach (var region in regions)
        {
            if (Mathf.Abs(region.RegionColor.r - color.r) < tolerance &&
                Mathf.Abs(region.RegionColor.g - color.g) < tolerance &&
                Mathf.Abs(region.RegionColor.b - color.b) < tolerance)
                return region;
        }
        return null;
    }
    public List<General> GetPlayerGeneral()
    {
        return factions.Find(x => x.FactionID == selectedFactionId).generals;
    }
    
    public General GetGeneral(int generalID)
    {
        return Allgenerals.Find(x => x.GeneralID == generalID);
    }
    public void FireGeneral(int generalID)
    {
        General general = Allgenerals.Find(x => x.GeneralID == generalID);
        general.FactionID = 0;
        RefreshGeneralFaction();

    }

    //무장들 세력갱신 (무소속 포함)
    public void RefreshGeneralFaction()
    {
        foreach (var faction in factions)
        {
            faction.generals = Allgenerals.FindAll(x => x.FactionID == faction.FactionID);
        }
        noneFactionGenerals = Allgenerals.FindAll(x => x.FactionID == 0);
    }

    public bool UseActionPoint(int amount = 1)
    {
        if (ActionPoint >= amount)
        {
            ActionPoint -= amount;
            return true;
        }
        return false;
    }
    public void ResetActionPoint()
    {
        ActionPoint = Define.MaxActionPoint;
    }
    // TurnManager의 NextTurn 메서드에서 호출되어야 합니다.
    public void OnTurnEnd()
    {
        ResetActionPoint();
        CheckGameEndCondition();
        // 기타 턴 종료 시 필요한 로직
    }
    private void CheckGameEndCondition()
    {
        if (isGameOver) return;

        Faction playerFaction = factions.Find(x => x.FactionID == selectedFactionId);
        if (playerFaction == null) return;

        // 승리 조건: 모든 지역 점령
        bool hasWon = true;
        foreach (var region in regions)
        {
            if (region.controllingFactionId != selectedFactionId)
            {
                hasWon = false;
                break;
            }
        }

        // 패배 조건: 지역이 하나도 없음
        bool hasLost = playerFaction.controlledRegions.Count == 0;

        if (hasWon || hasLost)
        {
            isGameOver = true;
            OnGameEnd?.Invoke(hasWon);
            ShowGameEndPopup(hasWon);
        }
    }

    private void ShowGameEndPopup(bool isVictory)
    {
        UI_GameEndPopup popup = Managers.UI.ShowPopupUI<UI_GameEndPopup>();
        popup.SetInfo(isVictory);
    }

#region Save & Load	

// easySave 써서 저장하기
public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

    public void SaveGame()
    {

    }

    public void LoadGame()
    {

    }
    #endregion
}
