using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static Define;

/// <summary>
/// 스토리 진행 화면 UI
/// - 스토리 텍스트 및 선택지
/// - 플레이어/파티 상태 표시
/// - 선택지 결과 표시
/// </summary>
public class UI_StoryScene : UI_Scene
{
    #region UI Enums
    enum Texts
    {
        NodeTitleText,      // 노드 제목
        StoryText,          // 스토리 내용
        PlayerNameText,     // 플레이어 이름
        PlayerLevelText,    // 레벨
        PlayerHPText,       // HP
        PlayerMPText,       // MP
        PlayerExpText,      // EXP
        GoldText,           // 골드
        Companion1NameText, // 동료1 이름
        Companion2NameText, // 동료2 이름
    }

    enum Buttons
    {
        MenuButton,         // 메뉴 (저장, 설정 등)
    }

    enum Sliders
    {
        PlayerHPBar,        // 플레이어 HP 바
        PlayerMPBar,        // 플레이어 MP 바
        PlayerExpBar,       // 플레이어 EXP 바
        Companion1HPBar,    // 동료1 HP 바
        Companion2HPBar,    // 동료2 HP 바
    }

    enum GameObjects
    {
        ChoiceContainer,    // 선택지 버튼 생성 위치
        PlayerStatusPanel,  // 플레이어 상태 패널
        CompanionPanel,     // 동료 패널
        Companion1Panel,    // 동료1 패널
        Companion2Panel,    // 동료2 패널
        ResultPanel,        // 선택지 결과 패널
    }

    enum Images
    {
        StoryBackgroundImage,   // 배경 이미지
        PlayerPortrait,         // 플레이어 초상화
        Companion1Portrait,     // 동료1 초상화
        Companion2Portrait,     // 동료2 초상화
    }
    #endregion

