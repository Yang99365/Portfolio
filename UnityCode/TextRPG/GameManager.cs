using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using static Define;
using System.Collections.Generic;


public class GameManager
{
    #region Save Data
    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public string lastSaveTime;

        // 플레이어 데이터
        public PlayerSaveData playerData;

        // 진행 상태
        public int currentNodeId = 1;               // 현재 스토리 노드
        public int completedNodesCount = 0;         // 완료한 노드 수

        // 재화
        public int gold = 0;

        // 인벤토리
        public InventorySaveData inventory;

        // 파티 (동료)
        public PartySaveData party;

        // 플래그 (스토리 진행 상태)
        public SerializableDictionary<string, bool> storyFlags = new SerializableDictionary<string, bool>();

        [NonSerialized]
        public int currentBattleEnemyId = 1;

        [NonSerialized]
        public bool lastBattleVictory = false;

        [NonSerialized]
        public List<Data.RewardData> lastBattleRewards = null;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int characterId = 1;                 // 플레이어 캐릭터 ID
        public string playerName = "모험가";
        public int level = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        // 현재 스탯
        public int currentHp = 50;
        public int maxHp = 50;
        public int currentMp = 20;
        public int maxMp = 20;

        // 성장 스탯
        public int strength = 5;
        public int intelligence = 5;
        public int agility = 5;
        public int charisma = 5;

        // 사용 가능한 포인트
        public int statPoints = 0;
        public int skillPoints = 0;

        // 장착 장비 (아이템 ID)
        public int equippedWeapon = 0;
        public int equippedArmor = 0;
        public int equippedAccessory = 0;

        // 습득한 스킬 (스킬 ID 리스트)
        public SerializableList<int> learnedSkills = new SerializableList<int>();

