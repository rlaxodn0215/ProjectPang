using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace ProjectPang
{
	[CreateAssetMenu(fileName = "SymbolData", menuName = "Scriptable Objects/SymbolData")]
	public class SymbolData : SOBase
	{
		public int Key;
		public string PrefabPath;

	#if UNITY_EDITOR
		public override void UpdateData(List<string> dataList)
		{
			base.UpdateData(dataList);

			if (dataList.Count > 0 && !string.IsNullOrEmpty(dataList[0]))
			{
				try
				{
					var targetType = typeof(int);
					if (targetType.IsEnum)
						Key = (int)Enum.Parse(targetType, dataList[0]);
					else
						Key = (int)Convert.ChangeType(dataList[0], targetType);
				}
				catch { }
			}
			if (dataList.Count > 1 && !string.IsNullOrEmpty(dataList[1]))
			{
				try
				{
					var targetType = typeof(string);
					if (targetType.IsEnum)
						PrefabPath = (string)Enum.Parse(targetType, dataList[1]);
					else
						PrefabPath = (string)Convert.ChangeType(dataList[1], targetType);
				}
				catch { }
			}

			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	#endif
	}
}
