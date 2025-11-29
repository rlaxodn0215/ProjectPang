using System.Collections.Generic;
using UnityEngine;

namespace ProjectPang
{
	public enum ESymbol
	{
		None = -2,
		Block,
		A,
		B,
		C,
		Max,
	}

	public struct SymbolData
	{
		public ESymbol Symbol;
		public int ChanceWeight;
		public int Point;
	}

	public struct PatternData
	{
		public bool IsDefault;
		public int MinGetPointCount;
		public List<string> ListPattern;
	}

	public struct GameData
	{
		public string DataName;
		public string DataPath;
		public int RowCount;
		public int ColCount;
		public List<SymbolData> ListSymbolData;
		public List<PatternData> ListPatternData;
		public bool IsTimeAttack;
		public int TimeLimit;
		public bool IsTargetPoint;
		public int TargetPoint;
		public bool IsRandomBlock;
		public int BlockCount;
		public List<int> ListBlockIndex;
	}
}
