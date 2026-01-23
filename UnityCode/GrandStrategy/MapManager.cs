using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
public class MapManager
{
    private TaskCompletionSource<bool> _texturesLoadedTcs;
    public Task TexturesLoaded => _texturesLoadedTcs?.Task ?? Task.CompletedTask;

    /*
    // 타이틀씬의 UI에서 선택한 시나리오 맵을 가져와서 그걸 기반으로 맵을 로드하고
    // 맵프리펩을 생성해줘야하는데, 이건 맵매니저에 함수를 만들고 ScenarioScene에서 호출해주는 방식으로
    // 쉐이더를 적용하고, 맵을 렌더링하고, 맵의 이벤트를 처리하고

    */
    public Image mapImage; // 시나리오씬 맵
    public Material MapMaterial; 
    Texture2D ColormapTexture;
    Texture2D ViewmapTexutre;
    Texture2D colorMapTextureCopy;

    
    //배틀맵
    public GameObject BattleMap { get; private set; }
    public string BattleMapName { get; private set; }
    public Grid CellGrid { get; private set; }


    //SenarioScene에서 생성한 map프리펩을 받아와서 맵을 생성
    //public void CreateMap(GameObject map)
    //{

    //    LoadMaterial(map);

    //}
    public void CreateMap(GameObject map)
    {
        LoadMaterialAsync(map);
    }
    //public void LoadMaterial(GameObject map)
    //{
    //    map.GetComponent<Image>().material = Managers.Resource.Load<Material>("Material_Map");
    //    LoadTexture(map.GetComponent<Image>().material, 
    //        Managers.Game.selectedScenario.viewMapTexture, 
    //        Managers.Game.selectedScenario.colorMapTexture);


    //    MapMaterial = mapImage.material;
    //    ColormapTexture = MapMaterial.GetTexture("_ColorMap") as Texture2D;
    //    ViewmapTexutre = MapMaterial.GetTexture("_MainTex") as Texture2D;

    //    colorMapTextureCopy = new Texture2D(ColormapTexture.width, ColormapTexture.height);
    //    colorMapTextureCopy.SetPixels(ColormapTexture.GetPixels());
    //    colorMapTextureCopy.Apply();

    //}
    private async void LoadMaterialAsync(GameObject map)
    {
        map.GetComponent<Image>().material = Managers.Resource.Load<Material>("Material_Map");

        // 두 텍스처의 로딩 완료를 기다림
        await LoadTextureAsync(map.GetComponent<Image>().material,
            Managers.Game.selectedScenario.viewMapTexture,
            Managers.Game.selectedScenario.colorMapTexture);

        MapMaterial = mapImage.material;
        ColormapTexture = MapMaterial.GetTexture("_ColorMap") as Texture2D;
        ViewmapTexutre = MapMaterial.GetTexture("_MainTex") as Texture2D;

        colorMapTextureCopy = new Texture2D(ColormapTexture.width, ColormapTexture.height);
        colorMapTextureCopy.SetPixels(ColormapTexture.GetPixels());
        colorMapTextureCopy.Apply();

        // 텍스처 로딩이 완료된 후 색상 업데이트
        UpdateRegionColorsAccordingToFactions();
    }

    //private void LoadTexture(Material material, string viewTextureAddress, string colorTextureAddress)
    //{
    //    // ViewMapTexture 로드
    //    Addressables.LoadAssetAsync<Texture>(viewTextureAddress).Completed += viewHandle => {
    //        if (viewHandle.Status == AsyncOperationStatus.Succeeded)
    //        {
    //            Texture viewMapTexture = viewHandle.Result;
    //            material.SetTexture("_MainTex", viewMapTexture);
    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to load view map texture.");
    //        }
    //    };

    //    // ColorMapTexture 로드
    //    Addressables.LoadAssetAsync<Texture>(colorTextureAddress).Completed += colorHandle => {
    //        if (colorHandle.Status == AsyncOperationStatus.Succeeded)
    //        {
    //            Texture colorMapTexture = colorHandle.Result;
    //            material.SetTexture("_ColorMap", colorMapTexture);


    //        }
    //        else
    //        {
    //            Debug.LogError("Failed to load color map texture.");
    //        }
    //    };

