using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Mastery_SubItem : UI_Base
{
    enum Buttons
    {
        UpgradeButton
    }

    enum Texts
    {
        MasteryNameText,
        CurrentLevelText,
        CurrentBonusText,
        NextBonusText,
        CostText,
        MaxLevelText  // "MAX" 표시용
    }

    enum Images
    {
        MasteryIcon // Init 할떄 설정해야함, 쓸 이미지 없어서 미사용중
    }

    private Data.MasteryData _masteryData;
    private int _currentLevel;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindImages(typeof(Images));

        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnClickUpgradeButton);

        return true;
    }

    public void SetInfo(Data.MasteryData masteryData)
    {
        _masteryData = masteryData;
        _currentLevel = Managers.Mastery.GetMasteryLevel(masteryData.masteryId);

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (_masteryData == null)
            return;

        _currentLevel = Managers.Mastery.GetMasteryLevel(_masteryData.masteryId);

        // 마스터리 이름
        GetText((int)Texts.MasteryNameText).text = _masteryData.masteryName;

        // 현재 레벨
        GetText((int)Texts.CurrentLevelText).text = $"Lv.{_currentLevel}";

        // 현재 보너스
        float currentBonus = _masteryData.GetTotalBonusForLevel(_currentLevel);
        GetText((int)Texts.CurrentBonusText).text = $"현재: +{currentBonus:F1}%";

        // 최대 레벨 체크
        if (_currentLevel >= _masteryData.maxLevel)
        {
            // MAX 상태
            GetText((int)Texts.MaxLevelText).gameObject.SetActive(true);
            GetText((int)Texts.NextBonusText).gameObject.SetActive(false);
            GetText((int)Texts.CostText).gameObject.SetActive(false);
            GetButton((int)Buttons.UpgradeButton).gameObject.SetActive(false);
        }
        else
        {
            // 업그레이드 가능
            GetText((int)Texts.MaxLevelText).gameObject.SetActive(false);
            GetText((int)Texts.NextBonusText).gameObject.SetActive(true);
            GetText((int)Texts.CostText).gameObject.SetActive(true);
            GetButton((int)Buttons.UpgradeButton).gameObject.SetActive(true);

            // 다음 레벨 보너스
            float nextBonus = _masteryData.GetTotalBonusForLevel(_currentLevel + 1);
            GetText((int)Texts.NextBonusText).text = $"다음: +{nextBonus:F1}%";

            // 비용
            int cost = _masteryData.GetCostForLevel(_currentLevel + 1);
            GetText((int)Texts.CostText).text = $"{cost}G";

            // 업그레이드 버튼 활성화/비활성화
            bool canUpgrade = Managers.Mastery.CanUpgrade(_masteryData.masteryId);
            GetButton((int)Buttons.UpgradeButton).interactable = canUpgrade;

            // 골드 부족 시 빨간색 표시
            if (Managers.Game.Gold < cost)
            {
                GetText((int)Texts.CostText).color = Color.red;
            }
            else
            {
                GetText((int)Texts.CostText).color = Color.white;
            }
        }
    }

    private void OnClickUpgradeButton(PointerEventData evt)
    {
        if (_masteryData == null)
            return;

        int cost = _masteryData.GetCostForLevel(_currentLevel + 1);

        // 골드 부족 체크
        if (Managers.Game.Gold < cost)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "골드 부족",
                message: $"업그레이드에 필요한 골드가 부족합니다.\n필요: {cost}G\n보유: {Managers.Game.Gold}G"
            );
            return;
        }

        // 업그레이드 확인 팝업
        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "마스터리 업그레이드",
            message: $"{_masteryData.masteryName} Lv.{_currentLevel} → Lv.{_currentLevel + 1}\n\n" +
                     $"효과: +{_masteryData.GetTotalBonusForLevel(_currentLevel)}% → +{_masteryData.GetTotalBonusForLevel(_currentLevel + 1)}%\n" +
                     $"비용: {cost}G\n\n" +
                     $"업그레이드하시겠습니까?",
            onConfirm: () =>
            {
                bool success = Managers.Mastery.UpgradeMastery(_masteryData.masteryId);
                if (success)
                {
                    Debug.Log($"마스터리 업그레이드 성공: {_masteryData.masteryName}");
                }
            },
            confirmButtonText: "업그레이드",
            cancelButtonText: "취소"
        );
    }
}