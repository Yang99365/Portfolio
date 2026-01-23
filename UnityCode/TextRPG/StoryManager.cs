using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;


// 스토리 진행 및 선택지 처리
public class StoryManager
{
    #region Properties
    private Data.StoryNodeData _currentNode;
    private Data.ChoiceData _lastSelectedChoice;

    // 현재 스토리 노드
    public Data.StoryNodeData CurrentNode => _currentNode;


    // 현재 노드 ID
    public int CurrentNodeId => _currentNode?.nodeId ?? 1;

    public bool IsStoryActive { get; private set; }
    #endregion

    #region Events
    public event Action<Data.StoryNodeData> OnNodeLoaded;
    public event Action<Data.ChoiceData> OnChoiceSelected;
    public event Action<EChoiceResultType, Data.ChoiceData> OnChoiceResult;
    public event Action<List<Data.RewardData>> OnRewardsGained;
    public event Action<int> OnBattleTriggered;     // enemyId
    public event Action<int> OnCompanionJoined;     // characterId
    public event Action OnStoryEnded;
    #endregion

    #region Initialization
    public void Init()
    {
        Debug.Log("StoryManager Initialized");
    }

    // 스토리 시작 (새 게임)
    public void StartStory()
    {
        LoadNode(1);  // 시작 노드
        IsStoryActive = true;
        Managers.Game.GameState = EGameState.Story;
    }


    // 특정 노드 로드
    public void LoadNode(int nodeId)
    {
        if (!Managers.Data.StoryNodeDict.TryGetValue(nodeId, out var nodeData))
        {
            Debug.LogError($"Story node not found: {nodeId}");
            return;
        }

        _currentNode = nodeData;

        // 노드 로드 후 즉시 CurrentNodeId 업데이트 및 저장
        Managers.Game.CurrentNodeId = nodeId;
        Managers.Game.SaveGame(); // 즉시 저장!

        // BGM 재생
        if (!string.IsNullOrEmpty(nodeData.bgmKey))
        {
            // TODO: Managers.Sound.Play(ESound.Bgm, nodeData.bgmKey);
        }

        OnNodeLoaded?.Invoke(nodeData);
        Debug.Log($"Loaded Node {nodeId}: {nodeData.nodeTitle}");

        // 이벤트 처리
        HandleNodeEvent();
    }
    #endregion

    #region Node Event Handling


    // 노드 이벤트 처리
    private void HandleNodeEvent()
    {
        if (_currentNode == null)
            return;

        switch (_currentNode.eventType)
        {
            case EStoryEventType.None:
                // 일반 텍스트, 아무것도 안 함
                break;

            case EStoryEventType.Battle:
                // 전투 발생 (선택지 선택 후 처리)
                break;

            case EStoryEventType.GetItem:
                // 아이템 즉시 획득
                GiveReward(new Data.RewardData
                {
                    rewardType = ERewardType.Item,
                    rewardId = _currentNode.eventId,
                    amount = 1
                });
                break;

            case EStoryEventType.GetCompanion:
                // 동료 즉시 영입
                AddCompanion(_currentNode.eventId);
                break;

            case EStoryEventType.LevelUp:
                // 강제 레벨업 (튜토리얼용)
                Managers.Player.GainExp(Managers.Player.ExpToNextLevel);
                break;

            case EStoryEventType.GameOver:
                // 게임오버
                HandleGameOver();
                break;

            case EStoryEventType.Ending:
                // 엔딩
                HandleEnding();
                break;
        }
    }

    #endregion

    #region Choice System

    // 선택지 선택
    public void SelectChoice(int choiceIndex)
    {
        if (_currentNode == null || _currentNode.choices == null)
        {
            Debug.LogError("No current node or choices!");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= _currentNode.choices.Count)
        {
            Debug.LogError($"Invalid choice index: {choiceIndex}");
            return;
        }

        var choice = _currentNode.choices[choiceIndex];

        // 1. 아이템 체크 (필수 아이템이 있는 경우)
        if (choice.requiredItemId > 0)
        {
            if (!Managers.Inventory.HasItem(choice.requiredItemId, choice.requiredItemCount))
            {
                Debug.Log($"Choice failed: missing item {choice.requiredItemId}");

                OnChoiceSelected?.Invoke(choice);
                OnChoiceResult?.Invoke(EChoiceResultType.Failure, choice);

                // 실패 노드로 이동
                if (choice.failureNodeId > 0)
                {
                    Debug.Log($"Moving to failure node {choice.failureNodeId}");
                    LoadNode(choice.failureNodeId);
                }
                else
                {
                    Debug.LogWarning("No failure node for item requirement!");
                }
                return;
            }
        }

        OnChoiceSelected?.Invoke(choice);

        // 2. 스탯 체크
        EChoiceResultType resultType = CheckChoiceRequirement(choice);
        OnChoiceResult?.Invoke(resultType, choice);

        // 3. 결과 처리
        ProcessChoiceResult(choice, resultType);
    }

