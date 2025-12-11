using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectPang
{
	public class UISettingsPopup : MonoBehaviour
	{
		[Header("Language Settings")] [SerializeField]
		private TMP_Dropdown _languageDropdown;

		[Header("Display Settings")] [SerializeField]
		private TMP_Dropdown _displayResolutionDropdown;

		[SerializeField] private TMP_Dropdown _windowTypeDropdown;

		[Header("Audio Settings")]
		[SerializeField] private Slider _masterVolumeSlider;
		[SerializeField] private TMP_Text _maseterVolumeText;
		[SerializeField] private Slider _bgmVolumeSlider;
		[SerializeField] private TMP_Text _bgmVolumeText;
		[SerializeField] private Slider _sfxVolumeSlider;
		[SerializeField] private TMP_Text _sfxVolumeText;

		private Resolution[] _displayResolutions;

		private void Start()
		{
			InitLanguage();
			InitWindowType();
			InitResolutions();
			InitVolumeSliders();
		}

		private void InitLanguage()
		{
			_languageDropdown.ClearOptions();

			var languages = Managers.Localization.GetLanguageList();
			_languageDropdown.AddOptions(languages);

			// 중복 방지
			_languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);

			// 기본 언어 설정
			_languageDropdown.value = 0;
			_languageDropdown.RefreshShownValue();

			OnLanguageChanged(0); // 기본 언어 적용

			_languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
		}

		private void OnLanguageChanged(int index)
		{
			var selectedLang = _languageDropdown.options[index].text;
			Managers.Localization.ChangeLanguage(selectedLang);
		}

		private void InitResolutions()
		{
			_displayResolutions = Screen.resolutions;

			_displayResolutionDropdown.ClearOptions();

			var options = new List<string>();
			var currentIndex = 0;

			foreach (var t in _displayResolutions)
			{
				var option = $"{t.width} x {t.height}";

				// 같은 해상도 중복 제거
				if (!options.Contains(option))
					options.Add(option);

				// 현재 해상도 체크
				if (t.width == Screen.currentResolution.width &&
				    t.height == Screen.currentResolution.height)
				{
					currentIndex = options.Count - 1;
				}
			}

			_displayResolutionDropdown.AddOptions(options);
			_displayResolutionDropdown.value = currentIndex;
			_displayResolutionDropdown.RefreshShownValue();

			// 드롭다운 변경 이벤트 연결
			_displayResolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
		}

		private void InitWindowType()
		{
			_windowTypeDropdown.ClearOptions();

			var modes = new List<string>()
			{
				"Fullscreen",
				"Fullscreen Window",
				"Windowed"
			};

			_windowTypeDropdown.AddOptions(modes);

			// 현재 모드에 맞게 기본값 설정
			var index = Screen.fullScreenMode switch
			{
				FullScreenMode.ExclusiveFullScreen => 0,
				FullScreenMode.FullScreenWindow => 1,
				FullScreenMode.Windowed => 2,
				_ => 0
			};

			_windowTypeDropdown.value = index;
			_windowTypeDropdown.RefreshShownValue();

			// 드롭다운 변경 이벤트 등록
			_windowTypeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
		}

		private void OnResolutionChanged(int index)
		{
			var res = _displayResolutionDropdown.options[index].text.Split('x');

			var width = int.Parse(res[0].Trim());
			var height = int.Parse(res[1].Trim());

			// 최신 방식: refreshRateRatio 사용
			var refreshRateRatio = Screen.currentResolution.refreshRateRatio;

			Screen.SetResolution(
				width,
				height,
				Screen.fullScreenMode,
				refreshRateRatio
			);
		}

		private void OnWindowModeChanged(int index)
		{
			Screen.fullScreenMode = index switch
			{
				0 => // Fullscreen
					FullScreenMode.ExclusiveFullScreen,
				1 => // Fullscreen Window
					FullScreenMode.FullScreenWindow,
				2 => // Windowed
					FullScreenMode.Windowed,
				_ => Screen.fullScreenMode
			};
		}

		private void InitVolumeSliders()
		{
			_masterVolumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
			_bgmVolumeSlider.onValueChanged.AddListener(ChangeBGMVolume);
			_sfxVolumeSlider.onValueChanged.AddListener(ChangeSFXVolume);
		}

		private void ChangeMasterVolume(float volume)
		{
			Managers.Sound.SetVolume(EAudioMixerType.Master, volume);
			_maseterVolumeText.text = $"{(int)(volume * 100f)} / 100";
		}

		private void ChangeBGMVolume(float volume)
		{
			Managers.Sound.SetVolume(EAudioMixerType.BGM, volume);
			_bgmVolumeText.text = $"{(int)(volume * 100f)} / 100";
		}

		private void ChangeSFXVolume(float volume)
		{
			Managers.Sound.SetVolume(EAudioMixerType.SFX, volume);
			_sfxVolumeText.text = $"{(int)(volume * 100f)} / 100";
		}
	}
}
