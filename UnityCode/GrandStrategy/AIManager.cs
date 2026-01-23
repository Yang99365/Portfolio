using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIManager
{
    private const float DIPLOMACY_ACTION_CHANCE = 0.05f; // 5% 확률로 외교 행동
    private System.Random random = new System.Random();

    #region MapActionAI
    public void ProcessAITurn(Faction aiFaction)
    {
        // 멸망한 세력은 행동하지 않음
        if (aiFaction.controlledRegions.Count == 0)
        {
            Debug.Log($"{aiFaction.FactionName} is defeated and cannot take actions.");
            return;
        }

        // 1. 건물 업그레이드 결정
        ProcessBuildingUpgrades(aiFaction);

        // 2. 외교 행동 결정
        ProcessDiplomacyActions(aiFaction);
    }

    private void ProcessBuildingUpgrades(Faction faction)
    {
        foreach (Region region in faction.controlledRegions)
        {
            // 각 건물 타입에 대해 업그레이드 시도
            foreach (Building building in region.Buildings)
            {
                // 건물 업그레이드 비용의 3배 이상의 골드를 보유중일 때만 업그레이드 고려
                if (faction.GetResourceAmount(Define.EResourceType.Gold) >= building.UpgradeCost * 3)
                {
                    float upgradeChance = 0.3f; // 30% 기본 확률

                    // 건물 레벨이 낮을수록 업그레이드 확률 증가
                    upgradeChance += (5 - building.Level) * 0.1f;

                    if (Random.value < upgradeChance)
                    {
                        region.UpgradeBuilding(building.Type);
                        Debug.Log($"{faction.FactionName}이(가) {region.RegionName}의 {building.Type}을(를) 업그레이드했습니다. (레벨 {building.Level})");
                    }
                }
            }
        }
    }

    private void ProcessDiplomacyActions(Faction faction)
    {
        if (Random.value > DIPLOMACY_ACTION_CHANCE)
            return;

        List<Faction> otherFactions = Managers.Game.factions
            .Where(f => f.FactionID != faction.FactionID)
            .ToList();

        if (otherFactions.Count == 0)
            return;

        // 무작위로 대상 세력 선택
        Faction targetFaction = otherFactions[Random.Range(0, otherFactions.Count)];
        bool isTargetPlayer = (targetFaction.FactionID == Managers.Game.selectedFactionId);

        // 현재 관계에 따른 행동 결정
        if (faction.allies.Contains(targetFaction))
        {
            // 동맹 관계일 때
            if (Random.value < 0.2f) // 20% 확률로 동맹 파기
            {
                faction.BreakAlliance(targetFaction);
                Debug.Log($"{faction.FactionName}이(가) {targetFaction.FactionName}와(과)의 동맹을 파기했습니다.");
            }
        }
        else if (faction.enemies.Contains(targetFaction))
        {
            // 적대 관계일 때
            if (Random.value < 0.3f) // 30% 확률로 휴전 제안
            {
                if (isTargetPlayer)
                {
                    // 플레이어의 선택을 기다림
                    UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    popup.SetInfo(
                        $"{faction.FactionName}이(가) 휴전을 제안했습니다. 수락하시겠습니까?",
                        () => {
                            faction.MakeTruce(targetFaction);
                            Debug.Log($"플레이어가 {faction.FactionName}의 휴전 제안을 수락했습니다.");
                        },
                        "휴전 제안"
                    );
                }
                else
                {
                    faction.MakeTruce(targetFaction);
                    Debug.Log($"{faction.FactionName}이(가) {targetFaction.FactionName}와(과) 휴전했습니다.");
                }
            }
        }
        else
        {
            // 중립 관계일 때
            if (Random.value < 0.4f) // 40% 확률로 전쟁 선포
            {
                faction.DeclareWar(targetFaction);
                Debug.Log($"{faction.FactionName}이(가) {targetFaction.FactionName}에게 전쟁을 선포했습니다.");
            }
            else if (Random.value < 0.3f) // 30% 확률로 동맹 제안
            {
                if (isTargetPlayer)
                {
                    // 플레이어의 선택을 기다림
                    UI_ConfirmPopup popup = Managers.UI.ShowPopupUI<UI_ConfirmPopup>();
                    popup.SetInfo(
                        $"{faction.FactionName}이(가) 동맹을 제안했습니다. 수락하시겠습니까?",
                        () => {
                            faction.FormAlliance(targetFaction);
                            Debug.Log($"플레이어가 {faction.FactionName}의 동맹 제안을 수락했습니다.");
                        },
                        "동맹 제안"
                    );
                }
                else
                {
                    faction.FormAlliance(targetFaction);
                    Debug.Log($"{faction.FactionName}이(가) {targetFaction.FactionName}와(과) 동맹을 맺었습니다.");
                }
            }
        }
    }
    #endregion
}