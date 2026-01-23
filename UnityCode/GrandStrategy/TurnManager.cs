using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class TurnManager
{
    public int turn = 1;
    public EGamePhase gamePhase = EGamePhase.Firsttime;

    public delegate void TurnChanged();
    public static event TurnChanged OnTurnChanged;


    void NowGamePhase()
    {
        if (turn == 1)
        {
            gamePhase = EGamePhase.Firsttime;
        }
        else if (turn >= 2 && turn <= 10)
        {
            gamePhase = EGamePhase.EarlyGame;
        }
        else if (turn >= 11 && turn <= 20)
        {
            gamePhase = EGamePhase.MidGame;
        }
        else if (turn >= 21)
        {
            gamePhase = EGamePhase.LateGame;
        }
    }

    public void NextTurn()
    {
        foreach (var faction in Managers.Game.factions)
        {
            if (faction.controlledRegions.Count > 0)
            {
                faction.CollectResources();
            }
        }

        foreach (var faction in Managers.Game.factions)
        {
            if (faction.FactionID != Managers.Game.selectedFactionId) // 플레이어 세력이 아닌 경우
            {
                Managers.AI.ProcessAITurn(faction);
            }
        }


        turn++;
        NowGamePhase();
        Managers.Shop.RerollShop();
        Managers.Game.OnTurnEnd();
        OnTurnChanged?.Invoke();
    }

    // NextTurn() 함수를 호출하면 추가적으로 모든 세력들에게 자신이 가진 지역에 대한 자원을 계산하고,
    // 그에 따른 자원 변동을 적용하는 함수를 호출해야 합니다.

    // 자원 변동을 적용하는 함수는 다음과 같은 형태로 작성
    // public void ApplyResourceChange()
    // {
    //     각 세력별로 소유하고있는 지역을 체크하면서 그곳의 건물레벨에 따라 임시변수sum에 값을 담아서 더해주고
    //     더한 후 다음 세력으로 넘어가서 같은 작업을 반복
    //     이후 UI에 TurnChanged 이벤트 구독한거에 맞게 UI에 반영
    // }

    // 시나리오 이벤트 DB같은게 있으면 가져와서 턴체크하고 조건체크해서 이벤트 발생
    // 적들의 건물을  랜덤한 확률로 골드체크해서 그 세력의 소유 지역을 랜덤으로 골라 그지역 건물 업글(임시테스트용이기에)
    // 건물 업글했는지 체크하기위해 Debug.Log도 작성해야함.


    // 적행동 관련 ------------------------------------------------
    // 랜덤한 확률로 이벤트 하나를 골라 적 세력의 행동으로 실행,(자원이 충분한 이벤트만 가져와서 고르기) 디버그로그 필요
    // 랜덤 확률로 호감도(구현안함) 체크해서 랜덤확률로 전쟁선포
    //턴지날수록 자동으로 호감도 감소 OnTurnChanged 이벤트로 게임매니저에게 전?달 해서 게임매니저가 호감도 조절해줘야할듯함.

}
