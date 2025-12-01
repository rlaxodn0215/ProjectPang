using System.Collections.Generic;
using System.Linq;
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
		/// 패턴이 적용되는지 확인
		/// </summary>
		/// <param name="boardSlot"></param>
		/// <param name="patternData"></param>
		/// <returns> 패턴이 적용되는 인덱스들 </returns>
		public static List<int> CheckPattern(List<SlotInfo> boardSlot, int rowCount, List<PatternCustomData> patternData,
			int selectIndex)
		{
			var patternSlots = new List<int>();
			var totalCount = boardSlot.Count;
			var colCount = totalCount / rowCount;

			foreach (var pattern in patternData)
			{
				// 기본 설정 패턴일 경우 BFS로 근처 같은 심볼 개수 카운트
				if (pattern.IsDefault)
				{
					var sameSymbolIndices = GetDefaultPatternIndices(boardSlot, rowCount, colCount, selectIndex);
					if (sameSymbolIndices.Count >= pattern.MinGetPointCount)
					{
						// 기본 패턴을 적용할 경우 patternData 밖에 없음으로 바로 반환
						return sameSymbolIndices;
					}
				}
				// pattern 적용해서 맞는지 확인
				else
				{
					var customPatternIndices = GetCustomPatternIndices(boardSlot, rowCount, pattern);

					//patternSlots.AddRange(customPatternIndices.Distinct()); // -> customPatternIndices 내부에서만 적용
					// 중복 인덱스 제거
					patternSlots.AddRange(customPatternIndices
							.Where(index => !patternSlots.Contains(index))
					);
				}
			}

			return patternSlots;
		}

		/// <summary>
		/// BFS로 기본 패턴 인덱스 탐색
		/// </summary>
		/// <param name="board"></param>
		/// <param name="rowCount"></param>
		/// <param name="colCount"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		private static List<int> GetDefaultPatternIndices(List<SlotInfo> board, int rowCount, int colCount, int start)
		{
			var result = new List<int>();
			var q = new Queue<int>();
			var visited = new HashSet<int>();

			var targetSymbol = board[start].Symbol;
			q.Enqueue(start);
			visited.Add(start);

			// 8 방향
			int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
			int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

			while (q.Count > 0)
			{
				var current = q.Dequeue();
				result.Add(current);

				var curX = current / colCount;
				var curY = current % colCount;

				for (var i = 0; i < 8; i++)
				{
					var nx = curX + dx[i];
					var ny = curY + dy[i];

					if (nx < 0 || nx >= rowCount || ny < 0 || ny >= colCount)
					{
						continue;
					}

					var nextIndex = nx * colCount + ny;

					if (visited.Contains(nextIndex))
					{
						continue;
					}

					if (board[nextIndex].Symbol != targetSymbol)
					{
						continue;
					}

					visited.Add(nextIndex);
					q.Enqueue(nextIndex);
				}
			}

			return result;
		}

		/// <summary>
		/// 커스텀 패턴 적용
		/// </summary>
		/// <param name="slotInfoList"></param>
		/// <param name="rowCount"></param>
		/// <param name="patternData"></param>
		/// <returns></returns>
		private static List<int> GetCustomPatternIndices(List<SlotInfo> slotInfoList, int rowCount,
			PatternCustomData patternData)
		{
			var result = new HashSet<int>();

			if (slotInfoList == null || slotInfoList.Count == 0)
			{
				return result.ToList();
			}

			var totalSlot = slotInfoList.Count;
			var colCount = totalSlot / rowCount;

			var pattern = ParsePattern(patternData.CustomPattern);
			if (pattern == null || pattern.Length != totalSlot)
			{
				return result.ToList();
			}

			// Pattern에서 1인 상대 좌표 얻기
			var patternPoints = new List<(int r, int c)>();
			for (var i = 0; i < totalSlot; i++)
			{
				if (pattern[i] == 1)
				{
					patternPoints.Add((i / colCount, i % colCount));
				}
			}

			if (patternPoints.Count == 0)
			{
				return result.ToList();
			}

			// 보드의 모든 좌표를 시작점으로 검사
			for (var r = 0; r < rowCount; r++)
			{
				for (var c = 0; c < colCount; c++)
				{
					var baseSymbol = slotInfoList[r * colCount + c].Symbol;

					var matched = new List<int>();
					var match = true;

					foreach (var (pr, pc) in patternPoints)
					{
						var tr = r + pr;
						var tc = c + pc;

						// 보드 밖이면 불가능
						if (tr < 0 || tr >= rowCount || tc < 0 || tc >= colCount)
						{
							match = false;
							break;
						}

						var index = tr * colCount + tc;

						// Symbol 다르면 fail
						if (slotInfoList[index].Symbol != baseSymbol)
						{
							match = false;
							break;
						}

						matched.Add(index);
					}

					if (!match)
					{
						continue;
					}

					// 패턴이 여러 곳에서 매칭되었을 때 
					foreach (var idx in matched)
					{
						result.Add(idx);
					}
				}
			}

			return result.ToList();

			// Pattern parser
			int[] ParsePattern(string str)
			{
				if (string.IsNullOrEmpty(str))
				{
					return null;
				}

				var token = str.Split(',');
				var arr = new int[token.Length];

				for (var i = 0; i < token.Length; i++)
				{
					arr[i] = int.TryParse(token[i], out var v) ? v : 0;
				}

				return arr;
			}
		}
	}
}
