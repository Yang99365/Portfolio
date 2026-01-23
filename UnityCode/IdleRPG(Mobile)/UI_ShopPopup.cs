using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_ShopPopup : UI_Popup
{
    enum Buttons
    {
        RerollButton,
        PurchaseButton  // 구매 버튼
    }

    enum GameObjects
    {
        ShopContent
    }

    enum Texts
    {
        ItemNameText,
        PriceText,
        GoldText,
        RerollCostText
    }

    private List<UI_ShopItem_SubItem> _shopSlots = new List<UI_ShopItem_SubItem>();
    private UI_ShopItem_SubItem _selectedSlot;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.RerollButton).gameObject.BindEvent(OnClickRerollButton);
        GetButton((int)Buttons.PurchaseButton).gameObject.BindEvent(OnClickPurchaseButton);

        // 이벤트 구독
        Managers.Shop.OnShopChanged -= RefreshUI;
        Managers.Shop.OnShopChanged += RefreshUI;

        Managers.Game.OnCurrencyChanged -= OnCurrencyChanged;
        Managers.Game.OnCurrencyChanged += OnCurrencyChanged;

        // 초기 설정
        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);
        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);


        CreateShopSlots();

        // 상점 초기화
        if (Managers.Shop != null)
        {
            Managers.Shop.Initialize();
        }

        RefreshUI();
        UpdateGoldText();

        return true;
    }

    private void OnDestroy()
    {
        Managers.Shop.OnShopChanged -= RefreshUI;
        Managers.Game.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void CreateShopSlots()
    {
        Transform content = GetObject((int)GameObjects.ShopContent)?.transform;
        if (content == null)
        {
            Debug.LogError("Shop content transform not found");
            return;
        }

        // 최대 30개 슬롯 미리 생성
        int maxSlotCount = 30;
        for (int i = 0; i < maxSlotCount; i++)
        {
            UI_ShopItem_SubItem slot = Managers.UI.MakeSubItem<UI_ShopItem_SubItem>(content);
            if (slot != null)
            {
                slot.OnItemClicked = OnItemClicked;
                _shopSlots.Add(slot);
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void RefreshUI()
    {
        List<Item> currentItems = Managers.Shop.GetShopItems();

        // 모든 슬롯 비활성화
        foreach (var slot in _shopSlots)
        {
            slot.gameObject.SetActive(false);
        }

        // 필요한 만큼만 활성화
        for (int i = 0; i < currentItems.Count; i++)
        {
            if (i < _shopSlots.Count)
            {
                _shopSlots[i].SetInfo(currentItems[i]);
                _shopSlots[i].gameObject.SetActive(true);
            }
        }

        UpdateRerollCostText();

        // 선택 해제
        ClearSelection();
    }

    private void UpdateGoldText()
    {
        GetText((int)Texts.GoldText).text = $"Gold: {Managers.Game.Gold}";
    }

    private void UpdateRerollCostText()
    {
        GetText((int)Texts.RerollCostText).text = $"Reroll: {Managers.Shop.RerollCost}G";
    }

    private void OnCurrencyChanged(ECurrencyType type, int amount)
    {
        if (type == ECurrencyType.Gold)
        {
            UpdateGoldText();
        }
    }

    private void OnItemClicked(UI_ShopItem_SubItem clickedSlot)
    {
        if (clickedSlot == null || clickedSlot.Item == null)
            return;

        // 이전 선택 해제
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
        }

        // 새로운 선택
        _selectedSlot = clickedSlot;
        _selectedSlot.SetSelected(true);

        // 아이템 정보 표시
        GetText((int)Texts.ItemNameText).gameObject.SetActive(true);
        GetText((int)Texts.ItemNameText).text = clickedSlot.Item.ItemData.baseName;

        GetText((int)Texts.PriceText).gameObject.SetActive(true);
        int totalPrice = clickedSlot.Item.ItemData.itemPrice * clickedSlot.Item.Count;
        GetText((int)Texts.PriceText).text = $"가격: {totalPrice}G";

        // 구매 버튼 활성화
        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(true);
    }

    private void ClearSelection()
    {
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }

        GetText((int)Texts.ItemNameText).gameObject.SetActive(false);
        GetText((int)Texts.PriceText).gameObject.SetActive(false);
        GetButton((int)Buttons.PurchaseButton).gameObject.SetActive(false);
    }

    #region Button Events

    private void OnClickRerollButton(PointerEventData evt)
    {
        // 골드 체크
        if (Managers.Game.Gold < Managers.Shop.RerollCost)
        {
            // 골드 부족 알림
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "골드 부족",
                message: $"리롤에 필요한 골드가 부족합니다.\n필요: {Managers.Shop.RerollCost}G\n보유: {Managers.Game.Gold}G"
            );
            return;
        }

        // 리롤 확인 팝업
        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "상점 리롤",
            message: $"{Managers.Shop.RerollCost}G를 사용하여\n상점 아이템을 새로 고치시겠습니까?",
            onConfirm: () =>
            {
                // 골드 차감
                Managers.Game.Gold -= Managers.Shop.RerollCost;

                // 상점 리롤
                Managers.Shop.RerollShop();
                ClearSelection();
                Debug.Log("상점 리롤 완료");
            },
            confirmButtonText: "리롤",
            cancelButtonText: "취소"
        );
    }

    private void OnClickPurchaseButton(PointerEventData evt)
    {
        if (_selectedSlot == null || _selectedSlot.Item == null)
            return;

        Item selectedItem = _selectedSlot.Item;
        int totalPrice = selectedItem.ItemData.itemPrice * selectedItem.Count;

        // 골드 부족 체크
        if (Managers.Game.Gold < totalPrice)
        {
            UI_ConfirmPopup errorPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
            errorPopup.SetInfoAsAlert(
                title: "골드 부족",
                message: $"아이템을 구매할 골드가 부족합니다.\n필요: {totalPrice}G\n보유: {Managers.Game.Gold}G"
            );
            return;
        }

        // 구매 확인 팝업
        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: "아이템 구매",
            message: $"{selectedItem.ItemData.baseName} x{selectedItem.Count}\n가격: {totalPrice}G\n\n구매하시겠습니까?",
            onConfirm: () =>
            {
                // 구매 시도
                bool success = Managers.Shop.BuyItem(selectedItem.InstanceId);

                if (success)
                {
                    // 구매 성공 알림 (선택사항)
                    // UI_ConfirmPopup successPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    // successPopup.SetInfoAsAlert(
                    //     title: "구매 완료",
                    //     message: $"{selectedItem.ItemData.baseName}을(를) 구매했습니다!"
                    // );
                }
                else
                {
                    Debug.LogError("아이템 구매 실패");
                }
            },
            confirmButtonText: "구매",
            cancelButtonText: "취소"
        );
    }

    #endregion
}