    // 선택지 요구사항 체크
    private EChoiceResultType CheckChoiceRequirement(Data.ChoiceData choice)
    {
        // 요구 스탯이 없으면 일반 성공
        if (choice.requiredStat == EStatType.None || choice.requiredStatValue <= 0)
        {
            return EChoiceResultType.Normal;
        }

        int playerStat = Managers.Player.GetStat(choice.requiredStat);

        // 크리티컬 성공
        if (choice.criticalStatValue > 0 && playerStat >= choice.criticalStatValue)
        {
            Debug.Log($"★ Critical Success! {choice.requiredStat}: {playerStat} >= {choice.criticalStatValue}");
            return EChoiceResultType.CriticalSuccess;
        }

        // 일반 성공
        if (playerStat >= choice.requiredStatValue)
        {
            Debug.Log($"? Success! {choice.requiredStat}: {playerStat} >= {choice.requiredStatValue}");
            return EChoiceResultType.Success;
        }

        // 실패
        Debug.Log($"? Failure! {choice.requiredStat}: {playerStat} < {choice.requiredStatValue}");
        return EChoiceResultType.Failure;
    }


    // 선택지 결과 처리
    private void ProcessChoiceResult(Data.ChoiceData choice, EChoiceResultType resultType)
    {
        // 아이템 소비 (여기까지 왔다면 아이템이 있다는 뜻)
        if (choice.consumeItem && choice.requiredItemId > 0)
        {
            bool consumed = Managers.Inventory.ConsumeItem(choice.requiredItemId, choice.requiredItemCount);
            if (!consumed)
            {
                Debug.LogError("Failed to consume item! This shouldn't happen.");
                // 이미 HasItem으로 체크했으므로 여기 오면 안 됨
            }
        }

        List<Data.RewardData> rewards = null;
        int nextNodeId = choice.nextNodeId;

        // 결과에 따른 보상 및 다음 노드 결정
        switch (resultType)
        {
            case EChoiceResultType.CriticalSuccess:
            case EChoiceResultType.Success:
            case EChoiceResultType.Normal:
                rewards = choice.successRewards;
                break;

            case EChoiceResultType.Failure:
                rewards = choice.failureRewards;
                // 실패 시 다른 노드로 이동
                if (choice.failureNodeId > 0)
                {
                    nextNodeId = choice.failureNodeId;
                }
                break;
        }

        // 보상 지급
        if (rewards != null && rewards.Count > 0)
        {
            GiveRewards(rewards);
        }

        // 전투 체크 (현재 노드의 이벤트 타입)
        if (_currentNode.eventType == EStoryEventType.Battle)
        {
            // 전투 전에 선택지 저장
            _lastSelectedChoice = choice;
            StartBattle(_currentNode.eventId);
            return;  // 전투 후 OnBattleEnd에서 처리
        }

        // 일반 노드는 바로 다음 노드로 이동
        if (nextNodeId > 0)
        {
            LoadNode(nextNodeId);
        }
        else if (nextNodeId == -1)
        {
            // 엔딩
            HandleEnding();
        }
    }

    #endregion

    #region Reward System

    // 보상 지급 (여러 개)
    private void GiveRewards(List<Data.RewardData> rewards)
    {
        foreach (var reward in rewards)
        {
            GiveReward(reward);
        }

        OnRewardsGained?.Invoke(rewards);
    }


    // 보상 지급 (단일)
    private void GiveReward(Data.RewardData reward)
    {
        switch (reward.rewardType)
        {
            case ERewardType.Exp:
                Managers.Player.GainExp(reward.amount);
                Debug.Log($"Reward: Exp +{reward.amount}");
                break;

            case ERewardType.Gold:
                Managers.Game.AddGold(reward.amount);
                Debug.Log($"Reward: Gold +{reward.amount}");
                break;

            case ERewardType.Item:
                // TODO: 인벤토리 시스템 구현 후
                Debug.Log($"Reward: Item {reward.rewardId} x{reward.amount}");
                break;

            case ERewardType.StatPoint:
                Managers.Game.PlayerData.statPoints += reward.amount;
                Debug.Log($"Reward: Stat Points +{reward.amount}");
                break;

            case ERewardType.Companion:
                AddCompanion(reward.rewardId);
                break;

            case ERewardType.Skill:
                Managers.Player.LearnSkill(reward.rewardId);
                Debug.Log($"Reward: Learned Skill {reward.rewardId}");
                break;
        }
    }

    #endregion

    #region Battle Integration

    // 전투 시작
    private void StartBattle(int enemyId)
    {
        Debug.Log($"Starting battle with enemy {enemyId}");

        IsStoryActive = false;
        Managers.Game.GameState = EGameState.Battle;

        // 적 ID를 BattleScene에 전달
        Managers.Game.SaveData.currentBattleEnemyId = enemyId; // 임시 저장

        OnBattleTriggered?.Invoke(enemyId);

        // BattleScene으로 전환
        Managers.Scene.LoadScene(EScene.Battle);
    }

