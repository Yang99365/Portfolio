using UnityEngine;
using System.Collections;

public class FlyMovement : Movement 
{
  public override IEnumerator Traverse (Tile tile)
  {
    // 시작 타일과 대상 타일 사이의 거리를 저장합니다
    float dist = Mathf.Sqrt(Mathf.Pow(tile.pos.x - unit.tile.pos.x, 2) + Mathf.Pow(tile.pos.y - unit.tile.pos.y, 2));
    unit.Place(tile);
    // 지상 타일을 통과하지 않을 만큼 높이 날아갑니다.
    float y = Tile.stepHeight * 10;
    float duration = (y - jumper.position.y) * 0.5f;
    Tweener tweener = jumper.MoveToLocal(new Vector3(0, y, 0), duration, EasingEquations.EaseInOutQuad);
    while (tweener != null)
      yield return null;
    // 일반 방향을 향하도록 회전
    Directions dir;
    Vector3 toTile = (tile.center - transform.position);
    if (Mathf.Abs(toTile.x) > Mathf.Abs(toTile.z))
      dir = toTile.x > 0 ? Directions.East : Directions.West;
    else
      dir = toTile.z > 0 ? Directions.North : Directions.South;
    yield return StartCoroutine(Turn(dir));
    //올바른 위치로 이동
    duration = dist * 0.5f;
    tweener = transform.MoveTo(tile.center, duration, EasingEquations.EaseInOutQuad);
    while (tweener != null)
      yield return null;
    // 땅
    duration = (y - tile.center.y) * 0.5f;
    tweener = jumper.MoveToLocal(Vector3.zero, 0.5f, EasingEquations.EaseInOutQuad);
    while (tweener != null)
      yield return null;
  }
}
