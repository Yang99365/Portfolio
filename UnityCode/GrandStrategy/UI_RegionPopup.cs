using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using static UnityEditor.Progress;
#endif
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public class UI_RegionPopup : UI_Popup
{
    private List<Button> regionActionButtons = new List<Button>();
    private List<Button> factionActionButtons = new List<Button>();

    
    // 턴넘길때 건물단계에 따라 추가로 휙득

    
    public Region selectedRegion;

    
    enum RegionInfoText
    {
        RegionNameText,
        PopulationText,
        ControllingFactionNameText,
    }
    enum GameObjects
    {
        ActionList,
    }

    public enum RegionAction
    {
        SellPopulation,
        BuyPopulation,
        BuildFarm,
        BuildMine,
        TalentHire,
    }
    public enum FactionAction
    {
        DeclareWar,
        War,
        Truce,
        Alliance,
        AllianceBreak,
    }
    private void LoadButtonPrefab()
    {
        // Load Region Action Button Prefab
        Addressables.LoadAssetAsync<GameObject>("UI_RegionAction").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InitializeButtons(handle.Result, true);
            }
            else
            {
                Debug.LogError("Failed to load white button prefab.");
            }
        };

        // Load Faction Action Button Prefab
        Addressables.LoadAssetAsync<GameObject>("UI_FactionAction").Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                InitializeButtons(handle.Result, false);
            }
            else
            {
                Debug.LogError("Failed to load yellow button prefab.");
            }
        };
    }
    private void InitializeButtons(GameObject buttonPrefab, bool isRegionAction)
    {
        GameObject actionListParent = Get<GameObject>((int)GameObjects.ActionList);
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);

        if (isRegionAction)
        {
            foreach (RegionAction action in System.Enum.GetValues(typeof(RegionAction)))
            {
                var button = Instantiate(buttonPrefab, actionListParent.transform).GetComponent<Button>();
                button.name = action.ToString(); // 이름을 열거형 값으로 설정
                button.GetComponentInChildren<Text>().text = action.ToString();
                button.onClick.AddListener(() => ExecuteRegionAction(action)); // 버튼 클릭 이벤트 바인딩
                
                button.gameObject.SetActive(false);
                regionActionButtons.Add(button);
            }
        }
        else
        {
            foreach (FactionAction action in System.Enum.GetValues(typeof(FactionAction)))
            {
                var button = Instantiate(buttonPrefab, actionListParent.transform).GetComponent<Button>();
                button.name = action.ToString(); // 이름을 열거형 값으로 설정
                button.GetComponentInChildren<Text>().text = action.ToString();
                button.onClick.AddListener(() => ExecuteFactionAction(action)); // 버튼 클릭 이벤트 바인딩
                button.gameObject.SetActive(false);
                factionActionButtons.Add(button);
            }
        }
    }
    // 지역 행동 실행
    private void ExecuteRegionAction(RegionAction action)
    {
        if (!Managers.Game.UseActionPoint())
        {
            Debug.Log("Not enough Action Points!");
            return;
        }
        switch (action)
        {
            case RegionAction.BuildFarm:
                selectedRegion.UpgradeBuilding(Building.BuildingType.Farm);
                break;
            case RegionAction.BuildMine:
                selectedRegion.UpgradeBuilding(Building.BuildingType.Mine); // 업그레이드 시도
                break;
            case RegionAction.SellPopulation:
                selectedRegion.SellPopulation(10, 10);
                break;
            case RegionAction.BuyPopulation:
                selectedRegion.BuyPopulation(10, 30);
                break;
            case RegionAction.TalentHire:
                Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
                playerFaction.HireGeneral();
                Managers.UI.CloseAllPopupUI(); // RegionPopup UI를 처음에 잘못만들어서 이렇게 다 닫아야함.
                break;
            default:
                Debug.Log("Action not implemented yet.");
                break;
        }
        
        RefreshUI(selectedRegion, Managers.Game.selectedFactionId); // UI 갱신
    }

    // 세력 행동 실행
    private void ExecuteFactionAction(FactionAction action)
    {
        if (!Managers.Game.UseActionPoint())
        {
            Debug.Log("Not enough Action Points!");
            return;
        }

        Faction playerFaction = Managers.Game.factions.Find(f => f.FactionID == Managers.Game.selectedFactionId);
        Faction targetFaction = Managers.Game.factions.Find(f => f.FactionID == selectedRegion.controllingFactionId);

        switch (action)
        {
            case FactionAction.DeclareWar:
                playerFaction.DeclareWar(targetFaction);
                break;
            case FactionAction.Truce:
                playerFaction.MakeTruce(targetFaction);
                break;
            case FactionAction.Alliance:
                playerFaction.FormAlliance(targetFaction);
                break;
            case FactionAction.AllianceBreak:
                playerFaction.BreakAlliance(targetFaction);
                break;
            case FactionAction.War:
                UI_WarPopup popup = Managers.UI.ShowPopupUI<UI_WarPopup>();
                popup.SetInfo(playerFaction, targetFaction, selectedRegion);
                break;
            default:
                Debug.Log("Action not implemented yet.");
                break;
        }

        RefreshUI(selectedRegion, Managers.Game.selectedFactionId); // Refresh the UI to reflect changes
    }

    public void SetInfo()
    {
        
        MapViewHighlighter mapViewHighlighter = FindObjectOfType<MapViewHighlighter>();
        if (mapViewHighlighter != null)
        {
            mapViewHighlighter.OnRegionClicked += RefreshUI;  // 이벤트에 RefreshUI 메서드 구독
        }
        else
        {
            Debug.LogError("MapViewHighlighter not found on the map!");
        }

        BindTexts(typeof(RegionInfoText));
        BindObjects(typeof(GameObjects));

        // 생성했을때 기본적으로 보여줄 정보를 설정하는 코드 작성
        GetText((int)RegionInfoText.RegionNameText).text = "Region Info";
        GetText((int)RegionInfoText.PopulationText).text = "Population: 100";
        GetText((int)RegionInfoText.ControllingFactionNameText).text = "Controlling Faction: FactionName";
        

        LoadButtonPrefab();

        //마지막으로 UI를 꺼두도록 설정
        gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        // 구독 해제
        MapViewHighlighter mapViewHighlighter = FindObjectOfType<MapViewHighlighter>();
        if (mapViewHighlighter != null)
        {
            mapViewHighlighter.OnRegionClicked -= RefreshUI;
        }
    }
    public void RefreshUI(Region region, int playerFactionID)
    {
        if(region == null)
        {
            gameObject.SetActive(false);
            return;
        }
        selectedRegion = region;
        gameObject.SetActive(true);
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == playerFactionID);

        GetText((int)RegionInfoText.RegionNameText).text = region.RegionName;
        GetText((int)RegionInfoText.PopulationText).text = "Population: " + region.population.ToString();
        if (Managers.Game.factions.Find(x => x.FactionID == region.controllingFactionId) == null)
        {
            GetText((int)RegionInfoText.ControllingFactionNameText).text = " None";
        }
        else
        {
            GetText((int)RegionInfoText.ControllingFactionNameText).text = Managers.Game.factions.Find(x => x.FactionID == region.controllingFactionId).FactionName;
        }
        
        bool isOwnedByPlayer = (region.controllingFactionId == playerFactionID);

        // 초기 상태에서 모든 버튼 비활성화
        regionActionButtons.ForEach(button => button.gameObject.SetActive(false));
        factionActionButtons.ForEach(button => button.gameObject.SetActive(false));

        // 지역 행동 버튼 활성화
        regionActionButtons.ForEach(button => button.gameObject.SetActive(isOwnedByPlayer));

        // 세력 관계에 따른 버튼 활성화
        // 무소속 지역은 따로 처리를 만들어야함. 시나리오2에 무소속지역이 있으므로 이를 처리해야함.
        factionActionButtons.ForEach(button =>
        {
            FactionAction actionType = (FactionAction)Enum.Parse(typeof(FactionAction), button.gameObject.name);
            bool shouldShow = false;

            if (isOwnedByPlayer)
            {
                // 내 지역인 경우 세력 관련 버튼은 모두 비활성화
                shouldShow = false;
            }
            else
            {
                switch (actionType)
                {
                    case FactionAction.DeclareWar:
                        // 전쟁 선포 버튼은 중립 세력에만 표시
                        shouldShow = playerFaction.neutrals.Exists(x => x.FactionID == region.controllingFactionId);
                        break;
                    case FactionAction.War:
                    case FactionAction.Truce:
                        // 전쟁 및 휴전 버튼은 적대 세력에만 표시
                        shouldShow = playerFaction.enemies.Exists(x => x.FactionID == region.controllingFactionId);
                        break;
                    case FactionAction.Alliance:
                        // 동맹 버튼은 중립 세력에만 표시
                        shouldShow = playerFaction.neutrals.Exists(x => x.FactionID == region.controllingFactionId);
                        break;
                    case FactionAction.AllianceBreak:
                        // 동맹 파기 버튼은 동맹 세력에만 표시
                        shouldShow = playerFaction.allies.Exists(x => x.FactionID == region.controllingFactionId);
                        break;
                }
            }
            button.gameObject.SetActive(shouldShow);
        });
    }
    public void CloseUI()
    {
        gameObject.SetActive(false);
    }
}
