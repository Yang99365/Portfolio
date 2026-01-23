using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;
using Newtonsoft.Json;

#region SaveData
[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;  // 나중에 마이그레이션용
    public int currentStage = 1;
    //Currecy
    public int Gold = 0;
    public int Gem = 0;

    public List<HeroSaveData> Heroes = new List<HeroSaveData>();
    public MasterySaveData Mastery = new MasterySaveData();
    public int ItemInstanceGenerator = 1;
    public int SkillInstanceGenerator = 1;
    public List<ItemSaveData> Items = new List<ItemSaveData>();
    public List<SkillSaveData> Skills = new List<SkillSaveData>();

    public int masteryAttackLevel = 0;
    public int masteryDefenseLevel = 0;
    public int masteryMaxHpLevel = 0;
    public int masteryAttackSpeedLevel = 0;
    public int masteryCritChanceLevel = 0;
    public int masteryCritDamageLevel = 0;
}
[System.Serializable]
public class HeroSaveData
{
    public int templateId;
    public int level = 1;
    public int exp = 0;
    public int slotIndex = -1; // -1 is not in party
    public bool isUnlocked = false;

    public int weaponId = 0;
    public int armorId = 0;
    public int accessoryId = 0;

    public List<SkillSaveData> skills = new List<SkillSaveData>();
}
[System.Serializable]
public class ItemSaveData
{
    public int instanceId;
    public int templateId;
    public int count = 1;
    public int equipSlot = -1; // -1 : inventory, 0 ~ : InstanceId of the hero who is equipped
}
public class SkillSaveData
{
    public int instanceId;
    public int skillId; // templateId
    public int level = 1;
    public int equipSlot = -1; // -1 : inventory, 0 ~ : InstanceId of the hero who is equipped
}
public class MasterySaveData
{
    public int attackLevel = 0;
    public int defenseLevel = 0;
    public int maxHpLevel = 0;
    public int attackSpeedLevel = 0;
    public int critChanceLevel = 0;
    public int critDamageLevel = 0;
}
#endregion
public class GameManager
{

