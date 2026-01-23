using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static Define;

/// <summary>
/// 전투 화면 UI
/// - 아군/적 상태 표시
/// - 전투 로그
/// - 전투 속도 조절
/// </summary>
public class UI_BattleScene : UI_Scene
{
    #region UI Enums
    enum Texts
    {
        TurnInfoText,       // 턴 정보
        BattleLogText,      // 전투 로그
        PlayerNameText,     // 플레이어 이름
        PlayerHPText,       // 플레이어 HP
        PlayerMPText,       // 플레이어 MP
        Companion1NameText, // 동료1 이름
        Companion1HPText,   // 동료1 HP
        Companion2NameText, // 동료2 이름
        Companion2HPText,   // 동료2 HP
        Enemy1NameText,     // 적1 이름
        Enemy1HPText,       // 적1 HP
        Enemy2NameText,     // 적2 이름
        Enemy2HPText,       // 적2 HP
        Enemy3NameText,     // 적3 이름
        Enemy3HPText,       // 적3 HP
        BattleSpeedText,    // 전투 속도 텍스트
    }

    enum Buttons
    {
        SpeedNormalButton,  // 1x 속도
        SpeedFastButton,    // 2x 속도
        SpeedVeryFastButton,// 3x 속도
        SkipBattleButton,   // 전투 스킵
    }

    enum Sliders
    {
        PlayerHPBar,        // 플레이어 HP 바
        PlayerMPBar,        // 플레이어 MP 바
        Companion1HPBar,    // 동료1 HP 바
        Companion1MPBar,    // 동료1 MP 바
        Companion2HPBar,    // 동료2 HP 바
        Companion2MPBar,    // 동료2 MP 바
        Enemy1HPBar,        // 적1 HP 바
        Enemy2HPBar,        // 적2 HP 바
        Enemy3HPBar,        // 적3 HP 바
    }

    enum GameObjects
    {
        AllyPanel,          // 아군 전체 패널
        PlayerPanel,        // 플레이어 패널
        Companion1Panel,    // 동료1 패널
        Companion2Panel,    // 동료2 패널
        EnemyPanel,         // 적 전체 패널
        Enemy1Panel,        // 적1 패널
        Enemy2Panel,        // 적2 패널
        Enemy3Panel,        // 적3 패널
        BattleLogPanel,     // 전투 로그 패널
        BattleControlPanel, // 전투 컨트롤 패널
        ResultPanel,        // 결과 패널
    }

    enum Images
    {
        PlayerSprite,       // 플레이어 스프라이트
        Companion1Sprite,   // 동료1 스프라이트
        Companion2Sprite,   // 동료2 스프라이트
        Enemy1Sprite,       // 적1 스프라이트
        Enemy2Sprite,       // 적2 스프라이트
        Enemy3Sprite,       // 적3 스프라이트
    }
    #endregion