    #region Fields
    private List<UI_ChoiceButton> _choiceButtons = new List<UI_ChoiceButton>();
    private Data.StoryNodeData _currentNode;
    private bool _isWaitingForChoice = false;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindSliders(typeof(Sliders));
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));

        // 버튼 이벤트
        GetButton((int)Buttons.MenuButton).gameObject.BindEvent(OnClickMenu);

        // 결과 패널 비활성화
        GetObject((int)GameObjects.ResultPanel)?.SetActive(false);

        // 이벤트 등록
        RegisterEvents();

        // 초기 UI 갱신
        RefreshUI();

        return true;
    }
    #endregion

    #region Event Registration
    private void RegisterEvents()
    {
        // StoryManager 이벤트
        if (Managers.Story != null)
        {
            Managers.Story.OnNodeLoaded += OnNodeLoaded;
            Managers.Story.OnChoiceResult += OnChoiceResult;
            Managers.Story.OnRewardsGained += OnRewardsGained;
        }

        // PlayerManager 이벤트
        if (Managers.Player != null)
        {
            Managers.Player.OnHpChanged += OnPlayerHpChanged;
            Managers.Player.OnMpChanged += OnPlayerMpChanged;
            Managers.Player.OnLevelUp += OnPlayerLevelUp;
            Managers.Player.OnExpGained += OnPlayerExpGained;
        }

        // GameManager 이벤트
        if (Managers.Game != null)
        {
            Managers.Game.OnGoldChanged += OnGoldChanged;
        }

        // PartyManager 이벤트
        if (Managers.Party != null)
        {
            Managers.Party.OnCompanionJoined += OnCompanionJoined;
            Managers.Party.OnCompanionLeft += OnCompanionLeft;
        }
    }

    private void UnregisterEvents()
    {
        if (Managers.Story != null)
        {
            Managers.Story.OnNodeLoaded -= OnNodeLoaded;
            Managers.Story.OnChoiceResult -= OnChoiceResult;
            Managers.Story.OnRewardsGained -= OnRewardsGained;
        }

        if (Managers.Player != null)
        {
            Managers.Player.OnHpChanged -= OnPlayerHpChanged;
            Managers.Player.OnMpChanged -= OnPlayerMpChanged;
            Managers.Player.OnLevelUp -= OnPlayerLevelUp;
            Managers.Player.OnExpGained -= OnPlayerExpGained;
        }

        if (Managers.Game != null)
        {
            Managers.Game.OnGoldChanged -= OnGoldChanged;
        }

        if (Managers.Party != null)
        {
            Managers.Party.OnCompanionJoined -= OnCompanionJoined;
            Managers.Party.OnCompanionLeft -= OnCompanionLeft;
        }
    }
    #endregion

    #region UI Refresh
    public void RefreshUI()
    {
        if (_init == false)
            return;

        RefreshPlayerStatus();
        RefreshCompanionStatus();
        RefreshGold();
    }

    private void RefreshPlayerStatus()
    {
        // 이름 및 레벨
        GetText((int)Texts.PlayerNameText).text = Managers.Player.PlayerName;
        GetText((int)Texts.PlayerLevelText).text = $"Lv.{Managers.Player.Level}";

        // HP
        GetText((int)Texts.PlayerHPText).text = $"{Managers.Player.CurrentHp}/{Managers.Player.MaxHp}";
        GetSlider((int)Sliders.PlayerHPBar).value = (float)Managers.Player.CurrentHp / Managers.Player.MaxHp;

        // MP
        GetText((int)Texts.PlayerMPText).text = $"{Managers.Player.CurrentMp}/{Managers.Player.MaxMp}";
        GetSlider((int)Sliders.PlayerMPBar).value = (float)Managers.Player.CurrentMp / Managers.Player.MaxMp;

        // EXP
        GetText((int)Texts.PlayerExpText).text = $"{Managers.Player.CurrentExp}/{Managers.Player.ExpToNextLevel}";
        GetSlider((int)Sliders.PlayerExpBar).value = (float)Managers.Player.CurrentExp / Managers.Player.ExpToNextLevel;
    }

    private void RefreshCompanionStatus()
    {
        var companions = Managers.Party.Companions;

        // 동료 1
        if (companions.Count > 0)
        {
            var companion1 = companions[0];
            GetObject((int)GameObjects.Companion1Panel).SetActive(true);
            GetText((int)Texts.Companion1NameText).text = companion1.CharacterData.characterName;

            var hpBar = GetSlider((int)Sliders.Companion1HPBar);
            hpBar.value = (float)companion1.CurrentHp / companion1.MaxHp;
        }
        else
        {
            GetObject((int)GameObjects.Companion1Panel).SetActive(false);
        }

        // 동료 2
        if (companions.Count > 1)
        {
            var companion2 = companions[1];
            GetObject((int)GameObjects.Companion2Panel).SetActive(true);
            GetText((int)Texts.Companion2NameText).text = companion2.CharacterData.characterName;

            var hpBar = GetSlider((int)Sliders.Companion2HPBar);
            hpBar.value = (float)companion2.CurrentHp / companion2.MaxHp;
        }
        else
        {
            GetObject((int)GameObjects.Companion2Panel).SetActive(false);
        }
    }

    private void RefreshGold()
    {
        GetText((int)Texts.GoldText).text = Managers.Game.Gold.ToString();
    }
    #endregion

    #region Story Display
    /// <summary>
    /// 스토리 노드가 로드되었을 때 호출
    /// </summary>
    private void OnNodeLoaded(Data.StoryNodeData nodeData)
    {
        _currentNode = nodeData;

        // 노드 제목 및 내용 표시
        GetText((int)Texts.NodeTitleText).text = nodeData.nodeTitle;
        GetText((int)Texts.StoryText).text = nodeData.storyText;

        // 배경 이미지 변경 (선택사항)
        if (!string.IsNullOrEmpty(nodeData.backgroundImageKey))
        {
            // TODO: 배경 이미지 로드
        }

        // 선택지 생성
        ClearChoices();
        CreateChoiceButtons(nodeData.choices);

        _isWaitingForChoice = true;
    }

    private void ClearChoices()
    {
        foreach (var button in _choiceButtons)
        {
            if (button != null && button.gameObject != null)
            {
                Managers.Resource.Destroy(button.gameObject);
            }
        }
        _choiceButtons.Clear();
    }

    private void CreateChoiceButtons(List<Data.ChoiceData> choices)
    {
        if (choices == null || choices.Count == 0)
            return;

        var container = GetObject((int)GameObjects.ChoiceContainer).transform;

        for (int i = 0; i < choices.Count; i++)
        {
            var choiceData = choices[i];
            var choiceButton = Managers.UI.MakeSubItem<UI_ChoiceButton>(container, "UI_ChoiceButton");

            if (choiceButton != null)
            {
                int index = i; // 클로저 변수
                choiceButton.SetInfo(choiceData, index, () => OnChoiceClicked(index));
                _choiceButtons.Add(choiceButton);
            }
        }
    }
    #endregion

    #region Choice Handling
    private void OnChoiceClicked(int choiceIndex)
    {
        if (!_isWaitingForChoice)
        {
            Debug.LogWarning("Not waiting for choice!");
            return;
        }

        _isWaitingForChoice = false;

        // 선택지 버튼 비활성화
        foreach (var button in _choiceButtons)
        {
            button.SetInteractable(false);
        }

        // StoryManager에 선택 전달
        Managers.Story.SelectChoice(choiceIndex);
    }

    private void OnChoiceResult(EChoiceResultType resultType, Data.ChoiceData choice)
    {
        // 선택지 결과 표시 (선택사항)
        StartCoroutine(ShowChoiceResult(resultType));
    }

    private IEnumerator ShowChoiceResult(EChoiceResultType resultType)
    {
        var resultPanel = GetObject((int)GameObjects.ResultPanel);
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            // 결과 텍스트 표시
            var resultText = resultPanel.GetComponentInChildren<TMP_Text>();
            if (resultText != null)
            {
                resultText.text = resultType switch
                {
                    EChoiceResultType.CriticalSuccess => "대성공!",
                    EChoiceResultType.Success => "성공!",
                    EChoiceResultType.Failure => "실패...",
                    _ => ""
                };
            }

            yield return new WaitForSeconds(1.5f);
            resultPanel.SetActive(false);
        }
    }

    private void OnRewardsGained(List<Data.RewardData> rewards)
    {
        // 보상 표시 (선택사항)
        Debug.Log($"Rewards gained: {rewards.Count}");
    }
    #endregion

    #region Event Handlers
    private void OnPlayerHpChanged(int currentHp, int maxHp)
    {
        RefreshPlayerStatus();
    }

    private void OnPlayerMpChanged(int currentMp, int maxMp)
    {
        RefreshPlayerStatus();
    }

    private void OnPlayerLevelUp(int newLevel)
    {
        RefreshPlayerStatus();
        // TODO: 레벨업 이펙트
    }

    private void OnPlayerExpGained(int amount)
    {
        RefreshPlayerStatus();
    }

    private void OnGoldChanged(int newGold)
    {
        RefreshGold();
    }

    private void OnCompanionJoined(PartyManager.Companion companion)
    {
        RefreshCompanionStatus();
    }

    private void OnCompanionLeft(PartyManager.Companion companion)
    {
        RefreshCompanionStatus();
    }

    private void OnClickMenu(PointerEventData evt)
    {
        // TODO: 메뉴 팝업 표시 (저장, 설정 등)
        Debug.Log("Menu button clicked");
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        UnregisterEvents();
        ClearChoices();
    }
    #endregion
}
