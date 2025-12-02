using UnityEngine;

namespace ProjectPang
{
	public class Managers : MonoBehaviour
	{
		private static Managers _instance;

		private DataManager _data = new DataManager();
		private SoundManager _sound = new SoundManager();
		private CustomManager _custom = new CustomManager();
		private UIManager _ui = new UIManager();
		private GameManager _game = new GameManager();

		public static DataManager Data => _instance._data;
		public static SoundManager Sound => _instance._sound;
		public static CustomManager Custom => _instance._custom;
		public static UIManager UI => _instance._ui;
		public static GameManager Game => _instance._game;

		private void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(gameObject);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(gameObject);
		}

		// Init managers
		private void Start()
		{
			Data.Initailize();
			//Sound.Initialize();
			Custom.Initialize();
			UI.Initialize(transform);
			Game.Initailize();
		}
	}
}
