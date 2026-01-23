using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static InventoryManager;

public class UI_WarGeneral_SubItem : UI_Base
{
    public delegate void WarGeneralSelected(General general, bool isSelect);
    public static event WarGeneralSelected OnGeneralSelected;
    enum Buttons
    {
        WarGeneralButton,
    }
    enum GameObjects
    {
        SelectMark,
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
    bool isSelect = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.WarGeneralButton).gameObject.BindEvent(OnClickWarGeneralButton);

        Refresh();

        return true;
    }

    public void SetInfo(int GeneralID)
    {
        _GeneralID = GeneralID;
        GetObject((int)GameObjects.SelectMark).SetActive(isSelect);
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

        GetObject((int)GameObjects.SelectMark).SetActive(isSelect);
    }

    void OnClickWarGeneralButton(PointerEventData evt)
    {
        if (Managers.Battle == null)
            return;

        isSelect = !isSelect;
        GetObject((int)GameObjects.SelectMark).SetActive(isSelect);

        General general = Managers.Game.GetGeneral(_GeneralID);
        if (general != null)
        {
            OnGeneralSelected?.Invoke(general, isSelect);
        }
    }

    public void ResetSelectGeneral()
    {
        //전투끝나면 선택된 무장 초기화
        isSelect = false;
        GetObject((int)GameObjects.SelectMark).SetActive(isSelect);
        //Managers.Battle.ResetGenerals(); 다른곳에서 호출하는게 나을듯함

    }


}
