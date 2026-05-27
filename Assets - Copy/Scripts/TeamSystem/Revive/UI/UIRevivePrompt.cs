using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
    public class UIRevivePrompt : UIWidget
    {
        [SerializeField]
        private TextMeshProUGUI _promptText;
        [SerializeField]
        private Image _progressBar;
        [SerializeField]
        private TextMeshProUGUI _playerNameText;
        [SerializeField]
        private string _promptMessage = "Hold [U] to Revive";

        private ReviveInteraction _reviveInteraction;
        private CanvasGroup _canvasGroup;

        protected override void OnTick()
        {
            base.OnTick();

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (_reviveInteraction == null && Context != null)
            {
                _reviveInteraction = FindFirstObjectByType<ReviveInteraction>();
                if (_reviveInteraction != null)
                {
                    Debug.Log("[UIRevivePrompt] Found ReviveInteraction component");
                }
                else
                {
                    Debug.LogWarning("[UIRevivePrompt] ReviveInteraction component not found in scene");
                }
            }

            if (_reviveInteraction == null)
            {
                HidePrompt();
                return;
            }

            var nearbyDowned = _reviveInteraction.GetNearbyDownedPlayer();
            if (nearbyDowned != null && nearbyDowned.IsDown)
            {
                ShowPrompt(nearbyDowned);
            }
            else
            {
                HidePrompt();
            }
        }

        private void ShowPrompt(ReviveSystem reviveSystem)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (_promptText != null)
            {
                _promptText.text = _promptMessage;
            }

            if (_playerNameText != null)
            {
                var player = reviveSystem.GetComponent<Player>();
                if (player != null)
                {
                    _playerNameText.text = player.Nickname;
                }
            }

            if (_progressBar != null)
            {
                float progress = reviveSystem.ReviveProgress;
                _progressBar.fillAmount = progress;
            }
        }

        private void HidePrompt()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }
    }
}
