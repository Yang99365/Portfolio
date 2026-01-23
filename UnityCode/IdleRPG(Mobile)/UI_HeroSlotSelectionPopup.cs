using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

/// <summary>
/// 영웅 배치 슬롯 선택 팝업
/// 4개의 슬롯을 표시하고 빈 슬롯을 선택하여 영웅을 배치합니다.
/// </summary>
public class UI_HeroSlotSelectionPopup : UI_Popup
{
    #region Enums
    enum Texts
    {
        TitleText,
        GuideText
    }

    enum Buttons
    {
        Slot0Button,
        Slot1Button,
        Slot2Button,
        Slot3Button,
        CancelButton,
    }

    enum GameObjects
    {
        Slot0Info,
        Slot1Info,
        Slot2Info,
        Slot3Info,
    }
    #endregion

    #region Fields
    private int _heroTemplateId;
    private Action<int> _onSlotSelected;
    private Dictionary<int, SlotInfo> _slotInfos = new Dictionary<int, SlotInfo>();

    private class SlotInfo
    {
        public GameObject InfoPanel;
        public TextMeshProUGUI HeroNameText;
        public TextMeshProUGUI HeroLevelText;
        public Image HeroIcon;
        public GameObject EmptyMark;
    }
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));

        // 타이틀 설정
        GetText((int)Texts.TitleText).text = "배치 슬롯 선택";
        GetText((int)Texts.GuideText).text = "영웅을 배치할 슬롯을 선택하세요";

        // 버튼 이벤트 바인딩
        GetButton((int)Buttons.Slot0Button).gameObject.BindEvent((evt) => OnClickSlot(evt, 0));
        GetButton((int)Buttons.Slot1Button).gameObject.BindEvent((evt) => OnClickSlot(evt, 1));
        GetButton((int)Buttons.Slot2Button).gameObject.BindEvent((evt) => OnClickSlot(evt, 2));
        GetButton((int)Buttons.Slot3Button).gameObject.BindEvent((evt) => OnClickSlot(evt, 3));
        GetButton((int)Buttons.CancelButton).gameObject.BindEvent(OnClickCancel);

        // 슬롯 정보 초기화
        InitializeSlotInfos();

        return true;
    }

    private void InitializeSlotInfos()
    {
        for (int i = 0; i < BattleManager.MAX_HERO_SLOTS; i++)
        {
            GameObject slotInfoObj = GetObject(i); // Slot0Info, Slot1Info, ...
            if (slotInfoObj != null)
            {
                var slotInfo = new SlotInfo
                {
                    InfoPanel = slotInfoObj,
                    HeroNameText = Util.FindChild<TextMeshProUGUI>(slotInfoObj, "HeroNameText", true),
                    HeroLevelText = Util.FindChild<TextMeshProUGUI>(slotInfoObj, "HeroLevelText", true),
                    HeroIcon = Util.FindChild<Image>(slotInfoObj, "HeroIcon", true),
                    EmptyMark = Util.FindChild(slotInfoObj, "EmptyMark", true)
                };

                _slotInfos[i] = slotInfo;
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 팝업 설정
    /// </summary>
    /// <param name="heroTemplateId">배치할 영웅의 템플릿 ID</param>
    /// <param name="onSlotSelected">슬롯 선택 시 호출될 콜백</param>
    public void SetInfo(int heroTemplateId, Action<int> onSlotSelected)
    {
        _heroTemplateId = heroTemplateId;
        _onSlotSelected = onSlotSelected;

        RefreshUI();
    }

    private void RefreshUI()
    {
        // 각 슬롯의 상태를 표시
        var deployedHeroes = Managers.Hero.GetDeployedHeroes();

        for (int slotIndex = 0; slotIndex < BattleManager.MAX_HERO_SLOTS; slotIndex++)
        {
            if (!_slotInfos.TryGetValue(slotIndex, out var slotInfo))
                continue;

            // 해당 슬롯에 배치된 영웅 찾기
            var hero = deployedHeroes.Find(h => h.SlotIndex == slotIndex);

            if (hero != null)
            {
                // 영웅이 배치된 슬롯
                ShowOccupiedSlot(slotInfo, hero);
            }
            else
            {
                // 빈 슬롯
                ShowEmptySlot(slotInfo, slotIndex);
            }
        }
    }

    private void ShowOccupiedSlot(SlotInfo slotInfo, Hero hero)
    {
        if (slotInfo.HeroNameText != null)
            slotInfo.HeroNameText.text = hero.HeroData.characterName;

        if (slotInfo.HeroLevelText != null)
            slotInfo.HeroLevelText.text = $"Lv.{hero.Level}";

        if (slotInfo.HeroIcon != null)
        {
            // TODO: 영웅 아이콘 로드
            slotInfo.HeroIcon.gameObject.SetActive(true);
            slotInfo.HeroIcon.color = Color.white;
        }

        if (slotInfo.EmptyMark != null)
            slotInfo.EmptyMark.SetActive(false);
    }

    private void ShowEmptySlot(SlotInfo slotInfo, int slotIndex)
    {
        if (slotInfo.HeroNameText != null)
            slotInfo.HeroNameText.text = "빈 슬롯";

        if (slotInfo.HeroLevelText != null)
            slotInfo.HeroLevelText.text = $"슬롯 {slotIndex + 1}";

        if (slotInfo.HeroIcon != null)
        {
            slotInfo.HeroIcon.gameObject.SetActive(false);
        }

        if (slotInfo.EmptyMark != null)
            slotInfo.EmptyMark.SetActive(true);
    }
    #endregion

    #region Event Handlers
    private void OnClickSlot(PointerEventData evt, int slotIndex)
    {
        if (!Managers.Battle.CanDeployHeroes)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "배치 불가",
                message: "전투가 이미 시작되어 배치를 변경할 수 없습니다."
            );
            return;
        }

        // 해당 슬롯에 이미 영웅이 있는지 확인
        var deployedHeroes = Managers.Hero.GetDeployedHeroes();
        var existingHero = deployedHeroes.Find(h => h.SlotIndex == slotIndex);

        if (existingHero != null)
        {
            // 이미 영웅이 있는 슬롯 - 교체 확인
            ShowReplaceConfirmation(existingHero, slotIndex);
        }
        else
        {
            // 빈 슬롯 - 바로 배치
            DeployHeroToSlot(slotIndex);
        }
    }

    private void ShowReplaceConfirmation(Hero existingHero, int slotIndex)
    {
        var heroData = Managers.Data.HeroDataDict.GetValueOrDefault(_heroTemplateId);
        if (heroData == null) return;

        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "영웅 교체",
            message: $"슬롯 {slotIndex + 1}에 배치된 {existingHero.HeroData.characterName}을(를)\n" +
                     $"{heroData.characterName}(으)로 교체하시겠습니까?",
            onConfirm: () =>
            {
                if (!Managers.Battle.CanDeployHeroes)
                {
                    UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    errorPopup.SetInfoAsAlert(
                        title: "교체 불가",
                        message: "전투가 이미 시작되어 영웅을 교체할 수 없습니다."
                    );
                    return;
                }

                // 기존 영웅 제거
                Managers.Battle.RemoveHeroFromSlot(slotIndex);

                // 새 영웅 배치
                DeployHeroToSlot(slotIndex);
            },
            confirmButtonText: "교체",
            cancelButtonText: "취소"
        );
    }

    private void DeployHeroToSlot(int slotIndex)
    {
        if (!Managers.Battle.CanDeployHeroes)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "교체 불가",
                message: "전투가 이미 시작되어 영웅을 교체할 수 없습니다."
            );
            return;
        }

        // 영웅 배치
        bool success = Managers.Battle.DeployHero(_heroTemplateId, slotIndex);

        if (success)
        {
            // 콜백 호출
            _onSlotSelected?.Invoke(slotIndex);

            // 팝업 닫기
            ClosePopupUI();

            Debug.Log($"Hero {_heroTemplateId} deployed to slot {slotIndex}");
        }
        else
        {
            // 실패 메시지
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "배치 실패",
                message: "영웅을 배치할 수 없습니다."
            );
        }
    }

    private void OnClickCancel(PointerEventData evt)
    {
        ClosePopupUI();
    }
    #endregion
}
