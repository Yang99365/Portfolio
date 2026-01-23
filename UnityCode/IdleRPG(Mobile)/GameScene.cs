using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
    #region Object Transform

    [Header("Battle Slots")]
    [SerializeField]
    private Transform[] _heroSlotTransforms = new Transform[4];

    [SerializeField]
    private Transform[] _monsterSlotTransforms = new Transform[4];

    private bool _isInitialized = false;

    private UI_GameScene _gameUI;
    #endregion

    #region Unity Lifecycle
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.GameScene;

        // Scene UI 생성
        _gameUI = Managers.UI.ShowSceneUI<UI_GameScene>();

        // 슬롯 Transform 검증
        StartCoroutine(InitializeGame());

        return true;
    }

    private IEnumerator InitializeGame()
    {
        Debug.Log("=== Game Initialization Start ===");

        // 1. 게임 로드 (세이브 파일이 없으면 NewGame()이 자동 호출됨)
        Managers.Game.LoadGame();

        // 프레임 대기 (Inspector에서 할당된 Transform이 준비될 때까지)
        yield return null;

        // 2. 슬롯 검증
        if (!ValidateSlots())
        {
            Debug.LogError("Slot transforms not properly assigned!");
            yield break;
        }

        // 3. BattleManager 초기화
        InitializeBattleManager();

        // 4. ★ SaveData에서 영웅 배치 복원 ★
        RestoreHeroesFromSaveData();

        // ★ 5. UI 갱신 (여기서 팝업도 강제 갱신) ★
        yield return new WaitForEndOfFrame(); // UI가 완전히 준비될 때까지 대기

        _gameUI.SetInfo();
        RefreshAllPopups();

        // 6. 잠시 대기 후 전투 시작
        yield return new WaitForSeconds(1f);

        Debug.Log("Starting battle...");
        Managers.Battle.StartBattle();

        // 7. 마스터리 적용
        if (Managers.Mastery != null)
        {
            Managers.Mastery.ApplyMasteryToAllHeroes();
            Debug.Log("Applied mastery bonuses on game start");
        }

        _isInitialized = true;


        Debug.Log("=== Game Initialization Complete ===");
    }
    private void RefreshAllPopups()
    {
        // GameScene UI의 현재 열린 팝업 갱신
        if (_gameUI != null)
        {
            _gameUI.ForceRefreshCurrentPopup();
        }
    }
    private void RestoreHeroesFromSaveData()
    {
        var saveData = Managers.Game.SaveData;
        if (saveData?.Heroes == null)
        {
            Debug.LogWarning("No hero data to restore!");
            return;
        }

        // SaveData에서 배치된 영웅들만 필터링 (slotIndex >= 0)
        var deployedHeroes = saveData.Heroes
            .Where(h => h.isUnlocked && h.slotIndex >= 0)
            .OrderBy(h => h.slotIndex)
            .ToList();

        if (deployedHeroes.Count == 0)
        {
            Debug.LogWarning("No heroes are deployed in save data!");
            return;
        }

        Debug.Log($"Restoring {deployedHeroes.Count} heroes from save data...");

        // 배치된 영웅들을 전장에 복원
        foreach (var heroSave in deployedHeroes)
        {
            bool deployed = Managers.Battle.DeployHero(heroSave.templateId, heroSave.slotIndex);
            if (deployed)
            {
                Debug.Log($"Restored hero {heroSave.templateId} to slot {heroSave.slotIndex}");
            }
            else
            {
                Debug.LogError($"Failed to restore hero {heroSave.templateId} to slot {heroSave.slotIndex}");
            }
        }
    }
    private bool ValidateSlots()
    {
        // Hero 슬롯 체크
        if (_heroSlotTransforms == null || _heroSlotTransforms.Length == 0)
        {
            Debug.LogError("Hero slot transforms array is null or empty!");
            return false;
        }

        // Monster 슬롯 체크
        if (_monsterSlotTransforms == null || _monsterSlotTransforms.Length == 0)
        {
            Debug.LogError("Monster slot transforms array is null or empty!");
            return false;
        }

        // 최소 1개 슬롯은 할당되어야 함
        if (_heroSlotTransforms[0] == null || _monsterSlotTransforms[0] == null)
        {
            Debug.LogError("First slot transforms must be assigned!");
            return false;
        }

        Debug.Log($"Slots validated - Heroes: {_heroSlotTransforms.Length}, Monsters: {_monsterSlotTransforms.Length}");
        return true;
    }

    private void InitializeBattleManager()
    {
        Managers.Hero.Init();
        // BattleManager 초기화
        Managers.Battle.Init();
        // InventoryManager 초기화
        Managers.Inventory.Init();

        // GameScene을 BattleManager에 등록
        Managers.Battle.SetCurrentScene(this);

        // 슬롯 Transform 전달
        Managers.Battle.SetSlotTransforms(_heroSlotTransforms, _monsterSlotTransforms);

        // 이벤트 등록
        RegisterBattleEvents();

        Debug.Log("BattleManager initialized");
    }

    private void DeployTestHeroes()
    {
        // 프로토타입 테스트용 영웅 배치
        // 나중에는 SaveData에서 로드한 영웅 정보로 배치

        // 첫 번째 슬롯에 기본 영웅 배치
        if (Managers.Data.HeroDataDict.ContainsKey(1001))
        {
            bool deployed = Managers.Battle.DeployHero(1001, 0);
            if (deployed)
            {
                Debug.Log("Hero deployed to slot 0");
            }
        }

        // 추가 영웅이 있다면 배치 (최대 4명)
        int slotIndex = 1;
        int[] additionalHeroIds = { 1002, 1003, 1004 }; // Warrior, Archer, Mage

        foreach (int heroId in additionalHeroIds)
        {
            if (slotIndex >= _heroSlotTransforms.Length)
                break;

            if (Managers.Data.HeroDataDict.ContainsKey(heroId) && _heroSlotTransforms[slotIndex] != null)
            {
                // 테스트를 위해 영웅 언락
                var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == heroId);
                if (saveData == null)
                {
                    saveData = new HeroSaveData
                    {
                        templateId = heroId,
                        level = 1,
                        exp = 0,
                        slotIndex = -1,
                        isUnlocked = true
                    };
                    Managers.Game.SaveData.Heroes.Add(saveData);
                }

                bool deployed = Managers.Battle.DeployHero(heroId, slotIndex);
                if (deployed)
                {
                    Debug.Log($"Hero {heroId} deployed to slot {slotIndex}");
                }
                slotIndex++;
            }
        }
    }
    private void InitializeStartingHero()
    {
        const int STARTING_HERO_ID = 1001;

        // SaveData 확인 (이어하기 지원)
        var existingHero = Managers.Game.SaveData.Heroes.Find(h => h.templateId == STARTING_HERO_ID);

        if (existingHero == null)
        {
            // CreateNewHero로 영웅 생성 (SaveData에 저장됨)
            Hero newHero = Managers.Hero.CreateNewHero(STARTING_HERO_ID);
            // ...
        }

        // 첫 번째 슬롯에 배치
        Managers.Battle.DeployHero(STARTING_HERO_ID, 0);
    }

    private void RegisterBattleEvents()
    {
        // 전투 상태 변경 이벤트
        Managers.Battle.OnBattleStateChanged += OnBattleStateChanged;

        // 전투 종료 이벤트
        Managers.Battle.OnBattleEnd += OnBattleEnd;

        // 스테이지 진행 이벤트
        Managers.Battle.OnStageProgress += OnStageProgress;
        Managers.Battle.OnStageCleared += OnStageCleared;
        Managers.Battle.OnStageFailed += OnStageFailed;

        // 영웅/몬스터 사망 이벤트
        Managers.Battle.OnHeroDeath += OnHeroDeath;
        Managers.Battle.OnMonsterDeath += OnMonsterDeath;
    }

    private void UnregisterBattleEvents()
    {
        if (Managers.Battle != null)
        {
            Managers.Battle.OnBattleStateChanged -= OnBattleStateChanged;
            Managers.Battle.OnBattleEnd -= OnBattleEnd;
            Managers.Battle.OnStageProgress -= OnStageProgress;
            Managers.Battle.OnStageCleared -= OnStageCleared;
            Managers.Battle.OnStageFailed -= OnStageFailed;
            Managers.Battle.OnHeroDeath -= OnHeroDeath;
            Managers.Battle.OnMonsterDeath -= OnMonsterDeath;
        }
    }
    #endregion

    #region Battle Event Handlers
    private void OnBattleStateChanged(EBattleState prevState, EBattleState newState)
    {
        Debug.Log($"Battle State Changed: {prevState} -> {newState}");

        switch (newState)
        {
            case EBattleState.Start:
                Debug.Log("Battle Starting...");
                break;
            case EBattleState.Battle:
                Debug.Log("Battle In Progress!");
                break;
            case EBattleState.Victory:
                Debug.Log("Victory!");
                break;
            case EBattleState.Defeat:
                Debug.Log("Defeat...");
                break;
        }
    }

    private void OnBattleEnd(bool victory)
    {
        if (victory)
        {
            Debug.Log("Battle Won! Moving to next stage...");
        }
        else
        {
            Debug.Log("Battle Lost! Retrying stage...");
        }
    }

    private void OnStageProgress(int stageNumber)
    {
        Debug.Log($"Starting Stage {stageNumber}");
        // UI 업데이트는 나중에
    }

    private void OnStageCleared(int stageNumber)
    {
        Debug.Log($"Stage {stageNumber} Cleared!");
        // 보상 표시 등
    }

    private void OnStageFailed(int stageNumber)
    {
        Debug.Log($"Stage {stageNumber} Failed!");
    }

    private void OnHeroDeath(Hero hero)
    {
        Debug.Log($"Hero {hero.HeroData.characterName} has died!");
    }

    private void OnMonsterDeath(Monster monster)
    {
        Debug.Log($"Monster {monster.MonsterData.monsterName} defeated!");
    }
    #endregion
    

    #region Cleanup
    private void OnDestroy()
    {
        Managers.Game.SaveGame();

        UnregisterBattleEvents();
    }
    private void OnApplicationQuit()
    {
        // 앱 종료 시 저장
        Managers.Game.SaveGame();
    }
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 모바일에서 백그라운드 진입 시 저장
            Managers.Game.SaveGame();
        }
    }
    public override void Clear()
    {
        Debug.Log("Clearing Game Scene");

        // 전투 중지
        if (Managers.Battle != null)
        {
            Managers.Battle.StopBattle();
        }

        // 이벤트 해제
        UnregisterBattleEvents();
    }
    #endregion
}
