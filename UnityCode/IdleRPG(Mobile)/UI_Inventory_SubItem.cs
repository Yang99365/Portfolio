using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_Inventory_SubItem : UI_Base, IPointerClickHandler, IBeginDragHandler,
    IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region Enums
    enum Images
    {
        Background,
        ItemIcon,
        RarityFrame,
        SelectedFrame,
    }

    enum Texts
    {
        ItemCountText,
    }
    #endregion

    #region Fields
    // Item data
    public int SlotIndex { get; set; } = -1;
    private Item _currentItem = null;
    private bool _isSelected = false;
    private bool _isInteractable = true;

    // Drag and drop
    private static UI_Inventory_SubItem _draggedSlot = null;
    private GameObject _dragIcon = null;
    private Transform _originalParent = null;

    // Events
    public event Action<UI_Inventory_SubItem, Item> OnItemClicked;
    public event Action<int, int> OnItemDropped;  // fromSlot, toSlot
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
    public void SetItem(Item item)
    {
        _currentItem = item;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_currentItem == null)
        {
            ClearSlot();
            return;
        }

        // Show item icon
        if (_currentItem.ItemData != null)
        {
            GetImage((int)Images.ItemIcon).sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[_currentItem.DataId].ItemImage);
            GetImage((int)Images.ItemIcon).gameObject.SetActive(true);
        }

        // Show item count if stackable
        if (_currentItem.Count > 1)
        {
            GetText((int)Texts.ItemCountText).text = _currentItem.Count.ToString();
            GetText((int)Texts.ItemCountText).gameObject.SetActive(true);
        }
        else
        {
            GetText((int)Texts.ItemCountText).gameObject.SetActive(false);
        }

        // Set rarity frame color
        SetRarityColor(_currentItem.ItemData.itemRairity);
    }

    private void ClearSlot()
    {
        GetImage((int)Images.ItemIcon).sprite = null;
        GetImage((int)Images.ItemIcon).gameObject.SetActive(false);
        GetText((int)Texts.ItemCountText).text = "";
        GetText((int)Texts.ItemCountText).gameObject.SetActive(false);
        _currentItem = null;
        SetSelected(false);
        SetRarityColor(EItemRarity.Normal);
    }

    private void SetRarityColor(EItemRarity rarity)
    {
        Color color = Color.gray;

        switch (rarity)
        {
            case EItemRarity.Normal:
                color = Color.gray;
                break;
            case EItemRarity.Rare:
                color = new Color(0.3f, 0.6f, 1f); // Blue
                break;
            case EItemRarity.Unique:
                color = new Color(0.6f, 0.3f, 1f); // Purple
                break;
            case EItemRarity.Legend:
                color = new Color(1f, 0.5f, 0f);   // Orange
                break;
        }

        GetImage((int)Images.RarityFrame).color = color;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        GetImage((int)Images.SelectedFrame).gameObject.SetActive(selected);
    }

    public void SetActiveState(bool active)
    {
        _isInteractable = active;

        // Set transparency based on interactable state
        float alpha = active ? 1f : 0.3f;
        
        if (GetImage((int)Images.ItemIcon) != null)
        {
            Color color = GetImage((int)Images.ItemIcon).color;
            color.a = alpha;
            GetImage((int)Images.ItemIcon).color = color;
        }

        if (GetImage((int)Images.Background) != null)
        {
            Color bgColor = GetImage((int)Images.Background).color;
            bgColor.a = alpha;
            GetImage((int)Images.Background).color = bgColor;
        }
    }
    #endregion

    #region Event Handlers
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Left click - select item
            OnItemClicked?.Invoke(this, _currentItem);
            
            Debug.Log($"Clicked Slot {SlotIndex}: {_currentItem?.ItemData?.baseName ?? "Empty"}");
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click - quick use/equip
            if (_currentItem != null)
            {
                HandleQuickAction();
            }
        }
    }

    private void HandleQuickAction()
    {
        if (_currentItem == null) return;

        switch (_currentItem.ItemType)
        {
            case EItemType.Consumable:
                if (Managers.Inventory.UseItem(_currentItem.InstanceId))
                {
                    Debug.Log($"Quick used {_currentItem.ItemData.baseName}");
                }
                break;

            case EItemType.Equipment:
                // TODO: Quick equip to first available hero
                Debug.Log($"Quick equip {_currentItem.ItemData.baseName}");
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_isInteractable || _currentItem == null) return;

        _draggedSlot = this;
        _originalParent = transform.parent;

        // Create drag icon
        CreateDragIcon(eventData);

        // Make current slot semi-transparent
        SetIconAlpha(0.5f);

        Debug.Log($"Begin drag from slot {SlotIndex}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragIcon != null)
        {
            _dragIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy drag icon
        if (_dragIcon != null)
        {
            Destroy(_dragIcon);
            _dragIcon = null;
        }

        // Reset transparency
        SetIconAlpha(1f);

        // Reset if no valid drop
        if (_draggedSlot == this)
        {
            _draggedSlot = null;
        }

        Debug.Log($"End drag from slot {SlotIndex}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!_isInteractable) return;
        if (_draggedSlot == null || _draggedSlot == this) return;

        Debug.Log($"Drop from slot {_draggedSlot.SlotIndex} to slot {SlotIndex}");

        // Invoke drop event
        OnItemDropped?.Invoke(_draggedSlot.SlotIndex, this.SlotIndex);

        // Reset drag state
        _draggedSlot = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable || _currentItem == null) return;

        // TODO: Show tooltip
        // ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // TODO: Hide tooltip
        // HideTooltip();
    }
    #endregion

    #region Helper Methods
    private void CreateDragIcon(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        // Create drag icon GameObject
        _dragIcon = new GameObject("DragIcon");
        _dragIcon.transform.SetParent(transform.root); // Canvas root

        // Add image component
        Image iconImage = _dragIcon.AddComponent<Image>();
        iconImage.sprite = GetImage((int)Images.ItemIcon).sprite;
        iconImage.raycastTarget = false;

        // Set size
        RectTransform rect = _dragIcon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 80);

        // Set initial position
        _dragIcon.transform.position = eventData.position;

        // Add canvas group for transparency
        CanvasGroup canvasGroup = _dragIcon.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    private void SetIconAlpha(float alpha)
    {
        if (GetImage((int)Images.ItemIcon) != null)
        {
            Color color = GetImage((int)Images.ItemIcon).color;
            color.a = alpha;
            GetImage((int)Images.ItemIcon).color = color;
        }
    }
    #endregion
}
