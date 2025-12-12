using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleContainer : MonoBehaviour
{
    // 최대 3명의 무장을 전투에 참가시킬수있다.
    public GeneralBase[] Playergenerals;
    public GeneralBase[] Enemygenerals;
    
    public int maxGeneral = 3;
    public int playerGeneral = 0;
    public int enemyGeneral = 0;

    public delegate void OnContainerChanged();
    public OnContainerChanged onContainerChangedCallback;

    // Singleton
    public static BattleContainer instance;
    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void Start()
    {
        Playergenerals = new GeneralBase[maxGeneral];
        Enemygenerals = new GeneralBase[maxGeneral];
    }
    // End of Singleton
    /*
    public void RemoveGeneral(GeneralBase general, bool isPlayer)
    {
        if (isPlayer)
        {
            for (int i = 0; i < Playergenerals.Length; i++)
            {
                if (Playergenerals[i] == general)
                {
                    Playergenerals[i] = null;
                    playerGeneral--;
                    onContainerChangedCallback?.Invoke();
                    return;
                }
            }
            Debug.Log("해당 아군 장군이 없습니다.");
        }
        else
        {
            for (int i = 0; i < Enemygenerals.Length; i++)
            {
                if (Enemygenerals[i] == general)
                {
                    Enemygenerals[i] = null;
                    enemyGeneral--;
                    onContainerChangedCallback?.Invoke();
                    return;
                }
            }
            Debug.Log("해당 적 장군이 없습니다.");
        }
    }
    */
    public void RemoveGeneral(GeneralBase general, bool isPlayer)
    {
        bool found = false;
        if (isPlayer)
        {
            for (int i = 0; i < Playergenerals.Length; i++)
            {
                if (Playergenerals[i] == general)
                {
                    Playergenerals[i] = null;
                    found = true;
                    playerGeneral = Mathf.Max(0, playerGeneral - 1); // 음수 방지
                    // 배열 압축
                    CompressArray(Playergenerals);
                    onContainerChangedCallback?.Invoke();
                    return;
                }
            }
            if (!found) Debug.Log("해당 아군 장군이 없습니다.");
        }
        else
        {
            for (int i = 0; i < Enemygenerals.Length; i++)
            {
                if (Enemygenerals[i] == general)
                {
                    Enemygenerals[i] = null;
                    found = true;
                    enemyGeneral = Mathf.Max(0, enemyGeneral - 1); // 음수 방지
                    // 배열 압축
                    CompressArray(Enemygenerals);
                    onContainerChangedCallback?.Invoke();
                    return;
                }
            }
            if (!found) Debug.Log("해당 적 장군이 없습니다.");
        }
    }

    void CompressArray(GeneralBase[] generals)
    {
        List<GeneralBase> tempList = new List<GeneralBase>();
        foreach (var general in generals)
        {
            if (general != null) tempList.Add(general);
        }

        // tempList의 내용을 generals 배열로 복사
        for (int i = 0; i < generals.Length; i++)
        {
            if (i < tempList.Count)
            {
                generals[i] = tempList[i];
            }
            else
            {
                generals[i] = null; // 남은 공간을 null로 채움
            }
        }
    }



    public void AddGeneral(GeneralBase general, bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerGeneral >= maxGeneral)
            {
                Debug.Log("아군 장군이 꽉 찼습니다.");
                return;
            }
            Playergenerals[playerGeneral] = general;
            playerGeneral++;
            onContainerChangedCallback?.Invoke();
        }
        else
        {
            if (enemyGeneral >= maxGeneral)
            {
                Debug.Log("적 장군이 꽉 찼습니다.");
                return;
            }
            Enemygenerals[enemyGeneral] = general;
            enemyGeneral++;
            onContainerChangedCallback?.Invoke();
        }
    }
    
    public GeneralBase GetGeneralBase(GeneralBase general)
    {
        return general;
    }
}
