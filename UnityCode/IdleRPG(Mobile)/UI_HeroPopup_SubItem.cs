using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_HeroPopup_SubItem : UI_Base
{
    #region Enums
    enum Images
    {
        HeroIcon,
        AttackIcon,
        DefenseIcon,
        HPIcon,
        SkillIcon_1,
        SkillIcon_2,
        SkillIcon_3,
        SkillIcon_4,
    }

    enum Texts
    {
        HeroNameText,
        HeroLevelText,
        AttackText,
        DefenseText,
        HPText,
        ActionButtonText, // 해금/출전/휴식 버튼 텍스트
    }

    enum Buttons
    {
        ActionButton, // 해금/출전/휴식 버튼
    }

    enum GameObjects
    {
        LockedOverlay, // 잠김 상태 오버레이
        DeployedMark, // 출전 중 표시
    }
    #endregion

    #region Fields
    private HeroManager.HeroDisplayInfo _heroInfo;
    private Data.HeroData _heroData;
    private Hero _deployedHero; // 배치된 영웅 인스턴스 (있을 경우)

    private const int UNLOCK_COST_GEM = 500; // 영웅 해금 비용 (임시)
    private const int MAX_SKILL_SLOTS = 4; // 최대 스킬 슬롯 수
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));

        // 버튼 이벤트
        GetButton((int)Buttons.ActionButton).gameObject.BindEvent(OnClickActionButton);

        return true;
    }
    #endregion

    #region Public Methods
    public void SetHeroInfo(HeroManager.HeroDisplayInfo heroInfo, Data.HeroData heroData)
    {
        _heroInfo = heroInfo;
        _heroData = heroData;

        // 배치된 영웅이면 실제 인스턴스 가져오기
        if (_heroInfo.IsDeployed && _heroInfo.InstanceId > 0)
        {
            _deployedHero = Managers.Hero.GetHero(_heroInfo.InstanceId);
        }
        else
        {
            _deployedHero = null;
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_heroData == null)
            return;

        bool isLocked = !_heroInfo.IsUnlocked;

        // 잠김 오버레이
        GetObject((int)GameObjects.LockedOverlay).SetActive(isLocked);

        if (isLocked)
        {
            // 잠긴 영웅
            GetText((int)Texts.HeroNameText).text = "???";
            GetText((int)Texts.HeroLevelText).text = "";
            GetText((int)Texts.AttackText).text = "?";
            GetText((int)Texts.DefenseText).text = "?";
            GetText((int)Texts.HPText).text = "?";
            GetText((int)Texts.ActionButtonText).text = "해금";

            // 아이콘 어둡게
            GetImage((int)Images.HeroIcon).color = new Color(0.3f, 0.3f, 0.3f, 1f);

            // 스킬 아이콘 숨김
            for (int i = 0; i < 4; i++)
            {
                GetImage((int)Images.SkillIcon_1 + i).gameObject.SetActive(false);
            }

            // 출전 마크 숨김
            GetObject((int)GameObjects.DeployedMark).SetActive(false);
        }
        else
        {
            // 해금된 영웅
            GetText((int)Texts.HeroNameText).text = _heroData.characterName;
            GetText((int)Texts.HeroLevelText).text = $"Lv.{_heroInfo.Level}";

            // 영웅 아이콘
            GetImage((int)Images.HeroIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.HeroDataDict[_heroData.characterId].spriteAddress);
            GetImage((int)Images.HeroIcon).color = Color.white;

            // 능력치 표시
            var stats = Managers.Hero.GetHeroStats(_heroData.characterId);

            GetText((int)Texts.AttackText).text = Mathf.FloorToInt(stats.Value.Attack).ToString();
            GetText((int)Texts.DefenseText).text = Mathf.FloorToInt(stats.Value.Defense).ToString();
            GetText((int)Texts.HPText).text = Mathf.FloorToInt(stats.Value.MaxHp).ToString();

            // 버튼 텍스트
            if (_heroInfo.IsDeployed)
            {
                GetText((int)Texts.ActionButtonText).text = "휴식";
                GetObject((int)GameObjects.DeployedMark).SetActive(true);
            }
            else
            {
                GetText((int)Texts.ActionButtonText).text = "출전";
                GetObject((int)GameObjects.DeployedMark).SetActive(false);
            }

            // 스킬 아이콘 표시
            UpdateSkillIcons();
        }
    }

    public HeroManager.HeroDisplayInfo GetHeroInfo()
    {
        return _heroInfo;
    }


    public Data.HeroData GetHeroData()
    {
        return _heroData;
    }
    #endregion

    #region Private Methods

    //private void UpdateSkillIcons()
    //{
    //    if (_deployedHero != null)
    //    {
    //        // 배치된 영웅: 실제 인스턴스에서 스킬 정보 가져오기
    //        for (int i = 0; i < 4; i++)
    //        {
    //            var skillCube = _deployedHero.GetSkillCubeAtSlot(i);
    //            UpdateSkillIconSlot(i, skillCube);
    //        }
    //    }
    //    else if (_heroInfo.IsUnlocked)
    //    {
    //        // 배치되지 않았지만 해금된 영웅: 세이브 데이터에서 스킬 정보 가져오기
    //        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == _heroData.characterId);
    //        if (saveData != null && saveData.skills != null && saveData.skills.Count > 0)
    //        {
    //            // 세이브된 스킬 표시
    //            for (int i = 0; i < 4; i++)
    //            {
    //                // 해당 슬롯에 저장된 스킬 찾기
    //                var skillSave = saveData.skills.Find(s => s.equipSlot == i);

    //                if (skillSave != null)
    //                {
    //                    // 스킬 데이터 가져오기
    //                    var skillData = Managers.Data.SkillDataDict.GetValueOrDefault(skillSave.skillId);
    //                    UpdateSkillIconSlot(i, null, skillData, skillSave.level);
    //                }
    //                else
    //                {
    //                    // 빈 슬롯
    //                    UpdateSkillIconSlot(i, null, null);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // 세이브된 스킬이 없으면 기본 스킬 표시
    //            var skillIds = _heroData.skillIds;
    //            if (skillIds != null)
    //            {
    //                for (int i = 0; i < 4; i++)
    //                {
    //                    if (i < skillIds.Count)
    //                    {
    //                        int skillId = skillIds[i];
    //                        var skillData = Managers.Data.SkillDataDict.GetValueOrDefault(skillId);
    //                        UpdateSkillIconSlot(i, null, skillData, 1);
    //                    }
    //                    else
    //                    {
    //                        UpdateSkillIconSlot(i, null, null);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                // 스킬이 아예 없으면 모두 빈 슬롯
    //                for (int i = 0; i < 4; i++)
    //                {
    //                    UpdateSkillIconSlot(i, null, null);
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        // 잠긴 영웅: 스킬 아이콘 숨김 (이미 RefreshUI에서 처리됨)
    //    }
    //}
    private void UpdateSkillIcons()
    {
        if (_deployedHero != null)
        {
            // 배치된 영웅: 실제 인스턴스에서 스킬 정보 가져오기
            UpdateSkillIconsFromDeployedHero();
        }
        else if (_heroInfo.IsUnlocked)
        {
            // 배치되지 않은 해금 영웅: 세이브 데이터에서 스킬 정보 가져오기
            UpdateSkillIconsFromSaveData();
        }
        // 잠긴 영웅: 스킬 아이콘 숨김 (RefreshUI에서 처리됨)
    }
    private void UpdateSkillIconsFromDeployedHero()
    {
        for (int i = 0; i < MAX_SKILL_SLOTS; i++)
        {
            var skillCube = _deployedHero.GetSkillCubeAtSlot(i);
            UpdateSkillIconSlot(i, skillCube);
        }
    }

    private void UpdateSkillIconsFromSaveData()
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == _heroData.characterId);

        if (saveData?.skills != null && saveData.skills.Count > 0)
        {
            // 저장된 스킬 표시
            UpdateSkillIconsFromSavedSkills(saveData.skills);
        }
        else
        {
            // 기본 스킬 표시
            UpdateSkillIconsFromDefaultSkills();
        }
    }

    private void UpdateSkillIconsFromSavedSkills(List<SkillSaveData> skills)
    {
        for (int i = 0; i < MAX_SKILL_SLOTS; i++)
        {
            var skillSave = skills.Find(s => s.equipSlot == i);

            if (skillSave != null)
            {
                var skillData = Managers.Data.SkillDataDict.GetValueOrDefault(skillSave.skillId);
                UpdateSkillIconSlot(i, null, skillData, skillSave.level);
            }
            else
            {
                UpdateSkillIconSlot(i, null, null);
            }
        }
    }

    private void UpdateSkillIconsFromDefaultSkills()
    {
        var skillIds = _heroData.skillIds;

        if (skillIds == null)
        {
            HideAllSkillIcons();
            return;
        }

        for (int i = 0; i < MAX_SKILL_SLOTS; i++)
        {
            if (i < skillIds.Count)
            {
                int skillId = skillIds[i];
                var skillData = Managers.Data.SkillDataDict.GetValueOrDefault(skillId);
                UpdateSkillIconSlot(i, null, skillData, 1);
            }
            else
            {
                UpdateSkillIconSlot(i, null, null);
            }
        }
    }

    private void HideAllSkillIcons()
    {
        for (int i = 0; i < MAX_SKILL_SLOTS; i++)
        {
            UpdateSkillIconSlot(i, null, null);
        }
    }

    private void UpdateSkillIconSlot(int slotIndex, SkillCube skillCube = null, Data.SkillData skillData = null, int skillLevel = 1)
    {
        if (slotIndex < 0 || slotIndex >= 4)
            return;

        Image iconImage = GetImage((int)Images.SkillIcon_1 + slotIndex);
        GameObject iconObject = iconImage.gameObject;

        // 스킬이 있으면
        if (skillCube != null)
        {
            skillData = skillCube.SkillData;
        }

        if (skillData != null)
        {
            iconObject.SetActive(true);

            //스킬 아이콘 로드
            iconImage.sprite = Managers.Resource.Load<Sprite>(Managers.Data.SkillDataDict[skillData.skillId].skillIcon);
        }
        else
        {
            // 빈 슬롯
            iconObject.SetActive(true);
            iconImage.sprite = null; // 빈 아이콘으로 설정하거나 임시로 null
            iconImage.color = new Color(0.5f, 0.5f, 0.5f, 0.3f); // 회색 반투명
        }
    }
    #endregion

    #region Event Handlers
    private void OnClickActionButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        bool isLocked = !_heroInfo.IsUnlocked;

        if (isLocked)
        {
            // 해금
            HandleUnlock();
        }
        else if (_heroInfo.IsDeployed)
        {
            // 휴식 (배치 해제)
            HandleUndeploy();
        }
        else
        {
            // 출전 (배치)
            HandleDeploy();
        }
    }

    private void HandleUnlock()
    {
        if (_heroData == null)
        {
            Debug.LogError("HeroData is null in HandleUnlock");
            return;
        }
        // 해금 비용 확인
        if (Managers.Game.Gem < UNLOCK_COST_GEM)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "젬 부족",
                message: $"영웅 해금에 필요한 젬이 부족합니다.\n필요: {UNLOCK_COST_GEM} 젬\n보유: {Managers.Game.Gem} 젬"
            );
            return;
        }

        // 해금 확인
        UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        popup.SetInfo(
            title: "영웅 해금",
            message: $"{_heroData.characterName}을(를) 해금하시겠습니까?\n비용: {UNLOCK_COST_GEM} 젬",
            onConfirm: () =>
            {
                // 젬 차감
                Managers.Game.Gem -= UNLOCK_COST_GEM;

                // 영웅 생성
                Managers.Hero.CreateNewHero(_heroData.characterId);

                Debug.Log($"Unlocked hero {_heroData.characterName}");

                // 성공 메시지
                UI_ConfirmPopup successPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                successPopup.SetInfoAsAlert(
                    title: "해금 완료",
                    message: $"{_heroData.characterName}을(를) 해금했습니다!"
                );
            },
            confirmButtonText: "해금",
            cancelButtonText: "취소"
        );
    }

    private void HandleDeploy()
    {
        if (!Managers.Battle.CanDeployHeroes)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "배치 불가",
                message: "전투 준비 중에만 영웅을 배치할 수 있습니다."
            );
            return;
        }

        if (_heroData == null || !_heroInfo.IsUnlocked)
        {
            Debug.LogWarning("Hero is locked or invalid");
            return;
        }

        // 슬롯 선택 팝업 열기
        UI_HeroSlotSelectionPopup slotPopup = Managers.UI.ShowPopupUI<UI_HeroSlotSelectionPopup>();
        slotPopup.SetInfo(_heroData.characterId, (selectedSlot) =>
        {
            Debug.Log($"Hero {_heroData.characterName} deployed to slot {selectedSlot}");
            // UI는 OnHeroDeployed 이벤트로 자동 갱신
        });
    }
    
    private void HandleUndeploy()
    {
        if (!Managers.Battle.CanDeployHeroes)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "배치 해제 불가",
                message: "전투 준비 중에만 영웅을 배치 해제할 수 있습니다."
            );
            return;
        }
        int deployedCount = Managers.Hero.GetDeployedHeroes().Count;
        if (deployedCount <= 1)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "배치 해제 불가",
                message: "최소 1명의 영웅은 전투에 배치되어야 합니다."
            );
            return;
        }
        // 배치 해제 확인
        UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        popup.SetInfo(
            title: "전투 제외",
            message: $"{_heroInfo.Name}을(를) 전투에서 제외하시겠습니까?",
            onConfirm: () =>
            {
                if (!Managers.Battle.CanDeployHeroes)
                {
                    UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    errorPopup.SetInfoAsAlert(
                        title: "배치 해제 불가",
                        message: "전투가 이미 시작되어 배치를 변경할 수 없습니다."
                    );
                    return;
                }

                // 다시 최소 1명 체크 (다른 영웅이 동시에 해제될 수도 있음)
                int currentDeployedCount = Managers.Hero.GetDeployedHeroes().Count;
                if (currentDeployedCount <= 1)
                {
                    UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    errorPopup.SetInfoAsAlert(
                        title: "배치 해제 불가",
                        message: "최소 1명의 영웅은 전투에 배치되어야 합니다."
                    );
                    return;
                }

                // BattleManager를 통해 배치 해제
                Managers.Hero.UndeployHero(_heroInfo.SlotIndex);

                Debug.Log($"Undeployed hero {_heroInfo.Name} from slot {_heroInfo.SlotIndex}");
            },
            confirmButtonText: "제외",
            cancelButtonText: "취소"
        );
    }
    #endregion
}
