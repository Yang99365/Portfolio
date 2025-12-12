using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    //컨트롤러가 아니라 아이템 데이타로 이름을 바꾸는 것이 좋을 것 같다.
    public GItemSO []items;
    
    public static ItemController instance;
    
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        items = Resources.LoadAll<GItemSO>("SO/Items");
    }
    
    public void Start()
    {
        //items = Resources.LoadAll<GItemSO>("SO/Items");
    }
    public GItemSO GetItem(string itemCode)
    {
        foreach (var item in items)
        {
            if (item.Code == itemCode)
            {
                return item;
            }
        }
        return null;
    }

    
}
