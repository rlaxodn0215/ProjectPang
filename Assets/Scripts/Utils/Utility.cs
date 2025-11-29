using System.Collections.Generic;
using UnityEngine;

namespace ProjectPang
{
	public static class Utility
	{
		private static readonly int ProbabilityMaxCount = GlobalValue.ProbabilityMaxCount;

		/// <summary>
		/// 연관된 확률들 중 특정 확률이 일어날 확률
		/// 리스트의 모든 확률의 값은 ProbabilityMaxCount와 같아야 한다.
		/// </summary>
		/// <param name="probabilityList"> 확률들 리스트 </param>
		/// <returns></returns>
		public static int GetRandomProbability(List<int> probabilityList)
		{
			var sum = 0;
			foreach (var item in probabilityList)
			{
				sum += item;
			}

			if (sum != ProbabilityMaxCount)
			{
				Debug.LogError("Sum of probability is not " + ProbabilityMaxCount);
				return -1;
			}

			sum = 0;
			var randomNum = Random.Range(1, ProbabilityMaxCount + 1);
			for (var i = 0; i < probabilityList.Count; i++)
			{
				sum += probabilityList[i];
				if (randomNum <= sum)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		///  특정 확율로 발생했는지 확인
		/// </summary>
		/// <param name="probability"> 발생할 확률 </param>
		/// <returns> 확률 발생 유무 </returns>
		public static bool IsChanceSuccess(int probability)
		{
			return Random.Range(0, ProbabilityMaxCount) < probability;
		}

		/// <summary>
		/// 패턴이 적용되는지 확인
		/// </summary>
		/// <param name="boardSlot"></param>
		/// <param name="patternData"></param>
		/// <returns> 패턴이 적용되는 인덱스들 </returns>
		public static List<int> CheckPattern(List<SlotInfo> boardSlot, List<PatternData> patternData, int selectIndex)
		{
			var patternSlots = new List<int>();
			foreach (var pattern in patternData)
			{
				// 기본 설정 패턴일 경우 BFS로 근처 같은 심볼 개수 카운트
				if (pattern.IsDefault)
				{
				}
				// pattern 적용해서 맞는 지 확인
				else
				{
				}
			}

			return patternSlots;
		}
	}
}
