using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ConfirmPopup : UI_Popup
{
    #region Enums
    enum Buttons
    {
        ConfirmButton,
        CancelButton,
    }

    enum Texts
    {
        TitleText,
        MessageText,
        ConfirmButtonText,
        CancelButtonText,
    }

    enum GameObjects
    {
        Background,
    }
    #endregion

    #region Fields
    private Action _onConfirm;
    private Action _onCancel;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // Bind UI elements
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        // Button events
        GetButton((int)Buttons.ConfirmButton).gameObject.BindEvent(OnClickConfirmButton);
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnClickCancelButton);

        // 기본 텍스트 설정
        GetText((int)Texts.ConfirmButtonText).text = "확인";
        GetText((int)Texts.CancelButtonText).text = "취소";

        return true;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 팝업 설정 및 표시
    /// </summary>
    /// <param name="title">팝업 제목</param>
    /// <param name="message">팝업 메시지</param>
    /// <param name="onConfirm">확인 버튼 콜백</param>
    /// <param name="onCancel">취소 버튼 콜백 (null 가능)</param>
    /// <param name="confirmButtonText">확인 버튼 텍스트 (기본: "확인")</param>
    /// <param name="cancelButtonText">취소 버튼 텍스트 (기본: "취소")</param>
    public void SetInfo(
        string title,
        string message,
        Action onConfirm,
        Action onCancel = null,
        string confirmButtonText = "확인",
        string cancelButtonText = "취소")
    {
        // 텍스트 설정
        GetText((int)Texts.TitleText).text = title;
        GetText((int)Texts.MessageText).text = message;
        GetText((int)Texts.ConfirmButtonText).text = confirmButtonText;
        GetText((int)Texts.CancelButtonText).text = cancelButtonText;

        // 콜백 설정
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }

    /// <summary>
    /// 간단한 알림 팝업 (확인 버튼만)
    /// </summary>
    public void SetInfoAsAlert(string title, string message, Action onConfirm = null)
    {
        SetInfo(title, message, onConfirm);

        // 취소 버튼 숨기기
        GetButton((int)Buttons.CancelButton).gameObject.SetActive(false);
    }
    #endregion

    #region Event Handlers
    private void OnClickConfirmButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        Action callback = _onConfirm;

        ClosePopupUI();

        callback?.Invoke();
    }

    private void OnClickCancelButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        Action callback = _onCancel;

        // 팝업 닫기
        ClosePopupUI();

        callback?.Invoke();
    }
    #endregion

    #region Cleanup
    public override void ClosePopupUI()
    {
        // 콜백 정리
        _onConfirm = null;
        _onCancel = null;

        base.ClosePopupUI();
    }
    #endregion
}