    // 전투 종료 콜백
    public void OnBattleEnd(bool victory, List<Data.RewardData> rewards = null)
    {
        IsStoryActive = true;
        Managers.Game.GameState = EGameState.Story;

        if (victory)
        {
            Debug.Log("Battle Victory!");

            // 보상 지급
            if (rewards != null && rewards.Count > 0)
            {
                GiveRewards(rewards);
            }

            // 다음 노드로 이동
            int nextNodeId = -1;

            if (_lastSelectedChoice != null && _lastSelectedChoice.nextNodeId > 0)
            {
                nextNodeId = _lastSelectedChoice.nextNodeId;
                _lastSelectedChoice = null;
            }
            else
            {
                // Fallback
                var choice = _currentNode?.choices?.FirstOrDefault();
                if (choice != null && choice.nextNodeId > 0)
                {
                    nextNodeId = choice.nextNodeId;
                }
            }

            if (nextNodeId > 0)
            {
                Debug.Log($"Battle won! Moving to node {nextNodeId}");
                // LoadNode가 알아서 CurrentNodeId 업데이트 & 저장
                LoadNode(nextNodeId);
            }
            else
            {
                Debug.LogWarning("No next node after battle!");
            }
        }
        else
        {
            Debug.Log("Battle Defeat!");

            if (_lastSelectedChoice != null && _lastSelectedChoice.failureNodeId > 0)
            {
                int failNodeId = _lastSelectedChoice.failureNodeId;
                LoadNode(failNodeId);
                _lastSelectedChoice = null;
            }
            else
            {
                HandleGameOver();
            }
        }
    }

    #endregion

    #region Companion System


    private void AddCompanion(int characterId)
    {
        bool success = Managers.Party.AddCompanion(characterId);

        if (success)
        {
            var characterData = Managers.Data.CharacterDict.GetValueOrDefault(characterId);
            if (characterData != null)
            {
                Debug.Log($"★ {characterData.characterName} joined the party!");
                OnCompanionJoined?.Invoke(characterId);
            }
        }
    }

    #endregion

    #region Story Flags

    // 스토리 플래그 설정
    public void SetFlag(string key, bool value)
    {
        Managers.Game.SetStoryFlag(key, value);
    }

    
    // 스토리 플래그 확인
    public bool GetFlag(string key)
    {
        return Managers.Game.GetStoryFlag(key);
    }

    #endregion

    #region Game Over & Ending


    // 게임오버 처리
    private void HandleGameOver()
    {
        Debug.Log("=== GAME OVER ===");
        IsStoryActive = false;

        // TODO: 게임오버 UI 표시
        // 타이틀로 돌아가거나 재시작 선택
    }


    // 엔딩 처리
    private void HandleEnding()
    {
        Debug.Log("=== THE END ===");
        IsStoryActive = false;

        OnStoryEnded?.Invoke();

        // 엔딩 타입에 따라 다른 처리
        // TODO: 엔딩 UI 표시
    }

    #endregion

    #region Choice Validation

    // 선택지를 선택할 수 있는지 확인 (UI용)
    public bool CanSelectChoice(Data.ChoiceData choice)
    {
        if (choice.requiredItemId > 0)
        {
            if (!Managers.Inventory.HasItem(choice.requiredItemId, choice.requiredItemCount))
            {
                return false;
            }
        }

        // 요구 스탯 확인
        if (choice.requiredStat != EStatType.None && choice.requiredStatValue > 0)
        {
            int playerStat = Managers.Player.GetStat(choice.requiredStat);
            // 요구사항을 만족하지 못해도 선택은 가능 (실패 루트)
            return true;
        }

        return true;
    }

    // 선택지 성공 확률 계산 (UI 표시용)
    public string GetChoiceSuccessInfo(Data.ChoiceData choice)
    {
        if (choice.requiredStat == EStatType.None || choice.requiredStatValue <= 0)
        {
            return "";  // 조건 없음
        }

        int playerStat = Managers.Player.GetStat(choice.requiredStat);

        if (choice.criticalStatValue > 0 && playerStat >= choice.criticalStatValue)
        {
            return " 크리티컬 성공!";
        }
        else if (playerStat >= choice.requiredStatValue)
        {
            return " 성공 가능";
        }
        else
        {
            return $" {choice.requiredStat} {choice.requiredStatValue} 필요";
        }
    }

    #endregion

    #region Debug

    public void DebugCurrentNode()
    {
        if (_currentNode == null)
        {
            Debug.Log("No current node");
            return;
        }

        Debug.Log("========== Current Story Node ==========");
        Debug.Log($"ID: {_currentNode.nodeId}");
        Debug.Log($"Title: {_currentNode.nodeTitle}");
        Debug.Log($"Text: {_currentNode.storyText}");
        Debug.Log($"Event Type: {_currentNode.eventType}");
        Debug.Log($"Choices: {_currentNode.choices?.Count ?? 0}");

        if (_currentNode.choices != null)
        {
            for (int i = 0; i < _currentNode.choices.Count; i++)
            {
                var choice = _currentNode.choices[i];
                Debug.Log($"  [{i}] {choice.choiceText}");
                if (choice.requiredStat != EStatType.None)
                {
                    Debug.Log($"      Required: {choice.requiredStat} >= {choice.requiredStatValue}");
                }
            }
        }

        Debug.Log("========================================");
    }

    #endregion
}