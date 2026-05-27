using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
    public class UIDownedState : UIWidget
    {
        [SerializeField]
        private TextMeshProUGUI _downedText;
        [SerializeField]
        private TextMeshProUGUI _timerText;

        private ReviveSystem _localReviveSystem;
        private Player _localPlayer;
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

            UpdateLocalPlayer();

            if (_localReviveSystem == null || !_localReviveSystem.IsDown)
            {
                HideDownedState();
                return;
            }

            ShowDownedState();
            UpdateDownedState();
        }

        private void UpdateLocalPlayer()
        {
            if (_localPlayer != null && _localReviveSystem != null)
                return;

            if (Context == null || Context.NetworkGame == null)
                return;

            _localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            if (_localPlayer != null)
            {
                _localReviveSystem = _localPlayer.GetComponent<ReviveSystem>();
            }
        }

        private void ShowDownedState()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        private void HideDownedState()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void UpdateDownedState()
        {
            if (_localReviveSystem == null)
                return;

            float bleedOutTime = _localReviveSystem.BleedOutProgress;

            if (_timerText != null)
            {
                _timerText.text = $"{Mathf.CeilToInt(bleedOutTime)}s";
            }

            if (_downedText != null)
            {
                if (_localReviveSystem.IsBeingRevived)
                {
                    var reviver = _localReviveSystem.GetRevivingPlayer();
                    if (reviver != null)
                    {
                        _downedText.text = $"{reviver.Nickname} is reviving you...";
                    }
                    else
                    {
                        _downedText.text = "Being Revived...";
                    }
                }
                else
                {
                    _downedText.text = "YOU ARE DOWNED";
                }
            }
        }
    }
}
