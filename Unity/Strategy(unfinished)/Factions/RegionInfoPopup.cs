using UnityEngine;
using UnityEngine.UI;

public class RegionInfoPopup : MonoBehaviour
{
    public GameObject popupPanel; // 팝업창 Panel
    public Text regionNameText; // 지역 이름을 표시할 Text
    public Text factionNameText; // 세력 이름을 표시할 Text
    public Text popultaionText;

    public GameObject FactionButtonLayout;
    public GameObject RegionButtonLayout;
    public GameObject warDeclareButton; // 전쟁 선포 버튼
    public GameObject warButton; // 전쟁 버튼
    public GameObject truceButton; // 휴전 신청 버튼
    public GameObject allyButton; // 동맹 맺기 버튼
    public GameObject allyBreakButton; // 동맹 파기 버튼

    public GameObject buildBarrackButton;
    public GameObject buildMineButton;
    public GameObject TalentHuntButton;
    public GameObject landmarkBuildButton;
    private DiplomacyManager diplomacyManager;
    private FactionManager factionManager;
    private RegionManager regionManager;
    

    void Start()
    {
        // DiplomacyManager의 인스턴스를 가져옴.
        diplomacyManager = FindObjectOfType<DiplomacyManager>();
        factionManager = FindObjectOfType<FactionManager>();
        regionManager = FindObjectOfType<RegionManager>();

        // 버튼에 리스너 추가
        buildBarrackButton.GetComponent<Button>().onClick.AddListener(() => UpgradeBuilding("Barrack"));
        buildMineButton.GetComponent<Button>().onClick.AddListener(() => UpgradeBuilding("Mine"));
        landmarkBuildButton.GetComponent<Button>().onClick.AddListener(() => UpgradeBuilding("Landmark"));

        HidePopup();
    }

    // 팝업창을 보여주는 함수
    public void ShowPopup(string regionName, string factionName, int popultaionNum, string playerFactionName)
    {
        popupPanel.SetActive(true);
        regionNameText.text = "지역명 : "+ regionName;
        factionNameText.text = "세력 명 : "+ factionName;
        popultaionText.text = "인구 : " + popultaionNum;

        DiplomacyStatus status = diplomacyManager.GetDiplomacyStatus(factionName, playerFactionName);
        
        Faction regionFaction = regionManager.GetRegionFaction(regionName);
        Faction playerFaction = factionManager.GetPlayerFaction();
        Region currentRegion = regionManager.GetRegionByName(regionName); // 지역 정보를 가져옵니다.
        regionManager.selectedRegion = currentRegion;
        // 플레이어가 소유한 세력의 지역을 클릭한 경우 버튼을 보이지 않게 합니다.
        if(regionFaction == playerFaction)
        {
            FactionButtonLayout.SetActive(false);
            RegionButtonLayout.SetActive(true);
            landmarkBuildButton.SetActive(currentRegion.canBuildLandmark);
            return;
        }
        else
        {
            RegionButtonLayout.SetActive(false);
            FactionButtonLayout.SetActive(true);
            warDeclareButton.SetActive(status == DiplomacyStatus.Neutral);
            warButton.SetActive(status == DiplomacyStatus.War);
            truceButton.SetActive(status == DiplomacyStatus.War);
            allyButton.SetActive(status == DiplomacyStatus.Neutral);
            allyBreakButton.SetActive(status == DiplomacyStatus.Ally);
        }
        // 버튼 상태 업데이트
        
    }
    

    // 팝업창을 숨기는 함수
    public void HidePopup()
    {
        regionManager.selectedRegion = null;
        popupPanel.SetActive(false);
    }

    public void DeclareWar()
    {
        // 우호도 -100 고정시키기.. 만들어야 AI행동에 편하겟지..?
        string factionName = factionNameText.text.Split(' ')[3];
        string playerFactionName = factionManager.GetPlayerFaction().factionName;
        diplomacyManager.UpdateDiplomacyStatus(factionName, playerFactionName, DiplomacyStatus.War);
        HidePopup();
    }
    
    public void MakeTruce()
    {
        // 위와 동일하게 구현
        string factionName = factionNameText.text.Split(' ')[3];
        string playerFactionName = factionManager.GetPlayerFaction().factionName;
        diplomacyManager.UpdateDiplomacyStatus(factionName, playerFactionName, DiplomacyStatus.Neutral);
        HidePopup();
    }

    public void MakeAlly()
    {
        // 동맹 턴수, 조건 등 띄우는 창 만들어야함
        // 위와 동일하게 구현
        string factionName = factionNameText.text.Split(' ')[3];
        string playerFactionName = factionManager.GetPlayerFaction().factionName;
        diplomacyManager.UpdateDiplomacyStatus(factionName, playerFactionName, DiplomacyStatus.Ally);
        HidePopup();
    }

    public void BreakAlly()
    {
        // 파기하면 우호도가 떨어지도록
        // 위와 동일하게 구현
        string factionName = factionNameText.text.Split(' ')[3];
        string playerFactionName = factionManager.GetPlayerFaction().factionName;
        diplomacyManager.UpdateDiplomacyStatus(factionName, playerFactionName, DiplomacyStatus.Neutral);
        HidePopup();
    }

    public void UpgradeBuilding(string buildingType)
    {
        string regionName = regionNameText.text.Split(' ')[2]; // 지역 이름 추출
        Region region = regionManager.GetRegionByName(regionName); // 지역 정보 가져오기

        if (buildingType == "Barrack")
        {
            // Barrack 업그레이드 로직
            if(region.barracksLevel >= region.maxBarracksLevel)
            {
                Debug.Log("최대 레벨입니다.");
                return;
            }
            regionManager.UpgradeBuilding(region, "Barrack");
        }
        else if (buildingType == "Mine")
        {
            // Mine 업그레이드 로직
            if (region.mineLevel >= region.maxMineLevel)
            {
                Debug.Log("최대 레벨입니다.");
                return;
            }
            regionManager.UpgradeBuilding(region, "Mine");
        }
        else if (buildingType == "Landmark")
        {
            // Landmark 업그레이드 로직
            if (region.landmarkBuildingLevel >= region.landmarkBuildingMaxLevel)
            {
                Debug.Log("최대 레벨입니다.");
                return;
            }
            regionManager.UpgradeBuilding(region, "Landmark");
        }
    }
}