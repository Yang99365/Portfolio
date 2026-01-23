using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_HeroEquipmentSlotPopup : UI_Popup
{
    #region Enums
    enum Buttons
    {
        CloseButton,
        Hero1Button,
        Hero2Button,
        Hero3Button,
        Hero4Button,
        WeaponSlotButton,
        ArmorSlotButton,
        AccessorySlotButton,
    }

    enum Texts
    {
        TitleText,
        GuideText,
        ItemNameText,
        AttackText,
        DefenseText,
        MaxHpText,
        AttackSpeedText,
        CritChanceText,
        CritDamageText,
        SelectedHeroNameText,
        SelectedHeroClassText,
        WeaponSlotTypeText,
        WeaponEquippedItemNameText,
        WeaponEquippedItemStatsText,
        ArmorSlotTypeText,
        ArmorEquippedItemNameText,
        ArmorEquippedItemStatsText,
        AccessorySlotTypeText,
        AccessoryEquippedItemNameText,
        AccessoryEquippedItemStatsText,
    }

    enum GameObjects
    {
        ItemInfoPanel,
        HeroListPanel,
        EquipmentSlotsPanel,
    }

    enum Images
    {
        Hero1Icon,
        Hero2Icon,
        Hero3Icon,
        Hero4Icon,
        ItemIconImage,
        WeaponEquippedItemIcon,
        ArmorEquippedItemIcon,
        AccessoryEquippedItemIcon,
    }
    #endregion

    #region Fields
    private Item _itemToEquip; // 장착할 아이템
    private Hero _selectedHero;
    private EEquipmentType _selectedSlotType;
    private Action _onEquipSuccess;

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

        // Equipment slot buttons
        GetButton((int)Buttons.WeaponSlotButton).gameObject.BindEvent((evt) => OnClickEquipmentSlot(evt, EEquipmentType.Weapon));
        GetButton((int)Buttons.ArmorSlotButton).gameObject.BindEvent((evt) => OnClickEquipmentSlot(evt, EEquipmentType.Armor));
        GetButton((int)Buttons.AccessorySlotButton).gameObject.BindEvent((evt) => OnClickEquipmentSlot(evt, EEquipmentType.Accessory));

        // Initialize
        GetText((int)Texts.TitleText).text = "장비 장착";
        GetText((int)Texts.GuideText).text = "영웅을 선택한 후 장비 슬롯을 클릭하세요";

        // 슬롯 타입 텍스트 설정
        GetText((int)Texts.WeaponSlotTypeText).text = "무기";
        GetText((int)Texts.ArmorSlotTypeText).text = "방어구";
        GetText((int)Texts.AccessorySlotTypeText).text = "액세서리";

        // 영웅 버튼 리스트 구성
        _heroButtons.Add(GetButton((int)Buttons.Hero1Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero2Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero3Button));
        _heroButtons.Add(GetButton((int)Buttons.Hero4Button));

        return true;
    }

    /// <summary>
    /// 팝업을 열 때 장착할 아이템을 설정합니다.
    /// </summary>
    public void SetItemToEquip(Item item, Action onEquipSuccess = null)
    {
        _itemToEquip = item;
        _onEquipSuccess = onEquipSuccess;

        if (item != null && item.ItemData != null)
        {
            // 아이템 정보 표시
            UpdateItemInfo();

            // 가이드 텍스트 업데이트
            GetText((int)Texts.GuideText).text =
                $"[{item.ItemData.baseName}]을(를) 장착할 영웅을 선택하세요";
        }

        RefreshUI();
    }
    #endregion

    #region UI Update
    private void RefreshUI()
    {
        if (!_init) return;

        // 배치된 영웅 목록 표시
        UpdateHeroList();

        // 선택된 영웅의 장비 슬롯 표시
        if (_selectedHero != null)
        {
            UpdateEquipmentSlots();
        }
        else
        {
            // 영웅이 선택되지 않으면 장비 슬롯 패널 비활성화
            GetObject((int)GameObjects.EquipmentSlotsPanel).SetActive(false);
        }
    }

    private void UpdateItemInfo()
    {
        if (_itemToEquip == null || _itemToEquip.ItemData == null)
            return;

        // 아이템 이름
        GetText((int)Texts.ItemNameText).text = _itemToEquip.ItemData.baseName;

        // 아이템 아이콘
        GetImage((int)Images.ItemIconImage).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[_itemToEquip.DataId].ItemImage);

        // 장비 아이템인 경우 스탯 표시
        if (_itemToEquip is EquipmentItem equipItem)
        {
            var stats = equipItem.EquipmentData.stats;

            GetText((int)Texts.AttackText).text = stats.attack > 0 ? $"공격력: +{stats.attack}" : "";
            GetText((int)Texts.DefenseText).text = stats.defense > 0 ? $"방어력: +{stats.defense}" : "";
            GetText((int)Texts.MaxHpText).text = stats.maxHealth > 0 ? $"체력: +{stats.maxHealth}" : "";
            GetText((int)Texts.AttackSpeedText).text = stats.attackSpeed > 0 ? $"공격속도: +{stats.attackSpeed}" : "";
            GetText((int)Texts.CritChanceText).text = stats.criticalChance > 0 ? $"크리티컬 확률: +{stats.criticalChance}%" : "";
            GetText((int)Texts.CritDamageText).text = stats.criticalDamage > 0 ? $"크리티컬 데미지: +{stats.criticalDamage}%" : "";
        }
    }

    
    private void UpdateHeroList()
    {
        var equipmentData = _itemToEquip?.ItemData as Data.EquipmentData;

        for (int slotIndex = 0; slotIndex < _heroButtons.Count; slotIndex++)
        {
            // 슬롯 인덱스로 직접 조회
            Hero hero = Managers.Hero.GetHeroAtSlot(slotIndex);

            if (hero != null)
            {
                _heroButtons[slotIndex].gameObject.SetActive(true);

                Images heroIconEnum = (Images)Enum.Parse(typeof(Images), $"Hero{slotIndex + 1}Icon");
                var heroIcon = GetImage((int)heroIconEnum);
                if (heroIcon != null && !string.IsNullOrEmpty(hero.HeroData.spriteAddress))
                {
                    Sprite heroSprite = Managers.Resource.Load<Sprite>(hero.HeroData.spriteAddress);
                    if (heroSprite != null)
                    {
                        heroIcon.sprite = heroSprite;
                        heroIcon.gameObject.SetActive(true);
                    }
                }

                var buttonText = _heroButtons[slotIndex].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = hero.HeroData.characterName;
                }

                bool canEquip = true;
                if (equipmentData != null &&
                    equipmentData.classRestriction > 0 &&
                    equipmentData.classRestriction != (int)hero.HeroData.characterClass)
                {
                    canEquip = false;
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

    private void UpdateEquipmentSlots()
    {
        if (_selectedHero == null) return;

        GetObject((int)GameObjects.EquipmentSlotsPanel).SetActive(true);

        // 선택된 영웅 정보 표시
        GetText((int)Texts.SelectedHeroNameText).text = $"선택된 영웅: {_selectedHero.HeroData.characterName}";
        GetText((int)Texts.SelectedHeroClassText).text = $"클래스: {_selectedHero.HeroData.characterClass}";

        // 영웅의 장착된 장비 가져오기
        var equippedItems = _selectedHero.GetAllEquippedItems();

        // 각 장비 슬롯 업데이트
        UpdateEquipmentSlotUI(EEquipmentType.Weapon, equippedItems);
        UpdateEquipmentSlotUI(EEquipmentType.Armor, equippedItems);
        UpdateEquipmentSlotUI(EEquipmentType.Accessory, equippedItems);

        // 장착 가능한 슬롯 하이라이트
        HighlightEquippableSlot();
    }

    private void UpdateEquipmentSlotUI(EEquipmentType slotType, Dictionary<EEquipmentType, Data.EquipmentData> equippedItems)
    {
        // 해당 슬롯에 장착된 장비 가져오기
        equippedItems.TryGetValue(slotType, out var equippedData);

        // 슬롯에 따라 UI 요소 가져오기
        Buttons buttonEnum;
        Texts nameTextEnum;
        Texts statsTextEnum;
        Images iconEnum;

        switch (slotType)
        {
            case EEquipmentType.Weapon:
                buttonEnum = Buttons.WeaponSlotButton;
                nameTextEnum = Texts.WeaponEquippedItemNameText;
                statsTextEnum = Texts.WeaponEquippedItemStatsText;
                iconEnum = Images.WeaponEquippedItemIcon;
                break;
            case EEquipmentType.Armor:
                buttonEnum = Buttons.ArmorSlotButton;
                nameTextEnum = Texts.ArmorEquippedItemNameText;
                statsTextEnum = Texts.ArmorEquippedItemStatsText;
                iconEnum = Images.ArmorEquippedItemIcon;
                break;
            case EEquipmentType.Accessory:
                buttonEnum = Buttons.AccessorySlotButton;
                nameTextEnum = Texts.AccessoryEquippedItemNameText;
                statsTextEnum = Texts.AccessoryEquippedItemStatsText;
                iconEnum = Images.AccessoryEquippedItemIcon;
                break;
            default:
                return;
        }

        var nameText = GetText((int)nameTextEnum);
        var statsText = GetText((int)statsTextEnum);
        var iconImage = GetImage((int)iconEnum);

        if (equippedData != null)
        {
            // 장비가 장착되어 있는 경우
            nameText.text = equippedData.baseName;

            // 주요 스탯 표시
            string stats = "";
            if (equippedData.stats.attack > 0)
                stats += $"공격 +{equippedData.stats.attack} ";
            if (equippedData.stats.defense > 0)
                stats += $"방어 +{equippedData.stats.defense} ";
            if (equippedData.stats.maxHealth > 0)
                stats += $"체력 +{equippedData.stats.maxHealth}";

            statsText.text = stats;

            // 아이콘 표시
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[equippedData.baseId].ItemImage);
        }
        else
        {
            // 빈 슬롯
            nameText.text = "빈 슬롯";
            statsText.text = "";
            iconImage.gameObject.SetActive(false);
        }
    }

    private void HighlightEquippableSlot()
    {
        if (_itemToEquip == null) return;

        var equipmentData = _itemToEquip.ItemData as Data.EquipmentData;
        if (equipmentData == null) return;

        // 장착할 아이템의 타입에 맞는 슬롯만 하이라이트
        EEquipmentType targetSlotType = equipmentData.equipmentType;

        // 모든 슬롯 버튼의 색상 초기화
        ResetSlotColors();

        // 해당 슬롯만 초록색 테두리
        Button targetButton = null;
        switch (targetSlotType)
        {
            case EEquipmentType.Weapon:
                targetButton = GetButton((int)Buttons.WeaponSlotButton);
                break;
            case EEquipmentType.Armor:
                targetButton = GetButton((int)Buttons.ArmorSlotButton);
                break;
            case EEquipmentType.Accessory:
                targetButton = GetButton((int)Buttons.AccessorySlotButton);
                break;
        }

        if (targetButton != null)
        {
            var buttonImage = targetButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.3f, 1f, 0.3f); // 초록색
            }
        }
    }

    private void ResetSlotColors()
    {
        // 모든 장비 슬롯 버튼의 색상을 기본으로 되돌림
        var weaponButton = GetButton((int)Buttons.WeaponSlotButton).GetComponent<Image>();
        var armorButton = GetButton((int)Buttons.ArmorSlotButton).GetComponent<Image>();
        var accessoryButton = GetButton((int)Buttons.AccessorySlotButton).GetComponent<Image>();

        if (weaponButton != null) weaponButton.color = Color.white;
        if (armorButton != null) armorButton.color = Color.white;
        if (accessoryButton != null) accessoryButton.color = Color.white;
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

        _selectedHero = hero;

        GetText((int)Texts.SelectedHeroNameText).text = hero.HeroData.characterName;
        GetText((int)Texts.SelectedHeroClassText).text = hero.HeroData.characterClass.ToString();

        GetObject((int)GameObjects.EquipmentSlotsPanel).SetActive(true);
        UpdateEquipmentSlots();
    }

    private void OnClickEquipmentSlot(PointerEventData evt, EEquipmentType slotType)
    {
        if (_selectedHero == null)
        {
            Debug.LogWarning("영웅을 먼저 선택하세요!");
            return;
        }

        if (_itemToEquip == null)
        {
            Debug.LogWarning("장착할 아이템이 없습니다!");
            return;
        }

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        _selectedSlotType = slotType;

        // 장착할 아이템의 타입 확인
        var equipmentData = _itemToEquip.ItemData as Data.EquipmentData;
        if (equipmentData == null)
        {
            Debug.LogError("장비 아이템이 아닙니다!");
            return;
        }

        // 슬롯 타입이 일치하는지 확인
        if (equipmentData.equipmentType != slotType)
        {
            Debug.LogWarning($"이 슬롯에는 {slotType} 타입의 장비만 장착할 수 있습니다!");
            // 실패 메시지
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "오류",
                message: $"이 슬롯에는 {GetSlotTypeName(slotType)} 타입의 장비만 장착할 수 있습니다.",
                onConfirm: () =>
                {
                    ClosePopupUI(); // ← 에러 팝업 닫힌 후 팝업 닫기
                }
            );
            return;
        }

        // 해당 슬롯에 이미 장비가 장착되어 있는지 확인
        var currentEquipment = _selectedHero.GetEquippedItem(slotType);

        if (currentEquipment != null)
        {
            // 장비 교체 확인
            ShowReplaceConfirmation(currentEquipment);
        }
        else
        {
            // 빈 슬롯에 바로 장착
            EquipItem();
        }
    }

    private void ShowReplaceConfirmation(Data.EquipmentData oldEquipment)
    {
        string message = $"현재 장착된 장비:\n{oldEquipment.baseName}\n\n";
        message += $"새로 장착할 장비:\n{_itemToEquip.ItemData.baseName}\n\n";
        message += "장비를 교체하시겠습니까?";

        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "장비 교체",
            message: message,
            onConfirm: () =>
            {
                EquipItem();
            },
            onCancel: () =>
            {
                Debug.Log("장비 교체 취소");
            },
            confirmButtonText: "교체",
            cancelButtonText: "취소"
        );
    }

    private void EquipItem()
    {
        if (_selectedHero == null || _itemToEquip == null)
            return;

        // HeroManager를 통해 장비 장착
        bool success = Managers.Hero.EquipItemToHero(
            _selectedHero.HeroInstanceId,
            _itemToEquip.InstanceId
        );

        if (success)
        {
            Debug.Log($"장비 장착 성공: {_itemToEquip.ItemData.baseName} → {_selectedHero.HeroData.characterName}");

            // UI 갱신
            RefreshUI();

            _onEquipSuccess?.Invoke();

            // 팝업 닫기
            ClosePopupUI();
        }
        else
        {
            Debug.LogError("장비 장착 실패!");

            // 실패 메시지
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "오류",
                message: "장비를 장착할 수 없습니다.\n클래스 제한을 확인해주세요.",
                onConfirm: () =>
                {
                    ClosePopupUI(); // ← 에러 팝업 닫힌 후 팝업 닫기
                }
            );
        }
    }

    private void OnClickCloseButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ClosePopupUI();
    }
    #endregion

    #region Helpers
    private string GetSlotTypeName(EEquipmentType slotType)
    {
        switch (slotType)
        {
            case EEquipmentType.Weapon:
                return "무기";
            case EEquipmentType.Armor:
                return "방어구";
            case EEquipmentType.Accessory:
                return "액세서리";
            default:
                return "알 수 없음";
        }
    }
    #endregion

    #region Cleanup
    public override void ClosePopupUI()
    {
        // 초기화
        _itemToEquip = null;
        _selectedHero = null;
        _selectedSlotType = EEquipmentType.None;

        base.ClosePopupUI();
    }
    #endregion
}
