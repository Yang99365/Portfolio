using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ItemLootSystem : MonoBehaviour
{
    public ItemController itemController;
    public ShopData shopData;
    public GItemSO item;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
            shopData = GameObject.Find("ShopManager").GetComponent<ShopData>();
        else
            shopData = null;
    }

    void Start()
    {
        item = itemController.items[0]; // 배터리
    }
    void Loot()
    {
        Inventory.instance.Add(item, 45);
    }
    void Loot2()
    {
        Inventory.instance.Add(itemController.items[3],3);
    }
    
    // Z키를 누르면 아이템을 얻는다. 임시
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Loot();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Loot2();
        }
        
    }
}
