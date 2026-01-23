using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_InventoryPopup : UI_Popup
{
    #region Enums
    enum Buttons
    {
        AllButton,
        EquipButton,
        ConsumButton,
        MaterialButton,
        UseButton,
        SellButton,
    }

    enum Texts
    {
        TitleText,
        SlotCountText,
        ItemNameText,
        ItemDescText,
        ItemStatsText,
    }

    enum GameObjects
    {
        InventoryContent,
        ItemDetailPanel,
        CategoryButtons,
    }

    enum Images
    {
        ItemIcon,
    }
    #endregion

    #region Fields
    private List<UI_Inventory_SubItem> _inventorySlots = new List<UI_Inventory_SubItem>();
    private InventoryManager.EInventoryGroupType _currentCategory = InventoryManager.EInventoryGroupType.All;
    private UI_Inventory_SubItem _selectedSlot = null;
    private Item _selectedItem = null;
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
        GetButton((int)Buttons.EquipButton).gameObject.BindEvent(OnClickEquipButton);
        GetButton((int)Buttons.ConsumButton).gameObject.BindEvent(OnClickConsumButton);
        GetButton((int)Buttons.MaterialButton).gameObject.BindEvent(OnClickMaterialButton);
        GetButton((int)Buttons.UseButton).gameObject.BindEvent(OnClickUseButton);
        GetButton((int)Buttons.SellButton).gameObject.BindEvent(OnClickSellButton);

        // Initialize UI
        GetText((int)Texts.TitleText).text = "인벤토리";
        GetObject((int)GameObjects.ItemDetailPanel).SetActive(false);

        // Create inventory slots
        CreateInventorySlots();

        // Register events
        Managers.Inventory.OnInventoryChanged -= OnInventoryChanged;
        Managers.Inventory.OnInventoryChanged += OnInventoryChanged;

        // Initial refresh
        RefreshUI();

        return true;
    }

    private void CreateInventorySlots()
    {
        GameObject content = GetObject((int)GameObjects.InventoryContent);

        // Clear existing slots
        foreach (Transform child in content.transform)
        {
            Managers.Resource.Destroy(child.gameObject);
        }
        _inventorySlots.Clear();

        // Create new slots
        int slotCount = InventoryManager.DEFAULT_INVENTORY_SIZE;
        for (int i = 0; i < slotCount; i++)
        {
            UI_Inventory_SubItem subItem = Managers.UI.MakeSubItem<UI_Inventory_SubItem>(content.transform);
            subItem.SlotIndex = i;
            subItem.OnItemClicked -= OnSlotClicked;
            subItem.OnItemClicked += OnSlotClicked;
            subItem.OnItemDropped -= OnItemDropped;
            subItem.OnItemDropped += OnItemDropped;
            _inventorySlots.Add(subItem);
        }
    }
    #endregion

    #region UI Refresh
    public void RefreshUI()
    {
        if(_init == false)
            return;

        // Update category buttons highlight
        UpdateCategoryButtons();

        // Update inventory slots
        UpdateInventorySlots();

        // Update currency and slot count
        UpdateInventoryInfo();

        // Update item detail if selected
        if (_selectedItem != null)
        {
            UpdateItemDetail(_selectedItem);
        }
        
    }

    private void UpdateCategoryButtons()
    {
        // Reset all button colors
        GetButton((int)Buttons.AllButton).GetComponent<Image>().color = Color.white;
        GetButton((int)Buttons.EquipButton).GetComponent<Image>().color = Color.white;
        GetButton((int)Buttons.ConsumButton).GetComponent<Image>().color = Color.white;
        GetButton((int)Buttons.MaterialButton).GetComponent<Image>().color = Color.white;

        // Highlight selected category
        Color highlightColor = Color.yellow;
        switch (_currentCategory)
        {
            case InventoryManager.EInventoryGroupType.All:
                GetButton((int)Buttons.AllButton).GetComponent<Image>().color = highlightColor;
                break;
            case InventoryManager.EInventoryGroupType.Equipment:
                GetButton((int)Buttons.EquipButton).GetComponent<Image>().color = highlightColor;
                break;
            case InventoryManager.EInventoryGroupType.Consumable:
                GetButton((int)Buttons.ConsumButton).GetComponent<Image>().color = highlightColor;
                break;
            case InventoryManager.EInventoryGroupType.Material:
                GetButton((int)Buttons.MaterialButton).GetComponent<Image>().color = highlightColor;
                break;
        }
    }

    private void UpdateInventorySlots()
    {
        List<Item> items = Managers.Inventory.Items;

        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                Item item = items[i];
                _inventorySlots[i].SetItem(item);

                // Apply category filter
                bool shouldShow = ShouldShowItem(item);
                _inventorySlots[i].SetActiveState(shouldShow);
            }
            else
            {
                _inventorySlots[i].SetItem(null);
                _inventorySlots[i].SetActiveState(true);

                // Clear selection if selected slot is now empty
                if (_selectedSlot == _inventorySlots[i])
                {
                    //ClearSelection();
                }
            }
        }
    }

    private bool ShouldShowItem(Item item)
    {
        if (item == null) return true;

        switch (_currentCategory)
        {
            case InventoryManager.EInventoryGroupType.All:
                return true;
            case InventoryManager.EInventoryGroupType.Equipment:
                return item.ItemType == EItemType.Equipment;
            case InventoryManager.EInventoryGroupType.Consumable:
                return item.ItemType == EItemType.Consumable;
            case InventoryManager.EInventoryGroupType.Material:
                return item.ItemType == EItemType.Material;
            default:
                return true;
        }
    }

    private void UpdateInventoryInfo()
    {
 
        // Update slot count
        int currentCount = Managers.Inventory.ItemCount;
        int maxCount = InventoryManager.DEFAULT_INVENTORY_SIZE;
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

    private void UpdateItemDetail(Item item)
    {
        if (item == null)
        {
            GetObject((int)GameObjects.ItemDetailPanel).SetActive(false);
            return;
        }

        GetObject((int)GameObjects.ItemDetailPanel).SetActive(true);

        // Set item info
        GetText((int)Texts.ItemNameText).text = item.ItemData.baseName;

        // Set item icon
        GetImage((int)Images.ItemIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[_selectedItem.DataId].ItemImage);
        GetImage((int)Images.ItemIcon).gameObject.SetActive(true);
        
        // Set description based on item type
        string description = "";
        string stats = "";

        switch (item.ItemType)
        {
            case EItemType.Equipment:
                var equipItem = item as EquipmentItem;
                if (equipItem != null)
                {
                    description = $"타입: {equipItem.EquipmentData.equipmentType}\n";
                    description += $"레어도: {equipItem.EquipmentData.itemRairity}";
                    stats = GetEquipmentStats(equipItem);

                    GetButton((int)Buttons.UseButton).gameObject.SetActive(true);
                    GetButton((int)Buttons.UseButton).GetComponentInChildren<TextMeshProUGUI>().text = "장착";
                }
                break;

            case EItemType.Consumable:
                var consumableItem = item as ConsumableItem;
                if (consumableItem != null)
                {
                    var consumableData = consumableItem.ConsumableData;
                    description = $"타입: {consumableData.consumableType}\n";
                    description += $"효과: {consumableData.consumableEffectType}\n";
                    description += $"값: {consumableData.consumableEffectValue}";

                    GetButton((int)Buttons.UseButton).gameObject.SetActive(true);
                    GetButton((int)Buttons.UseButton).GetComponentInChildren<TextMeshProUGUI>().text = "사용";
                }
                break;

            case EItemType.Material:
                var materialItem = item as MaterialItem;
                if (materialItem != null)
                {
                    description = materialItem.MaterialData.materialDescription;
                    GetButton((int)Buttons.UseButton).gameObject.SetActive(false);
                }
                break;
        }

        GetText((int)Texts.ItemDescText).text = description;
        GetText((int)Texts.ItemStatsText).text = stats;

        // Sell button is always active for all items
        GetButton((int)Buttons.SellButton).gameObject.SetActive(true);
    }

    private string GetEquipmentStats(EquipmentItem item)
    {
        if (item?.EquipmentData == null) return "";

        string stats = "";
        var data = item.EquipmentData.stats;

        if (data.attack > 0)
            stats += $"공격력: +{data.attack}\n";
        if (data.defense > 0)
            stats += $"방어력: +{data.defense}\n";
        if (data.maxHealth > 0)
            stats += $"체력: +{data.maxHealth}\n";
        if (data.attackSpeed > 0)
            stats += $"공격속도: +{data.attackSpeed:F2}\n";
        if (data.criticalChance > 0)
            stats += $"치명타 확률: +{data.criticalChance * 100:F1}%\n";
        if (data.criticalDamage > 0)
            stats += $"치명타 데미지: +{data.criticalDamage * 100:F0}%";

        return stats;
    }
    #endregion

    #region Event Handlers
    private void OnInventoryChanged()
    {
        RefreshUI();
    }
    private void ChangeCategory(InventoryManager.EInventoryGroupType newCategory)
    {
        // 카테고리가 실제로 변경되는지 체크
        if (_currentCategory == newCategory)
        {
            RefreshUI();
            return;
        }

        // 카테고리 변경
        _currentCategory = newCategory;

        // 현재 선택된 아이템이 있는지 확인
        if (_selectedItem != null)
        {
            // 선택된 아이템이 새 카테고리에 속하는지 확인
            bool itemBelongsToNewCategory = ShouldShowItem(_selectedItem);

            // 새 카테고리에 속하지 않으면 선택 해제
            if (!itemBelongsToNewCategory)
            {
                ClearSelection();
            }
        }

        // UI 갱신
        RefreshUI();
    }
    private void OnSlotClicked(UI_Inventory_SubItem slot, Item item)
    {
        if (item == null)
        {
            ClearSelection();
            return;
        }

        // Update selected slot
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
        }

        _selectedSlot = slot;
        _selectedItem = item;

        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(true);
        }

        // Update item detail
        UpdateItemDetail(item);
    }

    private void OnItemDropped(int fromSlot, int toSlot)
    {
        Managers.Inventory.SwapItems(fromSlot, toSlot);

        RefreshUI();
    }
    #endregion

    #region Button Click Handlers
    private void OnClickCloseButton(PointerEventData evt)
    {
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        ClosePopupUI();
    }

    private void OnClickAllButton(PointerEventData evt)
    {
        ChangeCategory(InventoryManager.EInventoryGroupType.All);
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        RefreshUI();
    }

    private void OnClickEquipButton(PointerEventData evt)
    {
        ChangeCategory(InventoryManager.EInventoryGroupType.Equipment);
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        RefreshUI();
    }

    private void OnClickConsumButton(PointerEventData evt)
    {
        ChangeCategory(InventoryManager.EInventoryGroupType.Consumable);
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        RefreshUI();
    }

    private void OnClickMaterialButton(PointerEventData evt)
    {
        ChangeCategory(InventoryManager.EInventoryGroupType.Material);
        //Managers.Sound.Play(ESound.UI, "ButtonClick");
        RefreshUI();
    }

    private void OnClickUseButton(PointerEventData evt)
    {
        if (_selectedItem == null) return;

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        switch (_selectedItem.ItemType)
        {
            case EItemType.Equipment:
                UI_HeroEquipmentSlotPopup popup = Managers.UI.ShowPopupUI<UI_HeroEquipmentSlotPopup>();
                popup.SetItemToEquip(_selectedItem, onEquipSuccess: () =>
                {
                    ClearSelection(); // ← 선택 해제
                    RefreshUI();      // ← UI 갱신
                });
                
                Debug.Log($"Equip {_selectedItem.ItemData.baseName}");
                break;

            case EItemType.Consumable:
                if (Managers.Inventory.UseItem(_selectedItem.InstanceId))
                {
                    //만약 사용하고 남은 갯수가 0이면 선택 해제
                    if (Managers.Inventory.GetItemCount(_selectedItem.InstanceId) == 0)
                    {
                        ClearSelection();
                    }
                    else
                    {
                        RefreshUI();
                    }
                }
                break;
        }
    }


    //private void OnClickSellButton(PointerEventData evt)
    //{
    //    if (_selectedItem == null) return;

    //    //Managers.Sound.Play(ESound.UI, "ButtonClick");

    //    // Calculate sell price (50% of base price)
    //    int sellPrice = _selectedItem.ItemData.itemPrice / 2;
    //    int totalPrice = sellPrice * _selectedItem.Count;

    //    // TODO: Show confirmation popup
    //    // For now, sell immediately
    //    Managers.Game.Gold += totalPrice;
    //    Managers.Inventory.RemoveItem(_selectedItem.InstanceId);

    //    Debug.Log($"Sold {_selectedItem.ItemData.baseName} for {totalPrice} gold");

    //    _selectedItem = null;
    //    _selectedSlot = null;
    //    UpdateItemDetail(null);
    //    RefreshUI();
    //}
    private void OnClickSellButton(PointerEventData evt)
    {
        if (_selectedItem == null) return;

        //Managers.Sound.Play(ESound.UI, "ButtonClick");

        // Calculate sell price (50% of base price)
        int sellPrice = _selectedItem.ItemData.itemPrice / 2;
        int totalPrice = sellPrice * _selectedItem.Count;

        // Store item info for confirmation popup
        string itemName = _selectedItem.ItemData.baseName;
        int itemCount = _selectedItem.Count;
        int itemInstanceId = _selectedItem.InstanceId;

        // Show confirmation popup
        string title = "아이템 판매";
        string message = $"{itemName}";
        if (itemCount > 1)
        {
            message += $" x{itemCount}";
        }
        message += $"\n판매 가격: {totalPrice} Gold\n\n정말 판매하시겠습니까?";

        UI_ConfirmPopup confirmPopup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
        confirmPopup.SetInfo(
            title: title,
            message: message,
            onConfirm: () =>
            {
                // 확인 버튼 클릭 시 판매 실행
                Managers.Game.Gold += totalPrice;
                Managers.Inventory.RemoveItem(itemInstanceId);

                Debug.Log($"Sold {itemName} for {totalPrice} gold");

                // 선택 해제
                ClearSelection();

                // UI 갱신
                RefreshUI();
            },
            onCancel: () =>
            {
                // 취소 버튼 클릭 시 아무것도 하지 않음
                Debug.Log("판매 취소됨");
            },
            confirmButtonText: "판매",
            cancelButtonText: "취소"
        );
    }
    #endregion

    #region Cleanup
    private void ClearSelection()
    {
        // 슬롯 선택 해제
        if (_selectedSlot != null)
        {
            _selectedSlot.SetSelected(false);
            _selectedSlot = null;
        }

        // 아이템 선택 해제
        _selectedItem = null;

        // 디테일 패널 끄기
        UpdateItemDetail(null);
    }
    public override void ClosePopupUI()
    {
        // Unregister events
        Managers.Inventory.OnInventoryChanged -= OnInventoryChanged;

        foreach (var slot in _inventorySlots)
        {
            if (slot != null)
            {
                slot.OnItemClicked -= OnSlotClicked;
                slot.OnItemDropped -= OnItemDropped;
            }
        }

        base.ClosePopupUI();
    }
    private void OnDestroy()
    {

        // 이벤트 해제
        if (Managers.Inventory != null)
        {
            Managers.Inventory.OnInventoryChanged -= OnInventoryChanged;
        }

        // 슬롯 이벤트 해제
        if (_inventorySlots != null)
        {
            foreach (var slot in _inventorySlots)
            {
                if (slot != null)
                {
                    slot.OnItemClicked -= OnSlotClicked;
                    slot.OnItemDropped -= OnItemDropped;
                }
            }
        }
    }
    #endregion
}