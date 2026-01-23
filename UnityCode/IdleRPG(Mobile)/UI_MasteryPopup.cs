using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_MasteryPopup : UI_Popup
{

    enum GameObjects
    {
        MasteryContent
    }

    enum Texts
    {
        TitleText,
        GoldText,
    }

    private List<UI_Mastery_SubItem> _masteryItems = new List<UI_Mastery_SubItem>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        
        Managers.Mastery.OnMasteryChanged -= RefreshUI;
        Managers.Mastery.OnMasteryChanged += RefreshUI;

        Managers.Game.OnCurrencyChanged -= OnCurrencyChanged;
        Managers.Game.OnCurrencyChanged += OnCurrencyChanged;

        
        GetText((int)Texts.TitleText).text = "마스터리";

        
        CreateMasteryItems();

        RefreshUI();
        UpdateGoldText();

        return true;
    }

    private void OnDestroy()
    {
        Managers.Mastery.OnMasteryChanged -= RefreshUI;
        Managers.Game.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void CreateMasteryItems()
    {
        Transform content = GetObject((int)GameObjects.MasteryContent)?.transform;
        if (content == null)
        {
            Debug.LogError("Mastery content transform not found");
            return;
        }


        for (int i = 1; i <= 6; i++)
        {
            if (Managers.Data.MasteryDic.TryGetValue(i, out Data.MasteryData masteryData))
            {
                UI_Mastery_SubItem item = Managers.UI.MakeSubItem<UI_Mastery_SubItem>(content);
                if (item != null)
                {
                    item.SetInfo(masteryData);
                    _masteryItems.Add(item);
                }
            }
        }
    }

    public void RefreshUI()
    {
        // 모든 마스터리 항목 갱신
        foreach (var item in _masteryItems)
        {
            item.RefreshUI();
        }
    }

    private void UpdateGoldText()
    {
        GetText((int)Texts.GoldText).text = $"Gold: {Managers.Game.Gold}";
    }

    private void OnCurrencyChanged(ECurrencyType type, int amount)
    {
        if (type == ECurrencyType.Gold)
        {
            UpdateGoldText();
            RefreshUI();
        }
    }

}