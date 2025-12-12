using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AttackOption 
{
	#region Classes
	class Mark // 타일에 대한 정보를 저장합니다. AttackOption에서만 사용됩니다.
	{
		public Tile tile;
		public bool isMatch;
		
		public Mark (Tile tile, bool isMatch)
		{
			this.tile = tile;
			this.isMatch = isMatch;
		}
	}
	#endregion
	#region Fields
	public Tile target;
	public Directions direction;
	public List<Tile> areaTargets = new List<Tile>();
	public bool isCasterMatch; // 이동하기 좋은 위치인가?
	public Tile bestMoveTile { get; private set; } // 공격하기 가장 좋은 위치의 타일입니다.
	public int bestAngleBasedScore { get; private set; } // 공격 각도에 따른 점수입니다.
	List<Mark> marks = new List<Mark>();
	List<Tile> moveTargets = new List<Tile>();
	#endregion
	#region Public
	public void AddMoveTarget (Tile tile)
	{
		// 시전자에게 부정적인 영향을 미칠 타일로 이동하는 것을 허용하지 않습니다.
		if (!isCasterMatch && areaTargets.Contains(tile))
			return;
		moveTargets.Add(tile);
	}
	public void AddMark (Tile tile, bool isMatch)
	{
		marks.Add (new Mark(tile, isMatch));
	}

	// 원하는 유형의 대상 수에 따라 옵션의 점수를 매깁니다.
	public int GetScore (GeneralUnit caster, Ability ability)
	{
		GetBestMoveTarget(caster, ability);
		if (bestMoveTile == null)
			return 0;
		int score = 0;
		for (int i = 0; i < marks.Count; ++i)
		{
			if (marks[i].isMatch)
				score++;
			else
				score--;
		}
		if (isCasterMatch && areaTargets.Contains(bestMoveTile))
			score++;
		return score;
	}
	#endregion
	#region Private
	// 시전자가 공격할 수 있는 가장 효과적인 지점인 타일을 반환합니다.
	void GetBestMoveTarget (GeneralUnit caster, Ability ability)
	{
		if (moveTargets.Count == 0)
			return;
		
		if (IsAbilityAngleBased(ability)) // 공격 각도가 게임에서 중요한 요소인지 여부를 나타냅니다.
		{
			bestAngleBasedScore = int.MinValue;
			Tile startTile = caster.tile;
			Directions startDirection = caster.dir;
			caster.dir = direction;
			List<Tile> bestOptions = new List<Tile>();
			for (int i = 0; i < moveTargets.Count; ++i)
			{
				caster.Place(moveTargets[i]);
				int score = GetAngleBasedScore(caster);
				if (score > bestAngleBasedScore)
				{
					bestAngleBasedScore = score;
					bestOptions.Clear();
				}
				if (score == bestAngleBasedScore)
				{
					bestOptions.Add(moveTargets[i]);
				}
			}
			
			caster.Place(startTile);
			caster.dir = startDirection;
			FilterBestMoves(bestOptions);
			bestMoveTile = bestOptions[ UnityEngine.Random.Range(0, bestOptions.Count) ];
		}
		else
		{
			bestMoveTile = moveTargets[ UnityEngine.Random.Range(0, moveTargets.Count) ];
		}
	}
	
	bool IsAbilityAngleBased (Ability ability) // 적중률이 각도에 의존하는지 여부를 나타냅니다.
	{
		bool isAngleBased = false;
		for (int i = 0; i < ability.transform.childCount; ++i)
		{
			HitRate hr = ability.transform.GetChild(i).GetComponent<HitRate>();
			if (hr.IsAngleBased)
			{
				isAngleBased = true;
				break;
			}
		}
		return isAngleBased;
	}
	// 일치하는 대상 수에 따라 옵션 점수를 매깁니다.
	// 각 마크에 대한 공격 각도를 고려합니다.
	int GetAngleBasedScore (GeneralUnit caster)
	{
		int score = 0;
		for (int i = 0; i < marks.Count; ++i)
		{
			int value = marks[i].isMatch ? 1 : -1;
			int multiplier = MultiplierForAngle(caster, marks[i].tile);
			score += value * multiplier;
		}
		return score;
	}
	void FilterBestMoves (List<Tile> list)
	{
		if (!isCasterMatch)
			return;
		bool canTargetSelf = false;
		for (int i = 0; i < list.Count; ++i)
		{
			if (areaTargets.Contains(list[i]))
			{
				canTargetSelf = true;
				break;
			}
		}
		if (canTargetSelf)
		{
			for (int i = list.Count - 1; i >= 0; --i)
			{
				if (!areaTargets.Contains(list[i]))
					list.RemoveAt(i);
			}
		}
	}
	int MultiplierForAngle (GeneralUnit caster, Tile tile) // 여러명을 치게 될때의 공격 각도에 따른 점수를 반환합니다.
	{
		if (tile.content == null)
			return 0;
		GeneralUnit defender = tile.content.GetComponentInChildren<GeneralUnit>();
		if (defender == null)
			return 0;
		Facings facing = caster.GetFacing(defender);
		if (facing == Facings.Back)
			return 90;
		if (facing == Facings.Side)
			return 75;
		return 50;
	}
	#endregion
}