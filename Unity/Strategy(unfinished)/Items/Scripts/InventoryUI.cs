using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Toggle allToggle;
    public Toggle equipmentToggle;
    public Toggle consumableToggle;
    public Toggle ownableToggle;
    public GameObject inventoryPanel;

    //public FactionManager factionManager;//싱글톤으로 바꿧으니까 이거 필요없어짐
    
    public bool inventoryActive = false;
    public Slot[] slots;
    
    public Transform slotHolder;
    public TextMeshProUGUI sizeTxt;

    /*
    // singleton
    #region Singleton
    public static InventoryUI instance;
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    #endregion
    */
    

    private void Start() {
        
        Inventory.instance.onItemChangedCallback += UpdateUI;
        inventoryPanel.SetActive(inventoryActive);

        //shop
        shopPanel.SetActive(isShopActive);
        slots = slotHolder.GetComponentsInChildren<Slot>();
        shopSlots = shopSlotHolder.GetComponentsInChildren<ShopSlot>();
        // 모든 상점슬롯에 SlotNum을 부여한다.
        for(int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i].Init(this);
            shopSlots[i].slotNum = i;
        }
        
        //InitializeUI();
        UpdateUI();
        allToggle.onValueChanged.AddListener(delegate { OnToggleChanged(); });
        equipmentToggle.onValueChanged.AddListener(delegate { OnToggleChanged(); });
        consumableToggle.onValueChanged.AddListener(delegate { OnToggleChanged(); });
        ownableToggle.onValueChanged.AddListener(delegate { OnToggleChanged(); });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)&&FactionManager.instance.playerFactionSelected)
        {
            inventoryActive = !inventoryActive;
            inventoryPanel.SetActive(inventoryActive);
        }
    }
    public void OnToggleChanged()
    {
        UpdateToggleTransparency(allToggle, allToggle.isOn);
        UpdateToggleTransparency(equipmentToggle, equipmentToggle.isOn);
        UpdateToggleTransparency(consumableToggle, consumableToggle.isOn);
        UpdateToggleTransparency(ownableToggle, ownableToggle.isOn);
        
        if (allToggle.isOn)
        {
            foreach (var slot in slots)
            {

                slot.SetActiveState(true); // 모든 슬롯 활성화
            }
        }
        else
        {
            foreach (var slot in slots)
            {
                if (slot.item != null)
                {
                    // 현재 활성화된 토글에 따라 슬롯 활성화/비활성화
                    slot.SetActiveState(
                        (equipmentToggle.isOn && slot.item.Type == ItemType.Equipment) ||
                        (consumableToggle.isOn && slot.item.Type == ItemType.Consumable) ||
                        (ownableToggle.isOn && slot.item.Type == ItemType.Ownable)
                    );
                }
                else
                {
                    slot.SetActiveState(true);
                }
            }
        }
    }
    private void UpdateToggleTransparency(Toggle toggle, bool isActive)
    {
        Image toggleImage = toggle.GetComponent<Image>();
        if (toggleImage != null)
        {
            Color color = toggleImage.color;
            color.a = isActive ? 1.0f : 0.5f; // 활성화된 토글은 불투명, 비활성화된 토글은 반투명
            toggleImage.color = color;
        }
    }
    public void ControlInventory()
    {
        if(FactionManager.instance.playerFactionSelected)
        {
            inventoryActive = !inventoryActive;
            inventoryPanel.SetActive(inventoryActive);
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < Inventory.instance.items.Count)
            {
                slots[i].UpdateSlotUI(Inventory.instance.items[i]); // 항상 UI 업데이트 수행
            }
            else
            {
                slots[i].ClearSlot(); // 아이템이 없어진 슬롯 클리어
            }
            // inventory.items 리스트에 Null이 아닌 아이템 / 사이즈를 sizeTxt에 표시
            sizeTxt.text = Inventory.instance.items.FindAll(x => x != null).Count + " / " + Inventory.instance.space;
        }
    }

    public GameObject shopPanel;
    public bool isShopActive=false;

    public ShopData shopData;
    public ShopSlot[] shopSlots;
    public Transform shopSlotHolder;

    public void OpenShop()
    {
        isShopActive = !isShopActive;
        shopPanel.SetActive(isShopActive);
        shopData = FindObjectOfType<ShopData>();
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].isShopMode = isShopActive;
        }
        // 상점데이터에서 아이템을 가져와 슬롯에 넣는다.
        // 새로고침버튼을 누르면 새롭게 아이템을 가져오도록 코드를 짜야함.
        for (int i = 0; i < shopData.stocks.Count; i++)
        {
            shopSlots[i].item = shopData.stocks[i];
            shopSlots[i].UpdateSlotUI();
        }
    }
    public void DeactiveShop()
    {
        isShopActive = false;
        shopPanel.SetActive(isShopActive);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].isShopMode = isShopActive;
            slots[i].chkSell.SetActive(isShopActive);
        }

        
        // 상점창을 닫으면 상점데이터의 연결을 끊고, 상점슬롯을 모두 비운다. devgomdol 근데 왜..?
        shopData = null;
        for (int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i].RemoveSlot();
        }
        
    }
    public void SellBtn()
    {
        for (int i = slots.Length; i > 0; i--)
        {
            slots[i-1].SellItem();
        }
    }
    public void Buy(int num)
    {
        shopData.soldOuts[num] = true;
    }
}
