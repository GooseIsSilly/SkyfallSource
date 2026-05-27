using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using TPSBR.Backend;

namespace TPSBR
{
    public class BattlePassManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────────

        public static BattlePassManager Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────────

        [Header("Season Configuration")]
        [SerializeField] private BattlePassSeasonData _seasonData;

        // ── Public State ───────────────────────────────────────────────────────

        /// <summary>The active season's data asset.</summary>
        public BattlePassSeasonData SeasonData => _seasonData;

        /// <summary>Whether the local player owns the Battle Pass this season.</summary>
        public bool HasBattlePass { get; private set; }

        /// <summary>Current tier (1-based). Starts at 0 before any data is loaded.</summary>
        public int CurrentTier { get; private set; }

        /// <summary>XP accumulated within the current tier.</summary>
        public int CurrentBattlePassXP { get; private set; }

        /// <summary>XP needed to complete the current tier.</summary>
        public int XPToNextTier => _seasonData != null ? _seasonData.XPPerTier : 1000;

        /// <summary>Progress through the current tier, in the range [0, 1].</summary>
        public float TierProgress => XPToNextTier > 0 ? Mathf.Clamp01((float)CurrentBattlePassXP / XPToNextTier) : 0f;

        /// <summary>Time remaining until the season ends.</summary>
        public TimeSpan SeasonTimeRemaining
        {
            get
            {
                if (_seasonData == null) return TimeSpan.Zero;
                if (DateTime.TryParse(_seasonData.SeasonEndDateISO, null, DateTimeStyles.RoundtripKind, out DateTime end))
                    return end.ToUniversalTime() - DateTime.UtcNow;
                return TimeSpan.Zero;
            }
        }

        // ── Events ─────────────────────────────────────────────────────────────

        /// <summary>Fired when the player successfully purchases the Battle Pass.</summary>
        public event Action OnBattlePassPurchased;

        /// <summary>Fired each time the player reaches a new tier. Argument = new tier number.</summary>
        public event Action<int> OnTierReached;

        /// <summary>Fired whenever XP changes. Argument = current XP within the tier.</summary>
        public event Action<int> OnXPChanged;

        // ── Private State ──────────────────────────────────────────────────────

        private const string ClaimedKeyPrefix = "BP_Claimed_";

