using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectPang
{
	public class UIMapSelectPopup : MonoBehaviour
	{
		[SerializeField] private Button _playGameButton;

		private void Start()
		{
			_playGameButton.onClick.AddListener(OnPlayGameButton);
		}
        
		private void OnPlayGameButton()
		{
		}
	}
}
