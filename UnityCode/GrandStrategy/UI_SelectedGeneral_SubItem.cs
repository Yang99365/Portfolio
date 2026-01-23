using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SelectedGeneral_SubItem : UI_Base
{
    public delegate void ClickSelectedGeneral(int GeneralID);
    public static event ClickSelectedGeneral OnClickSelectedGeneral;
    enum Buttons
    {
        WarGeneralButton,
    }

    enum Images
    {
        Portrait,
    }
    enum Texts
    {
        GeneralName,
    }

    public int _GeneralID = -1;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.WarGeneralButton).gameObject.BindEvent(OnClickWarGeneralButton);

        Refresh();

        return true;
    }

    public void SetInfo(int GeneralID)
    {
        _GeneralID = GeneralID;
        Refresh();
    }

    public void Refresh()
    {
        if (_init == false)
            return;

        if (_GeneralID < 0)
            return;

        GetImage((int)Images.Portrait).sprite = Managers.Resource.Load<Sprite>(Managers.Data.GeneralDic[_GeneralID].spriteAddress);
        GetText((int)Texts.GeneralName).text = Managers.Data.GeneralDic[_GeneralID].name;
    }

    public void OnClickWarGeneralButton(PointerEventData evt)
    {
        OnClickSelectedGeneral?.Invoke(_GeneralID);
    }
}
