using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    /// <summary>
    /// Drives the Manhunt waiting-lobby UI and the fade-to-black role reveal sequence.
    ///
    /// Requires two child CanvasGroups on this GameObject:
    ///   - _waitingPanel     : "Waiting for players…" counter shown while frozen.
    ///   - _fadeOverlay      : Full-screen black Image used for the fade transition.
    ///   - _roleText         : TextMeshProUGUI shown while screen is black ("YOU ARE THE PREY" etc.).
    /// </summary>
    public class UIManhuntWidget : UIWidget
    {
        // ---- Inspector ----

        [Header("Waiting Panel")]
        [SerializeField] private GameObject _waitingPanel;
        [SerializeField] private TextMeshProUGUI _playerCountText;

        [Header("Fade Overlay")]
        [SerializeField] private CanvasGroup _fadeOverlay;
        [Tooltip("Seconds to fade the screen to black.")]
        [SerializeField] private float _fadeInDuration = 0.8f;
        [Tooltip("Seconds to hold on the black screen while the role text is visible.")]
        [SerializeField] private float _holdDuration = 2.5f;
        [Tooltip("Seconds to fade back to the game.")]
        [SerializeField] private float _fadeOutDuration = 0.8f;

        [Header("Role Text")]
        [SerializeField] private TextMeshProUGUI _roleText;
        [SerializeField] private Color _preyColor = new Color(1f, 0.2f, 0.2f);
        [SerializeField] private Color _hunterColor = new Color(0.2f, 0.6f, 1f);

        // ---- Private ----

        private ManhuntGameplayMode _manhunt;
        private Coroutine _revealCoroutine;

        // ---- UIWidget Interface ----

        protected override void OnVisible()
        {
            _manhunt = Context?.GameplayMode as ManhuntGameplayMode;

            if (_manhunt == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _manhunt.OnRoleRevealStarted += HandleRoleRevealStarted;
            _manhunt.OnGameplayUnlocked  += HandleGameplayUnlocked;

            // Ensure overlay starts fully transparent
            SetOverlayAlpha(0f);
            _fadeOverlay.gameObject.SetActive(false);

            if (_waitingPanel != null)
                _waitingPanel.SetActive(true);

            if (_roleText != null)
                _roleText.gameObject.SetActive(false);
        }

        protected override void OnHidden()
        {
            if (_manhunt != null)
            {
                _manhunt.OnRoleRevealStarted -= HandleRoleRevealStarted;
                _manhunt.OnGameplayUnlocked  -= HandleGameplayUnlocked;
            }

            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
                _revealCoroutine = null;
            }
        }

        protected override void OnTick()
        {
            if (_manhunt == null)
                return;

            // Update the waiting panel player count
            if (_waitingPanel != null && _waitingPanel.activeSelf)
            {
                int current = 0;
                if (Context?.NetworkGame != null)
                    current = Context.NetworkGame.ActivePlayerCount;

                if (_playerCountText != null)
                    _playerCountText.text = $"Waiting for players... {current}/10";
            }
        }

        // ---- Event Handlers ----

        private void HandleRoleRevealStarted(bool localIsPrey)
        {
            if (_revealCoroutine != null)
                StopCoroutine(_revealCoroutine);

            _revealCoroutine = StartCoroutine(PlayRevealSequence(localIsPrey));
        }

        private void HandleGameplayUnlocked()
        {
            // Hide waiting panel in case the coroutine already finished, but guard regardless
            if (_waitingPanel != null)
                _waitingPanel.SetActive(false);
        }

        // ---- Reveal Coroutine ----

        private IEnumerator PlayRevealSequence(bool localIsPrey)
        {
            // 1. Hide waiting panel
            if (_waitingPanel != null)
                _waitingPanel.SetActive(false);

            // 2. Fade to black
            _fadeOverlay.gameObject.SetActive(true);
            yield return StartCoroutine(FadeOverlay(0f, 1f, _fadeInDuration));

            // 3. Show role text
            if (_roleText != null)
            {
                _roleText.gameObject.SetActive(true);

                if (localIsPrey)
                {
                    _roleText.text  = "YOU ARE THE PREY\n\nSURVIVE 15 MINUTES";
                    _roleText.color = _preyColor;
                }
                else
                {
                    _roleText.text  = "YOU ARE A HUNTER\n\nHUNT THE PREY";
                    _roleText.color = _hunterColor;
                }
            }

            // 4. Hold on black
            yield return new WaitForSecondsRealtime(_holdDuration);

            // 5. Fade back in
            if (_roleText != null)
                _roleText.gameObject.SetActive(false);

            yield return StartCoroutine(FadeOverlay(1f, 0f, _fadeOutDuration));

            _fadeOverlay.gameObject.SetActive(false);
            _revealCoroutine = null;
        }

        // ---- Helpers ----

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            float elapsed = 0f;
            SetOverlayAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetOverlayAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetOverlayAlpha(to);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (_fadeOverlay != null)
                _fadeOverlay.alpha = alpha;
        }
    }
}