    #region Fields
    private const int MAX_LOG_LINES = 100;
    private Queue<string> _battleLogQueue = new Queue<string>();
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
        GetButton((int)Buttons.SpeedNormalButton).gameObject.BindEvent(OnClickSpeed1x);
        GetButton((int)Buttons.SpeedFastButton).gameObject.BindEvent(OnClickSpeed2x);
        GetButton((int)Buttons.SpeedVeryFastButton).gameObject.BindEvent(OnClickSpeed3x);
        GetButton((int)Buttons.SkipBattleButton).gameObject.BindEvent(OnClickSkipBattle);

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
        if (Managers.Battle != null)
        {
            Managers.Battle.OnBattleStateChanged += OnBattleStateChanged;
            Managers.Battle.OnBattleStarted += OnBattleStarted;
            Managers.Battle.OnTurnStart += OnTurnStart;
            Managers.Battle.OnActionExecuted += OnActionExecuted;
            Managers.Battle.OnDamageDealt += OnDamageDealt;
            Managers.Battle.OnHealingDone += OnHealingDone;
            Managers.Battle.OnParticipantDied += OnParticipantDied;
            Managers.Battle.OnBattleEnded += OnBattleEnded;
            Managers.Battle.OnBattleLogAdded += OnBattleLogAdded;
        }
    }

    private void UnregisterEvents()
    {
        if (Managers.Battle != null)
        {
            Managers.Battle.OnBattleStateChanged -= OnBattleStateChanged;
            Managers.Battle.OnBattleStarted -= OnBattleStarted;
            Managers.Battle.OnTurnStart -= OnTurnStart;
            Managers.Battle.OnActionExecuted -= OnActionExecuted;
            Managers.Battle.OnDamageDealt -= OnDamageDealt;
            Managers.Battle.OnHealingDone -= OnHealingDone;
            Managers.Battle.OnParticipantDied -= OnParticipantDied;
            Managers.Battle.OnBattleEnded -= OnBattleEnded;
            Managers.Battle.OnBattleLogAdded -= OnBattleLogAdded;
        }
    }
    #endregion

    #region UI Refresh
    public void RefreshUI()
    {
        if (_init == false)
            return;

        RefreshAllyStatus();
        RefreshEnemyStatus();
        RefreshTurnInfo();
        RefreshBattleSpeed();
    }

    private void RefreshAllyStatus()
    {
        // 플레이어
        var player = Managers.Battle.Player;
        if (player != null)
        {
            GetObject((int)GameObjects.PlayerPanel).SetActive(true);
            GetText((int)Texts.PlayerNameText).text = player.Name;
            GetText((int)Texts.PlayerHPText).text = $"{player.CurrentHp}/{player.MaxHp}";
            GetText((int)Texts.PlayerMPText).text = $"{player.CurrentMp}/{player.MaxMp}";

            GetSlider((int)Sliders.PlayerHPBar).value = (float)player.CurrentHp / player.MaxHp;
            GetSlider((int)Sliders.PlayerMPBar).value = (float)player.CurrentMp / player.MaxMp;
        }
        else
        {
            GetObject((int)GameObjects.PlayerPanel).SetActive(false);
        }

        // 동료들
        var companions = Managers.Battle.Companions;

        // 동료 1
        if (companions.Count > 0)
        {
            var companion = companions[0];
            GetObject((int)GameObjects.Companion1Panel).SetActive(true);
            GetText((int)Texts.Companion1NameText).text = companion.Name;
            GetText((int)Texts.Companion1HPText).text = $"{companion.CurrentHp}/{companion.MaxHp}";

            GetSlider((int)Sliders.Companion1HPBar).value = (float)companion.CurrentHp / companion.MaxHp;
            GetSlider((int)Sliders.Companion1MPBar).value = (float)companion.CurrentMp / companion.MaxMp;
        }
        else
        {
            GetObject((int)GameObjects.Companion1Panel).SetActive(false);
        }

        // 동료 2
        if (companions.Count > 1)
        {
            var companion = companions[1];
            GetObject((int)GameObjects.Companion2Panel).SetActive(true);
            GetText((int)Texts.Companion2NameText).text = companion.Name;
            GetText((int)Texts.Companion2HPText).text = $"{companion.CurrentHp}/{companion.MaxHp}";

            GetSlider((int)Sliders.Companion2HPBar).value = (float)companion.CurrentHp / companion.MaxHp;
            GetSlider((int)Sliders.Companion2MPBar).value = (float)companion.CurrentMp / companion.MaxMp;
        }
        else
        {
            GetObject((int)GameObjects.Companion2Panel).SetActive(false);
        }
    }

    private void RefreshEnemyStatus()
    {
        var enemies = Managers.Battle.Enemies;

        // 적 1
        if (enemies.Count > 0)
        {
            var enemy = enemies[0];
            GetObject((int)GameObjects.Enemy1Panel).SetActive(true);
            GetText((int)Texts.Enemy1NameText).text = enemy.Name;
            GetText((int)Texts.Enemy1HPText).text = $"{enemy.CurrentHp}/{enemy.MaxHp}";

            GetSlider((int)Sliders.Enemy1HPBar).value = (float)enemy.CurrentHp / enemy.MaxHp;
        }
        else
        {
            GetObject((int)GameObjects.Enemy1Panel).SetActive(false);
        }

        // 적 2
        if (enemies.Count > 1)
        {
            var enemy = enemies[1];
            GetObject((int)GameObjects.Enemy2Panel).SetActive(true);
            GetText((int)Texts.Enemy2NameText).text = enemy.Name;
            GetText((int)Texts.Enemy2HPText).text = $"{enemy.CurrentHp}/{enemy.MaxHp}";

            GetSlider((int)Sliders.Enemy2HPBar).value = (float)enemy.CurrentHp / enemy.MaxHp;
        }
        else
        {
            GetObject((int)GameObjects.Enemy2Panel).SetActive(false);
        }

        // 적 3
        if (enemies.Count > 2)
        {
            var enemy = enemies[2];
            GetObject((int)GameObjects.Enemy3Panel).SetActive(true);
            GetText((int)Texts.Enemy3NameText).text = enemy.Name;
            GetText((int)Texts.Enemy3HPText).text = $"{enemy.CurrentHp}/{enemy.MaxHp}";

            GetSlider((int)Sliders.Enemy3HPBar).value = (float)enemy.CurrentHp / enemy.MaxHp;
        }
        else
        {
            GetObject((int)GameObjects.Enemy3Panel).SetActive(false);
        }
    }

    private void RefreshTurnInfo()
    {
        var currentTurn = Managers.Battle.CurrentTurnParticipant;
        var turnCount = Managers.Battle.TurnCount;

        if (currentTurn != null)
        {
            GetText((int)Texts.TurnInfoText).text = $"Turn {turnCount} - {currentTurn.Name}의 차례";
        }
        else
        {
            GetText((int)Texts.TurnInfoText).text = $"Turn {turnCount}";
        }
    }

    private void RefreshBattleSpeed()
    {
        float speed = Managers.Battle.BattleSpeed;
        GetText((int)Texts.BattleSpeedText).text = $"x{speed:F1}";

        // 버튼 하이라이트
        GetButton((int)Buttons.SpeedNormalButton).interactable = (speed != 1.0f);
        GetButton((int)Buttons.SpeedFastButton).interactable = (speed != 2.0f);
        GetButton((int)Buttons.SpeedVeryFastButton).interactable = (speed != 3.0f);
    }
    #endregion

    #region Battle Log
    private void AddBattleLog(string message)
    {
        _battleLogQueue.Enqueue(message);

        // 최대 로그 라인 수 제한
        while (_battleLogQueue.Count > MAX_LOG_LINES)
        {
            _battleLogQueue.Dequeue();
        }

        // 전체 로그 업데이트
        UpdateBattleLogDisplay();
    }

    private void UpdateBattleLogDisplay()
    {
        string fullLog = string.Join("\n", _battleLogQueue);
        GetText((int)Texts.BattleLogText).text = fullLog;

        // 스크롤을 맨 아래로 (선택사항)
        // TODO: ScrollRect의 verticalNormalizedPosition을 0으로 설정
    }

    private void ClearBattleLog()
    {
        _battleLogQueue.Clear();
        GetText((int)Texts.BattleLogText).text = "";
    }
    #endregion

    #region Battle Event Handlers
    private void OnBattleStateChanged(EBattleState prevState, EBattleState newState)
    {
        Debug.Log($"Battle State: {prevState} -> {newState}");
        RefreshUI();
    }

    private void OnBattleStarted()
    {
        Debug.Log("Battle Started!");
        ClearBattleLog();
        AddBattleLog("=== 전투 시작 ===");
        RefreshUI();
    }

    private void OnTurnStart(BattleManager.BattleParticipant participant)
    {
        RefreshTurnInfo();
    }

    private void OnActionExecuted(BattleManager.BattleAction action)
    {
        // 액션은 이미 로그에 추가되므로 UI 갱신만
        RefreshAllyStatus();
        RefreshEnemyStatus();
    }

    private void OnDamageDealt(BattleManager.BattleParticipant target, int damage)
    {
        RefreshAllyStatus();
        RefreshEnemyStatus();

        // TODO: 데미지 이펙트 (스프라이트 흔들기 등)
    }

    private void OnHealingDone(BattleManager.BattleParticipant target, int healAmount)
    {
        RefreshAllyStatus();
        RefreshEnemyStatus();

        // TODO: 힐 이펙트
    }

    private void OnParticipantDied(BattleManager.BattleParticipant participant)
    {
        RefreshAllyStatus();
        RefreshEnemyStatus();

        // TODO: 사망 이펙트
    }

    private void OnBattleEnded(bool victory, List<Data.RewardData> rewards)
    {
        Debug.Log($"Battle Ended! Victory: {victory}");

        StartCoroutine(ShowBattleResult(victory, rewards));
    }

    private void OnBattleLogAdded(string message)
    {
        AddBattleLog(message);
    }
    #endregion

    #region Battle Result
    private IEnumerator ShowBattleResult(bool victory, List<Data.RewardData> rewards)
    {
        yield return new WaitForSeconds(1.0f);

        var resultPanel = GetObject((int)GameObjects.ResultPanel);
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);

            // 결과 텍스트 표시
            var resultText = resultPanel.GetComponentInChildren<TMP_Text>();
            if (resultText != null)
            {
                if (victory)
                {
                    string rewardText = "승리!\n\n획득 보상:\n";
                    if (rewards != null)
                    {
                        foreach (var reward in rewards)
                        {
                            rewardText += $"- {reward.rewardType}: {reward.amount}\n";
                        }
                    }
                    resultText.text = rewardText;
                }
                else
                {
                    resultText.text = "패배...";
                }
            }

            yield return new WaitForSeconds(3.0f);
            resultPanel.SetActive(false);
        }

        // 스토리 씬으로 복귀
        yield return new WaitForSeconds(0.5f);
        // BattleManager가 StoryManager에 콜백하여 복귀
    }
    #endregion

    #region Button Handlers
    private void OnClickSpeed1x(PointerEventData evt)
    {
        Managers.Battle.SetBattleSpeed(1.0f);
        RefreshBattleSpeed();
    }

    private void OnClickSpeed2x(PointerEventData evt)
    {
        Managers.Battle.SetBattleSpeed(2.0f);
        RefreshBattleSpeed();
    }

    private void OnClickSpeed3x(PointerEventData evt)
    {
        Managers.Battle.SetBattleSpeed(3.0f);
        RefreshBattleSpeed();
    }

    private void OnClickSkipBattle(PointerEventData evt)
    {
        Managers.Battle.SkipBattle();
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        UnregisterEvents();
        ClearBattleLog();
    }
    #endregion
}
