using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_ScenarioScene : UI_Scene
{
    enum Buttons
    {
        GeneralListButton,
        InventoryButton,
        ShopButton,
        MenuButton,
        TurnEndButton,
    }

    enum Texts
    {
        TurnText,
        GoldText,
        WoodText,
        StoneText,
        IronText,
        FoodText,
        HorseText,
        ActionPointText,
    }
    // 이미지는 프리펩 속 아이콘에 각자 할당해놔야함 (아이콘 자원이 아직 없음)

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.GeneralListButton).gameObject.BindEvent(OnClickGeneralListButton);
        GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventoryButton);
        GetButton((int)Buttons.ShopButton).gameObject.BindEvent(OnClickShopButton);
        GetButton((int)Buttons.MenuButton).gameObject.BindEvent(OnClickMenuButton);
        GetButton((int)Buttons.TurnEndButton).gameObject.BindEvent(OnClickTurnEnButton);


        Refresh();

        
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
        playerFaction.OnResourceChanged += RefreshResourceText;
        Managers.Game.OnActionPointChanged += RefreshResourceText;

        return true;
    }

    public void SetInfo()
    {
        
        Refresh();
    }


    public void Refresh()
    {
        if (_init == false)
            return;

        RefreshResourceText();
        RefreshTurnText();
    }
    private void OnDisable()
    {
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
        if (playerFaction != null)
        {
            playerFaction.OnResourceChanged -= RefreshResourceText;
        }
        Managers.Game.OnActionPointChanged -= RefreshResourceText;
    }
    private void OnEnable()
    {
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
        if (playerFaction != null)
        {
            playerFaction.OnResourceChanged += RefreshResourceText;
        }
        Managers.Game.OnActionPointChanged += RefreshResourceText;

    }

    void OnClickGeneralListButton(PointerEventData evt)
    {
 
        Managers.UI.CloseAllPopupUI();
        UI_GeneralListPopup popup = Managers.UI.ShowPopupUI<UI_GeneralListPopup>();
        popup.SetInfo();
        
    }

    void OnClickInventoryButton(PointerEventData evt)
    {
        
        Managers.UI.CloseAllPopupUI();
        UI_InventoryPopup popup = Managers.UI.ShowPopupUI<UI_InventoryPopup>();
        popup.SetInfo();
        
    }

    void OnClickShopButton(PointerEventData evt)
    {
        Managers.UI.CloseAllPopupUI();
        UI_ShopPopup popup = Managers.UI.ShowPopupUI<UI_ShopPopup>();
        popup.SetInfo();
    }

    void OnClickMenuButton(PointerEventData evt)
    {
        //UI_MenuPopup popup = Managers.UI.ShowPopupUI<UI_MenuPopup>();

    }

    void OnClickTurnEnButton(PointerEventData evt)
    {
        
        MapViewHighlighter mapViewHighlighter = FindAnyObjectByType<MapViewHighlighter>();
        mapViewHighlighter.OffRegionPopup();
        Managers.UI.CloseAllPopupUI();
        Managers.Turn.NextTurn();
        Refresh();
    }
    public void RefreshTurnText()
    {
        GetText((int)Texts.TurnText).text = Managers.Turn.turn.ToString() + "T";
    }

    public void RefreshResourceText()
    {
        Faction playerFaction = Managers.Game.factions.Find(x => x.FactionID == Managers.Game.selectedFactionId);
        GetText((int)Texts.GoldText).text = playerFaction.GetResourceAmount(EResourceType.Gold).ToString();
        GetText((int)Texts.WoodText).text = playerFaction.GetResourceAmount(EResourceType.Wood).ToString();
        GetText((int)Texts.StoneText).text = playerFaction.GetResourceAmount(EResourceType.Stone).ToString();
        GetText((int)Texts.IronText).text = playerFaction.GetResourceAmount(EResourceType.Iron).ToString();
        GetText((int)Texts.FoodText).text = playerFaction.GetResourceAmount(EResourceType.Food).ToString();
        GetText((int)Texts.HorseText).text = playerFaction.GetResourceAmount(EResourceType.Horse).ToString();
        GetText((int)Texts.ActionPointText).text = "AP: " + Managers.Game.ActionPoint;
    }
    
}
