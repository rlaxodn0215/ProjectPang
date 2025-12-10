using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ProjectPang
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Audio;

	public class SoundManager
	{
		private Dictionary<ESound, SoundData> _soundData = new Dictionary<ESound, SoundData>();

		private AudioMixer _mixer;
		private AudioSource _bgmPlayer;
		private AudioSource _sfxPlayer;

		public void Initialize(AudioMixer mixer, AudioSource bgmSource, AudioSource sfxSource)
		{
			_mixer = mixer;
			_bgmPlayer = bgmSource;
			_sfxPlayer = sfxSource;

			_soundData = DataManager.LoadData<ESound, SoundData>(GlobalValue.SOpath);
		}

		public void Play(ESound key)
		{
			if (!_soundData.TryGetValue(key, out var data))
			{
				Debug.LogError($"Sound Not Found: {key}");
				return;
			}

			var clip = Resources.Load<AudioClip>(data.AudioFilePath);
			_sfxPlayer.pitch = data.Pitch;
			_sfxPlayer.PlayOneShot(clip, data.Volume);
		}

		public void PlayBGM(ESound key)
		{
			if (!_soundData.TryGetValue(key, out var data))
			{
				Debug.LogError($"Sound Not Found: {key}");
				return;
			}

			var clip = Resources.Load<AudioClip>(data.AudioFilePath);

			_bgmPlayer.clip = clip;
			_bgmPlayer.loop = data.Loop;
			_bgmPlayer.pitch = data.Pitch;
			_bgmPlayer.volume = data.Volume;
			_bgmPlayer.Play();
		}

		// Volume Controls (Mixer)
		public void SetVolume(EAudioMixerType type, float volume)
		{
			var db = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
			_mixer.SetFloat(type.ToString(), db);
		}
	}
}
