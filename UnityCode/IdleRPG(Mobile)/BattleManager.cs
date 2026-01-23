using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class BattleManager
{
    #region Battle State
    private EBattleState _battleState = EBattleState.None;
    public EBattleState BattleState
    {
        get => _battleState;
        private set
        {
            if (_battleState != value)
            {
                EBattleState prev = _battleState;
                _battleState = value;
                OnBattleStateChanged?.Invoke(prev, value);
            }
        }
    }
    #endregion

    #region Battle Data
    // 전투 중인 영웅들 (슬롯 인덱스, 영웅)
    private Dictionary<int, Hero> _battleHeroes = new Dictionary<int, Hero>();

    // 전투 중인 몬스터들 (슬롯 인덱스, 몬스터)
    private Dictionary<int, Monster> _battleMonsters = new Dictionary<int, Monster>();

    // 현재 스테이지 정보
    private Data.StageData _currentStageData;
    private int _currentStageNumber = 1;
    private bool _isBossStage = false;

    // 전투 통계
    private int _totalDamageDealt = 0;
    private int _totalDamageTaken = 0;
    private float _battleStartTime = 0f;

    // 전투 코루틴
    private Coroutine _battleCoroutine;
    private Coroutine _stageProgressCoroutine;

    #endregion

    #region Battle Slots Configuration
    public const int MAX_HERO_SLOTS = 4;
    public const int MAX_MONSTER_SLOTS = 4;

    // 슬롯 위치 (GameScene에서 설정)
    private Transform[] _heroSlotTransforms = new Transform[MAX_HERO_SLOTS];
    private Transform[] _monsterSlotTransforms = new Transform[MAX_MONSTER_SLOTS];

    private GameScene _currentScene;
    #endregion

    #region Properties
    public int CurrentStageNumber => _currentStageNumber;
    public bool IsBossStage => _isBossStage;
    public Data.StageData CurrentStageData => _currentStageData;
    public float BattleDuration => Time.time - _battleStartTime;

    // 살아있는 유닛들
    public List<Hero> GetAllAliveHeroes() => _battleHeroes.Values.Where(h => h != null && !h.IsDead).ToList();
    public List<Monster> GetAllAliveMonsters() => _battleMonsters.Values.Where(m => m != null && !m.IsDead).ToList();

    // 전투 진행 여부
    public bool IsInBattle => BattleState == EBattleState.Battle;
    public bool CanDeployHeroes => BattleState == EBattleState.Start;
    #endregion

    #region Events
    public event Action<EBattleState, EBattleState> OnBattleStateChanged;
    public event Action OnMonstersSpawned;
    public event Action<bool> OnBattleEnd; // true: victory, false: defeat
    public event Action<Hero, Monster, float> OnHeroDealDamage;
    public event Action<Monster, Hero, float> OnMonsterDealDamage;
    public event Action<Hero> OnHeroDeath;
    public event Action<Monster> OnMonsterDeath;
    public event Action<int> OnStageProgress; // 현재 스테이지 번호
    public event Action<int> OnStageCleared; // 클리어한 스테이지 번호
    public event Action<int> OnStageFailed; // 실패한 스테이지 번호
    #endregion

    #region Initialization
    // BattleManager 초기화
    public void Init()
    {
        Debug.Log("BattleManager Initialized");

        // 저장된 진행도 로드
        LoadProgress();

        // 이벤트 등록
        RegisterEvents();
    }

    public void SetCurrentScene(GameScene scene)
    {
        _currentScene = scene;
    }

    // 슬롯 트랜스폼 설정 (GameScene에서 호출)
    public void SetSlotTransforms(Transform[] heroSlots, Transform[] monsterSlots)
    {
        if (heroSlots != null && heroSlots.Length <= MAX_HERO_SLOTS)
        {
            for (int i = 0; i < heroSlots.Length; i++)
            {
                _heroSlotTransforms[i] = heroSlots[i];
            }
        }

        if (monsterSlots != null && monsterSlots.Length <= MAX_MONSTER_SLOTS)
        {
            for (int i = 0; i < monsterSlots.Length; i++)
            {
                _monsterSlotTransforms[i] = monsterSlots[i];
            }
        }
    }

    // 진행도 로드
    private void LoadProgress()
    {
        _currentStageNumber = Managers.Game.SaveData.currentStage;
    }

    // 이벤트 등록
    private void RegisterEvents()
    {
        // HeroManager 이벤트
        //Managers.Hero.OnHeroDeployed += (hero, slot) => Debug.Log($"Hero deployed to slot {slot}");
        //Managers.Hero.OnHeroUndeployed += (hero, slot) => Debug.Log($"Hero removed from slot {slot}");

    }
    #endregion

    #region Hero Management (Through HeroManager)
    // 영웅 배치 (프로토타입용 - null saveData 전달)
    public bool DeployHero(int heroTemplateId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_HERO_SLOTS)
        {
            Debug.LogError($"Invalid slot index: {slotIndex}");
            return false;
        }

        // 전투 중에는 배치 변경 불가
        if (IsInBattle)
        {
            Debug.LogWarning("Cannot deploy hero during battle");
            return false;
        }

        // 기존 영웅이 있으면 제거
        if (_battleHeroes.ContainsKey(slotIndex))
        {
            RemoveHeroFromSlot(slotIndex);
        }

        // 프로토타입: 임시 세이브 데이터 생성 (HeroManager에 추가)
        var existingSave = Managers.Game.SaveData.Heroes.Find(h => h.templateId == heroTemplateId);
        if (existingSave == null)
        {
            existingSave = new HeroSaveData
            {
                templateId = heroTemplateId,
                level = 1,
                exp = 0,
                slotIndex = slotIndex,
                isUnlocked = true
            };
            Managers.Game.SaveData.Heroes.Add(existingSave);
        }

        // HeroManager를 통해 영웅 배치
        var hero = Managers.Hero.DeployHero(heroTemplateId, slotIndex, _heroSlotTransforms[slotIndex]);
        if (hero == null)
        {
            Debug.LogError($"Failed to deploy hero {heroTemplateId}");
            return false;
        }

        // 전투 딕셔너리에 추가
        _battleHeroes[slotIndex] = hero;

        // 이벤트 등록
        hero.OnDeath += OnHeroDeathHandler;
        hero.OnDealDamage += (damage) => OnHeroDealDamage?.Invoke(hero, hero.Target as Monster, damage);

        Debug.Log($"Hero {hero.HeroData.characterName} deployed to battle slot {slotIndex}");
        return true;
    }

    // 영웅 제거
    public void RemoveHeroFromSlot(int slotIndex)
    {
        if (_battleHeroes.TryGetValue(slotIndex, out var hero))
        {
            // 이벤트 해제
            hero.OnDeath -= OnHeroDeathHandler;

            // HeroManager를 통해 제거
            Managers.Hero.UndeployHero(slotIndex);

            // 전투 딕셔너리에서 제거
            _battleHeroes.Remove(slotIndex);
        }
    }

    // 모든 영웅 회복
    private void RestoreAllHeroes()
    {
        foreach (var hero in _battleHeroes.Values)
        {
            if (hero != null)
            {
                hero.FullRestore();
            }
        }
    }

    // 영웅 사망 핸들러
    private void OnHeroDeathHandler(Creature creature)
    {
        var hero = creature as Hero;
        if (hero != null)
        {
            OnHeroDeath?.Invoke(hero);

            // 모든 영웅이 죽었는지 체크
            if (GetAllAliveHeroes().Count == 0)
            {
                // 패배
                OnBattleDefeat();
            }
        }
    }
    #endregion

    #region Monster Management
    // 스테이지에 맞는 몬스터 스폰
    private void SpawnStageMonsters()
    {
        // 기존 몬스터 모두 제거
        ClearAllMonsters();

        // 스테이지 데이터 로드
        if (!Managers.Data.StageDataDict.TryGetValue(_currentStageNumber, out _currentStageData))
        {
            Debug.LogError($"Stage data not found for stage {_currentStageNumber}");
            return;
        }

        // 일반 몬스터인지 보스인지 판단
        _isBossStage = (_currentStageNumber % 10 == 0); // 10스테이지마다 보스

        if (_isBossStage)
        {
            // 보스 몬스터 스폰
            SpawnBossMonster();
        }
        else
        {
            // 일반 몬스터 스폰
            SpawnNormalMonsters();
        }

        OnMonstersSpawned?.Invoke();
    }

    // 일반 몬스터 스폰
    private void SpawnNormalMonsters()
    {
        if (_currentStageData == null) return;

        // normalMonsterId가 콤마로 구분된 경우 처리
        string[] monsterIds = _currentStageData.normalMonsterId.Split(',');
        int monstersPerType = _currentStageData.normalMonsterCount / monsterIds.Length;

        int slotIndex = 0;
        foreach (string monsterId in monsterIds)
        {
            if (int.TryParse(monsterId.Trim(), out int id))
            {
                for (int i = 0; i < monstersPerType && slotIndex < MAX_MONSTER_SLOTS; i++)
                {
                    SpawnMonster(id, slotIndex);
                    slotIndex++;
                }
            }
        }
    }

    // 보스 몬스터 스폰
    private void SpawnBossMonster()
    {
        if (_currentStageData == null) return;

        if (int.TryParse(_currentStageData.bossMonsterId, out int bossId))
        {
            // 보스는 중앙 슬롯에 스폰 (슬롯 1 or 1,2)
            SpawnMonster(bossId, 1);
        }
    }

    // 개별 몬스터 스폰
    private void SpawnMonster(int monsterId, int slotIndex)
    {
        if (slotIndex >= MAX_MONSTER_SLOTS) return;

        Monster monster = Managers.Object.Spawn<Monster>(_monsterSlotTransforms[slotIndex].position, monsterId);
        if (monster == null)
        {
            Debug.LogError($"Failed to spawn monster {monsterId}");
            return;
        }

        // 몬스터 정보 설정
        monster.SetMonsterInfo(monsterId, _currentStageNumber, slotIndex);
        monster.transform.SetParent(_monsterSlotTransforms[slotIndex]);
        monster.transform.localPosition = Vector3.zero;

        // 전투 딕셔너리에 추가
        _battleMonsters[slotIndex] = monster;

        // 이벤트 등록
        monster.OnDeath += OnMonsterDeathHandler;
        monster.OnDealDamage += (damage) => OnMonsterDealDamage?.Invoke(monster, monster.Target as Hero, damage);
        monster.OnMonsterKilled += (gold, exp) => DistributeRewards(gold, exp);

        Debug.Log($"Monster {monster.MonsterData.monsterName} spawned at slot {slotIndex}");
    }

    // 모든 몬스터 제거
    private void ClearAllMonsters()
    {
        foreach (var monster in _battleMonsters.Values)
        {
            if (monster != null)
            {
                monster.OnDeath -= OnMonsterDeathHandler;
                Managers.Object.Despawn(monster);
            }
        }
        _battleMonsters.Clear();
    }

    // 몬스터 사망 핸들러
    private void OnMonsterDeathHandler(Creature creature)
    {
        var monster = creature as Monster;
        if (monster != null)
        {
            OnMonsterDeath?.Invoke(monster);

            // 슬롯에서 제거
            var slot = _battleMonsters.FirstOrDefault(x => x.Value == monster).Key;
            _battleMonsters.Remove(slot);

            // 모든 몬스터가 죽었는지 체크
            if (GetAllAliveMonsters().Count == 0)
            {
                ClearAllHeroTargets();
                // 승리
                OnBattleVictory();
            }
        }
    }
    private void ClearAllHeroTargets()
    {
        foreach (var hero in _battleHeroes.Values)
        {
            if (hero != null)
            {
                hero.SetTarget(null);
            }
        }
    }
    // 보상 분배
    private void DistributeRewards(int gold, int exp)
    {
        // 골드는 이미 Monster에서 GameManager에 추가됨

        // 경험치는 살아있는 영웅들에게 분배
        var aliveHeroes = GetAllAliveHeroes();
        if (aliveHeroes.Count > 0)
        {
            int expPerHero = exp / aliveHeroes.Count;
            foreach (var hero in aliveHeroes)
            {
                hero.GainExperience(expPerHero);
            }
        }
    }
    #endregion

    #region Battle Flow
    // 전투 시작
    public void StartBattle()
    {
        if (BattleState == EBattleState.Battle)
        {
            Debug.LogWarning("Battle already in progress");
            return;
        }

        // 배치된 영웅이 있는지 확인
        if (_battleHeroes.Count == 0)
        {
            Debug.LogError("No heroes deployed!");
            return;
        }

        BattleState = EBattleState.Start;

        if (_battleCoroutine != null)
        {
            _currentScene.StopCoroutine(_battleCoroutine);
        }
        _battleCoroutine = _currentScene.StartCoroutine(BattleRoutine());
    }

    // 전투 루틴
    private IEnumerator BattleRoutine()
    {
        // 전투 시작 준비
        yield return PrepareForBattle();

        // 몬스터 스폰
        SpawnStageMonsters();
        yield return new WaitForSeconds(2.5f); // 스폰 연출

        // 전투 상태로 전환
        BattleState = EBattleState.Battle;
        _battleStartTime = Time.time;

        StartAllCreatureAI();

        // 전투 진행 (자동 전투이므로 대기)
        yield return WaitForBattleEnd();

        _battleCoroutine = null;
    }

    // 전투 준비
    private IEnumerator PrepareForBattle()
    {
        Debug.Log($"Preparing for Stage {_currentStageNumber}");

        // 영웅들 준비
        RestoreAllHeroes();

        // UI 업데이트
        OnStageProgress?.Invoke(_currentStageNumber);


        yield return null;
    }

    // 전투 종료 대기
    private IEnumerator WaitForBattleEnd()
    {
        // 전투가 끝날 때까지 대기
        while (BattleState == EBattleState.Battle)
        {
            // 전투 상황 체크는 OnMonsterDeathHandler와 OnHeroDeathHandler에서 처리
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion

    #region Stage Progression
    // 전투 승리
    private void OnBattleVictory()
    {
        StopAllCreatureAI();
        BattleState = EBattleState.Victory;

        Debug.Log($"Stage {_currentStageNumber} Cleared! Battle Duration: {BattleDuration:F1}s");

        // 스테이지 클리어 이벤트
        OnStageCleared?.Invoke(_currentStageNumber);
        OnBattleEnd?.Invoke(true);

        // 다음 스테이지로
        _currentStageNumber++;
        Managers.Game.SaveData.currentStage = _currentStageNumber;

        // 자동 진행
        if (_stageProgressCoroutine != null)
        {
            _currentScene.StopCoroutine(_stageProgressCoroutine);
        }
        _stageProgressCoroutine = _currentScene.StartCoroutine(AutoProgressToNextStage());
    }

    // 전투 패배
    private void OnBattleDefeat()
    {
        StopAllCreatureAI();
        BattleState = EBattleState.Defeat;

        Debug.Log($"Stage {_currentStageNumber} Failed! Battle Duration: {BattleDuration:F1}s");

        // 스테이지 실패 이벤트
        OnStageFailed?.Invoke(_currentStageNumber);
        OnBattleEnd?.Invoke(false);

        // 이전 스테이지로 (파밍)
        if (_currentStageNumber > 1)
        {
            _currentStageNumber--;
            Managers.Game.SaveData.currentStage = _currentStageNumber;
        }

        // 자동 재시작
        if (_stageProgressCoroutine != null)
        {
            _currentScene.StopCoroutine(_stageProgressCoroutine);
        }
        _stageProgressCoroutine = _currentScene.StartCoroutine(AutoRestartStage());
    }

    // 다음 스테이지 자동 진행
    private IEnumerator AutoProgressToNextStage()
    {
        // 전리품 획득 시간
        yield return new WaitForSeconds(2f);

        // 영웅 회복
        RestoreAllHeroes();

        // 잠시 대기
        yield return new WaitForSeconds(1f);

        // 다음 전투 시작
        StartBattle();

        _stageProgressCoroutine = null;
    }

    // 스테이지 자동 재시작
    private IEnumerator AutoRestartStage()
    {
        // 패배 연출
        yield return new WaitForSeconds(2f);

        // 영웅 회복
        RestoreAllHeroes();

        // 잠시 대기
        yield return new WaitForSeconds(1f);

        // 전투 재시작
        StartBattle();

        _stageProgressCoroutine = null;
    }

    #endregion

    #region Public Methods
    // 전투 일시정지
    public void PauseBattle()
    {
        if (BattleState == EBattleState.Battle)
        {
            BattleState = EBattleState.Pause;
            Time.timeScale = 0f;
        }
    }

    // 전투 재개
    public void ResumeBattle()
    {
        if (BattleState == EBattleState.Pause)
        {
            BattleState = EBattleState.Battle;
            Time.timeScale = 1f;
        }
    }

    // 전투 중단
    public void StopBattle()
    {
        if (_battleCoroutine != null)
        {
            _currentScene.StopCoroutine(_battleCoroutine);
            _battleCoroutine = null;
        }

        if (_stageProgressCoroutine != null)
        {
            _currentScene.StopCoroutine(_stageProgressCoroutine);
            _stageProgressCoroutine = null;
        }
        StopAllCreatureAI();

        BattleState = EBattleState.None;
        Time.timeScale = 1f;
    }

    // 수동으로 다음 스테이지
    public void MoveToNextStage()
    {
        _currentStageNumber++;
        Managers.Game.SaveData.currentStage = _currentStageNumber;
        StartBattle();
    }

    // 스테이지 선택
    public void SelectStage(int stageNumber)
    {
        if (stageNumber > 0 && stageNumber <= Managers.Data.StageDataDict.Count)
        {
            _currentStageNumber = stageNumber;
            Managers.Game.SaveData.currentStage = _currentStageNumber;
            StartBattle();
        }
    }

    // 전투 통계 리셋
    public void ResetBattleStats()
    {
        _totalDamageDealt = 0;
        _totalDamageTaken = 0;
    }
    private void StartAllCreatureAI()
    {
        // 모든 영웅 AI 시작
        foreach (var hero in _battleHeroes.Values)
        {
            if (hero != null && !hero.IsDead)
            {
                hero.StartAI();
            }
        }

        // 모든 몬스터 AI 시작
        foreach (var monster in _battleMonsters.Values)
        {
            if (monster != null && !monster.IsDead)
            {
                monster.StartAI();
            }
        }

        Debug.Log($"All creature AI started! Heroes: {_battleHeroes.Count}, Monsters: {_battleMonsters.Count}");
    }
    private void StopAllCreatureAI()
    {
        foreach (var hero in _battleHeroes.Values)
        {
            if (hero != null)
            {
                hero.StopAI();
            }
        }

        foreach (var monster in _battleMonsters.Values)
        {
            if (monster != null)
            {
                monster.StopAI();
            }
        }

        Debug.Log("All creature AI stopped");
    }
    #endregion

    #region Cleanup
    public void Clear()
    {
        StopBattle();
        ClearAllMonsters();

        foreach (var hero in _battleHeroes.Values)
        {
            if (hero != null)
            {
                hero.OnDeath -= OnHeroDeathHandler;
            }
        }

        _battleHeroes.Clear();
        _battleMonsters.Clear();
    }
    #endregion
}