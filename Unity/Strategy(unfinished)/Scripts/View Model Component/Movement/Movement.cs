using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public abstract class Movement : MonoBehaviour
{
    public int range { get { return stats[StatTypes.MOV]; }}
	public int jumpHeight { get { return stats[StatTypes.JMP]; }}
    protected GeneralUnit unit;
    protected Transform jumper;
    protected Stats stats;

    public abstract IEnumerator Traverse (Tile tile);

    protected virtual void Awake ()
    {
        unit = GetComponent<GeneralUnit>();
        jumper = transform.Find("Jumper");
    }
    protected virtual void Start ()
	{
		stats = GetComponent<Stats>();
	}

    public virtual List<Tile> GetTilesInRange (Board board)
    {
        List<Tile> retValue = board.Search( unit.tile, ExpandSearch );
        Filter(retValue);
        return retValue;
    }
    protected virtual bool ExpandSearch (Tile from, Tile to)
    {
        return (from.distance + 1) <= range;
    }
    protected virtual void Filter (List<Tile> tiles) // 이동 가능한 타일만 남김
    {
        for (int i = tiles.Count - 1; i >= 0; --i)
            if (tiles[i].content != null || tiles[i].isPassable == false) // 타일에 콘텐츠가 있거나 이동 불가능한 타일이면
                tiles.RemoveAt(i);
    }
    protected virtual IEnumerator Turn (Directions dir)
    {
    TransformLocalEulerTweener t = (TransformLocalEulerTweener)transform.RotateToLocal(dir.ToEuler(), 0.25f, EasingEquations.EaseInOutQuad);
    
    // 북쪽과 서쪽 사이를 회전할 때 단위처럼 보이도록 예외를 만들어야 합니다.
    // 가장 효율적인 방식으로 회전합니다(0과 360이 동일하게 처리되므로).
    if (Mathf.Approximately(t.startTweenValue.y, 0f) && Mathf.Approximately(t.endTweenValue.y, 270f))
			t.startTweenValue = new Vector3(t.startTweenValue.x, 360f, t.startTweenValue.z);
		else if (Mathf.Approximately(t.startTweenValue.y, 270) && Mathf.Approximately(t.endTweenValue.y, 0))
			t.endTweenValue = new Vector3(t.startTweenValue.x, 360f, t.startTweenValue.z);

    unit.dir = dir;
    
    while (t != null)
        yield return null;
    }
}