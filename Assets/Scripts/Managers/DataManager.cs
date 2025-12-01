using System.Collections.Generic;
using UnityEngine;

namespace ProjectPang
{
	// 데이터를 관리하는 매니저
	public class DataManager
	{
		public void Initailize()
		{
		}

		public static Dictionary<TEnum, TData> LoadData<TEnum, TData>(string rootPath)
			where TEnum : System.Enum where TData : ScriptableObject
		{
			// 모든 ScriptableObject를 rootPath 이하에서 로드
			var dictionary = new Dictionary<TEnum, TData>();
			var allData = Resources.LoadAll<TData>(rootPath);

			foreach (var data in allData)
			{
				// 파일 이름 = enum 이름과 동일하다고 가정
				var name = data.name;

				foreach (TEnum value in System.Enum.GetValues(typeof(TEnum)))
				{
					if (value.ToString() != name)
					{
						continue;
					}

					dictionary[value] = data;
					break;
				}
			}

			Debug.Log($"[DataManager] Loaded {typeof(TData).Name}: {dictionary.Count}개");
			return dictionary;
		}
	}
}
