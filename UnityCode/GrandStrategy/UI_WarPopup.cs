using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
using static UI_SelectedGeneral_SubItem;

public class UI_WarPopup : UI_Popup
{
    enum Buttons
    {
        CloseButton,
        WarButton,
    }
    enum Texts
    {
        RegionNameText,
        RegionTypeText,
        AttackFactionName,
        DefenseFactionName,
        SelectCount,
    }
    enum GameObjects
    {
        SelectedGeneralContent,
        MyGeneralContent,
    }

    private List<UI_WarGeneral_SubItem> myGenerals = new List<UI_WarGeneral_SubItem>();
    private List<UI_SelectedGeneral_SubItem> selectedGenerals = new List<UI_SelectedGeneral_SubItem>();
    private const int MAX_SELECT_COUNT = 5;

    private Faction attackFaction;  // 공격하는 세력 (플레이어)
    private Faction defenseFaction; // 방어하는 세력
    private Region targetRegion;    // 전투가 일어나는 지역

    private void OnDisable()
    {
        // 초기화시키기
        //UI_WarGeneral_SubItem.OnGeneralSelected -= OnGeneralSelected;
        //UI_SelectedGeneral_SubItem.OnClickSelectedGeneral -= OnClickSelectedGeneral;
        ClearSelectedGenerals();
        foreach (var item in myGenerals)
        {
            item.ResetSelectGeneral();
        }
        /*
         * 전투끝나면 호출하도록
        if (Managers.Battle != null)
            Managers.Battle.ResetGenerals();
        */
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);
        GetButton((int)Buttons.WarButton).gameObject.BindEvent(OnClickWarButton);

        // myGenerals에 무장넣어주기
        {
            var parent = GetObject((int)GameObjects.MyGeneralContent).transform;
            for (int i = 0; i < 30; i++)
            {
                UI_WarGeneral_SubItem item = Managers.UI.MakeSubItem<UI_WarGeneral_SubItem>(parent);
                myGenerals.Add(item);
            }
        }

        UI_WarGeneral_SubItem.OnGeneralSelected += OnGeneralSelected;
        UI_SelectedGeneral_SubItem.OnClickSelectedGeneral += OnClickSelectedGeneral;


