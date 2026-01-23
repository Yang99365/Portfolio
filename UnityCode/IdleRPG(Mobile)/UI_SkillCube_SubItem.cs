using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_SkillCube_SubItem : UI_Base, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Enums
    enum Images
    {
        Background,
        SkillIcon,
        RarityFrame,
        SelectedFrame,
        SkillTypeIcon, // Active/Passive 표시용
    }

    enum Texts
    {
        SkillLevelText,
        SkillNameText, // 선택적
    }
    #endregion

    #region Fields
    // Slot data
    public int SlotIndex { get; set; } = -1;
    private SkillCube _currentSkillCube = null;
    private bool _isSelected = false;
    private bool _isInteractable = true;

    // Events
    public event Action<UI_SkillCube_SubItem, SkillCube> OnSkillCubeClicked;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        // Initialize UI
        GetImage((int)Images.SelectedFrame).gameObject.SetActive(false);
        ClearSlot();

        return true;
    }
    #endregion

    #region Slot Management
    public void SetSkillCube(SkillCube cube)
    {
        _currentSkillCube = cube;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_currentSkillCube == null)
        {
            ClearSlot();
            return;
        }

        // 스킬 아이콘 표시
        GetImage((int)Images.SkillIcon).gameObject.SetActive(true);
        GetImage((int)Images.SkillIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.SkillDataDict[_currentSkillCube.DataId].skillIcon);


        // 레벨 표시
        GetText((int)Texts.SkillLevelText).text = $"{_currentSkillCube.Level}";
        GetText((int)Texts.SkillLevelText).gameObject.SetActive(true);

        // 스킬 이름 표시 (선택적 - UI에 따라)
        if (GetText((int)Texts.SkillNameText) != null)
        {
            GetText((int)Texts.SkillNameText).text = _currentSkillCube.SkillData.skillName;
            GetText((int)Texts.SkillNameText).gameObject.SetActive(true);
        }

        // 희귀도 프레임 색상
        SetRarityColor(_currentSkillCube.Rarity);

        // 스킬 타입 아이콘 표시
        SetSkillTypeIcon(_currentSkillCube.SkillType);
    }

    private void ClearSlot()
    {
        // 아이콘 초기화
        GetImage((int)Images.SkillIcon).sprite = null;
        GetImage((int)Images.SkillIcon).gameObject.SetActive(false);

        // 텍스트 초기화
        GetText((int)Texts.SkillLevelText).text = "";
        GetText((int)Texts.SkillLevelText).gameObject.SetActive(false);

        if (GetText((int)Texts.SkillNameText) != null)
        {
            GetText((int)Texts.SkillNameText).text = "";
            GetText((int)Texts.SkillNameText).gameObject.SetActive(false);
        }

        // 스킬 타입 아이콘 숨기기
        GetImage((int)Images.SkillTypeIcon).gameObject.SetActive(false);

        // 선택 해제
        _currentSkillCube = null;
        SetSelected(false);

        // 희귀도 프레임 초기화
        SetRarityColor(ESkillRairity.Common);
    }

    private void SetRarityColor(ESkillRairity rarity)
    {
        Color color = rarity switch
        {
            ESkillRairity.Common => Color.gray,
            ESkillRairity.Rare => new Color(0.3f, 0.6f, 1f),      // Blue
            ESkillRairity.Unique => new Color(0.6f, 0.3f, 1f),    // Purple
            ESkillRairity.Epic => new Color(1f, 0.5f, 0f),        // Orange
            ESkillRairity.Legend => new Color(1f, 0.9f, 0f),      // Gold
            _ => Color.gray
        };

        GetImage((int)Images.RarityFrame).color = color;
    }

    private void SetSkillTypeIcon(ESkillType skillType)
    {
        var typeIcon = GetImage((int)Images.SkillTypeIcon);

        if (typeIcon == null) return;

        typeIcon.gameObject.SetActive(true);

        // 스킬 타입에 따른 색상 또는 아이콘 변경
        switch (skillType)
        {
            case ESkillType.Active:
                // TODO: Active 스킬 아이콘 로드
                typeIcon.color = new Color(1f, 0.3f, 0.3f); // 빨간색 계열
                break;
            case ESkillType.Passive:
                // TODO: Passive 스킬 아이콘 로드
                typeIcon.color = new Color(0.3f, 1f, 0.3f); // 초록색 계열
                break;
            default:
                typeIcon.gameObject.SetActive(false);
                break;
        }
    }

    private Color GetSkillTypeColor(ESkillType skillType)
    {
        return skillType switch
        {
            ESkillType.Active => new Color(1f, 0.5f, 0.5f),   // 연한 빨강
            ESkillType.Passive => new Color(0.5f, 1f, 0.5f),  // 연한 초록
            _ => Color.white
        };
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        GetImage((int)Images.SelectedFrame).gameObject.SetActive(selected);
    }

    public void SetActiveState(bool active)
    {
        _isInteractable = active;

        // 투명도로 활성/비활성 표시
        float alpha = active ? 1f : 0.3f;

        var skillIcon = GetImage((int)Images.SkillIcon);
        if (skillIcon != null)
        {
            Color color = skillIcon.color;
            color.a = alpha;
            skillIcon.color = color;
        }

        var background = GetImage((int)Images.Background);
        if (background != null)
        {
            Color bgColor = background.color;
            bgColor.a = alpha;
            background.color = bgColor;
        }
    }
    #endregion

    #region Event Handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 좌클릭 - 스킬큐브 선택
            OnSkillCubeClicked?.Invoke(this, _currentSkillCube);

            string cubeName = _currentSkillCube?.SkillData?.skillName ?? "Empty";
            Debug.Log($"Clicked SkillCube Slot {SlotIndex}: {cubeName}");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable || _currentSkillCube == null) return;

        // TODO: 툴팁 표시
        // ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: 툴팁 숨기기
        // HideTooltip();
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// 스킬큐브의 간단한 정보 텍스트 반환 (툴팁용)
    /// </summary>
    public string GetTooltipText()
    {
        if (_currentSkillCube == null)
            return "빈 슬롯";

        string tooltip = $"{_currentSkillCube.SkillData.skillName}\n";
        tooltip += $"Lv.{_currentSkillCube.Level} | {_currentSkillCube.Rarity}\n";
        tooltip += $"타입: {_currentSkillCube.SkillType}\n";

        if (_currentSkillCube.SkillType == ESkillType.Active)
        {
            tooltip += $"쿨다운: {_currentSkillCube.SkillData.cooldown}초\n";
        }

        tooltip += $"\n{_currentSkillCube.SkillData.description}";

        return tooltip;
    }
    #endregion
}