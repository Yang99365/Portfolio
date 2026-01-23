using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class BattleManager
{
    private Faction attackingFaction;
    private Faction defendingFaction;
    private Region targetRegion;

    //ScenarioScene
    public List<General> myGenerals = new List<General>();
    public List<General> enemyGenerals = new List<General>();

    //BattleScene
    public List<GameObject> allUnits = new List<GameObject>();
    public List<GameObject> myUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();
    public List<GameObject> unitSelected = new List<GameObject>();

    public bool isBattleEnded = false;
    public ETeamType victoriousTeam = ETeamType.None;
    public delegate void BattleEndDelegate(ETeamType winner);
    public event BattleEndDelegate OnBattleEnd;
    public void SetupBattle(Faction attacker, Faction defender, Region target)
    {
        attackingFaction = attacker;
        defendingFaction = defender;
        targetRegion = target;
    }
    public void InitializeBattle()
    {
        // 전투 상태 변수 초기화
        isBattleEnded = false;
        victoriousTeam = ETeamType.None;

        // 유닛 리스트 초기화
        allUnits.Clear();
        myUnits.Clear();
        enemyUnits.Clear();
        unitSelected.Clear();

        // 이벤트 리스너 초기화 (필요한 경우)
        if (OnBattleEnd != null)
        {
            foreach (var d in OnBattleEnd.GetInvocationList())
            {
                OnBattleEnd -= (BattleEndDelegate)d;
            }
        }

        Debug.Log("Battle Manager initialized for new battle");
    }
    public List<GeneralUnit> GetSelectedUnits()
    {
        // unitSelected 리스트의 유닛들을 GeneralUnit 컴포넌트로 변환하여 반환
        return unitSelected
            .Where(obj => obj != null && obj.GetComponent<GeneralUnit>() != null)
            .Select(obj => obj.GetComponent<GeneralUnit>())
            .ToList();
    }

    #region InScenarioScene
    public void AddmyGenerals(General general, bool isSelect)
    {
        if (general == null)
            return;

        if (myGenerals == null)
            myGenerals = new List<General>();

        if (isSelect)
        {
            if (!myGenerals.Contains(general))
            {
                // 리스트의 마지막에 추가하여 순서 보장
                myGenerals.Add(general);
            }
        }
        else
        {
            // 제거할 때는 순서 유지하면서 제거
            int index = myGenerals.IndexOf(general);
            if (index != -1)
            {
                myGenerals.RemoveAt(index);
            }
        }
    }

    public void AddEnemyGenerals(General general)
    {
        if (general == null)
            return;

        if (enemyGenerals == null)
            enemyGenerals = new List<General>();

        if (!enemyGenerals.Contains(general))
        {
            enemyGenerals.Add(general);
        }
    }

    public void ResetGenerals()
    {
        if (myGenerals != null)
            myGenerals.Clear();
        if (enemyGenerals != null)
            enemyGenerals.Clear();
    }

    public int GetMyGeneralCount()
    {
        return myGenerals?.Count ?? 0;
    }
    public int GetEnemyGeneralCount()
    {
        return enemyGenerals?.Count ?? 0;
    }

    public bool HasEnemyGeneral(General general)
    {
        return enemyGenerals?.Contains(general) ?? false;
    }

    public bool HasGeneral(General general)
    {
        return myGenerals?.Contains(general) ?? false;
    }
    //테스트용
    public void NowGeneral()
    {
        //현재 무장 개수 말하기
        Debug.Log($"현재 무장 개수 : {myGenerals.Count}");
    }
    #endregion
    #region InBattleScene

    public void AddUnit(GameObject unit)
    {
        if (unit == null)
            return;

        if (allUnits == null)
            allUnits = new List<GameObject>();

        allUnits.Add(unit);
        
    }
    

    public void RemoveUnit(GameObject unit) // 전투가 끝날때 모든 유닛이 이걸 호출하도록
    {
        if (unit == null)
            return;

        if (allUnits == null)
            return;

        if (allUnits.Contains(unit))
        {
            allUnits.Remove(unit);
        }
        
    }

    public bool CanSelectUnit(GameObject obj)
    {
        GeneralUnit unit = obj.GetComponent<GeneralUnit>();
        return unit != null && unit.Team == ETeamType.My;
    }

    public void DeselectAll()
    {
        foreach (var unit in unitSelected)
        {
            EnableUnitMovement(unit, false);
            TriggerSelectionIndicator(unit, false);
        }

        //groundMarker.SetActive(false);
        unitSelected.Clear();
    }

    public void SelectByClicking(GameObject obj)
    {
        if (!CanSelectUnit(obj))
            return;

        DeselectAll();

        unitSelected.Add(obj);

        TriggerSelectionIndicator(obj, true);
        EnableUnitMovement(obj, true);
        Debug.Log("선택");

    }
    public void MultiSelect(GameObject obj)
    {
        if (!CanSelectUnit(obj))
            return;

        if (unitSelected.Contains(obj) == false)
        {
            unitSelected.Add(obj);

            TriggerSelectionIndicator(obj, true);
            EnableUnitMovement(obj, true);
        }
        else
        {
            EnableUnitMovement(obj, false);
            TriggerSelectionIndicator(obj, false);
            unitSelected.Remove(obj);
        }
    }

    public void EnableUnitMovement(GameObject obj, bool canMove)
    {
        if (obj == null)
            return;
        obj.GetComponent<GeneralUnit>().isSelect = canMove;
    }

    private void TriggerSelectionIndicator(GameObject obj, bool isVisible)
    {
        obj.transform.GetChild(1).gameObject.SetActive(isVisible);
    }
    #endregion
    public void MoveSelectedUnits(Vector2 targetPosition)
    {
        if (unitSelected.Count == 0) return;

        List<Vector2> positions = GetFormationPositions(targetPosition, unitSelected.Count);

        for (int i = 0; i < unitSelected.Count; i++)
        {
            if (unitSelected[i].TryGetComponent<GeneralUnit>(out var unit))
            {
                Vector2 dest = positions[i];
                // A* 그래프에서 이동 가능한 위치인지 확인
                GridGraph grid = AstarPath.active.data.gridGraph;
                GraphNode node = grid.GetNearest(new Vector3(dest.x, dest.y, 0)).node;

                if (node != null && !node.Walkable)
                {
                    // 주변 8방향을 검사하여 이동 가능한 가장 가까운 위치 찾기
                    Vector2[] directions = {
                        Vector2.up, Vector2.right, Vector2.down, Vector2.left,
                        new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                        new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                    };

                    float searchRadius = 1f;
                    foreach (var dir in directions)
                    {
                        Vector2 checkPos = dest + dir * searchRadius;
                        GraphNode checkNode = grid.GetNearest(new Vector3(checkPos.x, checkPos.y, 0)).node;
                        if (checkNode != null && checkNode.Walkable)
                        {
                            dest = checkPos;
                            break;
                        }
                    }
                }
                unit.MoveToPosition(dest);
            }
        }
    }

    private List<Vector2> GetFormationPositions(Vector2 center, int count)
    {
        List<Vector2> positions = new List<Vector2>();

        if (count == 1)
        {
            positions.Add(center);
            return positions;
        }

        float radius = count * 0.5f;  // 유닛 수에 따라 원형 대형 크기 조정
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            float x = center.x + radius * Mathf.Cos(angle);
            float y = center.y + radius * Mathf.Sin(angle);
            positions.Add(new Vector2(x, y));
        }

        return positions;
    }
    public List<Vector2> GetCircularFormation(int unitCount, float radius)
    {
        List<Vector2> positions = new List<Vector2>();
        float angleStep = 360f / unitCount;
        float startAngle = 90f; // 시작 각도를 90도로 설정하여 앞쪽부터 배치

        for (int i = 0; i < unitCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            float radian = currentAngle * Mathf.Deg2Rad;

            // radius를 더 작게 조정 (0.8f 정도로)
            float x = radius * 0.8f * Mathf.Cos(radian);
            float y = radius * 0.8f * Mathf.Sin(radian);
            positions.Add(new Vector2(x, y));
        }

        return positions;
    }

    public List<Vector2> GetSquareFormation(int unitCount, float spacing)
    {
        List<Vector2> positions = new List<Vector2>();
        int sideLength = Mathf.CeilToInt(Mathf.Sqrt(unitCount));

        for (int i = 0; i < unitCount; i++)
        {
            int row = i / sideLength;
            int col = i % sideLength;
            float x = (col - (sideLength - 1) / 2f) * spacing;
            float y = (row - (sideLength - 1) / 2f) * spacing;
            positions.Add(new Vector2(x, y));
        }

        return positions;
    }

    #region BattleScene
    public void UnitDeath(Creature unit)
    {
        GameObject unitObj = unit.gameObject;
        RemoveUnit(unitObj);

        CheckBattleResult();
    }
    private void CheckBattleResult()
    {
        if (isBattleEnded) return;

        // 각 팀의 살아있는 유닛 수 확인
        bool myTeamAlive = allUnits.Any(unit =>
        {
            GeneralUnit generalUnit = unit.GetComponent<GeneralUnit>();
            return generalUnit != null && generalUnit.Team == ETeamType.My &&
                   generalUnit.State != ECreatureState.Dead;
        });

        bool enemyTeamAlive = allUnits.Any(unit =>
        {
            GeneralUnit generalUnit = unit.GetComponent<GeneralUnit>();
            return generalUnit != null && generalUnit.Team == ETeamType.Enemy &&
                   generalUnit.State != ECreatureState.Dead;
        });

        // 승리 조건 체크
        if (!myTeamAlive && enemyTeamAlive)
        {
            EndBattle(ETeamType.Enemy);
        }
        else if (myTeamAlive && !enemyTeamAlive)
        {
            EndBattle(ETeamType.My);
        }
        else if (!myTeamAlive && !enemyTeamAlive)
        {
            EndBattle(ETeamType.None); // 무승부
        }
    }

    private void EndBattle(ETeamType winner)
    {
        isBattleEnded = true;
        victoriousTeam = winner;

        // 모든 살아있는 유닛의 AI 정지
        foreach (var unit in allUnits)
        {
            GeneralUnit generalUnit = unit.GetComponent<GeneralUnit>();
            if (generalUnit != null && generalUnit.State != ECreatureState.Dead)
            {
                generalUnit.State = ECreatureState.Idle;
                if (generalUnit.ai != null)
                    generalUnit.ai.isStopped = true;
            }
        }

        Debug.Log($"Battle Ended! Winner: {winner}");
        OnBattleEnd?.Invoke(winner);

        // 전투 종료 후 시나리오 씬으로 전환 또는 결과 UI 표시 등의 처리
        HandleBattleEnd(winner);
    }

    private void HandleBattleEnd(ETeamType winner)
    {
        // 전투 결과에 따른 처리
        switch (winner)
        {
            case ETeamType.My:
                // 플레이어 승리 처리
                TransferTerritory(attackingFaction, defendingFaction, targetRegion);
                Debug.Log("Player Victory!");
                break;
            case ETeamType.Enemy:
                // 적 승리 처리
                Debug.Log("Enemy Victory!");
                break;
            case ETeamType.None:
                // 무승부 처리
                Debug.Log("Battle ended in a draw!");
                break;
        }

        // 3초 후 시나리오 씬으로 전환
        BattleScene battleScene = GameObject.FindObjectOfType<BattleScene>();
        if (battleScene != null)
        {
            battleScene.StartCoroutine(DelayedSceneChange());
        }
        else
        {
            Debug.LogError("BattleScene not found!");
        }
    }
    private IEnumerator DelayedSceneChange()
    {
        yield return new WaitForSeconds(3.0f);
        Managers.Scene.UnloadScene(EScene.BattleScene);
        // 기존 유닛들 정리
        foreach (var unit in allUnits.ToList())
        {
            if (unit != null)
            {
                GeneralUnit generalUnit = unit.GetComponent<GeneralUnit>();
                if (generalUnit != null)
                {
                    Managers.Object.Despawn(generalUnit);
                }
            }
        }
    }

    private void TransferTerritory(Faction newOwner, Faction previousOwner, Region region)
    {
        // 이전 소유자의 지역 목록에서 제거
        previousOwner.RemoveControlledRegion(region);

        // 새로운 소유자의 지역 목록에 추가
        newOwner.AddControlledRegion(region);

        // 지역의 소유 세력 ID 업데이트
        region.controllingFactionId = newOwner.FactionID;

        // 맵 색상 업데이트
        Managers.Map.UpdateRegionColorsAccordingToFactions();
    }
    #endregion
}

