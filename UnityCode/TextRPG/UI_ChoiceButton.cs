using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using static Define;
using System.Collections.Generic;

public class UI_ChoiceButton : UI_Base
{
    #region UI Enums
    enum Buttons
    {
        ChoiceButton,
    }

    enum Texts
    {
        ChoiceText,         // 선택지 내용
        RequirementText,    // 요구 조건 (스탯, 아이템)
    }

    enum GameObjects
    {
        RequirementPanel,   // 요구 조건 패널
    }
    #endregion

    #region Fields
    private Data.ChoiceData _choiceData;
    private int _choiceIndex;
    private Action _onClickCallback;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        // 버튼 이벤트
        GetButton((int)Buttons.ChoiceButton).gameObject.BindEvent(OnClickChoice);

        return true;
    }
    #endregion

    #region Public Methods
    public void SetInfo(Data.ChoiceData choiceData, int choiceIndex, Action onClickCallback)
    {
        _choiceData = choiceData;
        _choiceIndex = choiceIndex;
        _onClickCallback = onClickCallback;

        RefreshUI();
    }

    public void SetInteractable(bool interactable)
    {
        GetButton((int)Buttons.ChoiceButton).interactable = interactable;
    }
    #endregion

    #region UI Refresh
    private void RefreshUI()
    {
        if (_choiceData == null)
            return;

        // 선택지 텍스트
        GetText((int)Texts.ChoiceText).text = _choiceData.choiceText;

        // 요구 조건 체크
        bool canSelect = CheckRequirements();
        SetInteractable(canSelect);

        // 요구 조건 텍스트 표시
        DisplayRequirements();
    }

    

    private void DisplayRequirements()
    {
        var requirementPanel = GetObject((int)GameObjects.RequirementPanel);
        var requirementText = GetText((int)Texts.RequirementText);

        string requirementStr = "";

        // 스탯 요구 조건
        if (_choiceData.requiredStat != EStatType.None && _choiceData.requiredStatValue > 0)
        {
            int playerStat = Managers.Player.GetStat(_choiceData.requiredStat);
            string statName = GetStatName(_choiceData.requiredStat);

            if (playerStat >= _choiceData.requiredStatValue)
            {
                // 성공 가능
                requirementStr = $"[{statName} {_choiceData.requiredStatValue}] <color=green>✓</color>";
            }
            else
            {
                // 실패 예상
                requirementStr = $"[{statName} {_choiceData.requiredStatValue}] <color=red>✗</color>";
            }

            // 크리티컬 성공 가능
            if (_choiceData.criticalStatValue > 0 && playerStat >= _choiceData.criticalStatValue)
            {
                requirementStr += " <color=yellow>★</color>"; // 크리티컬 마크
            }
        }

        // 아이템 요구 조건
        if (_choiceData.requiredItemId > 0)
        {
            //var itemData = Managers.Data.ItemDict[_choiceData.requiredItemId];
            var itemData = Managers.Data.ItemDict.GetValueOrDefault(_choiceData.requiredItemId);
            if (itemData != null)
            {
                bool hasItem = Managers.Inventory.HasItem(_choiceData.requiredItemId, _choiceData.requiredItemCount);
                string itemName = itemData.itemName;

                if (requirementStr.Length > 0)
                    requirementStr += " ";

                if (hasItem)
                {
                    requirementStr += $"[{itemName} x{_choiceData.requiredItemCount}] <color=green>✓</color>";
                }
                else
                {
                    requirementStr += $"[{itemName} x{_choiceData.requiredItemCount}] <color=red>✗</color>";
                }

                if (_choiceData.consumeItem)
                {
                    requirementStr += " <color=orange>(소비)</color>";
                }
            }
        }

        // 요구 조건이 있으면 패널 표시
        if (!string.IsNullOrEmpty(requirementStr))
        {
            requirementPanel.SetActive(true);
            requirementText.text = requirementStr;
        }
        else
        {
            requirementPanel.SetActive(false);
        }
    }

    private string GetStatName(EStatType statType)
    {
        return statType switch
        {
            EStatType.Strength => "힘",
            EStatType.Intelligence => "지능",
            EStatType.Agility => "민첩",
            EStatType.Charisma => "매력",
            _ => statType.ToString()
        };
    }
    #endregion

    #region Event Handlers
    private void OnClickChoice(PointerEventData evt)
    {
        // ⭐ 다시 한 번 체크
        bool canSelect = CheckRequirements();

        if (!canSelect)
        {
            Debug.LogWarning("Cannot select this choice - requirements not met!");
            return; // 클릭 무시
        }

        Debug.Log($"Choice {_choiceIndex} clicked: {_choiceData.choiceText}");
        _onClickCallback?.Invoke();
    }

    private bool CheckRequirements()
    {
        // 아이템 체크
        if (_choiceData.requiredItemId > 0)
        {
            if (!Managers.Inventory.HasItem(_choiceData.requiredItemId, _choiceData.requiredItemCount))
            {
                return false;
            }
        }

        // 스탯 체크는 실패해도 선택 가능
        return true;
    }
    #endregion
}
