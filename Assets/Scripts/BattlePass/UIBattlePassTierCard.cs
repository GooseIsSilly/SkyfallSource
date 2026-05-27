using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TPSBR.UI;

namespace TPSBR
{
    /// <summary>Single vertical tier column displayed in the Battle Pass tier grid.</summary>
    public class UIBattlePassTierCard : MonoBehaviour
    {
        // ── Serialized ─────────────────────────────────────────────────────────

        [Header("Tier Number")]
        [SerializeField] private TextMeshProUGUI _tierNumberText;

        [Header("Free Reward")]
        [SerializeField] private Image        _freeRewardIcon;
        [SerializeField] private GameObject   _freeLockIcon;
        [SerializeField] private GameObject   _freeClaimedBadge;

        [Header("Premium Reward")]
        [SerializeField] private Image        _premiumRewardIcon;
        [SerializeField] private GameObject   _premiumLockIcon;
        [SerializeField] private GameObject   _premiumClaimedBadge;
        [SerializeField] private Image        _premiumRowBackground;

        [Header("State Visuals")]
        [SerializeField] private Image        _currentTierHighlight;

        // ── Private State ──────────────────────────────────────────────────────

        private BattlePassTierEntry  _entry;
        private Action<int, bool>    _onClicked;
        private bool                 _isUnlocked;

        private static readonly Color PremiumRowColor   = new Color(0.8f, 0.65f, 0.1f, 0.3f);
        private static readonly Color PremiumRowLocked  = new Color(0.3f, 0.3f, 0.3f, 0.3f);

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Performs full setup with data. Call once after instantiation.</summary>
        public void Setup(BattlePassTierEntry entry, bool hasBattlePass, bool isCurrentTier, bool isUnlocked, Action<int, bool> onClicked)
        {
            _entry     = entry;
            _onClicked = onClicked;
            _isUnlocked = isUnlocked;

            if (_tierNumberText != null)
                _tierNumberText.text = entry.Tier.ToString();

            ApplyFreeReward(entry, isUnlocked);
            ApplyPremiumReward(entry, hasBattlePass, isUnlocked);
            SetCurrentTierHighlight(isCurrentTier);
        }

        /// <summary>Refreshes lock/unlock and badge state without rebuilding the whole card.</summary>
        public void Refresh(bool hasBattlePass, int currentTier)
        {
            if (_entry == null) return;

            bool isUnlocked = _entry.Tier <= currentTier;
            _isUnlocked = isUnlocked;

            bool freeClaimed    = BattlePassManager.Instance != null && !BattlePassManager.Instance.CanClaimReward(_entry.Tier, false);
            bool premiumClaimed = BattlePassManager.Instance != null && !BattlePassManager.Instance.CanClaimReward(_entry.Tier, true);

            SetActive(_freeLockIcon,      !isUnlocked && _entry.FreeReward != null);
            SetActive(_freeClaimedBadge,  isUnlocked  && freeClaimed && _entry.FreeReward != null);
            SetActive(_premiumLockIcon,   (!hasBattlePass || !isUnlocked) && _entry.PremiumReward != null);
            SetActive(_premiumClaimedBadge, hasBattlePass && isUnlocked && premiumClaimed && _entry.PremiumReward != null);

            if (_premiumRowBackground != null)
                _premiumRowBackground.color = hasBattlePass ? PremiumRowColor : PremiumRowLocked;

            SetCurrentTierHighlight(_entry.Tier == currentTier);
        }

        // ── Button Callbacks ───────────────────────────────────────────────────

        /// <summary>Called by a Button component on the free reward cell.</summary>
        public void OnFreeRewardClicked()
        {
            _onClicked?.Invoke(_entry.Tier, false);
        }

        /// <summary>Called by a Button component on the premium reward cell.</summary>
        public void OnPremiumRewardClicked()
        {
            _onClicked?.Invoke(_entry.Tier, true);
        }

        // ── Private Helpers ────────────────────────────────────────────────────

        private void ApplyFreeReward(BattlePassTierEntry entry, bool isUnlocked)
        {
            bool hasFree = entry.FreeReward != null;

            SetActive(_freeRewardIcon?.gameObject, hasFree);
            if (hasFree && _freeRewardIcon != null)
                _freeRewardIcon.sprite = entry.FreeReward.Icon;

            bool freeClaimed = BattlePassManager.Instance != null && !BattlePassManager.Instance.CanClaimReward(entry.Tier, false);

            SetActive(_freeLockIcon,     hasFree && !isUnlocked);
            SetActive(_freeClaimedBadge, hasFree && isUnlocked && freeClaimed);
        }

        private void ApplyPremiumReward(BattlePassTierEntry entry, bool hasBattlePass, bool isUnlocked)
        {
            bool hasPremium = entry.PremiumReward != null;

            SetActive(_premiumRewardIcon?.gameObject, hasPremium);
            if (hasPremium && _premiumRewardIcon != null)
                _premiumRewardIcon.sprite = entry.PremiumReward.Icon;

            bool premiumClaimed = BattlePassManager.Instance != null && !BattlePassManager.Instance.CanClaimReward(entry.Tier, true);

            SetActive(_premiumLockIcon,    hasPremium && (!hasBattlePass || !isUnlocked));
            SetActive(_premiumClaimedBadge, hasPremium && hasBattlePass && isUnlocked && premiumClaimed);

            if (_premiumRowBackground != null)
                _premiumRowBackground.color = hasBattlePass ? PremiumRowColor : PremiumRowLocked;
        }

        private void SetCurrentTierHighlight(bool isCurrentTier)
        {
            if (_currentTierHighlight != null)
                _currentTierHighlight.gameObject.SetActive(isCurrentTier);
        }

        private static void SetActive(GameObject obj, bool active)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}
