using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SelectBoxPopup : UI_Popup
{
    enum GameObjects
    {
        SelectionBox
    }
    private Camera mainCamera;
    private RectTransform boxVisual;
    private Rect selectionBox;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private LayerMask unitLayer;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));

        mainCamera = Camera.main;
        boxVisual = GetObject((int)GameObjects.SelectionBox).GetComponent<RectTransform>();
        unitLayer = LayerMask.GetMask("GeneralUnit");

        // 초기 설정
        startPosition = Vector2.zero;
        endPosition = Vector2.zero;
        DrawVisual();

        return true;
    }
    void Update()
    {
        // 마우스 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox = new Rect();
        }

        // 드래그 중
        if (Input.GetMouseButton(0))
        {
            
            endPosition = Input.mousePosition;
            DrawVisual();
            DrawSelection();
        }

        // 마우스 버튼 해제
        if (Input.GetMouseButtonUp(0))
        {
            SelectUnits();
            startPosition = Vector2.zero;
            endPosition = Vector2.zero;
            DrawVisual();
        }
    }
    void DrawVisual()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;
        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        boxVisual.position = boxCenter;

        Vector2 boxSize = new Vector2(
            Mathf.Abs(boxStart.x - boxEnd.x),
            Mathf.Abs(boxStart.y - boxEnd.y)
        );
        boxVisual.sizeDelta = boxSize;
    }

    void DrawSelection()
    {
        // X축 설정
        if (Input.mousePosition.x < startPosition.x)
        {
            selectionBox.xMin = Input.mousePosition.x;
            selectionBox.xMax = startPosition.x;
        }
        else
        {
            selectionBox.xMin = startPosition.x;
            selectionBox.xMax = Input.mousePosition.x;
        }

        // Y축 설정
        if (Input.mousePosition.y < startPosition.y)
        {
            selectionBox.yMin = Input.mousePosition.y;
            selectionBox.yMax = startPosition.y;
        }
        else
        {
            selectionBox.yMin = startPosition.y;
            selectionBox.yMax = Input.mousePosition.y;
        }
    }

    void SelectUnits()
    {
        // BattleManager의 allUnits 리스트를 순회하며 선택 영역 내의 유닛 선택
        foreach (var unit in Managers.Battle.allUnits)
        {
            if (unit != null &&
            selectionBox.Contains(mainCamera.WorldToScreenPoint(unit.transform.position)) &&
            Managers.Battle.CanSelectUnit(unit))  // 선택 가능 여부 확인
            {
                Managers.Battle.MultiSelect(unit);
            }
        }
    }
}
