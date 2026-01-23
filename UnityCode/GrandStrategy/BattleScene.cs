using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BattleScene : BaseScene
{
    private GameObject battleMap;
    private List<GameObject> scenarioUIElements = new List<GameObject>();
    private Camera scenarioCamera;

    private List<Transform> playerSpawnPoints = new List<Transform>();
    private List<Transform> enemySpawnPoints = new List<Transform>();

    public LayerMask clickable;
    public LayerMask ground;
    public GameObject groundMarker;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        SceneType = Define.EScene.BattleScene;

        // ScenarioScene의 카메라 비활성화
        scenarioCamera = SceneManager.GetSceneByName("ScenarioScene")
            .GetRootGameObjects()
            .SelectMany(go => go.GetComponentsInChildren<Camera>())
            .FirstOrDefault();
        AudioListener scenarioAudioListener = scenarioCamera.GetComponent<AudioListener>();
        if (scenarioCamera != null)
            scenarioCamera.gameObject.SetActive(false);
        
        Managers.Battle.InitializeBattle();

        // 맵 생성 (우선 기본 맵으로)
        Managers.Map.LoadBattleMap("BattleMap1");
        battleMap = Managers.Map.BattleMap;

        // 그 후 카메라 생성(맵 외곽 제한을 위해)
        CameraController camera = Camera.main.GetOrAddComponent<CameraController>();
        camera.Init();

        clickable = LayerMask.GetMask("GeneralUnit");
        ground = LayerMask.GetMask("Ground");

        groundMarker = Managers.Resource.Instantiate("GroundMarker");
        if (groundMarker != null)
        {
            groundMarker.SetActive(false); // 처음에는 비활성화
        }

        InitializeSpawnPoints();

        SpawnMyUnits();
        SpawnEnemyUnits();

        

        UI_SelectBoxPopup selectionBox = Managers.UI.ShowBaseUI<UI_SelectBoxPopup>();
        
        

        return true;
    }
    private void InitializeSpawnPoints() // 맵에 따라 출전 유닛 수 제한이 있을수있으니 임시용
    {
        if (battleMap == null)
        {
            Debug.LogError("Battle map is not loaded!");
            return;
        }
        
        // SpawnPositions 찾기
        Transform spawnPositions = battleMap.transform.Find("SpawnPositions");
        if (spawnPositions == null)
        {
            Debug.LogError("SpawnPositions not found in battle map!");
            return;
        }

        // PlayerGenerals의 스폰 포인트 찾기
        Transform playerGenerals = spawnPositions.Find("PlayerGenerals");
        if (playerGenerals != null)
        {
            for (int i = 1; i <= 5; i++)
            {
                Transform spawnPoint = playerGenerals.Find($"Spawn{i}");
                if (spawnPoint != null)
                {
                    playerSpawnPoints.Add(spawnPoint);
                    Debug.Log($"Found player spawn point {i} at {spawnPoint.position}");
                }
                else
                {
                    Debug.LogError($"Player Spawn{i} not found!");
                }
            }
        }
        else
        {
            Debug.LogError("PlayerGenerals not found in SpawnPositions!");
        }

        // EnemyGenerals의 스폰 포인트 찾기
        Transform enemyGenerals = spawnPositions.Find("EnemyGenerals");
        if (enemyGenerals != null)
        {
            for (int i = 6; i <= 10; i++)
            {
                Transform spawnPoint = enemyGenerals.Find($"Spawn{i}");
                if (spawnPoint != null)
                {
                    enemySpawnPoints.Add(spawnPoint);
                    Debug.Log($"Found enemy spawn point {i} at {spawnPoint.position}");
                }
                else
                {
                    Debug.LogError($"Enemy Spawn{i} not found!");
                }
            }
        }
        else
        {
            Debug.LogError("EnemyGenerals not found in SpawnPositions!");
        }
    }
    // 나중에 유닛 스폰에 사용할 메서드들
    public Transform GetPlayerSpawnPoint(int index)
    {
        if (index < 0 || index >= playerSpawnPoints.Count)
            return null;
        return playerSpawnPoints[index];
    }

    public Transform GetEnemySpawnPoint(int index)
    {
        if (index < 0 || index >= enemySpawnPoints.Count)
            return null;
        return enemySpawnPoints[index];
    }

    public override void Clear()
    {

        //전투종료후 시나리오씬으로 돌아오면서 호출해야함.
        // 소환한 유닛들 제거, 맵 제거, 스폰 포인트 제거
        playerSpawnPoints.Clear();
        enemySpawnPoints.Clear();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("왼쪽 클릭");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, clickable);

            if (hit.collider != null)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    Managers.Battle.MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    Debug.Log("클릭한 오브젝트: " + hit.collider.gameObject.name);
                    Managers.Battle.SelectByClicking(hit.collider.gameObject);
                }
                
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift)==false)
                {
                    Managers.Battle.DeselectAll();
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log($"우클릭 감지, 선택된 유닛 수: {Managers.Battle.unitSelected.Count}");
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);     
            // 선택된 유닛이 있을 때만 마커 표시
            if (Managers.Battle.unitSelected.Count > 0)
            {
                Debug.Log("유닛 선택됨, 마커 표시 시도");
                groundMarker.transform.position = mousePosition;
                groundMarker.SetActive(false);
                groundMarker.SetActive(true);
                Debug.Log($"마커 위치: {groundMarker.transform.position}");
            }

            // 이동 명령
            Debug.Log("이동 명령 시도");
            Managers.Battle.MoveSelectedUnits(mousePosition);
        }
    }
    private void SpawnMyUnits()
    {
        if (playerSpawnPoints.Count == 0 || Managers.Battle.myGenerals.Count == 0)
            return;
        for (int i = 0; i < Managers.Battle.myGenerals.Count && i < playerSpawnPoints.Count; i++)
        {
            Vector3 spawnPos = GetPlayerSpawnPoint(i).position;
            GeneralUnit playerGeneral = Managers.Object.Spawn<GeneralUnit>(spawnPos,
                               Managers.Battle.myGenerals[i].GeneralID);
            playerGeneral.Team = Define.ETeamType.My;

            if (playerGeneral != null)
            {
                Debug.Log($"Successfully spawned player general: {Managers.Battle.myGenerals[i].GeneralName}");
            }
        }
    }
    private void SpawnEnemyUnits()
    {
        if (enemySpawnPoints.Count == 0 || Managers.Battle.enemyGenerals.Count == 0)
            return;

        for (int i = 0; i < Managers.Battle.enemyGenerals.Count && i < enemySpawnPoints.Count; i++)
        {
            Vector3 spawnPos = GetEnemySpawnPoint(i).position;
            GeneralUnit enemyGeneral = Managers.Object.Spawn<GeneralUnit>(spawnPos,
                Managers.Battle.enemyGenerals[i].GeneralID);
            enemyGeneral.Team = Define.ETeamType.Enemy;

            if (enemyGeneral != null)
            {
                Debug.Log($"Successfully spawned enemy general: {Managers.Battle.enemyGenerals[i].GeneralName}");
            }
        }
    }
    
}
