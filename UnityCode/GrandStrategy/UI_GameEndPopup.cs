using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_GameEndPopup : UI_Popup
{
    enum Texts
    {
        ResultText
    }
    enum Buttons
    {
        RestartButton,
        QuitButton
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));

        GetButton((int)Buttons.RestartButton).gameObject.BindEvent(OnClickRestartButton);
        GetButton((int)Buttons.QuitButton).gameObject.BindEvent(OnClickQuitButton);

        return true;
    }
    public void SetInfo(bool isVictory)
    {
        if (isVictory)
        {
            GetText((int)Texts.ResultText).text = "Victory!";
        }
        else
        {
            GetText((int)Texts.ResultText).text = "Game Over";
        }
    }

    void OnClickRestartButton(PointerEventData evt)
    {
        // 게임 재시작 - 타이틀 씬으로 이동
        Managers.Scene.LoadScene(Define.EScene.TitleScene);
    }

    void OnClickQuitButton(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
