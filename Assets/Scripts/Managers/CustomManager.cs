using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ProjectPang
{
	public class CustomManager
	{
		public Dictionary<string, GameData> CustomData = new Dictionary<string, GameData>();

		private string _persistentFolderPath;
		private string _streamingFolderPath;

		public void Initialize()
		{
			_persistentFolderPath = Path.Combine(Application.persistentDataPath, "GameData");
			_streamingFolderPath = Path.Combine(Application.streamingAssetsPath, "GameData");

			// persistent 폴더 없으면 생성
			if (!Directory.Exists(_persistentFolderPath))
			{
				Directory.CreateDirectory(_persistentFolderPath);
			}

			// ---------- (1) StreamingAssets → persistentDataPath 복사 ----------
			CopyFromStreamingAssets();

			// ---------- (2) persistent 의 JSON 모두 로드 ----------
			LoadAllCustomData();
		}

		public void CreateCustomData(string fileName, GameData data)
		{
			if (data == null)
			{
				Debug.LogError("[CustomManager] GameData is null!");
				return;
			}

			// 폴더 생성 (없으면 만들어짐)
			Directory.CreateDirectory(_persistentFolderPath);

			var path = Path.Combine(_persistentFolderPath, fileName + ".json");

			// JSON 직렬화
			var json = JsonUtility.ToJson(data, true);

			// 파일 쓰기
			File.WriteAllText(path, json);

			Debug.Log($"[CustomManager] Saved new JSON: {path}");

			// 메모리 캐싱도 자동 반영
			CustomData[fileName] = data;
		}

		public void ModifyCustomData(string fileName, GameData data)
		{
			var path = Path.Combine(_persistentFolderPath, fileName + ".json");
			if (File.Exists(path))
			{
				File.WriteAllText(path, JsonUtility.ToJson(data));
			}
		}

		public void DeleteCustomData(string fileName)
		{
			var path = Path.Combine(_persistentFolderPath, fileName + ".json");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		// (1) StreamingAssets → persistentDataPath로 복사
		private void CopyFromStreamingAssets()
		{
			if (!Directory.Exists(_streamingFolderPath))
			{
				Debug.LogWarning("[CustomManager] StreamingAssets/GameData 폴더 없음");
				return;
			}

			var files = Directory.GetFiles(_streamingFolderPath, "*.json");

			foreach (var file in files)
			{
				var fileName = Path.GetFileName(file);
				var dest = Path.Combine(_persistentFolderPath, fileName);

				// 이미 있다면 덮어쓰지 않음 (원하면 덮어쓰기하도록 변경 가능)
				if (File.Exists(dest))
				{
					continue;
				}

				File.Copy(file, dest);
				Debug.Log($"[CustomManager] Copied : {file} -> {dest}");
			}
		}

		private void LoadAllCustomData()
		{
			// 폴더 안 JSON 파일들을 모두 가져오기
			var files = Directory.GetFiles(_persistentFolderPath, "*.json");

			foreach (var file in files)
			{
				try
				{
					var json = File.ReadAllText(file);
					var data = JsonUtility.FromJson<GameData>(json);

					if (data == null)
					{
						Debug.LogWarning($"{file} JSON 파싱 실패");
						continue;
					}

					// 키는 파일 이름에서 .json 제거
					var key = Path.GetFileNameWithoutExtension(file);

					if (!CustomData.ContainsKey(key))
					{
						CustomData.Add(key, data);
					}
					else
					{
						CustomData[key] = data;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"파일 {file} 읽기 오류: {ex}");
				}
			}

			Debug.Log($"[CustomManager] Loaded {CustomData.Count} GameData files.");
		}
	}
}
