using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static Define;


public class UI_TitleScene : UI_Scene
{
    #region UI Enums
    enum Buttons
    {
        NewGameButton,
        ContinueButton,
        ExitButton,
    }

    enum Texts
    {
        TitleText,
        VersionText,
    }

    enum GameObjects
    {
        ButtonContainer,
    }
    #endregion

    #region Fields
    private bool _hasSaveFile = false;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        // 버튼 이벤트 등록
        GetButton((int)Buttons.NewGameButton).gameObject.BindEvent(OnClickNewGame);
        GetButton((int)Buttons.ContinueButton).gameObject.BindEvent(OnClickContinue);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExit);

        // UI 초기화
        RefreshUI();

        return true;
    }

    private void RefreshUI()
    {
        if (_init == false)
            return;

        // 세이브 파일 확인
        _hasSaveFile = Managers.Game.HasSaveFile();

        // Continue 버튼 활성화/비활성화
        var continueButton = GetButton((int)Buttons.ContinueButton);
        continueButton.interactable = _hasSaveFile;

        // 비활성화된 버튼 색상 조정 (선택사항)
        if (!_hasSaveFile)
        {
            var colors = continueButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            continueButton.colors = colors;
        }

        // 타이틀 텍스트
        GetText((int)Texts.TitleText).text = "모험가 이야기";

        // 버전 텍스트 (선택사항)
        GetText((int)Texts.VersionText).text = "v0.1";
    }
    #endregion

    #region Button Handlers
    private void OnClickNewGame(PointerEventData evt)
    {
        Debug.Log("New Game button clicked");

        // 기존 세이브 파일이 있으면 경고 팝업 (선택사항)
        if (_hasSaveFile)
        {
            // TODO: 확인 팝업 표시
            // "기존 세이브 파일을 덮어쓰시겠습니까?"
            // 지금은 바로 시작
        }

        StartNewGame();
    }

    private void OnClickContinue(PointerEventData evt)
    {
        Debug.Log("Continue button clicked");

        if (!_hasSaveFile)
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        ContinueGame();
    }

    private void OnClickExit(PointerEventData evt)
    {
        Debug.Log("Exit button clicked");

        Managers.Game.QuitGame();
    }
    #endregion

    #region Game Flow
    private void StartNewGame()
    {
        // 새 게임 시작
        Managers.Game.NewGame();

        // Story 씬으로 전환
        Managers.Scene.LoadScene(EScene.Story);
    }

    private void ContinueGame()
    {
        // 세이브 파일 로드
        Managers.Game.LoadGame();

        // Story 씬으로 전환
        Managers.Scene.LoadScene(EScene.Story);
    }
    #endregion
}
