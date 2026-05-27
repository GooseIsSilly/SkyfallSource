using TPSBR.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace TPSBR
{
	public class LoadingScene : MonoBehaviour
	{
		// PUBLIC MEMBERS

		public bool IsFading => _activeFader != null && _activeFader.IsFinished == false;

		// PRIVATE MEMBERS

		[SerializeField]
		private UIFader _fadeInObject;
		[SerializeField]
		private UIFader _fadeOutObject;
		[SerializeField]
		private TextMeshProUGUI _status;
		[SerializeField]
		private TextMeshProUGUI _statusDescription;
		[SerializeField]
		private TextMeshProUGUI _tipText;
		[SerializeField]
		private UIYesNoDialogView _dialog;

		private UIFader _activeFader;

		private string[] _tips = new string[]
		{
            "Press X to fall faster, then press X + Space before landing.",
			"Stay alert! Enemies can be anywhere.",
			"Check your corners and listen for footsteps.",
			"Shield potions are your best friend before a fight.",
			"High ground gives you a strategic advantage."
		};

		// PUBLIC METHODS

		public void FadeIn()
		{
			_fadeInObject.SetActive(true);
			_fadeOutObject.SetActive(false);

			_activeFader = _fadeInObject;
			
			if (_tipText != null)
			{
				_tipText.text = _tips[Random.Range(0, _tips.Length)];
			}
		}

		public void FadeOut()
		{
			_dialog.Close_Internal();

			_fadeInObject.SetActive(false);
			_fadeOutObject.SetActive(true);

			_activeFader = _fadeOutObject;
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_fadeInObject.SetActive(false);
			_fadeOutObject.SetActive(false);

			_dialog.Initialize(null, null);
		}

		protected void Update()
		{
			_status.text = Global.Networking.Status;
			_statusDescription.text = Global.Networking.StatusDescription;

			if (Keyboard.current.escapeKey.wasPressedThisFrame == true)
			{
				_dialog.Open_Internal();

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible   = true;

				_dialog.HasClosed += (result) =>
				{
					if (result == true)
					{
						Global.Networking.StopGame();
					}
				};
			}
		}

		protected void OnDestroy()
		{
			if (_dialog != null)
			{
				_dialog.Deinitialize();
			}
		}
	}
}
