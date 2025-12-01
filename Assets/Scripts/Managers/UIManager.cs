using System.Collections.Generic;
using UnityEngine;

namespace ProjectPang
{
	// Title, Game -> Scene에 이미 소환
	// Custom만 나오면 된다. (소환시 Json 파일 캐싱) 
	// UI를 관리하는 매니저
	public class UIManager
	{
		private Transform _uiParent;
		private Dictionary<EUI, UIData> _uiData = new Dictionary<EUI, UIData>();
		private Dictionary<EUI, GameObject> _uiObject = new Dictionary<EUI, GameObject>();

		public void Initialize(Transform parent)
		{
			_uiParent = parent;
			_uiData = DataManager.LoadData<EUI, UIData>(GlobalValue.SOpath);
			foreach (var data in _uiData)
			{
				// 메모리에만 넣는다 (소환X)
				_uiObject[data.Key] = Resources.Load<GameObject>(data.Value.PrefabPath);
			}
		}

		public void OpenUI(EUI ui)
		{
			 //var obj = Object.Instantiate(_uiObject[ui], _uiParent);
			 //_uiObject[ui] = obj;
		}

		public void CloseUI(EUI ui)
		{
			//Object.Destroy(_uiObject[ui]);
		}
	}
}
