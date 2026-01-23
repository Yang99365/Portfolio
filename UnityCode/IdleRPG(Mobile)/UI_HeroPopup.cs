using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_HeroPopup : UI_Popup
{
    #region Enums
    enum Texts
    {
        TitleText,
    }

    enum GameObjects
    {
        HeroListContent, // 스크롤뷰 컨텐츠
    }
    #endregion

    #region Fields
    private List<UI_HeroPopup_SubItem> _heroItems = new List<UI_HeroPopup_SubItem>();
    private bool _isInitialized = false;
    #endregion

    #region Initialization
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindTexts(typeof(Texts));
        BindObjects(typeof(GameObjects));

        // 제목 설정
        GetText((int)Texts.TitleText).text = "영웅";

        // 이벤트 구독
        if (Managers.Hero != null)
        {
            Managers.Hero.OnHeroCreated += OnHeroCreatedHandler;
            Managers.Hero.OnHeroDeployed += OnHeroDeployedHandler;
            Managers.Hero.OnHeroUndeployed += OnHeroUndeployedHandler;
            Managers.Hero.OnHeroStatsChanged += OnHeroStatsChangedHandler;
            Managers.Hero.OnHeroEquippedItem += OnHeroEquippedItemHandler;
            Managers.Hero.OnHeroUnequippedItem += OnHeroUnequippedItemHandler;
        }
        Managers.Mastery.OnMasteryChanged -= RefreshUI;
        Managers.Mastery.OnMasteryChanged += RefreshUI;

        // 영웅 목록 로드
        //RefreshHeroList();
        // 영웅 목록 초기 로드
        InitializeHeroList();

        return true;
    }
    #endregion
    #region Hero List Management
    private void InitializeHeroList()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("Hero list already initialized");
            return;
        }

        Transform content = GetObject((int)GameObjects.HeroListContent).transform;
        var allHeroData = Managers.Data.HeroDataDict.Values.OrderBy(h => h.position).ToList();

        // 모든 영웅 데이터에 대해 UI 아이템 생성
        foreach (var heroData in allHeroData)
        {
            CreateHeroItem(content, heroData);
        }

        _isInitialized = true;
        Debug.Log($"Hero list initialized: {_heroItems.Count} heroes");
    }
    private void CreateHeroItem(Transform parent, Data.HeroData heroData)
    {
        UI_HeroPopup_SubItem item = Managers.UI.MakeSubItem<UI_HeroPopup_SubItem>(parent);

        var displayInfo = GetHeroDisplayInfo(heroData);
        item.SetHeroInfo(displayInfo, heroData);

        _heroItems.Add(item);
    }
    private HeroManager.HeroDisplayInfo GetHeroDisplayInfo(Data.HeroData heroData)
    {
        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == heroData.characterId);

        if (saveData != null)
        {
            // 이미 생성된 영웅
            return CreateDisplayInfoFromSaveData(heroData, saveData);
        }
        else
        {
            // 아직 생성되지 않은 영웅 (잠김)
            return CreateDisplayInfoForLockedHero(heroData);
        }
    }
    private HeroManager.HeroDisplayInfo CreateDisplayInfoFromSaveData(Data.HeroData heroData, HeroSaveData saveData)
    {
        var displayInfo = new HeroManager.HeroDisplayInfo
        {
            TemplateId = heroData.characterId,
            Name = heroData.characterName,
            Level = saveData.level,
            Experience = saveData.exp,
            IsDeployed = saveData.slotIndex >= 0,
            SlotIndex = saveData.slotIndex,
            Class = heroData.characterClass,
            IsUnlocked = saveData.isUnlocked,
            InstanceId = 0
        };

        // 배치된 영웅이면 실제 인스턴스 ID 찾기
        if (displayInfo.IsDeployed)
        {
            var deployedHero = Managers.Hero.GetHeroByTemplateId(heroData.characterId);
            if (deployedHero != null)
            {
                displayInfo.InstanceId = deployedHero.HeroInstanceId;
            }
        }

        return displayInfo;
    }
    private HeroManager.HeroDisplayInfo CreateDisplayInfoForLockedHero(Data.HeroData heroData)
    {
        return new HeroManager.HeroDisplayInfo
        {
            TemplateId = heroData.characterId,
            Name = heroData.characterName,
            Level = 1,
            Experience = 0,
            IsDeployed = false,
            SlotIndex = -1,
            Class = heroData.characterClass,
            IsUnlocked = false,
            InstanceId = 0
        };
    }
    private void RefreshExistingItems()
    {
        foreach (var item in _heroItems)
        {
            if (item != null)
            {
                var heroData = item.GetHeroData();
                if (heroData != null)
                {
                    var displayInfo = GetHeroDisplayInfo(heroData);
                    item.SetHeroInfo(displayInfo, heroData);
                }
            }
        }

        Debug.Log($"Hero list refreshed: {_heroItems.Count} heroes");
    }
    private void RecreateHeroList()
    {
        // 기존 아이템 제거
        ClearHeroItems();

        // 새로 생성
        _isInitialized = false;
        InitializeHeroList();
    }
    private void ClearHeroItems()
    {
        foreach (var item in _heroItems)
        {
            if (item != null && item.gameObject != null)
            {
                Managers.Resource.Destroy(item.gameObject);
            }
        }
        _heroItems.Clear();
    }
    #endregion
    #region Public Methods

    //public void RefreshHeroList()
    //{
    //    // 기존 아이템 제거
    //    foreach (var item in _heroItems)
    //    {
    //        if (item != null && item.gameObject != null)
    //        {
    //            Managers.Resource.Destroy(item.gameObject);
    //        }
    //    }
    //    _heroItems.Clear();

    //    Transform content = GetObject((int)GameObjects.HeroListContent).transform;

    //    // 모든 영웅 데이터 가져오기 (JSON에서)
    //    var allHeroData = Managers.Data.HeroDataDict.Values.ToList();

    //    // 세이브 데이터에서 영웅 정보 가져오기
    //    foreach (var heroData in allHeroData)
    //    {
    //        // 세이브 데이터 찾기
    //        var saveData = Managers.Game.SaveData.Heroes.Find(h => h.templateId == heroData.characterId);

    //        HeroManager.HeroDisplayInfo displayInfo;

    //        if (saveData != null)
    //        {
    //            // 이미 생성된 영웅
    //            displayInfo = new HeroManager.HeroDisplayInfo
    //            {
    //                TemplateId = heroData.characterId,
    //                Name = heroData.characterName,
    //                Level = saveData.level,
    //                Experience = saveData.exp,
    //                IsDeployed = saveData.slotIndex >= 0,
    //                SlotIndex = saveData.slotIndex,
    //                Class = heroData.characterClass,
    //                IsUnlocked = saveData.isUnlocked,
    //                InstanceId = 0
    //            };

    //            // 배치된 영웅이면 실제 인스턴스 ID 찾기
    //            if (displayInfo.IsDeployed)
    //            {
    //                var deployedHero = Managers.Hero.GetHeroByTemplateId(heroData.characterId);
    //                if (deployedHero != null)
    //                {
    //                    displayInfo.InstanceId = deployedHero.HeroInstanceId;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // 아직 생성되지 않은 영웅 (잠김)
    //            displayInfo = new HeroManager.HeroDisplayInfo
    //            {
    //                TemplateId = heroData.characterId,
    //                Name = heroData.characterName,
    //                Level = 1,
    //                Experience = 0,
    //                IsDeployed = false,
    //                SlotIndex = -1,
    //                Class = heroData.characterClass,
    //                IsUnlocked = false,
    //                InstanceId = 0
    //            };
    //        }

    //        // UI 아이템 생성
    //        UI_HeroPopup_SubItem item = Managers.UI.MakeSubItem<UI_HeroPopup_SubItem>(content);
    //        item.SetHeroInfo(displayInfo, heroData);

    //        _heroItems.Add(item);
    //    }

    //    Debug.Log($"Hero list refreshed: {_heroItems.Count} heroes");
    //}
    public void RefreshHeroList()
    {
        if (!_isInitialized)
        {
            InitializeHeroList();
            return;
        }

        // 영웅 데이터 개수 확인
        var allHeroData = Managers.Data.HeroDataDict.Values.ToList();

        if (allHeroData.Count != _heroItems.Count)
        {
            // 영웅 개수가 변경된 경우 (새 영웅 추가 등) - 재생성 필요
            Debug.Log("Hero count changed, recreating list");
            RecreateHeroList();
        }
        else
        {
            // 개수가 같으면 기존 아이템만 갱신
            RefreshExistingItems();
        }
    }


    public void RefreshUI()
    {
        RefreshExistingItems();
        foreach (var item in _heroItems)
        {
            if (item != null)
            {
                item.RefreshUI();
            }
        }
    }
    // 특정 영웅만 갱신 (성능 최적화)
    public void RefreshHero(int templateId)
    {
        var targetItem = _heroItems.Find(item =>
            item != null && item.GetHeroData()?.characterId == templateId);

        if (targetItem != null)
        {
            var heroData = targetItem.GetHeroData();
            var displayInfo = GetHeroDisplayInfo(heroData);
            targetItem.SetHeroInfo(displayInfo, heroData);
        }
    }
    #endregion

    #region Event Handlers
    private void OnHeroCreatedHandler(Hero hero)
    {
        RefreshExistingItems();
        //RefreshHeroList();
    }

    private void OnHeroDeployedHandler(Hero hero, int slotIndex)
    {
        // 배치 상태만 변경되므로 UI만 갱신
        if (hero != null)
        {
            RefreshHero(hero.DataTemplateID);
        }
        else
        {
            RefreshExistingItems();
        }
        //RefreshHeroList();
    }

    private void OnHeroUndeployedHandler(Hero hero, int slotIndex)
    {
        // 배치 해제 상태만 변경되므로 UI만 갱신
        if (hero != null)
        {
            RefreshHero(hero.DataTemplateID);
        }
        else
        {
            RefreshExistingItems();
        }
        //RefreshHeroList();
    }
    private void OnHeroStatsChangedHandler(int templateId)
    {
        // 특정 영웅만 갱신할 수도 있지만, 간단하게 전체 갱신
        //RefreshUI();

        // 특정 영웅의 스탯만 변경되므로 해당 영웅만 갱신
        RefreshHero(templateId);
    }
    private void OnHeroEquippedItemHandler(Hero hero, Item item)
    {
        //RefreshUI();
        // 장비 장착 시 해당 영웅만 갱신
        if (hero != null)
        {
            RefreshHero(hero.DataTemplateID);
        }
    }
    private void OnHeroUnequippedItemHandler(Hero hero, Item item)
    {
        //RefreshUI();
        // 장비 해제 시 해당 영웅만 갱신
        if (hero != null)
        {
            RefreshHero(hero.DataTemplateID);
        }
    }
    #endregion

    #region Cleanup
    public override void ClosePopupUI()
    {
        base.ClosePopupUI();
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (Managers.Hero != null)
        {
            Managers.Hero.OnHeroCreated -= OnHeroCreatedHandler;
            Managers.Hero.OnHeroDeployed -= OnHeroDeployedHandler;
            Managers.Hero.OnHeroUndeployed -= OnHeroUndeployedHandler;
            Managers.Hero.OnHeroStatsChanged -= OnHeroStatsChangedHandler;
            Managers.Hero.OnHeroEquippedItem -= OnHeroEquippedItemHandler;
            Managers.Hero.OnHeroUnequippedItem -= OnHeroUnequippedItemHandler;
        }
        Managers.Mastery.OnMasteryChanged -= RefreshUI;
    }
    #endregion
}
