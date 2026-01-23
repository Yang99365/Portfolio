using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using static Define;

public class UI_ConfirmPopup : UI_Popup
{
    enum Buttons
    {
        YesButton,
        NoButton,
    }
    enum Texts
    {
        ConfirmText,
        ItemNameText,
    }
    enum GameObjects
    {
        EmptySide
    }

    private Action onConfirm;
    public override bool Init()
    {
        if (base.Init() == false)
            return false;
      
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));

        GetButton((int)Buttons.YesButton).onClick.AddListener(OnYesClick);
        GetButton((int)Buttons.NoButton).onClick.AddListener(OnNoClick);


        return true;
    }

    public void SetInfo(string message, Action onConfirm, string itemName)
    {
        GetText((int)Texts.ConfirmText).text = message;
        GetText((int)Texts.ItemNameText).text = itemName;
        this.onConfirm = onConfirm;
    }

    private void OnYesClick()
    {
        onConfirm?.Invoke();
        Managers.UI.CloseAllPopupUI();
        //Managers.UI.ShowPopupUI<UI_InventoryPopup>();
    }

    private void OnNoClick()
    {
        Managers.UI.CloseAllPopupUI();
        //Managers.UI.ShowPopupUI<UI_InventoryPopup>();
    }
    public bool isActive()
    {
        return gameObject.activeSelf;
    }
}
