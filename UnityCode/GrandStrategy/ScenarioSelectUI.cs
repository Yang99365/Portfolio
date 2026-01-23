using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Define;

public class ScenarioSelectUI : MonoBehaviour
{
    public Image mapPreview;

    public GameObject selectScenarioPrefab;
    public Transform selectScenarioContent;

    public GameObject factionTogglePrefab;
    public Transform factionToggleContent;

    public ScenarioMapData selectedScenario { get; private set; }

    public int selectedFactionId { get; private set; }

    private Dictionary<int, Data.ScenarioMapData> _scenarioMapDataDic;
    //private List<GameObject> activeScenarioButtons = new List<GameObject>(); // 풀링용
    //private List<GameObject> activeFactionToggles = new List<GameObject>(); // 풀링용

    public void Start()
    {
        _scenarioMapDataDic = Managers.Data.ScenarioMapDic;

        CreateSelectScenarioPrefab();

    }
    void OnDisable()
    {
        mapPreview.gameObject.SetActive(false);
        foreach (Transform child in factionToggleContent)
        {
            Destroy(child.gameObject);
        }
    }
    void CreateSelectScenarioPrefab()
    {
        //기존 버튼 제거
        /*
        foreach (Transform child in selectScenarioContent)
        {
            Destroy(child.gameObject);
        }
        */
        foreach (var scenario in _scenarioMapDataDic)
        {
            Debug.Log($"Creating button for scenario: {scenario.Value.scenarioName}");

            GameObject scenarioPrefab = Instantiate(selectScenarioPrefab, selectScenarioContent);
            Button button = scenarioPrefab.GetComponent<Button>();
            Text buttonText = scenarioPrefab.GetComponentInChildren<Text>();
            Image buttonImage = scenarioPrefab.GetComponent<Image>();

            buttonText.text = scenario.Value.scenarioName;
            buttonImage.sprite = Managers.Resource.Load<Sprite>(scenario.Value.viewMapTexture);
            button.onClick.AddListener(() => ShowScenarioPreview(scenario.Key));


        }
        /* 풀링으로 하려했으나 스택방식이라 그런지 다시부를떄마다 순서가 뒤바뀜
        foreach (var scenario in _scenarioMapDataDic)
        {
            Debug.Log($"Creating button for scenario: {scenario.Value.scenarioName}");
            
            GameObject scenarioPrefab = Managers.Pool.Pop(selectScenarioPrefab);
            if (scenarioPrefab == null)
            {
                Debug.LogError($"Failed to pop button from pool for scenario: {scenario.Value.scenarioName}");
                continue;
            }
            scenarioPrefab.transform.SetParent(selectScenarioContent, false);
            Button button = scenarioPrefab.GetComponent<Button>();
            Text buttonText = scenarioPrefab.GetComponentInChildren<Text>();

            buttonText.text = scenario.Value.scenarioName;
            button.onClick.AddListener(() => ShowScenarioPreview(scenario.Key));

            //activeScenarioButtons.Add(scenarioPrefab);
        }
        */
    }
    void ShowScenarioPreview(int scenarioId) // 시나리오맵프리펩 클릭시
    {
        selectedScenario = _scenarioMapDataDic[scenarioId];
        Debug.Log($"Selected scenario: {selectedScenario.scenarioName}");
        if (mapPreview.gameObject.activeSelf == false)
        {
            mapPreview.gameObject.SetActive(true);
        }

        // 미리보기 맵 업데이트
        StartCoroutine(LoadAndSetSprite(selectedScenario.colorMapTexture));

        // 플레이 가능한 세력 토글 생성
        UpdateFactionToggles(selectedScenario.playableFactionIds);

        // 여기에 추가로 지역 정보를 표시하는 로직을 구현할 수 있습니다.
        // 예: DisplayRegionInfo(selectedScenario.regionIds);
    }

