using System.Collections;
using UnityEngine;
using static Define;

public class BattleScene : BaseScene
{
    [Header("Battle Display")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform[] _companionTransforms = new Transform[2];
    [SerializeField] private Transform[] _enemyTransforms = new Transform[3];

    private UI_BattleScene _battleUI;
    private bool _isInitialized = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.Battle;
        Managers.Scene.SetCurrentScene(this);

        StartCoroutine(InitializeBattle());

        return true;
    }

    private IEnumerator InitializeBattle()
    {
        Debug.Log("=== Battle Scene Initialization Start ===");

        yield return null;

        // Validate transforms
        if (!ValidateTransforms())
        {
            Debug.LogError("Battle transforms not properly assigned");
            yield break;
        }

        // Show Battle UI
        _battleUI = Managers.UI.ShowSceneUI<UI_BattleScene>();

        yield return new WaitForEndOfFrame();

        // Register Events
        RegisterBattleEvents();

        // Setup battle display (sprites)
        SetupBattleDisplay();

        //  CRITICAL: BattleManager.StartBattle() 호출!
        // 어떤 적과 싸울지 결정 (임시로 enemyId = 1)
        int enemyId = GetBattleEnemyId();
        Managers.Battle.StartBattle(enemyId);

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        //  StartBattle 후에 코루틴 실행!
        StartCoroutine(Managers.Battle.AutoBattleCoroutine());

        _isInitialized = true;

        Debug.Log("Battle Scene Initialization Complete");
    }
    private int GetBattleEnemyId()
    {
        // StoryManager에서 전달받은 적 ID
        // 임시로 1번 적 (Goblin)
        return 1;
    }
    private bool ValidateTransforms()
    {
        if (_playerTransform == null)
        {
            Debug.LogError("Player transform not assigned!");
            return false;
        }

        if (_enemyTransforms == null || _enemyTransforms.Length == 0)
        {
            Debug.LogError("Enemy transforms not assigned!");
            return false;
        }

        Debug.Log("Battle transforms validated");
        return true;
    }

    private void SetupBattleDisplay()
    {
        // TODO: Load and display character/enemy sprites
        // This will be implemented when UI is ready

        Debug.Log("Battle display setup");
    }

    private void RegisterBattleEvents()
    {
        Managers.Battle.OnBattleStateChanged += OnBattleStateChanged;
        Managers.Battle.OnBattleStarted += OnBattleStarted;
        Managers.Battle.OnTurnStart += OnTurnStart;
        Managers.Battle.OnActionExecuted += OnActionExecuted;
        Managers.Battle.OnDamageDealt += OnDamageDealt;
        Managers.Battle.OnParticipantDied += OnParticipantDied;
        Managers.Battle.OnBattleEnded += OnBattleEnded;

        Debug.Log("Battle events registered");
    }

    private void UnregisterBattleEvents()
    {
        if (Managers.Battle != null)
        {
            Managers.Battle.OnBattleStateChanged -= OnBattleStateChanged;
            Managers.Battle.OnBattleStarted -= OnBattleStarted;
            Managers.Battle.OnTurnStart -= OnTurnStart;
            Managers.Battle.OnActionExecuted -= OnActionExecuted;
            Managers.Battle.OnDamageDealt -= OnDamageDealt;
            Managers.Battle.OnParticipantDied -= OnParticipantDied;
            Managers.Battle.OnBattleEnded -= OnBattleEnded;
        }
    }

    #region Battle Event Handlers

    private void OnBattleStateChanged(EBattleState prevState, EBattleState newState)
    {
        Debug.Log($"Battle State: {prevState} -> {newState}");

        if (_battleUI != null)
        {
            //_battleUI.UpdateBattleState(newState);
        }
    }

    private void OnBattleStarted()
    {
        Debug.Log("Battle Started!");

        if (_battleUI != null)
        {
            //_battleUI.ShowBattleStart();
        }
    }

    private void OnTurnStart(BattleManager.BattleParticipant participant)
    {
        Debug.Log($"{participant.Name}'s turn!");
    }

    private void OnActionExecuted(BattleManager.BattleAction action)
    {
        Debug.Log($"{action.Actor.Name} executed {action.ActionType}");

        // TODO: Play animation/effect
        if (_battleUI != null)
        {
            //_battleUI.ShowAction(action);
        }
    }

    private void OnDamageDealt(BattleManager.BattleParticipant target, int damage)
    {
        Debug.Log($"{target.Name} took {damage} damage!");

        // TODO: Shake sprite with DoTween
        if (_battleUI != null)
        {
            //_battleUI.ShowDamage(target, damage);
        }
    }

    private void OnParticipantDied(BattleManager.BattleParticipant participant)
    {
        Debug.Log($"{participant.Name} died!");

        // TODO: Death animation
        if (_battleUI != null)
        {
            //_battleUI.ShowDeath(participant);
        }
    }

    private void OnBattleEnded(bool victory, System.Collections.Generic.List<Data.RewardData> rewards)
    {
        Debug.Log($"Battle Ended! Victory: {victory}");

        StartCoroutine(HandleBattleEnd(victory, rewards));
    }

    #endregion

    #region Battle End

    private IEnumerator HandleBattleEnd(bool victory, System.Collections.Generic.List<Data.RewardData> rewards)
    {
        if (_battleUI != null)
        {
            //_battleUI.ShowBattleResult(victory, rewards);
        }

        // Wait for result display
        yield return new WaitForSeconds(2f);

        // 전투 결과를 GameManager에 임시 저장
        Managers.Game.SaveData.lastBattleVictory = victory;
        Managers.Game.SaveData.lastBattleRewards = rewards;

        // Return to Story Scene
        Managers.Scene.LoadScene(EScene.Story);
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        UnregisterBattleEvents();
    }

    public override void Clear()
    {
        Debug.Log("Clearing Battle Scene");
        UnregisterBattleEvents();
    }

    #endregion
}