    //}
    private async Task LoadTextureAsync(Material material, string viewTextureAddress, string colorTextureAddress)
    {
        var viewLoadTask = Addressables.LoadAssetAsync<Texture>(viewTextureAddress).Task;
        var colorLoadTask = Addressables.LoadAssetAsync<Texture>(colorTextureAddress).Task;

        await Task.WhenAll(viewLoadTask, colorLoadTask);

        Texture viewMapTexture = await viewLoadTask;
        Texture colorMapTexture = await colorLoadTask;

        material.SetTexture("_MainTex", viewMapTexture);
        material.SetTexture("_ColorMap", colorMapTexture);
    }
    public void LoadBattleMap(string mapName)
    {
        if (BattleMap != null)
            Managers.Resource.Destroy(BattleMap);

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        BattleMap = map;
        BattleMapName = mapName;
        CellGrid = map.GetComponent<Grid>();
    }
    public IEnumerator CreateMapCoroutine(GameObject map)
    {
        map.GetComponent<Image>().material = Managers.Resource.Load<Material>("Material_Map");
        Material material = map.GetComponent<Image>().material;

        var viewLoadOperation = Addressables.LoadAssetAsync<Texture>(Managers.Game.selectedScenario.viewMapTexture);
        var colorLoadOperation = Addressables.LoadAssetAsync<Texture>(Managers.Game.selectedScenario.colorMapTexture);

        // 두 텍스처가 모두 로드될 때까지 대기
        yield return viewLoadOperation;
        yield return colorLoadOperation;

        if (viewLoadOperation.Status == AsyncOperationStatus.Succeeded &&
            colorLoadOperation.Status == AsyncOperationStatus.Succeeded)
        {
            material.SetTexture("_MainTex", viewLoadOperation.Result);
            material.SetTexture("_ColorMap", colorLoadOperation.Result);

            MapMaterial = mapImage.material;
            ColormapTexture = MapMaterial.GetTexture("_ColorMap") as Texture2D;
            ViewmapTexutre = MapMaterial.GetTexture("_MainTex") as Texture2D;

            colorMapTextureCopy = new Texture2D(ColormapTexture.width, ColormapTexture.height);
            colorMapTextureCopy.SetPixels(ColormapTexture.GetPixels());
            colorMapTextureCopy.Apply();

            // 이제 텍스처가 준비되었으므로 색상 업데이트 수행
            UpdateRegionColorsAccordingToFactions();
        }
    }
    #region MapColoring


    public void UpdateRegionColorsAccordingToFactions()
    {
        if (ColormapTexture == null || colorMapTextureCopy == null)
        {
            Debug.LogError("Cannot update colors: Textures not loaded");
            return;
        }
        // 먼저 ViewMapTexture의 원본 상태로 복사본을 리셋
        colorMapTextureCopy.SetPixels(ColormapTexture.GetPixels());
        colorMapTextureCopy.Apply();

        Color[] colors = colorMapTextureCopy.GetPixels();
        Color[] colorMapColors = ColormapTexture.GetPixels();

        foreach (var faction in Managers.Game.factions)
        {
            
            foreach (var region in faction.controlledRegions)
            {
               
                for (int i = 0; i < colorMapColors.Length; i++)
                {
                    // ColorMapTexture를 사용해 지역을 식별하고
                    // ViewMapTexture의 해당 픽셀을 세력 색상으로 변경
                    if (IsColorEqual(colorMapColors[i], region.RegionColor))
                    {
                        colors[i] = faction.FactionColor;
                    }
                }
            }
        }

        colorMapTextureCopy.SetPixels(colors);
        colorMapTextureCopy.Apply();

        // 수정된 뷰맵 텍스처를 메인 텍스처로 설정
        MapMaterial.SetTexture("_MainTex", colorMapTextureCopy);
    }

    

    #endregion
    #region HelperMethods
    private bool IsColorEqual(Color a, Color b)
    {
        return Mathf.Approximately(a.r, b.r) &&
               Mathf.Approximately(a.g, b.g) &&
               Mathf.Approximately(a.b, b.b) &&
               Mathf.Approximately(a.a, b.a);
    }
    //private bool IsColorEqual(Color a, Color b)
    //{
    //    return a == b;
    //    //return Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a);
    //}
    private void ReplaceRegionColor(Color[] pixels, Color oldColor, Color newColor)
    {
        int count = 0; // 변경된 픽셀 수를 세기 위한 변수
        for (int i = 0; i < pixels.Length; i++)
        {
            if (IsColorEqual(pixels[i], oldColor))
            {
                pixels[i] = newColor;
                count++;
            }
        }
        Debug.Log($"Color changed for {count} pixels from {oldColor} to {newColor}");
    }
    #endregion


    // 맵생성하는 스크립트가 아니라 점령, 이벤트, 전투 등을 통해 변화하는 맵을 관리하는 스크립트
}
