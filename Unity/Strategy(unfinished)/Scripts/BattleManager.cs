using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    public Faction selectedFaction;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (newScene.buildIndex == 0)
        {
            BattleContainer.instance.Playergenerals = new GeneralBase[3];
            BattleContainer.instance.Enemygenerals = new GeneralBase[3];
            BattleContainer.instance.playerGeneral = 0;
            BattleContainer.instance.enemyGeneral = 0;
        }
            
    }

    // BattleScene으로 이동한다.
    public void StartBattle()
    {
        GetEnemyGenerals();
        // BattleContainer의 적 장군은 선택한 지역을 지닌 
        //그 세력이 보유한 무장들 중 랜덤으로 3명을 선택한다.
        // RegionManager의 지역정보를 가져와 해당지역에 맞는 씬을 로드한다.
        //RegionManager를 Map과 분리해야함. 그리고 지역정보만 담게하고
        //Map에는 따로 새 코드를 만들어야할듯함.

        // 배치되어있으면 전투씬으로 이동한다.
        if (BattleContainer.instance.Playergenerals[0] == null)
        {
            Debug.Log("아군 장군이 배치되지 않았습니다.");
            return;
        }
        SceneManager.LoadScene("TestBattle2");
    }
    public void GetEnemyGenerals()
    {
        // 선택한 지역의 세력을 가져온다.
        selectedFaction = FactionManager.instance.GetFactionByRegionName(RegionManager.instance.selectedRegion.regionName);
        // GeneralManager을 통해 AnotherFactionGenerals에 있는 선택한 지역의 세력이 보유한 무장들을 가져온다.하지만 무장의 체력이 0은 아닌지 확인해야한다.
        List<GeneralBase> generals = GeneralManager.instance.AnotherFactionGenerals.FindAll(g => g.faction == selectedFaction.factionName);
        List<GeneralBase> liveGenerals = generals.FindAll(g => g.hp > 0);
        // 무작위로 중복 없이 최대 3명을 선택한다. 3명 미만이라면 나머지는 null로 채운다. 
        for (int i = 0; i < 3; i++)
        {
            if (liveGenerals.Count > 0)
            {
                int randomIndex = Random.Range(0, liveGenerals.Count);
                BattleContainer.instance.Enemygenerals[i] = liveGenerals[randomIndex];
                BattleContainer.instance.enemyGeneral++;
                liveGenerals.RemoveAt(randomIndex);
            }
            else
            {
                BattleContainer.instance.Enemygenerals[i] = null;
            }
        }
    }
}