    GameSaveData _saveData = new GameSaveData();
    public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }
    
    public bool isGameOver = false;
    public event Action<bool> OnGameEnd;

    #region Game State
    private EGameState _gameState = EGameState.None;
    public EGameState GameState
    {
        get => _gameState;
        private set
        {
            if (_gameState != value)
            {
                EGameState prev = _gameState;
                _gameState = value;
                OnGameStateChanged?.Invoke(prev, value);
            }
        }
    }

    public int CurrentStage
    {
        get => SaveData.currentStage;
        private set
        {
            if (SaveData.currentStage != value)
            {
                SaveData.currentStage = value;
                OnStageChanged?.Invoke(value);
            }
        }
    }
    #endregion
    #region ID Generators
    // 아이템 인스턴스 ID 생성
    public int GenerateItemInstanceId()
    {
        return SaveData.ItemInstanceGenerator++;
    }

    // 스킬 인스턴스 ID 생성
    public int GenerateSkillInstanceId()
    {
        return SaveData.SkillInstanceGenerator++;
    }
    #endregion
    #region Currency
    public int Gold
    {
        get => SaveData.Gold;
        set
        {
            if (SaveData.Gold != value)
            {
                SaveData.Gold = value;
                OnCurrencyChanged?.Invoke(ECurrencyType.Gold, value);
            }
        }
    }
    public int Gem
    {
        get => SaveData.Gem;
        set
        {
            if (SaveData.Gem != value)
            {
                SaveData.Gem = value;
                OnCurrencyChanged?.Invoke(ECurrencyType.Gem, value);
            }
        }
    }
    public void AddCurrency(ECurrencyType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("AddCurrency: amount cannot be negative");
            return;
        }

        switch (type)
        {
            case ECurrencyType.Gold:
                Gold += amount;
                break;
            case ECurrencyType.Gem:
                Gem += amount;
                break;
            default:
                Debug.LogError("AddCurrency: unknown currency type");
                break;
        }
    }
    public bool SpendCurrency(ECurrencyType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("SpendCurrency: amount cannot be negative");
            return false;
        }

        switch (type)
        {
            case ECurrencyType.Gold:
                if (Gold >= amount)
                {
                    Gold -= amount;
                    return true;
                }
                break;
            case ECurrencyType.Gem:
                if (Gem >= amount)
                {
                    Gem -= amount;
                    return true;
                }
                break;
            default:
                Debug.LogError("SpendCurrency: unknown currency type");
                break;
        }

        Debug.LogWarning($"SpendCurrency: not enough {type}");
        return false;
    }


    #endregion
    #region Stage Management
    public void CompleteStage()
    {
        CurrentStage++;

        // 스테이지 클리어 보상
        GiveStageReward();

        GameState = EGameState.Victory;
    }

    private void GiveStageReward()
    {
        // 기본 보상
        int goldReward = CurrentStage * 100;
        //AddCurrency(ECurrencyType.Gold, goldReward);

        // 추가 보상 (아이템 등)
        if (Random.Range(0f, 1f) < 0.3f) // 30% 확률
        {
            GiveRandomItem();
        }

        OnStageCompleted?.Invoke(CurrentStage - 1, goldReward);
    }

    private void GiveRandomItem()
    {
        // 랜덤 아이템 지급 로직
        var equipmentIds = Managers.Data.EquipmentDic.Keys.ToList();
        if (equipmentIds.Count > 0)
        {
            int randomId = equipmentIds[Random.Range(0, equipmentIds.Count)];
            //CreateItem(randomId); 인벤매니저
        }
    }

    public void RestartStage()
    {
        GameState = EGameState.InGame;
        OnStageRestarted?.Invoke(CurrentStage);
    }

    public void ReturnToMainMenu()
    {
        GameState = EGameState.MainMenu;
    }
    #endregion
    #region Battle Management
    public void StartBattle()
    {
        GameState = EGameState.InGame;
        OnBattleStarted?.Invoke();
    }

    public void EndBattle(bool victory)
    {
        if (victory)
        {
            CompleteStage();
        }
        else
        {
            GameState = EGameState.GameOver;
            OnBattleLost?.Invoke();
        }
    }
    #endregion
    #region Item Management
    // 아이템과 같이 인벤토리 매니저로 장비관리
    #endregion
    #region Hero Management
    // 히어로 매니저로 따로 관리
    #endregion

    #region Save/Load System
    private const string SAVE_FILE_NAME = "savegame.json";
    private string SavePath => System.IO.Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);


    /// <summary>
    /// 게임 저장
    /// </summary>
    public void SaveGame()
    {
        try
        {
            // 배치된 영웅 정보 업데이트
            UpdateDeployedHeroesData();

            // JSON 직렬화 (Newtonsoft.Json 사용)
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto, // 다형성 지원
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(SaveData, settings);

            // 파일 저장
            System.IO.File.WriteAllText(SavePath, json);

            OnGameSaved?.Invoke();
            Debug.Log($"Game saved successfully at {SavePath}");
            Debug.Log($"Saved {SaveData.Heroes.Count} heroes, {SaveData.Items.Count} items, {SaveData.Skills.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 게임 로드
    /// </summary>
    public void LoadGame()
    {
        try
        {
            if (!System.IO.File.Exists(SavePath))
            {
                Debug.Log("No save file found. Creating new game.");
                NewGame();
                return;
            }

            string json = System.IO.File.ReadAllText(SavePath);

            // JSON 역직렬화 (Newtonsoft.Json 사용)
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto, // 다형성 지원
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            SaveData = JsonConvert.DeserializeObject<GameSaveData>(json, settings);

            // 데이터 유효성 검증
            if (!ValidateSaveData())
            {
                Debug.LogWarning("Save data is invalid. Creating new game.");
                NewGame();
                return;
            }

            OnGameLoaded?.Invoke();
            Debug.Log($"Game loaded successfully! Stage: {SaveData.currentStage}, Gold: {SaveData.Gold}");
            Debug.Log($"Loaded {SaveData.Heroes.Count} heroes, {SaveData.Items.Count} items, {SaveData.Skills.Count} skills");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}\n{e.StackTrace}");
            NewGame();
        }
    }

    /// <summary>
    /// 새 게임 시작
    /// </summary>
    public void NewGame()
    {
        SaveData = new GameSaveData
        {
            saveVersion = 1,
            currentStage = 1,
            Gold = 1000,
            Gem = 100,
            Heroes = new List<HeroSaveData>(),
            Items = new List<ItemSaveData>(),
            Skills = new List<SkillSaveData>(),
            ItemInstanceGenerator = 1,
            SkillInstanceGenerator = 1,
            masteryAttackLevel = 0,
            masteryDefenseLevel = 0,
            masteryMaxHpLevel = 0,
            masteryAttackSpeedLevel = 0,
            masteryCritChanceLevel = 0,
            masteryCritDamageLevel = 0
        };

        // 기본 영웅 추가 (Knight)
        SaveData.Heroes.Add(new HeroSaveData
        {
            templateId = 1001,
            level = 1,
            exp = 0,
            slotIndex = 0, // 아직 배치 안 함
            isUnlocked = true,
            weaponId = 0,
            armorId = 0,
            accessoryId = 0,
            skills = new List<SkillSaveData>()
        });

        OnNewGameStarted?.Invoke();
        Debug.Log("New game created!");
    }

    /// <summary>
    /// 세이브 파일 삭제
    /// </summary>
    public void DeleteSave()
    {
        try
        {
            if (System.IO.File.Exists(SavePath))
            {
                System.IO.File.Delete(SavePath);
                OnSaveDeleted?.Invoke();
                Debug.Log("Save file deleted!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    /// <summary>
    /// 세이브 파일 존재 여부
    /// </summary>
    public bool HasSaveFile()
    {
        return System.IO.File.Exists(SavePath);
    }

    /// <summary>
    /// 배치된 영웅 데이터 업데이트
    /// </summary>
    private void UpdateDeployedHeroesData()
    {
        foreach (var heroSave in SaveData.Heroes)
        {
            if (heroSave.slotIndex >= 0)
            {
                var hero = Managers.Hero.GetHeroByTemplateId(heroSave.templateId);
                if (hero != null)
                {
                    heroSave.level = hero.Level;
                    heroSave.exp = hero.Experience;
                }
            }
        }
    }

    /// <summary>
    /// 세이브 데이터 유효성 검증
    /// </summary>
    private bool ValidateSaveData()
    {
        if (SaveData == null)
            return false;

        if (SaveData.Heroes == null)
            SaveData.Heroes = new List<HeroSaveData>();

        if (SaveData.Items == null)
            SaveData.Items = new List<ItemSaveData>();

        if (SaveData.Skills == null)
            SaveData.Skills = new List<SkillSaveData>();

        if (SaveData.currentStage < 1)
            SaveData.currentStage = 1;

        if (SaveData.Gold < 0)
            SaveData.Gold = 0;

        if (SaveData.Gem < 0)
            SaveData.Gem = 0;

        return true;
    }
    #endregion


    #region Events
    // Game State Events
    public Action<EGameState, EGameState> OnGameStateChanged;
    public Action<int> OnStageChanged;
    public Action OnGameSaved;
    public Action OnGameLoaded;
    public Action OnNewGameStarted;
    public Action OnSaveDeleted;

    // Currency Events
    public Action<ECurrencyType, int> OnCurrencyChanged;

    // Battle Events
    public Action OnBattleStarted;
    public Action OnBattleLost;
    public Action<int, int> OnStageCompleted;
    public Action<int> OnStageRestarted;
    #endregion
}
