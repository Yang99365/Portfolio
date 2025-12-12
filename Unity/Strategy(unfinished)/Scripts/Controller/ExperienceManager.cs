using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Party = System.Collections.Generic.List<UnityEngine.GameObject>;
public static class ExperienceManager
{
	const float minLevelBonus = 1.5f;
	const float maxLevelBonus = 0.5f;
	public static void AwardExperience (int amount, Party party)
	{
		// 영웅 파티의 모든 순위 구성 요소 목록을 가져옵니다.
		List<Rank> ranks = new List<Rank>(party.Count);
		for (int i = 0; i < party.Count; ++i)
		{
			Rank r = party[i].GetComponent<Rank>();
			if (r != null)
				ranks.Add(r);
		}
		// 1단계: 액터 레벨 통계의 범위 결정
		int min = int.MaxValue;
		int max = int.MinValue;
		for (int i = ranks.Count - 1; i >= 0; --i)
		{
			min = Mathf.Min(ranks[i].LVL, min);
			max = Mathf.Max(ranks[i].LVL, max);
		}
		// 2단계: 배우의 레벨에 따라 보상 금액에 가중치를 부여합니다.
		float[] weights = new float[ranks.Count];
		float summedWeights = 0;
		for (int i = ranks.Count - 1; i >= 0; --i)
		{
			float percent = (float)(ranks[i].LVL - min) / (float)(max - min);
			weights[i] = Mathf.Lerp(minLevelBonus, maxLevelBonus, percent);
			summedWeights += weights[i];
		}
		// 3단계: 가중치를 적용한 보상 지급
		for (int i = ranks.Count - 1; i >= 0; --i)
		{
			int subAmount = Mathf.FloorToInt((weights[i] / summedWeights) * amount);
			ranks[i].EXP += subAmount;
		}
	}
}