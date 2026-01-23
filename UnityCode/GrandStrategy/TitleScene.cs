using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class TitleScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.TitleScene;

        StartLoadAssets();
        return true;
    }

    void StartLoadAssets() // 타이틀에서 필요한 리소스를 미리 로드, 로딩이 끝나면 StartImage를 활성화
    {
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count}/{totalCount}");
            //Managers.Data.Init(); // 빌드하면 중복값으로 61/88 이되서 임시용해봄.
            if (count == totalCount)
            {
                Managers.Data.Init(); // Load을 통해 데이터를 불러오기때문에 리소스 로딩이 끝나면 데이터를 초기화
                /*
                //데이터가 있는지 확인
                if (Managers.Game.LoadGame() == false)
                {
                    Managers.Game.InitGame();
                    Managers.Game.SaveGame();
                }
                */

            }
        });
    }
    
    public override void Clear()
    {

    }
}
