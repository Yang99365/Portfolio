using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using sound.playerSound;

public class InitBattleState : BattleState 
{
  public override void Enter ()
  {
    base.Enter ();
    StartCoroutine(Init());
  }
  IEnumerator Init ()
  {
    _PlayerSound.instance.PlayPlayerBGM(_PlayerSound.PlayerBgm.Bgm1); // 배경음악 재생 씬에 따라 다르게 설정
    board.Load( levelData ); // 레벨 데이터에 따라 보드와 브금을 로드
    Point p = new Point((int)levelData.tiles[0].x, (int)levelData.tiles[0].z);
    SelectTile(p);
    SpawnTestUnits ();
    AddVictoryCondition();
    owner.round = owner.gameObject.AddComponent<TurnOrderController>().Round();
    yield return null;
    //owner.ChangeState<SelectUnitState>();
    owner.ChangeState<CutSceneState>();
  }
  // 이 스크립트를 특정 스폰 위치에 유닛을 생성하도록 수정해야함, 레시피 목록도
  // 전투씬 진입 전 전략맵 씬에서 선택한 유닛을 가져와서 생성하도록 수정해야함
  void SpawnTestUnits ()
  {
    string[] recipes = new string[] // 레시피 목록 (전사 마법사 도적 (임시))
    {
      "Alaois",
			"Hania",
			"Kamau",
			"Enemy Rogue",
			"Enemy Warrior",
			"Enemy Wizard"
    };
    GameObject unitContainer = new GameObject("Units");
		unitContainer.transform.SetParent(owner.transform);

    List<Tile> locations = new List<Tile>(board.tiles.Values);
    for (int i = 0; i < recipes.Length; ++i)
    {
      int level = UnityEngine.Random.Range(9, 12);
      GameObject instance = UnitFactory.Create(recipes[i], level);
      instance.transform.SetParent(unitContainer.transform);
        
      int random = UnityEngine.Random.Range(0, locations.Count);
      Tile randomTile = locations[ random ];
      locations.RemoveAt(random);

      GeneralUnit unit = instance.GetComponent<GeneralUnit>();
      unit.Place( randomTile );
			unit.dir = (Directions)UnityEngine.Random.Range(0, 4);
      
      unit.Match();

      units.Add(unit);
    }

    SelectTile(units[0].tile.pos);
  }

  void AddVictoryCondition ()
	{
		DefeatTargetVictoryCondition vc = owner.gameObject.AddComponent<DefeatTargetVictoryCondition>();
		GeneralUnit enemy = units[ units.Count - 1 ];
		vc.target = enemy;
		Health health = enemy.GetComponent<Health>();
		health.MinHP = 10;
	}
  
}
