using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class InfiniteAbilityRange : AbilityRange 
{
	public override bool positionOriented { get { return false; }}// 위치 지향적인 능력 범위가 아닙니다.
	public override List<Tile> GetTilesInRange (Board board)
	{
		return new List<Tile>(board.tiles.Values);
	}
}