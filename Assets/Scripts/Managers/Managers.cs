using UnityEngine;

namespace ProjectPang
{
	public class Managers : MonoBehaviour
	{
		private static Managers _instance;

		private DataManager _data = new DataManager();
		private SoundManager _sound = new SoundManager();
		private CustomManager _custom = new CustomManager();
		private SceneManager _scene = new SceneManager();
		private UIManager _ui = new UIManager();
		private GameManager _game = new GameManager();

		public static DataManager Data => _instance._data;
		public static SoundManager Sound => _instance._sound;
		public static CustomManager Custom => _instance._custom;
		public static SceneManager Scene => _instance._scene;
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

			// awake specified managers
		}

		private void Start()
		{
			// init managers
		}
	}
}
