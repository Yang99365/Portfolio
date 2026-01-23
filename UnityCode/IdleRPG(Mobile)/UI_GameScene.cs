using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_GameScene : UI_Scene
{
    enum Buttons
    {
        DebugGoldButton,
        HeroTabButton,
        InventoryTabButton,
        SkillCubeTabButton,
        ShopTabButton,
        MasteryTabButton,
    }
    enum Texts
    {
        StageText,
        GoldText,
        GemText
    }
    enum Sliders
    {
        Hero_HPBar_0,
        Hero_HPBar_1,
        Hero_HPBar_2,
        Hero_HPBar_3, // Hero 0~3
        Monster_HPBar_0,
        Monster_HPBar_1,
        Monster_HPBar_2,
        Monster_HPBar_3, // Monster 0~3
    }
    enum GameObjects
    {
        HeroHPBarContainer,
        MonsterHPBarContainer,
        TabButtonContainer,
        TabPopupContainer
        // position for generate SubItem
    }
    enum Images
    {
        HeroTabHighlight,
        InventoryTabHighlight,
        SkillCubeTabHighlight,
        ShopTabHighlight,
        MasteryTabHighlight
    }
    #region Fields
    // 현재 활성화된 탭
    private enum TabType
    {
        Hero,
        Inventory,
        SkillCube,
        Shop,
        Mastery
    }

    private TabType _currentTab = TabType.Hero;
    private UI_Popup _currentPopup = null;

    // HP 바 관리
    private Dictionary<int, Hero> _slotToHero = new Dictionary<int, Hero>(); // 슬롯 → 영웅 매핑
    private Dictionary<int, Monster> _slotToMonster = new Dictionary<int, Monster>(); // 슬롯 → 몬스터 매핑
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));
        BindObjects(typeof(GameObjects));
        BindImages(typeof(Images));

        GetButton((int)Buttons.HeroTabButton).gameObject.BindEvent(OnClickHeroTab);
        GetButton((int)Buttons.InventoryTabButton).gameObject.BindEvent(OnClickInventoryTab);
        GetButton((int)Buttons.SkillCubeTabButton).gameObject.BindEvent(OnClickSkillCubeTab);
        GetButton((int)Buttons.ShopTabButton).gameObject.BindEvent(OnClickShopTab);
        GetButton((int)Buttons.MasteryTabButton).gameObject.BindEvent(OnClickMasteryTab);

        //debug
        GetButton((int)Buttons.DebugGoldButton).gameObject.BindEvent(OnClickDebugGoldButton);

        InitializeHPBars();

        RegisterEvents();

        ShowTab(TabType.Hero);

        Refresh();


        return true;
    }

    public void SetInfo()
    {
        Refresh();
        ForceRefreshCurrentPopup();
    }
    void Refresh()
    {
        if (_init == false)
            return;
        // Text refresh
        // Slider refresh, activate/deactivate according to the battle situation and adjust the values according to heroes and monsters
        RefreshGoldText();
        RefreshGemText();
        RefreshStageText();
    }

    public void ForceRefreshCurrentPopup()
    {
        if (_currentPopup == null)
            return;

        Debug.Log($"Force refreshing popup: {_currentPopup.GetType().Name}");

        if (_currentPopup is UI_HeroPopup heroPopup)
        {
            heroPopup.RefreshUI();
        }
        else if (_currentPopup is UI_InventoryPopup inventoryPopup)
        {
            inventoryPopup.RefreshUI();
        }
        else if (_currentPopup is UI_SkillCubePopup skillCubePopup)
        {
            skillCubePopup.RefreshUI();
        }
        else if (_currentPopup is UI_ShopPopup shopPopup)
        {
            shopPopup.RefreshUI();
        }
        else if (_currentPopup is UI_MasteryPopup masteryPopup)
        {
            masteryPopup.RefreshUI();
        }
    }
    void RefreshGoldText()
    {
        GetText((int)Texts.GoldText).text = Managers.Game.Gold.ToString();
    }
    void RefreshGemText()
    {
        GetText((int)Texts.GemText).text = Managers.Game.Gem.ToString();
    }
    void RefreshStageText()
    {
        // stage information in game manager..
        // Do we need a stage manager?.. Change background according to stage?
        GetText((int)Texts.StageText).text = Managers.Battle?.CurrentStageNumber.ToString();
    }
    #region HP Bar Management
    private void InitializeHPBars()
    {
        // 모든 영웅 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var heroHpBar = GetSlider((int)Sliders.Hero_HPBar_0 + i);
            if (heroHpBar != null)
            {
                heroHpBar.gameObject.SetActive(false);
                heroHpBar.value = 0f; // 슬라이더 값도 0으로 초기화
            }
        }

        // 모든 몬스터 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var monsterHpBar = GetSlider((int)Sliders.Monster_HPBar_0 + i);
            if (monsterHpBar != null)
            {
                monsterHpBar.gameObject.SetActive(false);
                monsterHpBar.value = 0f; // 슬라이더 값도 0으로 초기화
            }
        }

    }
    public void RefreshAllHPBars()
    {
        // 기존 이벤트 정리
        CleanupHPBarEvents();

        // 영웅 HP바 갱신
        RefreshHeroHPBars();

        // 몬스터 HP바 갱신
        RefreshMonsterHPBars();

    }
    private void CleanupHPBarEvents()
    {
        // 영웅 이벤트 해제
        foreach (var hero in _slotToHero.Values)
        {
            if (hero != null)
            {
                hero.OnHpChanged -= OnHeroHpChanged;
            }
        }

        // 몬스터 이벤트 해제
        foreach (var monster in _slotToMonster.Values)
        {
            if (monster != null)
            {
                monster.OnHpChanged -= OnMonsterHpChanged;
            }
        }
    }

    private void RefreshHeroHPBars()
    {
        // 모든 영웅 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + i);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
        }

        // 슬롯 매핑 초기화
        _slotToHero.Clear();

        // 배치된 영웅들 가져오기
        var deployedHeroes = Managers.Battle.GetAllAliveHeroes();

        foreach (var hero in deployedHeroes)
        {
            int slotIndex = hero.SlotIndex;
            if (slotIndex < 0 || slotIndex >= 4)
            {
                continue;
            }

            // 슬롯 매핑 저장
            _slotToHero[slotIndex] = hero;

            // 해당 슬롯의 HP바 가져오기
            var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + slotIndex);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(true);

                // HP 변경 이벤트 등록
                hero.OnHpChanged += OnHeroHpChanged;

                // 초기값 설정
                UpdateHeroHPBar(slotIndex, hero.Hp, hero.MaxHp);
            }
        }
    }

    private void RefreshMonsterHPBars()
    {
        // 모든 몬스터 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var hpBar = GetSlider((int)Sliders.Monster_HPBar_0 + i);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
        }

        // 슬롯 매핑 초기화
        _slotToMonster.Clear();

        // 배치된 몬스터들 가져오기
        var aliveMonsters = Managers.Battle.GetAllAliveMonsters();

        foreach (var monster in aliveMonsters)
        {
            int slotIndex = monster.SlotIndex;
            if (slotIndex < 0 || slotIndex >= 4)
            {
                continue;
            }

            // 슬롯 매핑 저장
            _slotToMonster[slotIndex] = monster;

            // 해당 슬롯의 HP바 가져오기
            var hpBar = GetSlider((int)Sliders.Monster_HPBar_0 + slotIndex);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(true);

                // HP 변경 이벤트 등록
                monster.OnHpChanged += OnMonsterHpChanged;

                // 초기값 설정
                UpdateMonsterHPBar(slotIndex, monster.Hp, monster.MaxHp, monster.IsBoss);
            }
        }
    }

   
    private void OnHeroHpChanged(float current, float max)
    {
        // 어떤 영웅의 HP가 변경되었는지 찾기
        var hero = _slotToHero.Values.FirstOrDefault(h => h != null && h.Hp == current && h.MaxHp == max);
        if (hero == null)
            return;

        int slotIndex = hero.SlotIndex;
        if (slotIndex >= 0 && slotIndex < 4)
        {
            UpdateHeroHPBar(slotIndex, current, max);
        }
    }
    private void OnMonsterHpChanged(float current, float max)
    {
        // 어떤 몬스터의 HP가 변경되었는지 찾기
        var monster = _slotToMonster.Values.FirstOrDefault(m => m != null && m.Hp == current && m.MaxHp == max);
        if (monster == null)
            return;

        int slotIndex = monster.SlotIndex;
        if (slotIndex >= 0 && slotIndex < 4)
        {
            UpdateMonsterHPBar(slotIndex, current, max, monster.IsBoss);
        }
    }
    private void UpdateHeroHPBar(int slotIndex, float current, float max)
    {
        if (slotIndex < 0 || slotIndex >= 4)
            return;

        var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + slotIndex);
        if (hpBar == null)
            return;

        // HP 비율 계산
        float ratio = max > 0 ? current / max : 0;
        hpBar.value = ratio;

        // HP 바 색상 변경
        var fillImage = hpBar.fillRect?.GetComponent<Image>();
        if (fillImage != null)
        {
            if (ratio > 0.5f)
                fillImage.color = Color.green;
            else if (ratio > 0.25f)
                fillImage.color = Color.yellow;
            else
                fillImage.color = Color.red;
        }

        // 죽었을 때 처리
        if (current <= 0)
        {
            hpBar.gameObject.SetActive(false);
        }
    }
    private void UpdateMonsterHPBar(int slotIndex, float current, float max, bool isBoss)
    {
        if (slotIndex < 0 || slotIndex >= 4)
            return;

        var hpBar = GetSlider((int)Sliders.Monster_HPBar_0 + slotIndex);
        if (hpBar == null)
            return;

        // HP 비율 계산
        float ratio = max > 0 ? current / max : 0;
        hpBar.value = ratio;

        // 보스 몬스터는 다른 색상
        var fillImage = hpBar.fillRect?.GetComponent<Image>();
        if (fillImage != null)
        {
            if (isBoss)
                fillImage.color = Color.magenta;
            else
                fillImage.color = Color.red;
        }

        // 죽었을 때 처리
        if (current <= 0)
        {
            hpBar.gameObject.SetActive(false);

            // 슬롯 매핑에서 제거
            if (_slotToMonster.ContainsKey(slotIndex))
            {
                var monster = _slotToMonster[slotIndex];
                if (monster != null)
                {
                    monster.OnHpChanged -= OnMonsterHpChanged;
                }
                _slotToMonster.Remove(slotIndex);
            }
        }
    }
    private void ActivateHeroHPBar(Hero hero, int slotIndex)
    {
        if (hero == null || slotIndex < 0 || slotIndex >= 4)
            return;

        // 기존에 해당 슬롯에 영웅이 있었다면 이벤트 해제
        if (_slotToHero.ContainsKey(slotIndex))
        {
            var oldHero = _slotToHero[slotIndex];
            if (oldHero != null)
            {
                oldHero.OnHpChanged -= OnHeroHpChanged;
            }
        }

        // 새 영웅 매핑
        _slotToHero[slotIndex] = hero;

        // HP바 활성화
        var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + slotIndex);
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(true);

            // HP 변경 이벤트 등록
            hero.OnHpChanged += OnHeroHpChanged;

            // 초기값 설정
            UpdateHeroHPBar(slotIndex, hero.Hp, hero.MaxHp);
        }

    }
    private void DeactivateHeroHPBar(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 4)
            return;

        // 이벤트 해제
        if (_slotToHero.ContainsKey(slotIndex))
        {
            var hero = _slotToHero[slotIndex];
            if (hero != null)
            {
                hero.OnHpChanged -= OnHeroHpChanged;
            }
            _slotToHero.Remove(slotIndex);
        }

        // HP바 비활성화
        var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + slotIndex);
        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(false);
        }

    }
    public void ClearMonsterHPBars()
    {
        // 모든 몬스터 이벤트 해제
        foreach (var monster in _slotToMonster.Values)
        {
            if (monster != null)
            {
                monster.OnHpChanged -= OnMonsterHpChanged;
            }
        }
        _slotToMonster.Clear();

        // 모든 몬스터 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var hpBar = GetSlider((int)Sliders.Monster_HPBar_0 + i);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
        }
    }


    public void ClearHeroHPBars()
    {
        // 모든 영웅 이벤트 해제
        foreach (var hero in _slotToHero.Values)
        {
            if (hero != null)
            {
                hero.OnHpChanged -= OnHeroHpChanged;
            }
        }
        _slotToHero.Clear();

        // 모든 영웅 HP바 비활성화
        for (int i = 0; i < 4; i++)
        {
            var hpBar = GetSlider((int)Sliders.Hero_HPBar_0 + i);
            if (hpBar != null)
            {
                hpBar.gameObject.SetActive(false);
            }
        }
    }
    #endregion
    #region Tab Management

    private void ShowTab(TabType tabType)
    {
        // 이전 팝업 닫기
        if (_currentPopup != null)
        {
            Managers.UI.ClosePopupUI(_currentPopup);
            _currentPopup = null;
        }

        // 탭 하이라이트 업데이트
        UpdateTabHighlight(tabType);

        // 새 팝업 열기
        switch (tabType)
        {
            case TabType.Hero:
                _currentPopup = Managers.UI.ShowPopupUI<UI_HeroPopup>();
                break;
            case TabType.Inventory:
                _currentPopup = Managers.UI.ShowPopupUI<UI_InventoryPopup>();
                break;
            case TabType.SkillCube:
                _currentPopup = Managers.UI.ShowPopupUI<UI_SkillCubePopup>();
                break;
            case TabType.Shop:
                _currentPopup = Managers.UI.ShowPopupUI<UI_ShopPopup>();
                break;
            case TabType.Mastery:
                _currentPopup = Managers.UI.ShowPopupUI<UI_MasteryPopup>();
                break;
        }

        _currentTab = tabType;

    }

    private void UpdateTabHighlight(TabType tabType)
    {
        // 모든 하이라이트 비활성화
        GetImage((int)Images.HeroTabHighlight).gameObject.SetActive(false);
        GetImage((int)Images.InventoryTabHighlight).gameObject.SetActive(false);
        GetImage((int)Images.SkillCubeTabHighlight).gameObject.SetActive(false);
        GetImage((int)Images.ShopTabHighlight).gameObject.SetActive(false);
        GetImage((int)Images.MasteryTabHighlight).gameObject.SetActive(false);

        // 선택된 탭 하이라이트 활성화
        switch (tabType)
        {
            case TabType.Hero:
                GetImage((int)Images.HeroTabHighlight).gameObject.SetActive(true);
                break;
            case TabType.Inventory:
                GetImage((int)Images.InventoryTabHighlight).gameObject.SetActive(true);
                break;
            case TabType.SkillCube:
                GetImage((int)Images.SkillCubeTabHighlight).gameObject.SetActive(true);
                break;
            case TabType.Shop:
                GetImage((int)Images.ShopTabHighlight).gameObject.SetActive(true);
                break;
            case TabType.Mastery:
                GetImage((int)Images.MasteryTabHighlight).gameObject.SetActive(true);
                break;
        }
    }
    private void OnClickHeroTab(PointerEventData evt)
    {
        if (_currentTab != TabType.Hero)
        {
            ShowTab(TabType.Hero);
            // TODO : 영웅은 영웅함성 사운드로 탭에 따라 사운드매니저로 재생
        }
    }

    private void OnClickInventoryTab(PointerEventData evt)
    {
        if (_currentTab != TabType.Inventory)
        {
            ShowTab(TabType.Inventory);
        }
    }

    private void OnClickSkillCubeTab(PointerEventData evt)
    {
        if (_currentTab != TabType.SkillCube)
        {
            ShowTab(TabType.SkillCube);
        }
    }

    private void OnClickShopTab(PointerEventData evt)
    {
        if (_currentTab != TabType.Shop)
        {
            ShowTab(TabType.Shop);
        }
    }
    private void OnClickMasteryTab(PointerEventData evt)
    {
        if (_currentTab != TabType.Mastery)
        {
            ShowTab(TabType.Mastery);
        }
    }

    #endregion
    #region Event Registration
    private void RegisterEvents()
    {
        // 게임 매니저 이벤트
        if (Managers.Game != null)
        {
            Managers.Game.OnCurrencyChanged += OnCurrencyChanged;
        }

        // 배틀 매니저 이벤트
        if (Managers.Battle != null)
        {
            Managers.Battle.OnStageProgress += OnStageProgress;
            Managers.Battle.OnBattleStateChanged += OnBattleStateChanged;
            Managers.Battle.OnMonstersSpawned += OnMonstersSpawned;
        }
        if (Managers.Hero != null)
        {
            Managers.Hero.OnHeroDeployed += OnHeroDeployed;
            Managers.Hero.OnHeroUndeployed += OnHeroUndeployed;
        }
        // 인벤토리 매니저 이벤트
        if (Managers.Inventory != null)
        {
            Managers.Inventory.OnInventoryChanged += OnInventoryChanged;
        }
        
    }

    private void UnregisterEvents()
    {
        if (Managers.Game != null)
        {
            Managers.Game.OnCurrencyChanged -= OnCurrencyChanged;
        }

        if (Managers.Battle != null)
        {
            Managers.Battle.OnStageProgress -= OnStageProgress;
            Managers.Battle.OnBattleStateChanged -= OnBattleStateChanged;
            Managers.Battle.OnMonstersSpawned -= OnMonstersSpawned;
        }
        if (Managers.Hero != null)
        {
            Managers.Hero.OnHeroDeployed -= OnHeroDeployed;
            Managers.Hero.OnHeroUndeployed -= OnHeroUndeployed;
        }
        if (Managers.Inventory != null)
        {
            Managers.Inventory.OnInventoryChanged -= OnInventoryChanged;
        }
        
    }
    #endregion
    #region Debug Button Click Handlers
    void OnClickDebugGoldButton(PointerEventData evt)
    {
        Managers.Game.Gold += 1000;
        Managers.Game.Gem += 1000;
        Managers.Inventory.AddTestItem();
        Managers.Inventory.AddRandomSkillCube(); // for test
        //경험치 디버그
        Managers.Battle.GetAllAliveHeroes().ForEach(hero => hero.GainExperience(5000));
        Refresh();
    }
    // 장비, 스킬큐브 디버그 버튼 추가 요망
    #endregion
    #region Event Handlers
    // 자원 관련
    private void OnCurrencyChanged(ECurrencyType type, int amount)
    {
        switch (type)
        {
            case ECurrencyType.Gold:
                RefreshGoldText();
                break;
            case ECurrencyType.Gem:
                RefreshGemText();
                break;
        }
    }
    private void OnHeroDeployed(Hero hero, int slotIndex)
    {
        ActivateHeroHPBar(hero, slotIndex);
    }

    private void OnHeroUndeployed(Hero hero, int slotIndex)
    {

        DeactivateHeroHPBar(slotIndex);
    }
    // 전투 UI
    private void OnStageProgress(int stageNumber)
    {
        RefreshStageText();

        // 새 스테이지 시작 시 몬스터 HP바 초기화
        ClearMonsterHPBars();
        RefreshMonsterHPBars();

    }
    private void OnMonstersSpawned()
    {
        // 몬스터 생성 직후 HP바 설정
        RefreshMonsterHPBars();

    }
    private void OnBattleStateChanged(EBattleState prevState, EBattleState newState)
    {
        switch (newState)
        {
            case EBattleState.Start:
                // Start 상태: 영웅 HP바가 없으면 초기화 (첫 시작 시)
                // 이미 배치된 영웅은 OnHeroDeployed로 HP바가 생성되어 있음
                RefreshHeroHPBars();
                break;

            case EBattleState.Battle:

                break;

            case EBattleState.Victory:
            case EBattleState.Defeat:
                // 전투 종료 시 몬스터 HP바만 정리
                // 영웅 HP바는 유지 (다음 스테이지에서도 사용)
                ClearMonsterHPBars();
                break;
        }
    }

    //탭 팝업
    private void OnInventoryChanged()
    {
        // 현재 열린 팝업이 인벤토리나 스킬 팝업이면 새로고침
        if (_currentPopup is UI_InventoryPopup inventoryPopup)
        {
            inventoryPopup.RefreshUI();
        }
        else if (_currentPopup is UI_SkillCubePopup skillCubePopup)
        {
            skillCubePopup.RefreshUI();
        }
    }
    
    #endregion

    private void OnDestroy()
    {
        UnregisterEvents();

        if (_currentPopup != null)
        {
            Managers.UI.ClosePopupUI(_currentPopup);
            _currentPopup = null;
        }

        // 이벤트 정리
        CleanupHPBarEvents();

        ClearMonsterHPBars();
        ClearHeroHPBars();

    }

    
}
