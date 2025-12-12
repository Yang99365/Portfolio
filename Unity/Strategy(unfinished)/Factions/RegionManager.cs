using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RegionManager : MonoBehaviour
{
    public List<Region> allRegions;
    public Region selectedRegion;
    public Image mapImage;

    public Texture2D colorMapTexture;
    public Texture2D viewMapTexture;
    private Texture2D colorMapTextureCopy;
    [SerializeField]
    private FactionManager factionManager;
    [SerializeField]
    private RegionInfoPopup infoPopup;
    [SerializeField]
    private Dictionary<Color, string> colorToRegionMap = new Dictionary<Color, string>();

    public static RegionManager instance;
    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            infoPopup = FindObjectOfType<RegionInfoPopup>();
            mapImage = GameObject.Find("Map").GetComponent<Image>();
           
        }
        else
        {
            infoPopup = null;
            mapImage = null;
        }
            
    }
    private void Awake()
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
        

        factionManager = FindObjectOfType<FactionManager>();
        infoPopup = FindObjectOfType<RegionInfoPopup>();

        InitializeColorToRegionMap();

        //mapImage = GetComponent<Image>();
        Material mat = mapImage.material;
        // 이미지 컴포넌트 및 스프라이트에서 Texture2D 얻기

        colorMapTexture = mat.GetTexture("_ColorMap") as Texture2D;
        viewMapTexture = mat.GetTexture("_MainTex") as Texture2D;
        colorMapTextureCopy = new Texture2D(colorMapTexture.width, colorMapTexture.height);
        colorMapTextureCopy.SetPixels(colorMapTexture.GetPixels());
        colorMapTextureCopy.Apply();
    }
    void Start()
    {
        UpdateRegionColorsAccordingToFactions();
        
    }
    
    public void OnClickRegion()
    {
        if(factionManager.playerFactionSelected == false)
        {
            return;
        }
        Debug.Log("Clicked on Map");
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapImage.GetComponent<RectTransform>(),
            Input.mousePosition,
            null,
            out localPoint
        );
        
        

        Rect r = mapImage.GetComponent<RectTransform>().rect;
        
        Vector2 uv = new Vector2((localPoint.x - r.x) / r.width, (localPoint.y - r.y) / r.height);
        Color regionColor = colorMapTexture.GetPixel(Mathf.FloorToInt(uv.x * colorMapTexture.width),
                                                     Mathf.FloorToInt(uv.y * colorMapTexture.height));
        
        Debug.Log("Clicked on Map at UV: " + uv + ", Color: " + regionColor);
        // 투명한 영역(빈 공간)인 경우 메서드 종료
        if (regionColor == Color.clear)
        {
            Debug.Log("Clicked on Empty Space");
            infoPopup.HidePopup();
            return;
        }

        string regionName = GetRegionName(regionColor);
        
        if (regionName == "UnknownRegion")
        {
            Debug.Log("Region Name not found");
            return; // 지역 이름을 찾지 못한 경우 메서드 종료
        }

        Faction owningFaction = GetRegionFaction(regionName);

        string factionName = owningFaction != null ? owningFaction.factionName : "No Owning Faction";
        Debug.Log("Clicked on Region: " + regionName + ", Owned by Faction: " + factionName);
        int populationNum = GetRegionByName(regionName).population;
        
        if (infoPopup != null)
        {
            infoPopup.ShowPopup(regionName, factionName, populationNum, factionManager.playerFaction.factionName);
        }
    }
    


    private void InitializeColorToRegionMap()
    {
        Dictionary<string, int> initialPopulationMap = new Dictionary<string, int>()
        {
            { "Region1", 100 },
            { "Region2", 150 },
            { "Region3", 200 }
        };
        
        Dictionary<string, string> landmarkMap = new Dictionary<string, string>()
        {
            { "Region1", "GreatCastle" },
            { "Region3", "MajesticMine" }
        };
        TextAsset textAsset = Resources.Load<TextAsset>("regions");
        string[] lines = textAsset.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrEmpty(line)) continue; // 빈 라인은 스킵

            string[] tokens = line.Split(',');
            string regionName = tokens[0];
            byte r = byte.Parse(tokens[1]);
            byte g = byte.Parse(tokens[2]);
            byte b = byte.Parse(tokens[3]);
            byte a = byte.Parse(tokens[4]);

            Color32 color = new Color32(r, g, b, a);

            int initialPopulation; // 초기 인구(인구가 없는 경우 defaultPopulation 사용)
            int defaultPopulation = 100;
            colorToRegionMap[color] = regionName;

            if (initialPopulationMap.TryGetValue(regionName, out initialPopulation))
            {
                string landmarkName = null;
                landmarkMap.TryGetValue(regionName, out landmarkName);  // 랜드마크가 있는지 확인

                allRegions.Add(new Region(color, regionName, initialPopulation, landmarkName));
            }
            else
            {
                string landmarkName = null;
                landmarkMap.TryGetValue(regionName, out landmarkName);
                allRegions.Add(new Region(color, regionName, defaultPopulation)); // 인구가 없는 경우 defaultPopulation 사용
            }
            
        }

    }

    public Region GetRegionByName(string regionName)
    {
        foreach (Region region in allRegions)
        {
            if (region.regionName == regionName)
            {
                return region;
            }
        }
        return null;
    }
    private string GetRegionName(Color regionColor)
    {
        foreach (KeyValuePair<Color, string> entry in colorToRegionMap)
        {
            if (IsColorEqual(entry.Key, regionColor))
            {
                return entry.Value;
            }
        }

        return "UnknownRegion";
    }
    private void ReplaceRegionColor(Color[] pixels, Color oldColor, Color newColor)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsColorEqual(pixels[i], oldColor))
            {
                pixels[i] = newColor;
            }
        }
    }
    private bool IsColorEqual(Color a, Color b)
    {
        return a == b;
        //return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);
        //return Vector4.Distance(a, b) < 0.01f;
    }
    
    public void UpdateRegionColorsAccordingToFactions()
    {
        Color[] colorPixels = colorMapTextureCopy.GetPixels();
        foreach (Faction faction in factionManager.allFactions) // 모든 세력에 대해서
        {
            foreach (string regionName in faction.controlledRegions) // 세력이 통제하는 각 지역에 대해서
            {
                Color regionColor = GetRegionColorByName(regionName); // 이 함수는 지역 이름으로 colorToRegionMap에서 색을 찾습니다.
                if (regionColor != null)
                {
                    Color newColor = faction.factionColor;
                    ReplaceRegionColor(colorPixels, regionColor, newColor);
                }
            }
        }
        Debug.Log("Updated Region Colors");
        colorMapTextureCopy.SetPixels(colorPixels);
        colorMapTextureCopy.Apply();
        viewMapTexture.SetPixels(colorPixels);
        viewMapTexture.Apply();
    }
    private Color GetRegionColorByName(string regionName)
    {
        foreach (KeyValuePair<Color, string> entry in colorToRegionMap)
        {
            if (entry.Value == regionName)
            {
                return entry.Key;
            }
        }
        return Color.clear; // 지역 이름에 해당하는 색이 없을 경우 Color.clear 반환
    }
    public Faction GetRegionFaction(string regionName)
    {
        foreach (Faction faction in factionManager.allFactions)
        {
            if (faction.controlledRegions.Contains(regionName))
            {
                return faction;
            }
            
        }

        return null; // No matching faction found
    }

    //외부에서 colorToRegionMap 불러오기
    public Dictionary<Color, string> GetColorToRegionMap()
    {
        return colorToRegionMap;
    }
    public void EndTurn()
    {
        // 1. 플레이어 세력 업데이트 로직
        // 예: factionManager.UpdatePlayerFaction();

        // 2. 다른 세력들의 행동을 처리
        // 예: factionManager.UpdateOtherFactions();

        // 3. 지역 업데이트
        UpdateRegions();
    }
    void UpdateRegions()
    {
        foreach (Region region in allRegions)
        {
            region.population += 10;  // 인구 증가 로직, 원하시는 대로 수정 가능
        }
    }
    
    /*public void OnAttackButtonPressed()
    {
        string targetRegionName = "Region1"; // 예시입니다. 실제로는 UI나 로직에 따라 결정될 것입니다.
        Faction attackingFaction = 

        factionManager.ChangeRegionOwnership(targetRegionName, attackingFaction);

        // 지역의 색깔을 업데이트
        UpdateRegionColorsAccordingToFactions();
    }
    */
    public void UpgradeBuilding(Region region, string buildingType)
    {
        // 건물 업그레이드 로직
        if (buildingType == "Barrack")
        {
            region.barracksLevel++;
        }
        else if (buildingType == "Mine")
        {
            region.mineLevel++;
        }
        else if (buildingType == "Landmark")
        {
            region.landmarkBuildingLevel++;
        }
    }
}