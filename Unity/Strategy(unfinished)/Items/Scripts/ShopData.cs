using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopData : MonoBehaviour
{
    public List<GItemSO> stocks = new List<GItemSO>();
    public bool[] soldOuts;
    public ItemController itemController;

    void Start()
    { // 상점용 임시 스크립트
        itemController = GameObject.Find("ItemManager").GetComponent<ItemController>();
        stocks.Add(itemController.items[0]);
        stocks.Add(itemController.items[1]);
        stocks.Add(itemController.items[2]);
        soldOuts = new bool[stocks.Count];
        for (int i = 0; i < stocks.Count; i++)
        {
            soldOuts[i] = false;
        }
    }
    

}