        private HashSet<string> _claimedRewards = new HashSet<string>();

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            LoadClaimedRewardsFromStorage();
        }

        private void OnEnable()
        {
            if (PlayerDataManager.Instance != null)
                PlayerDataManager.Instance.OnDataLoaded += OnPlayerDataLoaded;
        }

        private void OnDisable()
        {
            if (PlayerDataManager.Instance != null)
                PlayerDataManager.Instance.OnDataLoaded -= OnPlayerDataLoaded;
        }

        // ── Backend Sync ───────────────────────────────────────────────────────

        private void OnPlayerDataLoaded(PlayerGameData data)
        {
            if (data == null) return;

            HasBattlePass       = data.HasBattlePass;
            CurrentTier         = data.BattlePassTier;
            CurrentBattlePassXP = data.BattlePassXP;

            Debug.Log($"[BattlePassManager] Loaded: HasPass={HasBattlePass}, Tier={CurrentTier}, XP={CurrentBattlePassXP}");
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Deducts CloudCoins and calls /battlepass/purchase on the backend.</summary>
        public void PurchaseBattlePass(Action<bool, string> callback)
        {
            if (HasBattlePass)
            {
                callback?.Invoke(false, "Battle Pass already owned");
                return;
            }

            if (_seasonData == null)
            {
                callback?.Invoke(false, "Season data not assigned");
                return;
            }

            if (PlayerDataManager.Instance == null)
            {
                callback?.Invoke(false, "PlayerDataManager not available");
                return;
            }

            // Delegate to PlayerDataManager which already handles coin deduction + backend call
            PlayerDataManager.Instance.PurchaseBattlePass((success, message) =>
            {
                if (success)
                {
                    HasBattlePass = true;
                    OnBattlePassPurchased?.Invoke();
                    Debug.Log("[BattlePassManager] Battle Pass purchased successfully.");
                }
                else
                {
                    Debug.LogWarning($"[BattlePassManager] Purchase failed: {message}");
                }

                callback?.Invoke(success, message);
            });
        }

        /// <summary>Deducts CloudCoins and skips <paramref name="count"/> tiers instantly.</summary>
        public void PurchaseTiers(int count, Action<bool, string> callback)
        {
            if (count <= 0)
            {
                callback?.Invoke(false, "Invalid tier count");
                return;
            }

            if (_seasonData == null)
            {
                callback?.Invoke(false, "Season data not assigned");
                return;
            }

            int cost = count * _seasonData.TierSkipCost;

            if (PlayerDataManager.Instance == null)
            {
                callback?.Invoke(false, "PlayerDataManager not available");
                return;
            }

            PlayerGameData data = PlayerDataManager.Instance.CurrentData;
            if (data == null || data.CloudCoins < cost)
            {
                callback?.Invoke(false, $"Not enough CloudCoins (need {cost})");
                return;
            }

            // Deduct coins
            PlayerDataManager.Instance.AddCoins(-cost, (coinSuccess, newTotal) =>
            {
                if (!coinSuccess)
                {
                    callback?.Invoke(false, "Failed to deduct coins");
                    return;
                }

                // Apply tier skip locally
                int maxTier = _seasonData.TotalTiers;
                int newTier = Mathf.Min(CurrentTier + count, maxTier);
                int tiersGained = newTier - CurrentTier;

                for (int i = 0; i < tiersGained; i++)
                {
                    CurrentTier++;
                    OnTierReached?.Invoke(CurrentTier);
                }

                CurrentBattlePassXP = 0;

                // Persist to backend
                if (PlayerDataManager.Instance.CurrentData != null)
                {
                    PlayerDataManager.Instance.CurrentData.BattlePassTier = CurrentTier;
                    PlayerDataManager.Instance.CurrentData.BattlePassXP   = 0;
                    PlayerDataManager.Instance.SaveAllData();
                }

                Debug.Log($"[BattlePassManager] Skipped {tiersGained} tiers. Now tier {CurrentTier}.");
                callback?.Invoke(true, $"Skipped to tier {CurrentTier}");
            });
        }

        /// <summary>Awards XP and triggers tier-up events. Syncs to backend.</summary>
        public void AddXP(int amount)
        {
            if (_seasonData == null || amount <= 0) return;

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.AddBattlePassXP(amount, (success, newTier, newXP) =>
                {
                    if (!success) return;

                    int oldTier = CurrentTier;
                    CurrentTier         = newTier;
                    CurrentBattlePassXP = newXP;

                    OnXPChanged?.Invoke(CurrentBattlePassXP);

                    for (int t = oldTier + 1; t <= newTier; t++)
                        OnTierReached?.Invoke(t);
                });
            }
            else
            {
                // Offline fallback
                CurrentBattlePassXP += amount;

                while (_seasonData != null && CurrentBattlePassXP >= _seasonData.XPPerTier && CurrentTier < _seasonData.TotalTiers)
                {
                    CurrentBattlePassXP -= _seasonData.XPPerTier;
                    CurrentTier++;
                    OnTierReached?.Invoke(CurrentTier);
                }

                OnXPChanged?.Invoke(CurrentBattlePassXP);
            }
        }

        /// <summary>Returns the BattlePassTierEntry for a given 1-based tier number.</summary>
        public BattlePassTierEntry GetTierEntry(int tier)
        {
            return _seasonData != null ? _seasonData.GetTierEntry(tier) : null;
        }

        /// <summary>Returns whether the player can claim the reward for the given tier.</summary>
        public bool CanClaimReward(int tier, bool isPremium)
        {
            if (tier > CurrentTier) return false;
            if (isPremium && !HasBattlePass) return false;
            if (IsRewardClaimed(tier, isPremium)) return false;

            BattlePassTierEntry entry = GetTierEntry(tier);
            if (entry == null) return false;

            return isPremium ? entry.PremiumReward != null : entry.FreeReward != null;
        }

        /// <summary>Marks a reward as claimed (local + persisted via PersistentStorage).</summary>
        public void ClaimReward(int tier, bool isPremium, Action<bool> callback)
        {
            if (!CanClaimReward(tier, isPremium))
            {
                callback?.Invoke(false);
                return;
            }

            string key = BuildClaimKey(tier, isPremium);
            _claimedRewards.Add(key);
            PersistentStorage.SetBool(ClaimedKeyPrefix + key, true);
            PersistentStorage.Save();

            Debug.Log($"[BattlePassManager] Claimed reward: {key}");
            callback?.Invoke(true);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private bool IsRewardClaimed(int tier, bool isPremium)
        {
            return _claimedRewards.Contains(BuildClaimKey(tier, isPremium));
        }

        private static string BuildClaimKey(int tier, bool isPremium)
        {
            return $"{tier}_{(isPremium ? "premium" : "free")}";
        }

        private void LoadClaimedRewardsFromStorage()
        {
            _claimedRewards.Clear();

            if (_seasonData == null) return;

            for (int t = 1; t <= _seasonData.TotalTiers; t++)
            {
                string freeKey    = BuildClaimKey(t, false);
                string premiumKey = BuildClaimKey(t, true);

                if (PersistentStorage.GetBool(ClaimedKeyPrefix + freeKey, false))
                    _claimedRewards.Add(freeKey);

                if (PersistentStorage.GetBool(ClaimedKeyPrefix + premiumKey, false))
                    _claimedRewards.Add(premiumKey);
            }

            Debug.Log($"[BattlePassManager] Loaded {_claimedRewards.Count} claimed rewards from storage.");
        }
    }
}