    IEnumerator LoadAndSetSprite(string addressablePath)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(addressablePath);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (mapPreview.sprite != null)
            {
                Addressables.Release(mapPreview.sprite);
            }
            mapPreview.sprite = handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load sprite: {addressablePath}");
        }
    }
    

    void UpdateFactionToggles(List<int> factionIds) // 토글그룹 대충만들어서 최대3개만 만들어야함
    {
        // 기존 토글 제거

        foreach (Transform child in factionToggleContent)
        {
            Destroy(child.gameObject);
        }


        ToggleGroup toggleGroup = factionToggleContent.GetComponent<ToggleGroup>();

        // 새 토글 생성
        foreach (int factionId in factionIds)
        {
            GameObject toggleObj = Instantiate(factionTogglePrefab, factionToggleContent);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text toggleText = toggleObj.GetComponentInChildren<Text>();
            toggle.group = toggleGroup; // 토글 그룹 할당

            if (Managers.Data.FactionDic.TryGetValue(factionId, out FactionData factionData))
            {
                toggleText.text = factionData.factionName; // 세력 이름 설정
                Debug.Log($"Setting toggle for {factionData.factionName}");
            }
            else
            {
                toggleText.text = "Unknown Faction"; // 데이터 미발견 처리
            }

            toggle.isOn = false;
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bool isOn) => OnFactionToggled(factionId, isOn));

        }

        /* 풀링쓰려햇는데 스택이라 그런지 토글 세력이름 텍스트가 후입선출로 나와선지 이상함
        // 기존 토글을 풀에 반환
        foreach (GameObject toggle in activeFactionToggles)
        {
            Debug.Log($"Returning toggle: {toggle.GetComponentInChildren<Text>().text}");
            Managers.Pool.Push(toggle);
        }
        activeFactionToggles.Clear();

        ToggleGroup toggleGroup = factionToggleContent.GetComponent<ToggleGroup>();
        // 새 토글 생성
        foreach (int factionId in factionIds)
        {
            GameObject toggleObj = Managers.Pool.Pop(factionTogglePrefab);

            toggleObj.transform.SetParent(factionToggleContent, false);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.group = toggleGroup; // 토글 그룹 할당
            //toggle.isOn = false;
            Text toggleText = toggleObj.GetComponentInChildren<Text>();

            // 여기서 factionId를 사용하여 실제 faction 이름을 가져와야 합니다.
            if (Managers.Data.FactionDic.TryGetValue(factionId, out FactionData factionData))
            {
                toggleText.text = factionData.factionName; // 세력 이름 설정
                Debug.Log($"Setting toggle for {factionData.factionName}");
            }
            else
            {
                toggleText.text = "Unknown Faction"; // 데이터 미발견 처리
            }
            
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((bool isOn) => OnFactionToggled(factionId, isOn));

            activeFactionToggles.Add(toggleObj);
        }
        */
    }

    void OnFactionToggled(int factionId, bool isOn)
    {
        if (isOn)
        {
            Debug.Log($"Selected faction ID: {factionId}");
            // 여기에 선택된 세력 처리 로직 추가
            selectedFactionId = factionId;
        }

    }

    void OnDestroy()
    {
        /*
        // 모든 활성 버튼과 토글을 풀에 반환
        foreach (GameObject button in activeScenarioButtons)
        {
            Managers.Pool.Push(button);
        }
        
        foreach (GameObject toggle in activeFactionToggles)
        {
            Managers.Pool.Push(toggle);
        }
        */
    }

    public void CleanUp() // public으로 왜했지
    {

        /*
        // 모든 활성 버튼과 토글을 풀에 반환
        foreach (GameObject button in activeScenarioButtons)
        {
            Managers.Pool.Push(button);
        }
        activeScenarioButtons.Clear();

        
        foreach (GameObject toggle in activeFactionToggles)
        {
            Managers.Pool.Push(toggle);
        }
        activeFactionToggles.Clear();
        */
    }
    public void CloseUI()
    {
        CleanUp();
        // UI를 비활성화하거나 파괴하는 추가 로직
    }

    public void OnStartButtonClicked()
    {
        if (selectedScenario != null && selectedFactionId != 0)
        {
            Managers.Game.selectedScenario = selectedScenario;
            Managers.Game.selectedFactionId = selectedFactionId;
            //SceneManager.LoadScene("ScenarioScene");
            Managers.Scene.LoadScene(EScene.ScenarioScene);
        }
        else
        {
            Debug.LogError("No scenario selected!");
        }

    }
}