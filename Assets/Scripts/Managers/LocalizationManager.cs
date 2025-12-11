using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectPang
{
	public class LocalizationManager
	{
		public UnityAction<string> OnLanguageChanged;
		private Dictionary<string, LocalizationData> _localizationData = new Dictionary<string, LocalizationData>();

		public void Initailize()
		{
			_localizationData.Clear();

			var folderPath = Path.Combine(Application.streamingAssetsPath, "Language");

			var jsonFiles = new List<string>();

#if UNITY_ANDROID
    // Android에서는 StreamingAssets을 직접 Directory.GetFiles 할 수 없음
    string fileListPath = Path.Combine(folderPath, "filelist.txt");
    string fileListContent = string.Empty;

    // 파일 목록 제공 필요 (빌드시 포함시키거나 별도 파일 유지)
    fileListContent = UnityEngine.Networking.UnityWebRequest.Get(fileListPath).downloadHandler.text;

    jsonFiles.AddRange(fileListContent.Split('\n'));
#else
			// Windows / Mac / Linux / iOS
			if (Directory.Exists(folderPath))
			{
				jsonFiles.AddRange(Directory.GetFiles(folderPath, "*.json"));
			}
#endif

			foreach (var filePath in jsonFiles)
			{
				try
				{
					var json = File.ReadAllText(filePath);
					var data = JsonUtility.FromJson<LocalizationData>(json);

					if (!string.IsNullOrEmpty(data.Language))
					{
						_localizationData[data.Language] = data;
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError($"Localization JSON load failed: {filePath}\n{e}");
				}
			}

			Debug.Log($"Loaded Languages: {string.Join(", ", _localizationData.Keys)}");
		}

		public List<string> GetLanguageList()
		{
			return new List<string>(_localizationData.Keys);
		}

		public string GetText(string language, ELocalizeKey key)
		{
			if (_localizationData.ContainsKey(language) && _localizationData[language].ListText.Count > (int)key)
			{
				return _localizationData[language].ListText[(int)key];
			}

			return null;
		}

		public void ChangeLanguage(string language)
		{
			OnLanguageChanged?.Invoke(language);
		}
	}
}
