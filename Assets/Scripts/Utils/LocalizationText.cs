using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectPang
{
	public class LocalizationText : MonoBehaviour
	{
		[SerializeField] private ELocalizeKey _key;
		private TMP_Text _text;
		private string _language;

		private void Start()
		{
			_text = GetComponent<TMP_Text>();
			Managers.Localization.OnLanguageChanged += ChangeText;

			if (_language == null)
			{
				ChangeText("English"); // 기본은 영어로 설정 (초기화 안되는 것 방지)
			}
		}

		private void ChangeText(string language)
		{
			if (_text == null)
			{
				return;
			}

			_language = language;
			_text.text = Managers.Localization.GetText(language, _key);
		}
	}
}
