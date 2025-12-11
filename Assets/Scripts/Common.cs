using System;
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
		Max
	}

	public enum EUI
	{
		UITitleWindow,
		UIGameWindow,
		UIMapSelectPopup,
		UICustomPopup,
		UISettingsPopup,
	}

	public enum ESound
	{
		ButtonClick,
		GameOver,
		Max
	}

	public enum EAudioMixerType
	{
		Master,
		BGM,
		SFX
	}

	public enum ELocalizeKey // List의 인덱스와 동일
	{
		KawaiiPang,
		Start,
		Custom,
		Settings,
		Exit,
	}

	public struct SymbolCustomData
	{
		public ESymbol Symbol;
		public int ChanceWeight;
		public int Point;
	}

	public struct PatternCustomData
	{
		public bool IsDefault;
		public int MinGetPointCount;
		public string CustomPattern;
	}

	[Serializable]
	public class GameData
	{
		public string DataName;
		public string DataPath;
		public int RowCount;
		public int ColCount;
		public List<SymbolCustomData> ListSymbolCustomData;
		public List<PatternCustomData> ListPatternCustomData;
		public bool IsTimeAttack;
		public int TimeLimit;
		public bool IsTargetPoint;
		public int TargetPoint;
		public bool IsRandomBlock;
		public int BlockCount;
		public List<int> ListBlockIndex;
	}

	[Serializable]
	public class LocalizationData
	{
		public string Language;
		public List<string> ListText;
	}
}
