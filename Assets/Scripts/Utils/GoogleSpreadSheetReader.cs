using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;
using UnityEditor;
using UnityEngine;

namespace ProjectPang
{
	[CreateAssetMenu(fileName = "GoogleSpreadSheetReader", menuName = "Scriptable Objects/GoogleSpreadSheetReader")]
	public class GoogleSpreadSheetReader : ScriptableObject
	{
		[HideInInspector] public string SheetName;
		[SerializeField] private List<string> newClasses = new List<string>();

		// Google Spread Sheet
		private const string DataInstanceName = "Name";
		private const string IgnorePrefix = "#";

		public void CreateDataScript(GstuSpreadSheet ss)
		{
			var className = SheetName + "Data";
			CreateScript(className, ss); // Script 생성

			if (!newClasses.Contains(className))
			{
				newClasses.Add(className);
			}

			Debug.Log("Create Data Script : " + className);
		}

		public void CreateDataSO(GstuSpreadSheet ss)
		{
			if (newClasses.Count == 0)
			{
				Debug.LogError("Create Data Script First!");
				return;
			}

			var className = SheetName + "Data";
			GenericHelper.InvokeGeneric<GoogleSpreadSheetReader>("CreateSO",
				Type.GetType("ProjectPang." + className), new object[] { ss, GlobalValue.SOpath, className });
		}

		/// <summary>
		/// Sheet에 맞게 Script 갱신
		/// </summary>
		/// <param name="className"></param>
		/// <param name="ss"></param>
		private void CreateScript(string className, GstuSpreadSheet ss)
		{
			var data = new List<string>();
			var ssRow = ss.rows.secondaryKeyLink;
			var ssCol = ss.columns.secondaryKeyLink;
			if (ssCol == null)
			{
				Debug.LogError("Cannot find ss.columns.secondaryKeyLink");
				return;
			}

			var firstRowKey = "Key";
			foreach (var row in ssRow)
			{
				if (row.Value == 1)
				{
					firstRowKey = row.Key;
					break;
				}
			}

			foreach (var col in ssCol)
			{
				data.Add(ss[firstRowKey, col.Key].value);
			}

			string scriptPath = null;
			scriptPath = GetOrCreateScriptPathByClassName();
			File.WriteAllTextAsync(scriptPath, SOBase.GetNewScript(className, data));
			AssetDatabase.Refresh();

			return;

			string GetOrCreateScriptPathByClassName()
			{
				var scripts = Resources.FindObjectsOfTypeAll<MonoScript>();
				if (scripts != null)
				{
					foreach (var script in scripts)
					{
						if (script && script.name == className)
						{
							return AssetDatabase.GetAssetPath(script);
						}
					}
				}

				// 폴더 없으면 생성
				if (!Directory.Exists(GlobalValue.ScriptPath))
				{
					Directory.CreateDirectory(GlobalValue.ScriptPath);
				}

				var newScriptPath = Path.Combine(GlobalValue.ScriptPath, className + ".cs");
				File.WriteAllText(newScriptPath, "Hello World!");

				// 4️⃣ Unity에 반영
				AssetDatabase.Refresh();

				return newScriptPath;
			}
		}

		/// <summary>
		/// SO Data 생성
		/// </summary>
		/// <param name="ss"></param>
		/// <param name="folderPath"></param>
		/// <typeparam name="T"></typeparam>
		private static void CreateSO<T>(GstuSpreadSheet ss, string soPath, string folderPath) where T : SOBase
		{
			// 1️⃣ 경로 처리
			if (!soPath.StartsWith("Assets"))
			{
				Debug.LogError("❌ assetPath는 반드시 'Assets'로 시작해야 합니다.");
				return;
			}

			var ssRow = ss.rows.secondaryKeyLink;
			var ssCol = ss.columns.secondaryKeyLink;
			if (ssRow == null || ssCol == null)
			{
				Debug.LogError("Cannot find GstuSpreadSheet data");
				return;
			}

			foreach (var row in ssRow)
			{
				// 데이터들의 이름이 들어있는 첫번째 row 무시
				if (row.Value == 1) continue;

				var data = new List<string>();
				var fileName = "NoName";
				foreach (var col in ssCol)
				{
					// 변수 명이 #으로 시작하면 무시
					if (col.Key.StartsWith(IgnorePrefix)) continue;
					// 셀 데이터가 #으로 시작하면 무시
					if (ss[row.Key, col.Key].value.StartsWith(IgnorePrefix)) continue;

					// SO 이름 추출
					if (col.Key.Equals(DataInstanceName, StringComparison.OrdinalIgnoreCase))
					{
						fileName = ss[row.Key, col.Key].value;

						// 데이터 SO 이름은 무시
						continue;
					}

					data.Add(ss[row.Key, col.Key].value);
				}

				var assetPath = Path.Combine(soPath, folderPath);

				// 폴더 먼저 생성
				if (!Directory.Exists(assetPath))
				{
					Directory.CreateDirectory(assetPath);
				}

				assetPath = Path.Combine(assetPath, fileName + ".asset");
				// Debug.LogWarning("PATH : " + assetPath);

				// 2️⃣ 기존 에셋 확인
				var soAsset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
				if (soAsset == null)
				{
					// 3️⃣ 없으면 새 ScriptableObject 생성
					soAsset = CreateInstance<T>();

					// 4️⃣ 에셋 생성
					AssetDatabase.CreateAsset(soAsset, assetPath);
					Debug.Log($"Create new Data SO : {fileName}");
				}

				// 데이터 갱신
				soAsset.UpdateData(data);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"Update Data SO : {fileName}");
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(GoogleSpreadSheetReader))]
	public class GoogleSpreadSheetEditor : Editor
	{
		private static readonly string SheetID = "1XeQ1jyI0uPx0TNbsHG4OF6tW23yD5hsJ9V-yxwYsyck";

