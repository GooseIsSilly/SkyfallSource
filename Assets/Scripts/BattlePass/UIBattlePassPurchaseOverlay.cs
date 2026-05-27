using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TPSBR.UI;

namespace TPSBR
{
    /// <summary>
    /// Full-screen animated overlay that plays when purchasing the Battle Pass.
    /// Sequence: fade in → title slam → banner slide → particle burst → shockwave → confirm text → confirm button.
    /// </summary>
    public class UIBattlePassPurchaseOverlay : MonoBehaviour
    {
        // ── Serialized ─────────────────────────────────────────────────────────

        [Header("Root")]
        [SerializeField] private CanvasGroup _rootCanvasGroup;

        [Header("Title")]
        [SerializeField] private RectTransform _titleRect;
        [SerializeField] private TextMeshProUGUI _titleText;

        [Header("Banner")]
        [SerializeField] private RectTransform _bannerRect;

        [Header("FX")]
        [SerializeField] private ParticleSystem _burstParticles;
        [SerializeField] private RectTransform  _shockwaveRect;
        [SerializeField] private CanvasGroup    _shockwaveCanvasGroup;

        [Header("Confirm")]
        [SerializeField] private CanvasGroup    _confirmTextCanvasGroup;
        [SerializeField] private TextMeshProUGUI _confirmText;
        [SerializeField] private UIButton        _confirmButton;

        [Header("Transformation Icons")]
        [SerializeField] private CanvasGroup _beforeIconCanvasGroup;
        [SerializeField] private CanvasGroup _afterIconCanvasGroup;
        [SerializeField] private RectTransform _iconRoot;

        // ── Private State ──────────────────────────────────────────────────────

        private Action _onComplete;
        private Sequence _activeSequence;

        private const float OffScreenY  = 800f;
        private const float OffScreenX  = -800f;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            gameObject.SetActive(false);

            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        private void OnDestroy()
        {
            _activeSequence?.Kill();

            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmClicked);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Plays the full purchase animation sequence, then invokes <paramref name="onComplete"/>.</summary>
        public void Play(Action onComplete)
        {
            _onComplete = onComplete;

            gameObject.SetActive(true);
            ResetState();

            _activeSequence?.Kill();
            _activeSequence = BuildSequence();
            _activeSequence.Play();
        }

        /// <summary>Immediately hides the overlay without playing the outro.</summary>
        public void Hide()
        {
            _activeSequence?.Kill();
            gameObject.SetActive(false);
        }

        // ── Animation ──────────────────────────────────────────────────────────

        private Sequence BuildSequence()
        {
            Sequence seq = DOTween.Sequence();

            // Step 1 — Panel fade in
            if (_rootCanvasGroup != null)
                seq.Append(_rootCanvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad));

            // Step 2 — Initial Icon Slam & Shake (The Crack)
            if (_iconRoot != null && _beforeIconCanvasGroup != null)
            {
                _beforeIconCanvasGroup.alpha = 1f;
                _afterIconCanvasGroup.alpha = 0f;
                _iconRoot.localScale = Vector3.one * 2f;
                
                // Slam down
                seq.Append(_iconRoot.DOScale(1f, 0.25f).SetEase(Ease.InQuad));
                
                // Shake before cracking
                seq.Append(_iconRoot.DOShakePosition(0.5f, 15f, 20, 90, false, true));
            }

            // Step 3 — THE CRACK (Transformation)
            seq.AppendCallback(() =>
            {
                if (_beforeIconCanvasGroup != null) _beforeIconCanvasGroup.alpha = 0f;
                if (_afterIconCanvasGroup != null) _afterIconCanvasGroup.alpha = 1f;
                if (_burstParticles != null) _burstParticles.Play();
            });

            // Explosion effect on transform
            if (_iconRoot != null)
            {
                seq.Join(_iconRoot.DOPunchScale(Vector3.one * 0.5f, 0.4f, 10, 1f));
            }

            if (_shockwaveRect != null && _shockwaveCanvasGroup != null)
            {
                _shockwaveRect.localScale = Vector3.zero;
                _shockwaveCanvasGroup.alpha = 1f;
                seq.Join(_shockwaveRect.DOScale(5f, 0.5f).SetEase(Ease.OutQuad));
                seq.Join(_shockwaveCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad));
            }

            // Step 4 — Title text reveal ("BATTLE PASS UNLOCKED")
            if (_titleText != null) _titleText.text = "BATTLE PASS UNLOCKED";
            if (_titleRect != null)
            {
                seq.Append(_titleRect.DOScale(1.2f, 0.3f).From(0f).SetEase(Ease.OutBack));
            }

            // Step 5 — Banner and Confirm buttons
            if (_bannerRect != null)
            {
                seq.Append(_bannerRect.DOAnchorPosX(0f, 0.5f).SetEase(Ease.OutBack));
            }

            if (_confirmTextCanvasGroup != null)
                seq.Append(_confirmTextCanvasGroup.DOFade(1f, 0.5f));

            seq.AppendCallback(() =>
            {
                if (_confirmButton != null)
                {
                    _confirmButton.gameObject.SetActive(true);
                    _confirmButton.transform.localScale = Vector3.zero;
                    _confirmButton.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
                }
            });

            return seq;
        }

        private void ResetState()
        {
            if (_rootCanvasGroup != null)    _rootCanvasGroup.alpha   = 0f;

            if (_beforeIconCanvasGroup != null) _beforeIconCanvasGroup.alpha = 1f;
            if (_afterIconCanvasGroup != null)  _afterIconCanvasGroup.alpha = 0f;
            
            if (_iconRoot != null)
            {
                _iconRoot.localScale = Vector3.one;
                _iconRoot.anchoredPosition = Vector2.zero;
            }

            if (_titleRect != null)          _titleRect.localScale = Vector3.zero;
            if (_bannerRect != null)         _bannerRect.anchoredPosition = new Vector2(OffScreenX, _bannerRect.anchoredPosition.y);
            
            if (_shockwaveRect != null)      _shockwaveRect.localScale      = Vector3.zero;
            if (_shockwaveCanvasGroup != null) _shockwaveCanvasGroup.alpha   = 1f;

            if (_confirmTextCanvasGroup != null) _confirmTextCanvasGroup.alpha = 0f;
            if (_confirmButton != null)          _confirmButton.gameObject.SetActive(false);
        }

        // ── Button Handlers ────────────────────────────────────────────────────

        private void OnConfirmClicked()
        {
            _activeSequence?.Kill();

            if (_rootCanvasGroup != null)
            {
                _rootCanvasGroup.DOFade(0f, 0.25f).OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _onComplete?.Invoke();
                    _onComplete = null;
                });
            }
            else
            {
                gameObject.SetActive(false);
                _onComplete?.Invoke();
                _onComplete = null;
            }
        }
    }
}
