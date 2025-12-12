using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FactionManager : MonoBehaviour
{
    public List<Faction> allFactions; // 모든 세력을 리스트로 관리

    private Dictionary<Color, Faction> colorToFactionMap = new Dictionary<Color, Faction>();
    // 2. 지역 이름/ID와 소유주 세력 매핑

    private Dictionary<string, Faction> regionToFactionMap = new Dictionary<string, Faction>();

    private RegionManager regionManager;
    public DiplomacyManager diplomacyManager;
    public GeneralManager generalManager;
    
    public Faction playerFaction;
    public GameObject factionSelectionPanel;
    public Dropdown factionDropdown;

    public Text turnText; // 현재 턴을 표시할 Text 컴포넌트
    public Text goldText; // 현재 금을 표시할 Text 컴포넌트
    public int currentTurn; // 현재 턴

    public bool playerFactionSelected = false; // 플레이어 세력이 선택되었는지 여부

    // singleton 패턴 Don't Destroy On Load
    public static FactionManager instance;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            diplomacyManager = FindObjectOfType<DiplomacyManager>();
            factionSelectionPanel = GameObject.Find("SelectFactionPanel");
            factionDropdown = GameObject.Find("SelectFaction").GetComponent<Dropdown>();
            turnText = GameObject.Find("Turn").GetComponent<Text>();
            goldText = GameObject.Find("Gold").GetComponent<Text>();
            if(playerFactionSelected)
            {
                factionSelectionPanel.SetActive(false);
            }
        }
        else
        {
            diplomacyManager = null;
            factionSelectionPanel = null;
            factionDropdown = null;
            turnText = null;
            goldText = null;
        }
            
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        regionManager = FindObjectOfType<RegionManager>();
        diplomacyManager = FindObjectOfType<DiplomacyManager>();
        generalManager = FindObjectOfType<GeneralManager>();
        InitializeColorToFactionMap();
        InitializeRegionToFactionMap();
        InitializeFactionDropdown();
        
    }
    void Start()
    {
        InitializeDiplomacy();
        SetPlayerFaction(factionDropdown);
        
    }
    void Update()
    {
        // 현재 턴과 금을 UI에 표시
        UpdatePlayerUI();
    }

    private void InitializeColorToFactionMap()
    {
        allFactions.Clear(); // 리스트 비우기
        
        
        // 세력 객체를 생성하고 리스트에 추가 (데이터 구조로 생성하기엔 동적인 부분이 많아 코드로 생성)
        Faction faction1 = new Faction("A", new Color(1f, 0f, 0f), 1000);
        allFactions.Add(faction1);

        Faction faction2 = new Faction("B", new Color(0f, 1f, 0f), 1000);
        allFactions.Add(faction2);

        Faction faction3 = new Faction("C", new Color(0f, 0f, 1f), 1000);
        allFactions.Add(faction3);
        
    }

    //세력의 컨트롤지역 초기화
    private void InitializeRegionToFactionMap()
    {
        ResetAllRegionFactions();

        
        AssignRegionToFaction("Region1", allFactions[0]); 
        AssignRegionToFaction("Region2", allFactions[0]);
        AssignRegionToFaction("Region3", allFactions[0]);
        AssignRegionToFaction("Region4", allFactions[0]);
        AssignRegionToFaction("Region5", allFactions[1]);
        AssignRegionToFaction("Region6", allFactions[1]);
        AssignRegionToFaction("Region7", allFactions[1]);
        AssignRegionToFaction("Region8", allFactions[1]);
        AssignRegionToFaction("Region9", allFactions[2]);
        AssignRegionToFaction("Region10", allFactions[2]);
        AssignRegionToFaction("Region11", allFactions[2]);
        AssignRegionToFaction("Region12", allFactions[2]);
        AssignRegionToFaction("Region13", allFactions[2]);
        // ... 나머지 지역들에 대한 세력 할당 ...
    }
    private void ResetAllRegionFactions()
    {
        Dictionary<Color, string> colorToRegionMap = regionManager.GetColorToRegionMap();
        // 기본적으로 모든 지역을 특정 세력에 할당하지 않음 (null 값 사용)
        foreach (var region in colorToRegionMap.Values)
        {
            AssignRegionToFaction(region, null);
        }
    }

    //RegionManager 스크립트의 UpdateRegionColorsAccordingToFactions() 지역 점령시 색상 변경과 같이 써주기
    public void AssignRegionToFaction(string regionName, Faction faction)
    {
        foreach (var existingFaction in allFactions)
        {
            existingFaction.controlledRegions.Remove(regionName);
        }

        // 지역을 새로운 세력에 할당
        regionToFactionMap[regionName] = faction;

        // 해당 세력의 controlledRegions 리스트 업데이트
        if (faction != null)
        {
            if (!faction.controlledRegions.Contains(regionName))
                faction.controlledRegions.Add(regionName);
        }
    }
    public void UpdateAllFactions()
    {
        // 세력 정보 업데이트 로직 (예: allFactions 리스트 변경)
        // ...

        // 외교 상태도 갱신
        InitializeDiplomacy();
    }

    public void InitializeFactionDropdown()
    {
        List<string> options = new List<string>();
        foreach (Faction faction in allFactions)
        {
            options.Add(faction.factionName);
        }
        factionDropdown.ClearOptions();
        factionDropdown.AddOptions(options);
        factionDropdown.onValueChanged.AddListener(value => SetPlayerFaction(factionDropdown));

    }

    // 플레이어 세력 설정
    public void SetPlayerFaction(Dropdown dropdown)
    {
        string selectedFactionName = dropdown.options[dropdown.value].text;
        foreach (Faction faction in allFactions)
        {
            if (faction.factionName == selectedFactionName)
            {
                playerFaction = faction;
                //generalManager.userGenerals = generalManager.GetGeneralsByFaction(selectedFactionName);
                //generalListData.InitializeWithPlayerGenerals(generalManager.userGenerals);
                break;
            }
        }
    }
    public Faction GetPlayerFaction()
    {
        return playerFaction;
    }
    public Faction GetFactionByRegionName(string regionName)
    {
        if (regionToFactionMap.ContainsKey(regionName))
        {
            return regionToFactionMap[regionName];
        }
        return null;
    }
    public void ConfirmFactionSelection()
    {
        Debug.Log("플레이어 세력이 " + playerFaction.factionName + "으로 확정되었습니다.");
        // 플레이어 세력의 일반들을 UI에 표시
        //generalListController.PrepareGeneralUI();
        // 플레이어의 세력을 확정
        SetPlayerFaction(factionDropdown);
        // 패널을 비활성화하여 화면에서 제거
        factionSelectionPanel.SetActive(false);
        playerFactionSelected = true;
        generalManager.InitGeneral();
    }
    private void InitializeDiplomacy()
    {
        if (diplomacyManager != null)
        {
            diplomacyManager.InitializeDiplomacy(allFactions);
        }
    }
    void UpdatePlayerUI()
    {
        if (turnText != null)
        {
            turnText.text = "Turn: " + currentTurn;
        }

        if (goldText != null && playerFaction != null)
        {
            goldText.text = "Gold: " + playerFaction.gold; // 플레이어 세력의 금을 표시
        }
    }
    
    
}