        Refresh();
        return true;
    }

    public void SetInfo(Faction attackFaction, Faction defenseFaction, Region region)
    {
        this.attackFaction = attackFaction;
        this.defenseFaction = defenseFaction;
        this.targetRegion = region;


        GetText((int)Texts.RegionNameText).text = region.RegionName;
        //GetText((int)Texts.RegionTypeText).text = region. ~~ 지역의 타입은 미구현으로 주석처리
        GetText((int)Texts.AttackFactionName).text = attackFaction.FactionName;
        GetText((int)Texts.DefenseFactionName).text = defenseFaction.FactionName;
        UpdateSelectCount();
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;
        RefreshWarGeneral(myGenerals);
        RecreateSelectedGenerals();
        UpdateSelectCount();
    }

    //void RefreshWarGeneral(List<UI_WarGeneral_SubItem> list)
    //{
    //    List<General> generals = Managers.Game.GetPlayerGeneral();

    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (i < generals.Count)
    //        {
    //            list[i].gameObject.SetActive(true);
    //            list[i].SetInfo(generals[i].GeneralID);
    //        }
    //        else
    //        {
    //            list[i].gameObject.SetActive(false);
    //        }
    //    }
    //}
    void RefreshWarGeneral(List<UI_WarGeneral_SubItem> list)
    {
        List<General> generals = Managers.Game.GetPlayerGeneral();

        for (int i = 0; i < list.Count; i++)
        {
            if (i < generals.Count)
            {
                list[i].gameObject.SetActive(true);
                list[i].SetInfo(generals[i].GeneralID);

                // BattleManager에 이미 선택된 무장인지 확인하고 SelectFrame 활성화
                if (Managers.Battle.myGenerals.Any(x => x.GeneralID == generals[i].GeneralID))
                {
                    list[i].transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                list[i].gameObject.SetActive(false);
            }
        }
    }
    private void OnGeneralSelected(General general, bool isSelect)
    {
        if (isSelect)
        {
            if (selectedGenerals.Count >= MAX_SELECT_COUNT)
            {
                Debug.Log($"최대 {MAX_SELECT_COUNT}명의 무장만 선택할 수 있습니다.");
                // 선택 실패 시 WarGeneral의 선택 상태를 되돌림
                var warGeneral = myGenerals.Find(x => x._GeneralID == general.GeneralID);
                if (warGeneral != null)
                {
                    warGeneral.ResetSelectGeneral();
                }
                return;
            }


            Managers.Battle.AddmyGenerals(general, true);
        }
        else
        {
            Managers.Battle.AddmyGenerals(general, false);

        }
        // 선택된 무장 UI를 모두 재생성
        RecreateSelectedGenerals();
        UpdateSelectCount();
        //TEST
        Managers.Battle.NowGeneral();

    }
    private void UpdateSelectCount()
    {
        GetText((int)Texts.SelectCount).text = $"{selectedGenerals.Count}";
    }

    private void ClearSelectedGenerals()
    {
        var parent = GetObject((int)GameObjects.SelectedGeneralContent).transform;
        foreach (Transform child in parent)
        {
            Managers.Resource.Destroy(child.gameObject);
        }
        selectedGenerals.Clear();
        UpdateSelectCount();

    }

    private void OnClickSelectedGeneral(int generalId)
    {
        // SelectedGeneral을 클릭하면 선택 해제
        var selectedItem = selectedGenerals.Find(x => x._GeneralID == generalId);
        if (selectedItem != null)
        {
            // 해당하는 WarGeneral의 선택도 해제
            var warGeneral = myGenerals.Find(x => x._GeneralID == generalId);
            if (warGeneral != null)
            {
                warGeneral.ResetSelectGeneral();
                // BattleManager에서도 제거
                Managers.Battle.AddmyGenerals(Managers.Game.GetGeneral(generalId), false);
            }


            RecreateSelectedGenerals();
            UpdateSelectCount();

            //TEST
            Managers.Battle.NowGeneral();
        }
    }
    private void RecreateSelectedGenerals()
    {
        // 기존 UI 모두 제거
        var parent = GetObject((int)GameObjects.SelectedGeneralContent).transform;
        foreach (Transform child in parent)
        {
            Managers.Resource.Destroy(child.gameObject);
        }
        selectedGenerals.Clear();

        // BattleManager의 선택된 무장 목록을 기반으로 새로 생성
        if (Managers.Battle.myGenerals != null)
        {
            for (int i = 0; i < Managers.Battle.myGenerals.Count; i++)
            {
                var general = Managers.Battle.myGenerals[i];
                if (general != null)
                {
                    UI_SelectedGeneral_SubItem item = Managers.UI.MakeSubItem<UI_SelectedGeneral_SubItem>(parent);
                    item.SetInfo(general.GeneralID);
                    selectedGenerals.Add(item);

                    // 순서 보장을 위해 명시적으로 위치 설정
                    item.transform.SetSiblingIndex(i);
                }
            }
        }
    }
    private void OnClickCloseButton(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI();
    }

    private void OnClickWarButton(PointerEventData evt)
    {
        if (selectedGenerals.Count == 0)
        {
            Debug.Log("전투를 시작하려면 최소 1명의 무장을 선택해야 합니다.");
            return;
        }

        Managers.Battle.SetupBattle(attackFaction, defenseFaction, targetRegion);

        // 전투 시작
        SelectEnemyGenerals();

        // 현재 열린 모든 팝업을 닫음
        Managers.UI.CloseAllPopupUI();

        // BattleManager 상태 확인
        Debug.Log($"Before scene change - MyGenerals count: {Managers.Battle.myGenerals?.Count ?? 0}");
        if (Managers.Battle.myGenerals != null)
        {
            foreach (var general in Managers.Battle.myGenerals)
            {
                Debug.Log($"General ready for battle: {general?.GeneralName ?? "null"}");
            }
        }
        if (Managers.Battle.GetMyGeneralCount() > 0)
            Managers.Scene.LoadBattleScene(EScene.BattleScene);
        //Managers.Battle.StartBattle();
    }
    private void SelectEnemyGenerals()
    {
        if (defenseFaction == null || defenseFaction.generals == null)
            return;

        List<General> availableGenerals = defenseFaction.generals;
        List<General> selectedEnemies = new List<General>();

        if (availableGenerals.Count <= 5)
        {
            // 5명 이하면 전부 선택
            foreach (var general in availableGenerals)
            {
                Managers.Battle.AddEnemyGenerals(general);
            }
        }
        else
        {
            // 6명 이상이면 랜덤하게 5명 선택
            // 리스트를 섞고 앞에서 5명 선택
            List<General> shuffledGenerals = new List<General>(availableGenerals);
            for (int i = shuffledGenerals.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                General temp = shuffledGenerals[i];
                shuffledGenerals[i] = shuffledGenerals[randomIndex];
                shuffledGenerals[randomIndex] = temp;
            }

            for (int i = 0; i < 5 && i < shuffledGenerals.Count; i++)
            {
                Managers.Battle.AddEnemyGenerals(shuffledGenerals[i]);
            }
        }

        Debug.Log($"Selected {Managers.Battle.enemyGenerals.Count} enemy generals for battle");
    }

}
