using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class StoryScene : BaseScene
{
    private UI_StoryScene _storyUI;
    private bool _isInitialized = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.Story;
        Managers.Scene.SetCurrentScene(this);

        StartCoroutine(InitializeStory());

        return true;
    }

    private IEnumerator InitializeStory()
    {
        Debug.Log("=== Story Scene Initialization Start ===");

        // 전투 결과 확인
        bool returningFromBattle = (Managers.Game.SaveData.lastBattleRewards != null);
        bool battleVictory = Managers.Game.SaveData.lastBattleVictory;
        var battleRewards = Managers.Game.SaveData.lastBattleRewards;

        // 1. Game Load or New Game
        if (Managers.Game.HasSaveFile())
        {
            Managers.Game.LoadGame();
            Debug.Log("Save file loaded");
        }
        else
        {
            Managers.Game.NewGame();
            Debug.Log("New game created");
        }

        yield return null;

        // 2. Initialize Managers
        Managers.Player.Init();
        Managers.Party.Init();
        Managers.Story.Init();
        Managers.Battle.Init();

        // 3. Show Story UI
        _storyUI = Managers.UI.ShowSceneUI<UI_StoryScene>();

        yield return new WaitForEndOfFrame();

        // 4. Register Events
        RegisterStoryEvents();

        // 5. Wait for UI setup
        yield return new WaitForSeconds(0.5f);

        // 전투에서 돌아온 경우 OnBattleEnd 호출
        if (returningFromBattle)
        {
            Debug.Log($"Returning from battle - Victory: {battleVictory}");

            // StoryManager에 전투 종료 알림
            Managers.Story.OnBattleEnd(battleVictory, battleRewards);

            // 전투 데이터 초기화
            Managers.Game.SaveData.lastBattleVictory = false;
            Managers.Game.SaveData.lastBattleRewards = null;
        }
        else
        {
            // 일반 진입
            if (Managers.Game.CurrentNodeId > 1)
            {
                Managers.Story.LoadNode(Managers.Game.CurrentNodeId);
                Debug.Log($"Continuing from Node {Managers.Game.CurrentNodeId}");
            }
            else
            {
                Managers.Story.StartStory();
                Debug.Log("Starting new story from Node 1");
            }
        }

        _isInitialized = true;

        Debug.Log("=== Story Scene Initialization Complete ===");
    }

    private void RegisterStoryEvents()
    {
        // Story node loaded
        Managers.Story.OnNodeLoaded += OnNodeLoaded;

        // Choice selected
        Managers.Story.OnChoiceSelected += OnChoiceSelected;
        Managers.Story.OnChoiceResult += OnChoiceResult;

        // Rewards gained
        Managers.Story.OnRewardsGained += OnRewardsGained;

        // Battle triggered
        Managers.Story.OnBattleTriggered += OnBattleTriggered;

        // Companion joined
        Managers.Story.OnCompanionJoined += OnCompanionJoined;

        // Story ended
        Managers.Story.OnStoryEnded += OnStoryEnded;

        Debug.Log("Story events registered");
    }

    private void UnregisterStoryEvents()
    {
        if (Managers.Story != null)
        {
            Managers.Story.OnNodeLoaded -= OnNodeLoaded;
            Managers.Story.OnChoiceSelected -= OnChoiceSelected;
            Managers.Story.OnChoiceResult -= OnChoiceResult;
            Managers.Story.OnRewardsGained -= OnRewardsGained;
            Managers.Story.OnBattleTriggered -= OnBattleTriggered;
            Managers.Story.OnCompanionJoined -= OnCompanionJoined;
            Managers.Story.OnStoryEnded -= OnStoryEnded;
        }
    }

    #region Story Event Handlers

    private void OnNodeLoaded(Data.StoryNodeData nodeData)
    {
        Debug.Log($"Node Loaded: {nodeData.nodeId} - {nodeData.nodeTitle}");

        if (_storyUI != null)
        {
            //_storyUI.DisplayNode(nodeData);
        }

        // Auto save on node change
        Managers.Game.SaveGame();
    }

    private void OnChoiceSelected(Data.ChoiceData choice)
    {
        Debug.Log($"Choice Selected: {choice.choiceText}");
    }

    private void OnChoiceResult(EChoiceResultType resultType, Data.ChoiceData choice)
    {
        Debug.Log($"Choice Result: {resultType}");

        if (_storyUI != null)
        {
            //_storyUI.ShowChoiceResult(resultType);
        }
    }

    private void OnRewardsGained(System.Collections.Generic.List<Data.RewardData> rewards)
    {
        Debug.Log($"Rewards gained: {rewards.Count} items");

        if (_storyUI != null)
        {
            //_storyUI.ShowRewards(rewards);
        }
    }

    private void OnBattleTriggered(int enemyId)
    {
        Debug.Log($"Battle triggered with enemy {enemyId}");

        // Transition to Battle Scene
        StartCoroutine(TransitionToBattle(enemyId));
    }

    private void OnCompanionJoined(int characterId)
    {
        var characterData = Managers.Data.CharacterDict.GetValueOrDefault(characterId);
        if (characterData != null)
        {
            Debug.Log($"Companion joined: {characterData.characterName}");

            if (_storyUI != null)
            {
                //_storyUI.ShowCompanionJoined(characterData);
            }
        }
    }

    private void OnStoryEnded()
    {
        Debug.Log("Story ended!");

        if (_storyUI != null)
        {
            //_storyUI.ShowEnding();
        }

        // 3초 후 타이틀로
        StartCoroutine(ReturnToTitleAfterDelay(3f));
    }

    private IEnumerator ReturnToTitleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Managers.Scene.LoadScene(EScene.Title);
    }

    #endregion

    #region Battle Transition

    private IEnumerator TransitionToBattle(int enemyId)
    {
        Debug.Log("Transitioning to battle...");

        // Fade out or transition effect
        yield return new WaitForSeconds(0.5f);

        // Load Battle Scene
        Managers.Scene.LoadScene(EScene.Battle);
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        Managers.Game.SaveGame();
        UnregisterStoryEvents();
    }

    private void OnApplicationQuit()
    {
        Managers.Game.SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Managers.Game.SaveGame();
        }
    }

    public override void Clear()
    {
        Debug.Log("Clearing Story Scene");
        UnregisterStoryEvents();
    }

    #endregion
}
