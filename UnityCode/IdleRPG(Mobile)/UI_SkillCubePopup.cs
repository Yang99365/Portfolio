using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_SkillCubePopup : UI_Popup
{
    #region Enums
    enum Buttons
    {
        AllButton,
        ActiveButton,
        PassiveButton,
        EquipButton,
        SellButton,
        EnhanceButton,
    }

    enum Texts
    {
        TitleText,
        SlotCountText,
        SkillNameText,
        SkillLevelText,
        SkillRarityText,
        SkillDescText,
        SkillCooldownText,
        SkillEffectsText,
    }

    enum GameObjects
    {
        SkillCubeContent,
        SkillDetailPanel,
        FilterButtons,
    }

    enum Images
    {
        SkillIcon,
    }
    #endregion

    #region Fields
    private List<UI_SkillCube_SubItem> _skillCubeSlots = new List<UI_SkillCube_SubItem>();
    private ESkillType _currentFilter = ESkillType.None; //None = All
    private UI_SkillCube_SubItem _selectedSlot = null;
    private SkillCube _selectedSkillCube = null;
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
        BindImages(typeof(Images));

        // Button events
        
        GetButton((int)Buttons.AllButton).gameObject.BindEvent(OnClickAllButton);
        GetButton((int)Buttons.ActiveButton).gameObject.BindEvent(OnClickActiveButton);
        GetButton((int)Buttons.PassiveButton).gameObject.BindEvent(OnClickPassiveButton);
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        GetButton((int)Buttons.EnhanceButton).gameObject.BindEvent(OnClickEnhanceButton);
        GetButton((int)Buttons.SellButton).gameObject.BindEvent(OnClickSellButton);

        // Initialize UI
        GetText((int)Texts.TitleText).text = "스킬큐브";
        GetObject((int)GameObjects.SkillDetailPanel).SetActive(false);

        // Create skill cube slots
        CreateSkillCubeSlots();

        // Register events
        Managers.Inventory.OnSkillCubeAdded -= OnSkillCubeInventoryChanged;
        Managers.Inventory.OnSkillCubeAdded += OnSkillCubeInventoryChanged;
        Managers.Inventory.OnSkillCubeRemoved -= OnSkillCubeInventoryChanged;
        Managers.Inventory.OnSkillCubeRemoved += OnSkillCubeInventoryChanged;

        // Initial refresh
        RefreshUI();

        return true;
    }

    private void CreateSkillCubeSlots()
    {
        GameObject content = GetObject((int)GameObjects.SkillCubeContent);

        // Clear existing slots
        foreach (Transform child in content.transform)
        {
            Managers.Resource.Destroy(child.gameObject);
        }
        _skillCubeSlots.Clear();

        // Create new slots
        int slotCount = InventoryManager.DEFAULT_SKILLCUBE_SIZE;
        for (int i = 0; i < slotCount; i++)
        {
            UI_SkillCube_SubItem subItem = Managers.UI.MakeSubItem<UI_SkillCube_SubItem>(content.transform);
            subItem.SlotIndex = i;
            subItem.OnSkillCubeClicked -= OnSlotClicked;
            subItem.OnSkillCubeClicked += OnSlotClicked;
            _skillCubeSlots.Add(subItem);
        }
    }
    #endregion

    #region UI Refresh
    public void RefreshUI()
    {
        if (_init == false)
            return;

        // Update filter buttons highlight
        UpdateFilterButtons();

        // Update skill cube slots
        UpdateSkillCubeSlots();

        // Update slot count
        UpdateSlotCount();

        // Update skill detail if selected
        if (_selectedSkillCube != null)
        {
            UpdateSkillDetail(_selectedSkillCube);
        }
        else
        {
            UpdateSkillDetail(null);
        }
    }

    private void UpdateFilterButtons()
    {
        // Reset all button colors
        GetButton((int)Buttons.AllButton).GetComponent<Image>().color = Color.white;
        GetButton((int)Buttons.ActiveButton).GetComponent<Image>().color = Color.white;
        GetButton((int)Buttons.PassiveButton).GetComponent<Image>().color = Color.white;

        // Highlight selected filter
        Color highlightColor = Color.yellow;
        switch (_currentFilter)
        {
            case ESkillType.None: // All
                GetButton((int)Buttons.AllButton).GetComponent<Image>().color = highlightColor;
                break;
            case ESkillType.Active:
                GetButton((int)Buttons.ActiveButton).GetComponent<Image>().color = highlightColor;
                break;
            case ESkillType.Passive:
                GetButton((int)Buttons.PassiveButton).GetComponent<Image>().color = highlightColor;
                break;
        }
    }

    private void UpdateSkillCubeSlots()
    {
        List<SkillCube> skillCubes = Managers.Inventory.SkillCubes;

        for (int i = 0; i < _skillCubeSlots.Count; i++)
        {
            if (i < skillCubes.Count && skillCubes[i] != null)
            {
                SkillCube cube = skillCubes[i];
                _skillCubeSlots[i].SetSkillCube(cube);

                // Apply filter
                bool shouldShow = ShouldShowSkillCube(cube);
                _skillCubeSlots[i].SetActiveState(shouldShow);
            }
            else
            {
                // 빈 슬롯 처리
                _skillCubeSlots[i].SetSkillCube(null);
                _skillCubeSlots[i].SetActiveState(true);

                // 선택된 슬롯이 비워진 경우
                if (_selectedSlot == _skillCubeSlots[i])
                {
                    ClearSelection();
                }
            }
        }
    }

    private bool ShouldShowSkillCube(SkillCube cube)
    {
        if (cube == null) return true;

        // 필터 적용
        if (_currentFilter == ESkillType.None) // All
            return true;

        return cube.SkillType == _currentFilter;
    }

    private void UpdateSlotCount()
    {
        int currentCount = Managers.Inventory.SkillCubeCount;
        int maxCount = InventoryManager.DEFAULT_SKILLCUBE_SIZE;
        GetText((int)Texts.SlotCountText).text = $"{currentCount}/{maxCount}";

        if (currentCount >= maxCount)
        {
            GetText((int)Texts.SlotCountText).color = Color.red;
        }
        else
        {
            GetText((int)Texts.SlotCountText).color = Color.black;
        }
    }

    private void UpdateSkillDetail(SkillCube cube)
    {
        if (cube == null)
        {
            GetObject((int)GameObjects.SkillDetailPanel).SetActive(false);
            return;
        }

        GetObject((int)GameObjects.SkillDetailPanel).SetActive(true);

        // 스킬 기본 정보
        GetText((int)Texts.SkillNameText).text = cube.SkillData.skillName;
        GetText((int)Texts.SkillLevelText).text = $"Lv.{cube.Level}";
        GetText((int)Texts.SkillRarityText).text = cube.Rarity.ToString();

        // 희귀도 색상 적용
        GetText((int)Texts.SkillRarityText).color = GetRarityColor(cube.Rarity);

        // 스킬 설명
        GetText((int)Texts.SkillDescText).text = cube.SkillData.description;

        // 쿨다운 정보
        string cooldownText = cube.SkillType == ESkillType.Active
            ? $"쿨다운: {cube.SkillData.cooldown}초"
            : "패시브 스킬";
        GetText((int)Texts.SkillCooldownText).text = cooldownText;

        // 스킬 아이콘
        GetImage((int)Images.SkillIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.SkillDataDict[_selectedSkillCube.DataId].skillIcon);


        // 스킬 효과 목록
        string effectsText = GetSkillEffectsText(cube);
        GetText((int)Texts.SkillEffectsText).text = effectsText;

        // 버튼 활성화
        GetButton((int)Buttons.EquipButton).gameObject.SetActive(true);
        GetButton((int)Buttons.EnhanceButton).gameObject.SetActive(true);
    }

    private string GetSkillEffectsText(SkillCube cube)
    {
        if (cube?.SkillData?.effects == null || cube.SkillData.effects.Count == 0)
            return "효과 없음";

        string effectsText = "[ 효과 ]\n";

        foreach (var effect in cube.SkillData.effects)
        {
            string effectDesc = "";

            switch (effect.effectType)
            {
                case ESkillEffectType.Damage:
                    effectDesc = $"• 데미지: 공격력의 {effect.value * 100:F0}%";
                    break;
                case ESkillEffectType.Heal:
                    effectDesc = $"• 회복: 최대 체력의 {effect.value * 100:F0}%";
                    break;
                case ESkillEffectType.Buff:
                    effectDesc = $"• 버프: {effect.value * 100:F0}% 증가 ({effect.duration}초)";
                    break;
                case ESkillEffectType.DeBuff:
                    effectDesc = $"• 디버프: {effect.value * 100:F0}% 감소 ({effect.duration}초)";
                    break;
                case ESkillEffectType.Summon:
                    effectDesc = $"• 소환: {effect.duration}초 동안";
                    break;
            }

            // 타겟 타입 추가
            string targetText = effect.skillTargetType switch
            {
                ESkillTargetType.Single => " (단일)",
                ESkillTargetType.Multi => " (다수)",
                ESkillTargetType.Self => " (자신)",
                ESkillTargetType.Random => " (무작위)",
                _ => ""
            };

            effectsText += effectDesc + targetText + "\n";
        }

        return effectsText;
    }

    private Color GetRarityColor(ESkillRairity rarity)
    {
        return rarity switch
        {
            ESkillRairity.Common => Color.gray,
            ESkillRairity.Rare => new Color(0.3f, 0.6f, 1f),      // Blue
            ESkillRairity.Unique => new Color(0.6f, 0.3f, 1f),    // Purple
            ESkillRairity.Epic => new Color(1f, 0.5f, 0f),        // Orange
            ESkillRairity.Legend => new Color(1f, 0.9f, 0f),      // Gold
            _ => Color.white
        };
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }

        _selectedSkillCube = null;
        UpdateSkillDetail(null);
    }

    private void ChangeFilter(ESkillType filterType)
    {
        if (_currentFilter == filterType)
        {
            RefreshUI();
            return;
        }

        _currentFilter = filterType;

        // 현재 선택된 스킬큐브가 필터에 맞지 않으면 선택 해제
        if (_selectedSkillCube != null)
        {
            bool shouldShow = ShouldShowSkillCube(_selectedSkillCube);
            if (!shouldShow)
            {
                ClearSelection();
            }
        }

        RefreshUI();
    }
    #endregion

    #region Event Handlers
    private void OnSkillCubeInventoryChanged(SkillCube cube)
    {
        RefreshUI();
    }

    private void OnSlotClicked(UI_SkillCube_SubItem slot, SkillCube cube)
    {
        // 빈 슬롯 클릭 시 선택 해제
        if (cube == null)
        {
            ClearSelection();
            return;
        }

        // 선택 업데이트
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
        }

        _selectedSlot = slot;
        _selectedSkillCube = cube;

        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(true);
        }

        // 상세 정보 업데이트
        UpdateSkillDetail(cube);
    }
    #endregion

    #region Button Click Handlers
    
    private void OnClickAllButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ChangeFilter(ESkillType.None); // None = All
    }

    private void OnClickActiveButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ChangeFilter(ESkillType.Active);
    }

    private void OnClickPassiveButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ChangeFilter(ESkillType.Passive);
    }

    private void OnClickEquipButton(PointerEventData evt)
    {
        if (_selectedSkillCube == null) return;

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        // TODO: 영웅 선택 팝업 표시
        Debug.Log($"Equip SkillCube: {_selectedSkillCube.GetName()}");

        // 영웅 스킬 슬롯 선택 팝업을 여기서 열 예정
        UI_HeroSkillSlotPopup popup = Managers.UI.ShowPopupUI<UI_HeroSkillSlotPopup>();
        popup.SetSkillCubeToEquip(_selectedSkillCube);

        ClearSelection();
    }

    private void OnClickEnhanceButton(PointerEventData evt)
    {
        if (_selectedSkillCube == null) return;

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        // TODO: 스킬 강화 기능 (나중에 구현)
        Debug.Log($"Enhance SkillCube: {_selectedSkillCube.GetName()}");

        // 강화 확인 팝업
        UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        popup.SetInfo(
            title: "스킬 강화",
            message: $"{_selectedSkillCube.GetName()}\n강화 기능은 아직 구현되지 않았습니다.",
            onConfirm: () => { /* TODO */ },
            confirmButtonText: "확인",
            cancelButtonText: "취소"
        );

        RefreshUI();
    }
    private void OnClickSellButton(PointerEventData evt)
    {
        if ( _selectedSkillCube == null ) return;

        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        Debug.Log($"Sell SkillCube: {_selectedSkillCube.GetName()}");
        // 판매 확인 팝업(등급에 따라 젬 제공)
        UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        popup.SetInfo(
            title: "스킬큐브 판매",
            message: $"{_selectedSkillCube.GetName()}을(를) 판매하시겠습니까?\n판매 후에는 복구할 수 없습니다.",
            onConfirm: () =>
            {
                int gemReward = _selectedSkillCube.Rarity switch
                {
                    ESkillRairity.Common => 5,
                    ESkillRairity.Rare => 15,
                    ESkillRairity.Unique => 30,
                    ESkillRairity.Epic => 60,
                    ESkillRairity.Legend => 100,
                    _ => 0
                };
                Managers.Game.Gem += gemReward;
                Managers.Inventory.RemoveSkillCube(_selectedSkillCube.InstanceId);

                // 판매 완료 팝업
                
                // 선택 해제 및 UI 갱신
                ClearSelection();
                RefreshUI();
            },
            onCancel: () =>
            {
                // 취소 시 아무 동작 없음
            },
            confirmButtonText: "판매",
            cancelButtonText: "취소"
        );
    }
    #endregion

    #region Cleanup
    public override void ClosePopupUI()
    {
        // Unregister events
        if (Managers.Inventory != null)
        {
            Managers.Inventory.OnSkillCubeAdded -= OnSkillCubeInventoryChanged;
            Managers.Inventory.OnSkillCubeRemoved -= OnSkillCubeInventoryChanged;
        }

        foreach (var slot in _skillCubeSlots)
        {
            if (slot != null)
            {
                slot.OnSkillCubeClicked -= OnSlotClicked;
            }
        }

        base.ClosePopupUI();
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (Managers.Inventory != null)
        {
            Managers.Inventory.OnSkillCubeAdded -= OnSkillCubeInventoryChanged;
            Managers.Inventory.OnSkillCubeRemoved -= OnSkillCubeInventoryChanged;
        }

        // 슬롯 이벤트 해제
        if (_skillCubeSlots != null)
        {
            foreach (var slot in _skillCubeSlots)
            {
                if (slot != null)
                {
                    slot.OnSkillCubeClicked -= OnSlotClicked;
                }
            }
        }
    }
    #endregion
}
