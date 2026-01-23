using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_HeroSkillSlotPopup : UI_Popup
{
    #region Enums
    enum Buttons
    {
        CloseButton,
        Hero1Button,
        Hero2Button,
        Hero3Button,
        Hero4Button,
        SkillSlot0,
        SkillSlot1,
        SkillSlot2,
        SkillSlot3,
    }

    enum Texts
    {
        TitleText,
        SelectedHeroNameText,
        Slot0SkillNameText,
        Slot1SkillNameText,
        Slot2SkillNameText,
        Slot3SkillNameText,
        Slot0SkillLevelText,
        Slot1SkillLevelText,
        Slot2SkillLevelText,
        Slot3SkillLevelText,
        GuideText,
    }

    enum GameObjects
    {
        HeroListPanel,
        SkillSlotsPanel,
    }

    enum Images
    {
        SelectedHeroIcon,
        Slot0SkillIcon,
        Slot1SkillIcon,
        Slot2SkillIcon,
        Slot3SkillIcon,
    }
    #endregion

    #region Fields
    private SkillCube _skillCubeToEquip; // 장착할 스킬큐브
    private Hero _selectedHero;
    private int _selectedSlotIndex = -1;

    // 영웅 버튼들
    private List<Button> _heroButtons = new List<Button>();
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
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        // Hero selection buttons
        GetButton((int)Buttons.Hero1Button).gameObject.BindEvent((evt) => OnClickHeroButton(evt, 0));
        GetButton((int)Buttons.Hero2Button).gameObject.BindEvent((evt) => OnClickHeroButton(evt, 1));
        GetButton((int)Buttons.Hero3Button).gameObject.BindEvent((evt) => OnClickHeroButton(evt, 2));
        GetButton((int)Buttons.Hero4Button).gameObject.BindEvent((evt) => OnClickHeroButton(evt, 3));

        // Skill slot buttons
        GetButton((int)Buttons.SkillSlot0).gameObject.BindEvent((evt) => OnClickSkillSlot(evt, 0));
        GetButton((int)Buttons.SkillSlot1).gameObject.BindEvent((evt) => OnClickSkillSlot(evt, 1));
        GetButton((int)Buttons.SkillSlot2).gameObject.BindEvent((evt) => OnClickSkillSlot(evt, 2));
        GetButton((int)Buttons.SkillSlot3).gameObject.BindEvent((evt) => OnClickSkillSlot(evt, 3));

        // Initialize
        GetText((int)Texts.TitleText).text = "스킬 장착";
        GetText((int)Texts.GuideText).text = "영웅을 선택한 후 스킬 슬롯을 클릭하세요";

        // 영웅 버튼 리스트 구성
        _heroButtons.Add(GetButton((int)Buttons.Hero1Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero2Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero3Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero4Button));

        return true;
    }

    /// <summary>
    /// 팝업을 열 때 장착할 스킬큐브를 설정합니다.
    /// </summary>
    public void SetSkillCubeToEquip(SkillCube skillCube)
    {
        _skillCubeToEquip = skillCube;

        if (skillCube != null)
        {
            GetText((int)Texts.GuideText).text =
                $"[{skillCube.SkillData.skillName}] 을(를) 장착할 영웅과 슬롯을 선택하세요";
        }

        RefreshUI();
    }
    #endregion

    #region UI Refresh
    private void RefreshUI()
    {
        if (!_init) return;

        // 배치된 영웅 목록 표시
        UpdateHeroList();

        // 선택된 영웅의 스킬 슬롯 표시
        if (_selectedHero != null)
        {
            UpdateSkillSlots();
        }
        else
        {
            // 영웅이 선택되지 않으면 스킬 슬롯 패널 비활성화
            GetObject((int)GameObjects.SkillSlotsPanel).SetActive(false);
        }
    }

    
    private void UpdateHeroList()
    {
        for (int slotIndex = 0; slotIndex < _heroButtons.Count; slotIndex++)
        {
            // 슬롯 인덱스로 직접 조회
            Hero hero = Managers.Hero.GetHeroAtSlot(slotIndex);

            if (hero != null)
            {
                _heroButtons[slotIndex].gameObject.SetActive(true);

                var buttonText = _heroButtons[slotIndex].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = hero.HeroData.characterName;
                }

                bool canEquip = true;
                if (_skillCubeToEquip != null)
                {
                    canEquip = _skillCubeToEquip.CanEquipToHero(hero);
                }

                _heroButtons[slotIndex].interactable = canEquip;
            }
            else
            {
                // 빈 슬롯 처리
                _heroButtons[slotIndex].gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSkillSlots()
    {
        if (_selectedHero == null) return;

        GetObject((int)GameObjects.SkillSlotsPanel).SetActive(true);

        // 선택된 영웅 정보 표시
        GetText((int)Texts.SelectedHeroNameText).text = _selectedHero.HeroData.characterName;

        // 영웅의 장착된 스킬 가져오기
        var equippedSkills = _selectedHero.GetEquippedSkillCubes();

        // 각 스킬 슬롯 업데이트
        for (int slotIndex = 0; slotIndex < Hero.MAX_SKILL_SLOTS; slotIndex++)
        {
            UpdateSkillSlotUI(slotIndex, equippedSkills);
        }
    }

    private void UpdateSkillSlotUI(int slotIndex, Dictionary<int, SkillCube> equippedSkills)
    {
        // 해당 슬롯에 장착된 스킬큐브 가져오기
        SkillCube equippedCube = null;
        equippedSkills.TryGetValue(slotIndex, out equippedCube);

        // 스킬 이름 텍스트
        var nameText = GetText((int)Enum.Parse(typeof(Texts), $"Slot{slotIndex}SkillNameText"));
        // 스킬 레벨 텍스트
        var levelText = GetText((int)Enum.Parse(typeof(Texts), $"Slot{slotIndex}SkillLevelText"));
        // 스킬 아이콘
        var iconImage = GetImage((int)Enum.Parse(typeof(Images), $"Slot{slotIndex}SkillIcon"));

        if (equippedCube != null)
        {
            // 스킬이 장착되어 있는 경우
            nameText.text = equippedCube.SkillData.skillName;
            levelText.text = $"Lv.{equippedCube.Level}";

            //실제 스킬 아이콘 로드
            iconImage.color = GetSkillTypeColor(equippedCube.SkillType);
            iconImage.sprite = Managers.Resource.Load<Sprite>(equippedCube.SkillData.skillIcon);
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            // 빈 슬롯
            nameText.text = "빈 슬롯";
            levelText.text = "";
            iconImage.gameObject.SetActive(false);
        }
    }

    private Color GetSkillTypeColor(ESkillType skillType)
    {
        return skillType switch
        {
            ESkillType.Active => new Color(1f, 0.5f, 0.5f),
            ESkillType.Passive => new Color(0.5f, 1f, 0.5f),
            _ => Color.white
        };
    }
    #endregion

    #region Event Handlers
    private void OnClickHeroButton(PointerEventData evt, int heroIndex)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        Hero hero = Managers.Hero.GetHeroAtSlot(heroIndex);

        if (hero == null)
        {
            Debug.LogWarning($"No hero at slot {heroIndex}");
            return;
        }

        if (_skillCubeToEquip != null && !_skillCubeToEquip.CanEquipToHero(hero))
        {
            // 장착 불가 메시지
            return;
        }

        _selectedHero = hero;
        GetText((int)Texts.SelectedHeroNameText).text = hero.HeroData.characterName;

        GetObject((int)GameObjects.SkillSlotsPanel).SetActive(true);
        UpdateSkillSlots();
    }

    private void OnClickSkillSlot(PointerEventData evt, int slotIndex)
    {
        if (_selectedHero == null)
        {
            Debug.LogWarning("영웅을 먼저 선택하세요!");
            return;
        }

        if (_skillCubeToEquip == null)
        {
            Debug.LogWarning("장착할 스킬큐브가 없습니다!");
            return;
        }

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        _selectedSlotIndex = slotIndex;

        // 해당 슬롯에 이미 스킬이 장착되어 있는지 확인
        SkillCube currentSkill = _selectedHero.GetSkillCubeAtSlot(slotIndex);

        if (currentSkill != null)
        {
            // 스킬 교체 확인
            ShowReplaceConfirmation(currentSkill, slotIndex);
        }
        else
        {
            // 빈 슬롯에 바로 장착
            EquipSkillCube(slotIndex);
        }
    }

    private void ShowReplaceConfirmation(SkillCube oldSkill, int slotIndex)
    {
        string message = $"현재 장착된 스킬:\n{oldSkill.SkillData.skillName} (Lv.{oldSkill.Level})\n\n";
        message += $"새로 장착할 스킬:\n{_skillCubeToEquip.SkillData.skillName} (Lv.{_skillCubeToEquip.Level})\n\n";
        message += "스킬을 교체하시겠습니까?";

        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "스킬 교체",
            message: message,
            onConfirm: () =>
            {
                EquipSkillCube(slotIndex);
            },
            onCancel: () =>
            {
                Debug.Log("스킬 교체 취소");
            },
            confirmButtonText: "교체",
            cancelButtonText: "취소"
        );
    }

    private void EquipSkillCube(int slotIndex)
    {
        if (_selectedHero == null || _skillCubeToEquip == null)
            return;

        // HeroManager를 통해 스킬 장착
        bool success = Managers.Hero.EquipSkillToHero(
            _selectedHero.HeroInstanceId,
            _skillCubeToEquip.InstanceId,
            slotIndex
        );

        if (success)
        {
            Debug.Log($"스킬 장착 성공: {_skillCubeToEquip.SkillData.skillName} → {_selectedHero.HeroData.characterName}");

            // UI 갱신
            RefreshUI();

            // 성공 메시지 (선택적)
            // ShowSuccessMessage();

            // 팝업 닫기
            ClosePopupUI();
        }
        else
        {
            Debug.LogError("스킬 장착 실패!");

            // 실패 메시지
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "오류",
                message: "스킬을 장착할 수 없습니다.\n조건을 확인해주세요."
            );
        }
    }

    private void OnClickCloseButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ClosePopupUI();
    }
    #endregion

    #region Cleanup
    public override void ClosePopupUI()
    {
        // 초기화
        _skillCubeToEquip = null;
        _selectedHero = null;
        _selectedSlotIndex = -1;

        base.ClosePopupUI();
    }
    #endregion
}