        // 장착한 스킬 (최대 4개)
        public SerializableList<int> equippedSkills = new SerializableList<int>();
    }

    [Serializable]
    public class InventorySaveData
    {
        public SerializableList<ItemSlot> items = new SerializableList<ItemSlot>();
    }

    [Serializable]
    public class ItemSlot
    {
        public int itemId;
        public int count;
    }

    [Serializable]
    public class PartySaveData
    {
        public SerializableList<int> companionIds = new SerializableList<int>();
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue> { }

    [Serializable]
    public class SerializableList<T> : System.Collections.Generic.List<T> { }
    #endregion

    #region Properties
    private GameSaveData _saveData;
    public GameSaveData SaveData
    {
        get
        {
            if (_saveData == null)
                _saveData = new GameSaveData();
            return _saveData;
        }
        set => _saveData = value;
    }

    private EGameState _gameState = EGameState.Title;
    public EGameState GameState
    {
        get => _gameState;
        set
        {
            if (_gameState != value)
            {
                EGameState prev = _gameState;
                _gameState = value;
                OnGameStateChanged?.Invoke(prev, value);
            }
        }
    }

    public int Gold
    {
        get => SaveData.gold;
        set
        {
            if (SaveData.gold != value)
            {
                SaveData.gold = value;
                OnGoldChanged?.Invoke(value);
            }
        }
    }
    #endregion

    #region Events
    public event Action<EGameState, EGameState> OnGameStateChanged;
    public event Action<int> OnGoldChanged;
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    #endregion

    #region Constants
    private const string SAVE_FILE_NAME = "savegame.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    #endregion

    #region Initialization
    public void Init()
    {
        Debug.Log("GameManager Initialized");

        // 자동 저장 설정
        if (Managers.Data.ConfigData?.gameplay?.enableAutoSave == true)
        {
            // TODO: 자동 저장 코루틴 시작
        }
    }
    #endregion

    #region Save/Load System


    public void SaveGame()
    {
        try
        {
            SaveData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(SaveData, settings);
            File.WriteAllText(SavePath, json);

            OnGameSaved?.Invoke();
            Debug.Log($"Game Saved: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }


    public void LoadGame()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("No save file found. Creating new game.");
                NewGame();
                return;
            }

            string json = File.ReadAllText(SavePath);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            SaveData = JsonConvert.DeserializeObject<GameSaveData>(json, settings);

            if (!ValidateSaveData())
            {
                Debug.LogWarning("Save data is invalid. Creating new game.");
                NewGame();
                return;
            }

            OnGameLoaded?.Invoke();
            Debug.Log($"Game Loaded: Level {SaveData.playerData.level}, Node {SaveData.currentNodeId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            NewGame();
        }
    }


    public void NewGame()
    {
        SaveData = new GameSaveData
        {
            saveVersion = 1,
            lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            currentNodeId = 1,                      // 시작 노드
            completedNodesCount = 0,
            gold = 100,                             // 시작 골드

            playerData = new PlayerSaveData
            {
                characterId = 1,
                playerName = "모험가",
                level = 1,
                currentExp = 0,
                expToNextLevel = 100,

                currentHp = 50,
                maxHp = 50,
                currentMp = 20,
                maxMp = 20,

                strength = 5,
                intelligence = 5,
                agility = 5,
                charisma = 5,

                statPoints = 0,
                skillPoints = 0,

                equippedWeapon = 0,
                equippedArmor = 0,
                equippedAccessory = 0,

                learnedSkills = new SerializableList<int> { 1, 2 },    // 기본 스킬 2개
                equippedSkills = new SerializableList<int> { 1, 2 }    // 기본 스킬 장착
            },

            inventory = new InventorySaveData
            {
                items = new SerializableList<ItemSlot>
                {
                    //new ItemSlot { itemId = 1, count = 3 }  // 체력 포션 3개
                }
            },

            party = new PartySaveData
            {
                companionIds = new SerializableList<int>()  // 시작 시 동료 없음
            },

            storyFlags = new SerializableDictionary<string, bool>()
        };

        Debug.Log("New Game Created!");
    }

    public bool HasSaveFile()
    {
        return File.Exists(SavePath);
    }


    public void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    private bool ValidateSaveData()
    {
        if (SaveData == null)
            return false;

        if (SaveData.playerData == null)
            SaveData.playerData = new PlayerSaveData();

        if (SaveData.inventory == null)
            SaveData.inventory = new InventorySaveData();

        if (SaveData.party == null)
            SaveData.party = new PartySaveData();

        if (SaveData.storyFlags == null)
            SaveData.storyFlags = new SerializableDictionary<string, bool>();

        return true;
    }

    #endregion

    #region Game Flow

    public void StartGame()
    {
        GameState = EGameState.Story;

        // 스토리 매니저 초기화
        if (HasSaveFile())
        {
            LoadGame();
            Managers.Story.LoadNode(SaveData.currentNodeId);
        }
        else
        {
            NewGame();
            Managers.Story.StartStory();
        }
    }


    public void ReturnToTitle()
    {
        SaveGame();
        GameState = EGameState.Title;
        Managers.Scene.LoadScene(EScene.Title);
    }

    public void QuitGame()
    {
        SaveGame();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Story Flags

    public void SetStoryFlag(string flagKey, bool value)
    {
        if (SaveData.storyFlags.ContainsKey(flagKey))
            SaveData.storyFlags[flagKey] = value;
        else
            SaveData.storyFlags.Add(flagKey, value);
    }


    public bool GetStoryFlag(string flagKey)
    {
        return SaveData.storyFlags.ContainsKey(flagKey) && SaveData.storyFlags[flagKey];
    }

    #endregion

    #region Gold Management

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        Gold += amount;
        Debug.Log($"Gold +{amount} (Total: {Gold})");
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        Debug.Log($"Gold -{amount} (Total: {Gold})");
        return true;
    }

    #endregion

    #region Helpers

    public PlayerSaveData PlayerData => SaveData.playerData;

    public int CurrentNodeId
    {
        get => SaveData.currentNodeId;
        set => SaveData.currentNodeId = value;
    }

    #endregion
}