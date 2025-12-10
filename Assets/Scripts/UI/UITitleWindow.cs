using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectPang
{
	public class UITitleWindow : UIBase
	{
		[Header("Buttons")] 
		[SerializeField] private Button _startButton;
		[SerializeField] private Button _customButton;
		[SerializeField] private Button _settingButton;
		[SerializeField] private Button _exitButton;

		[Header("Popups")] 
		[SerializeField] private UIMapSelectPopup _mapSelectPopup;
		[SerializeField] private UICustomPopup _customPopup;
		[SerializeField] private UISettingsPopup _settingsPopup;

		private void Start()
		{
			_startButton.onClick.AddListener(OnStartButton);
			_customButton.onClick.AddListener(OnCustomButton);
			_settingButton.onClick.AddListener(OnSettingButton);
			_exitButton.onClick.AddListener(OnExitButton);
		}

		private void OnStartButton()
		{
		}

		private void OnCustomButton()
		{
		}

		private void OnSettingButton()
		{
		}

		private void OnExitButton()
		{
		}
	}
}
