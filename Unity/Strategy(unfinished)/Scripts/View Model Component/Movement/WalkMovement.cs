using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class WalkMovement : Movement 
{
    // Add code here
  protected override bool ExpandSearch (Tile from, Tile to)
  {
  // 두 타일 사이의 높이 거리가 유닛이 점프할 수 있는 것보다 크면 건너뜁니다.
  if ((Mathf.Abs(from.height - to.height) > jumpHeight))
    return false;
    
  // 적이 타일을 점유하고 있으면 건너뜁니다.
  if (to.content != null)
    return false;

  return base.ExpandSearch(from, to);
  }

  public override IEnumerator Traverse (Tile tile)
  {
    unit.Place(tile);
    // 유닛의 웨이포인트 목록을 작성합니다.
    // 타일을 대상 타일까지 시작
    List<Tile> targets = new List<Tile>();
    while (tile != null)
    {
      targets.Insert(0, tile);
      tile = tile.prev;
    }
    // 각 웨이포인트로 연속 이동
    for (int i = 1; i < targets.Count; ++i)
    {
      Tile from = targets[i-1];
      Tile to = targets[i];
      Directions dir = from.GetDirection(to);
      if (unit.dir != dir)
        yield return StartCoroutine(Turn(dir));
      if (from.height == to.height)
        yield return StartCoroutine(Walk(to));
      else
        yield return StartCoroutine(Jump(to));
    }
    yield return null;
  }

  IEnumerator Walk (Tile target)
  {
    Tweener tweener = transform.MoveTo(target.center, 0.5f, EasingEquations.Linear);
    while (tweener != null)
      yield return null;
  }

  IEnumerator Jump (Tile to)
  {
    Tweener tweener = transform.MoveTo(to.center, 0.5f, EasingEquations.Linear);
    Tweener t2 = jumper.MoveToLocal(new Vector3(0, Tile.stepHeight * 2f, 0), tweener.duration / 2f, EasingEquations.EaseOutQuad);
		t2.loopCount = 1;
		t2.loopType = EasingControl.LoopType.PingPong;
    while (tweener != null)
      yield return null;
  }
}