		private static readonly string[] WorkSheetList =
		{
			"Symbol",
			"UI",
			"Sound"
		};

		private GoogleSpreadSheetReader _reader;
		// private string _currentWorkSheet = "";
		// private int _selectedTypeIndex = 0;
		// private int _selectedWorkSheetIndex = 0;

		private void OnEnable()
		{
			_reader = (GoogleSpreadSheetReader)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			// GUILayout.Label("");
			// _selectedTypeIndex = EditorGUILayout.Popup("SO Type", _selectedTypeIndex, _types);
			//
			// if (GUILayout.Button("Get SO Assets"))
			// {
			// 	_reader.RefreshSOAssets(_types[_selectedTypeIndex]);
			// }
			//
			//
			// GUILayout.Label("");
			// _selectedWorkSheetIndex =
			// 	EditorGUILayout.Popup("Data Type", _selectedWorkSheetIndex, WorkSheetList);
			// _currentWorkSheet = WorkSheetList[_selectedWorkSheetIndex];
			//
			// if (GUILayout.Button("Refresh Data"))
			// {
			// 	RefreshData(_currentWorkSheet);
			// }

			// if (GUILayout.Button("Get All SO Assets"))
			// {
			// 	foreach (var type in _types)
			// 	{
			// 		_reader.GetSOAssets(type);
			// 	}
			//
			// 	Debug.Log("[GoogleSpreadSheetManager] : Get All SO Assets Finished");
			// }
			//
			// if (GUILayout.Button("Pull All Data"))
			// {
			// 	foreach (var sheet in WorkSheetList)
			// 	{
			// 		PullData(sheet);
			// 	}
			//
			// 	Debug.Log("[GoogleSpreadSheetManager] : Pull All Data Finished");
			// }


			if (GUILayout.Button("Create All Data Scripts"))
			{
				CreatAllDataScript().Forget();
			}

			if (GUILayout.Button("Create All Data SO"))
			{
				CreateAllDataSO().Forget();
			}
		}

		private async UniTaskVoid CreatAllDataScript()
		{
			foreach (var sheet in WorkSheetList)
			{
				await CreateDataScript(sheet);
			}
		}

		private async UniTaskVoid CreateAllDataSO()
		{
			foreach (var sheet in WorkSheetList)
			{
				await CreateDataSO(sheet);
			}
		}

		private async UniTask CreateDataScript(string currentWorkSheet)
		{
			var tcs = new UniTaskCompletionSource();
			_reader.SheetName = currentWorkSheet;
			SpreadsheetManager.Read(new GSTU_Search(SheetID, currentWorkSheet), (ss) =>
			{
				_reader.CreateDataScript(ss);
				tcs.TrySetResult();
			});

			await tcs.Task;
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private async UniTask CreateDataSO(string currentWorkSheet)
		{
			var tcs = new UniTaskCompletionSource();
			_reader.SheetName = currentWorkSheet;
			SpreadsheetManager.Read(new GSTU_Search(SheetID, currentWorkSheet), (ss) =>
			{
				_reader.CreateDataSO(ss);
				tcs.TrySetResult();
			});

			await tcs.Task;
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	public static class GenericHelper
	{
		/// <summary>
		/// 제너릭 메서드를 Type으로 실행합니다.
		/// </summary>
		public static object InvokeGeneric<TTarget>(string methodName, Type genericType, params object[] parameters)
		{
			// 예: typeof(TTarget) 안의 methodName<T> 를 genericType으로 실행
			var method = typeof(TTarget).GetMethod(methodName,
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
			if (method == null)
			{
				Debug.LogError($"❌ 메서드 '{methodName}'를 {typeof(TTarget).Name}에서 찾을 수 없습니다.");
				return null;
			}

			var genericMethod = method.MakeGenericMethod(genericType);
			return genericMethod.Invoke(null, parameters); // static 메서드라면 null
		}
	}
#endif
}
