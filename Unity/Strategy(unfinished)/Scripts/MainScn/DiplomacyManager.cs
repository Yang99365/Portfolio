using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DiplomacyStatus
{
    Neutral,
    Ally,
    War
}

public class DiplomacyManager : MonoBehaviour
{
    public Dictionary<string, Dictionary<string, DiplomacyStatus>> diplomacyStatus;

    public static DiplomacyManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        diplomacyStatus = new Dictionary<string, Dictionary<string, DiplomacyStatus>>();
    }

    public void InitializeDiplomacy(List<Faction> allFactions)
    {
        foreach (var faction in allFactions)
        {
            if (!diplomacyStatus.ContainsKey(faction.factionName))
            {
                diplomacyStatus[faction.factionName] = new Dictionary<string, DiplomacyStatus>();
            }

            foreach (var otherFaction in allFactions)
            {
                if (faction.factionName != otherFaction.factionName)
                {
                    diplomacyStatus[faction.factionName][otherFaction.factionName] = DiplomacyStatus.Neutral;
                }
            }
        }
    }
//외교상태 바꾸기
    public void UpdateDiplomacyStatus(string faction1, string faction2, DiplomacyStatus newStatus)
    {
        diplomacyStatus[faction1][faction2] = newStatus;
        diplomacyStatus[faction2][faction1] = newStatus; // 외교 상태는 양측에 적용
    }

//외교상태 가져오기
    public DiplomacyStatus GetDiplomacyStatus(string faction1, string faction2)
    {
        if (diplomacyStatus.ContainsKey(faction1) && diplomacyStatus[faction1].ContainsKey(faction2))
    {
        return diplomacyStatus[faction1][faction2];
    }
        else
    {
        // 적절한 예외 처리 또는 로깅
        return DiplomacyStatus.Neutral;  // 또는 다른 기본값
    }
    }
}
