using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ProjectPang
{
	public class SlotInfo
	{
		public ESymbol Symbol;
	}

	public class GameManager
	{
		public List<SlotInfo> ListBoardSlotInfo { get; private set; } = new List<SlotInfo>();

		public ESymbol CurrentSymbol { get; private set; }
		public ESymbol NextSymbol { get; private set; }

		// Events
		public UnityEvent<int, ESymbol>
			OnSlotClicked = new UnityEvent<int, ESymbol>(); // int - index, ESymbol - current symbol type

		public UnityEvent<ESymbol> OnUpdateSymbol = new UnityEvent<ESymbol>(); // ESymbol - new symbol type

		public UnityEvent<string, long>
			OnGetPoint = new UnityEvent<string, long>(); // string - to remove indices, long - point

		public UnityEvent OnReset = new UnityEvent();
		public UnityEvent OnUseTimeAttack = new UnityEvent();
		public UnityEvent OnUseTargetPoint = new UnityEvent();
		public UnityEvent OnGameOver = new UnityEvent();

		// 현재 게임에 적용되는 데이터
		public List<SymbolCustomData> ListUseSymbol = new List<SymbolCustomData>();
		public List<PatternCustomData> ListUsePattern = new List<PatternCustomData>();
		public int TimeLimit { get; private set; }
		public int TargetPoint { get; private set; }
		public long Point { get; private set; }

		private GameData _gameDataCache;
		private AsyncOperation _loadOperation;

		public void Initailize()
		{
			
		}

		public void SetupGameData(string jsonPath)
		{
			// json파일을 로드해서 초기화 진행
			
			// _gameDataCache 초기화
			Reset();
			
			// 씬 미리 로드
			_loadOperation = SceneManager.LoadSceneAsync("GameScene");
			if (_loadOperation != null)
			{
				_loadOperation.allowSceneActivation = false;
			}
		}

		public void StartGame()
		{
			if (_loadOperation != null)
			{
				_loadOperation.allowSceneActivation = true;
			}
		}

		/// <summary>
		/// 초기화된 데이터(_gameDataCache)로 게임 리셋
		/// </summary>
		public void Reset()
		{
			Debug.Log("Reset Game");

			// 데이터 리셋
			ListUseSymbol = _gameDataCache.ListSymbolCustomData;
			ListUsePattern = _gameDataCache.ListPatternCustomData;
			TimeLimit = _gameDataCache.TimeLimit;
			OnUseTimeAttack?.Invoke();
			TargetPoint = _gameDataCache.TargetPoint;
			OnUseTargetPoint?.Invoke();

			// 게임 플레이 리셋
			ListBoardSlotInfo.Clear();
			ListBoardSlotInfo = new List<SlotInfo>(_gameDataCache.RowCount * _gameDataCache.ColCount);
			foreach (var slotInfo in ListBoardSlotInfo)
			{
				slotInfo.Symbol = ESymbol.None;
			}

			if (_gameDataCache.IsRandomBlock)
			{
				for (var i = 0; i < _gameDataCache.BlockCount; i++)
				{
					var randomBlockIndex = Random.Range(0, _gameDataCache.RowCount * _gameDataCache.ColCount);
					ListBoardSlotInfo[randomBlockIndex].Symbol = ESymbol.Block;
				}
			}
			else
			{
				// 리스트에 선택된 블럭 설정
				foreach (var blockIndex in _gameDataCache.ListBlockIndex)
				{
					ListBoardSlotInfo[blockIndex].Symbol = ESymbol.Block;
				}
			}

			CurrentSymbol = GetNextSymbol();
			NextSymbol = GetNextSymbol();
			Point = 0;

			OnReset?.Invoke();
		}

		public void SelectSlot(int index)
		{
			Debug.Log("Board Slot Selected : " + index);

			if (index < 0 || index >= ListBoardSlotInfo.Count)
			{
				Debug.LogError("Index is out of range");
				return;
			}

			if (ListBoardSlotInfo[index].Symbol != ESymbol.None)
			{
				Debug.Log(
					$"Cannot Click Slot!: Index {index} is already filled with {ListBoardSlotInfo[index].Symbol}");
				return;
			}

			ListBoardSlotInfo[index].Symbol = CurrentSymbol;
			Point += CalculatePoint(index);
			UpdateSymbol();

			// 이벤트 호출
			OnSlotClicked?.Invoke(index, CurrentSymbol);
		}

		private long CalculatePoint(int selectedIndex)
		{
			var patternSlots =
				Utility.CheckPattern(ListBoardSlotInfo, _gameDataCache.RowCount, ListUsePattern, selectedIndex);
			if (patternSlots.Count == 0)
			{
				return 0;
			}

			var symbolPoint = ListUseSymbol
				.Find(symbolData => symbolData.Symbol == ListBoardSlotInfo[patternSlots[0]].Symbol).Point;

			long point = symbolPoint * patternSlots.Count;
			OnGetPoint?.Invoke(string.Join(",", patternSlots), point);

			return point;
		}

		private void UpdateSymbol()
		{
			CurrentSymbol = NextSymbol;
			NextSymbol = GetNextSymbol();
			OnUpdateSymbol?.Invoke(NextSymbol);
		}

		private ESymbol GetNextSymbol()
		{
			var list = new List<int>();

			foreach (var symbolData in ListUseSymbol)
			{
				list.Add(symbolData.ChanceWeight);
			}

			return (ESymbol)(Utility.GetRandomProbability(list) + 1);
		}
	}
}
