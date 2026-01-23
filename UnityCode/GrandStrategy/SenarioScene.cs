using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class SenarioScene : BaseScene
{
    GameObject map;
    public GameObject regionPopup;

    
    
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.ScenarioScene;

        #region Map
        //맵 프리펩을 생성
        map = Managers.Resource.Instantiate("MapPrefab", GameObject.Find("@UI_Map").transform);
        //Temp 일단 임시로 맵이미지를 바꾸지만 나중에는 맵매니저에서 함수를 호출해서 맵 이미지를 받아오고 쉐이더도 적용하도록
        // 맵매니저에서 지역의 색을 바꾸는 등의 함수를 호출하면 델리게이트 구독으로 호출해야겠다.
        string mapName = Managers.Game.selectedScenario.viewMapTexture;

        Managers.Map.mapImage = map.GetComponent<Image>();

        Managers.Game.CreateGame(); // 지역, 세력 생성

        // 맵 생성
        Managers.Map.CreateMap(map);
        Managers.Map.UpdateRegionColorsAccordingToFactions();
        // 맵 하이라이트 처리
        map.GetComponent<MapViewHighlighter>().MapHighliterAwake();

        // 맵 색칠
        Managers.Map.UpdateRegionColorsAccordingToFactions(); // 빌드에선 실행안됨.
        #endregion

        #region RegionUI

        // 이걸 나중에 UI_ScenarioScene에 바인딩하게 새로 작성해야하는데.. 귀찬네
        // 지역 정보 UI 생성
        regionPopup = Managers.Resource.Instantiate("UI_RegionPopup", GameObject.Find("@UI_Map").transform);

        // 지역 정보 UI에 정보 전달
        regionPopup.GetComponent<UI_RegionPopup>().SetInfo();

        // UI_ScenarioScene 생성
        UI_ScenarioScene sceneUI = Managers.UI.ShowSceneUI<UI_ScenarioScene>();
        sceneUI.GetComponent<Canvas>().sortingOrder = 10;
        sceneUI.SetInfo();
        Managers.Map.UpdateRegionColorsAccordingToFactions();

        //// 샵 리롤(첫 테이블로 세팅)
        //Managers.Shop.RerollShop();
        //#endregion
        Managers.Map.UpdateRegionColorsAccordingToFactions();
        return true;
    }

    #endregion
    
    public override void Clear()
    {

    }

   
}