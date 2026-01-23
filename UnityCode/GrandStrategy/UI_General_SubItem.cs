using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_General_SubItem : UI_Base
{
    enum Buttons
    {
        GeneralButton,
    }
    enum GameObjects
    {
        SelectFrame,
    }

    enum Images
    {
        GeneralImage,
    }

    int _GeneralID = -1;
    bool isSelect = false;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));

        GetButton((int)Buttons.GeneralButton).gameObject.BindEvent(OnClickGeneralButton);

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
        // 무장 정보를 받아와서 UI에 표시
        // 무장 정보를 받아오는 함수를 만들어야함
        if (_init == false)
            return;

        if (_GeneralID < 0)
            return;

        GetImage((int)Images.GeneralImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.GeneralDic[_GeneralID].spriteAddress);
        // 여기서 스프라이트 이미지를 Data에서 가져오긴하는데 뭐.. 타락같은 컨셉으로 유닛이 변하면 그냥 새유닛을 만드는게?
        // 스프라이트 바꾸기보단? 그냥 새로운 무장을 만드는게 나을듯

        GetObject((int)GameObjects.SelectFrame).SetActive(isSelect);
    }

    void OnClickGeneralButton(PointerEventData evt)
    {
        UI_GeneralListPopup popup = Managers.UI.ShowPopupUI<UI_GeneralListPopup>();
        popup.SetGeneralInfo(_GeneralID);
        this.SetSelect(true);
        //if(popup.selectedGeneral != null)
        //{
        //    Managers.UI.ShowPopupUI<UI_GeneralEquipPopup>().SetGeneral(popup.selectedGeneral);
        //}

    }

    public void SetSelect(bool isSelect)
    {
        this.isSelect = isSelect;
        GetObject((int)GameObjects.SelectFrame).SetActive(this.isSelect);
    }

}
