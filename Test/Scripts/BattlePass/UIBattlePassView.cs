using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TPSBR.UI;
using TPSBR.Backend;

namespace TPSBR
{
    /// <summary>
    /// Full-screen Battle Pass menu view.
    /// Extends UICloseView to integrate with the TPSBR UI context system.
    /// Displays a paginated 7-column tier grid, reward detail panel, tab bar,
    /// purchase animation, and buy-tiers panel.
    /// </summary>
    public class UIBattlePassView : UICloseView
    {
        // ── Serialized ─────────────────────────────────────────────────────────

        [Header("Season Info")]
        [SerializeField] private Graphic _seasonNameGraphic;
        [SerializeField] private Graphic _seasonTimerGraphic;
        [SerializeField] private Graphic _tierLevelGraphic;
        [SerializeField] private Slider  _tierProgressBar;

        // Internal getters to handle both TMP and legacy Text/RawImage if needed
        private void SetGraphicText(Graphic graphic, string text)
        {
            if (graphic == null) return;
            if (graphic is TextMeshProUGUI tmp) tmp.text = text;
            else if (graphic is Text legacy) legacy.text = text;
        }

        [Header("Tier Grid")]
        [SerializeField] private Transform       _tierCardContainer;
        [SerializeField] private GameObject      _tierCardPrefab;
        [SerializeField] private UIButton        _prevPageButton;
        [SerializeField] private UIButton        _nextPageButton;
        [SerializeField] private TextMeshProUGUI _pageText;

        [Header("Reward Detail Panel")]
        [SerializeField] private GameObject      _rewardDetailPanel;
        [SerializeField] private Image           _rewardDetailIcon;
        [SerializeField] private TextMeshProUGUI _rewardDetailName;
        [SerializeField] private TextMeshProUGUI _rewardDetailDescription;
        [SerializeField] private TextMeshProUGUI _rewardDetailRarity;

        [Header("Tab Bar")]
        [SerializeField] private UIButton   _tiersTabButton;
        [SerializeField] private UIButton   _challengesTabButton;
        [SerializeField] private GameObject _tiersContent;
        [SerializeField] private GameObject _challengesContent;

        [Header("Bottom Bar")]
        [SerializeField] private UIButton        _purchasePassButton;
        [SerializeField] private UIButton        _buyTiersButton;
        [SerializeField] private TextMeshProUGUI _coinBalanceText;

        [Header("Overlays")]
        [SerializeField] private UIBattlePassPurchaseOverlay _purchaseOverlay;
        [SerializeField] private UIBattlePassBuyTiersPanel   _buyTiersPanel;

        [Header("Challenges Tab")]
        [SerializeField] private UIBattlePassChallengesTab _challengesTab;

        // ── Private State ──────────────────────────────────────────────────────

        private const int TiersPerPage = 7;

        private static readonly Color TabActiveColor   = new Color(0.15f, 0.55f, 0.95f, 1f);
        private static readonly Color TabInactiveColor = new Color(0.25f, 0.25f, 0.32f, 1f);

        private readonly List<UIBattlePassTierCard> _tierCards = new List<UIBattlePassTierCard>();

        private int _currentPage = 0;
        private int _totalPages  = 1;

        // ── UICloseView Lifecycle ──────────────────────────────────────────────

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_tiersTabButton     != null) _tiersTabButton.onClick.AddListener(() => SetTab(true));
            if (_challengesTabButton != null) _challengesTabButton.onClick.AddListener(() => SetTab(false));

            if (_prevPageButton != null) _prevPageButton.onClick.AddListener(PrevPage);
            if (_nextPageButton != null) _nextPageButton.onClick.AddListener(NextPage);

            if (_purchasePassButton != null) _purchasePassButton.onClick.AddListener(OnPurchasePassClicked);
            if (_buyTiersButton     != null) _buyTiersButton.onClick.AddListener(OnBuyTiersClicked);

            if (BattlePassManager.Instance != null)
            {
                BattlePassManager.Instance.OnBattlePassPurchased += OnBattlePassPurchased;
                BattlePassManager.Instance.OnTierReached         += OnTierReached;
                BattlePassManager.Instance.OnXPChanged           += OnXPChanged;
            }

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged += OnCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded   += OnDataLoaded;
            }
        }

        protected override void OnDeinitialize()
        {
            if (_tiersTabButton     != null) _tiersTabButton.onClick.RemoveAllListeners();
            if (_challengesTabButton != null) _challengesTabButton.onClick.RemoveAllListeners();
            if (_prevPageButton     != null) _prevPageButton.onClick.RemoveAllListeners();
            if (_nextPageButton     != null) _nextPageButton.onClick.RemoveAllListeners();
            if (_purchasePassButton != null) _purchasePassButton.onClick.RemoveAllListeners();
            if (_buyTiersButton     != null) _buyTiersButton.onClick.RemoveAllListeners();

            if (BattlePassManager.Instance != null)
            {
                BattlePassManager.Instance.OnBattlePassPurchased -= OnBattlePassPurchased;
                BattlePassManager.Instance.OnTierReached         -= OnTierReached;
                BattlePassManager.Instance.OnXPChanged           -= OnXPChanged;
            }

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged -= OnCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded   -= OnDataLoaded;
            }

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.LoadPlayerData((success, data) =>
                {
                    if (success)
                    {
                        BuildTierGrid();
                        RefreshSeasonInfo();
                        UpdateCoinDisplay();
                    }
                });
            }
            else
            {
                BuildTierGrid();
                RefreshSeasonInfo();
                UpdateCoinDisplay();
            }

            SetTab(true);
        }

        protected override void OnClose()
        {
            ClearTierGrid();
            base.OnClose();
        }

        // ── Update ─────────────────────────────────────────────────────────────

        private void Update()
        {
            UpdateSeasonTimer();
        }

        // ── Public Helper ──────────────────────────────────────────────────────

        /// <summary>Populates the Challenges tab with the provided data. Call when the backend sends weekly challenges.</summary>
        public void SetChallenges(List<ChallengeEntryData> challenges)
        {
            _challengesTab?.Populate(challenges);
        }

        // ── Tab Switching ──────────────────────────────────────────────────────

        private void SetTab(bool tiersActive)
        {
            if (_tiersContent      != null) _tiersContent.SetActive(tiersActive);
            if (_challengesContent != null) _challengesContent.SetActive(!tiersActive);

            SetTabButtonVisual(_tiersTabButton,      tiersActive);
            SetTabButtonVisual(_challengesTabButton, !tiersActive);
        }

        private static void SetTabButtonVisual(UIButton button, bool active)
        {
            if (button == null) return;

            Image img = button.GetComponent<Image>();
            if (img != null)
                img.color = active ? TabActiveColor : TabInactiveColor;

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.color = active ? Color.white : new Color(0.75f, 0.75f, 0.75f, 1f);
        }

        // ── Tier Grid ──────────────────────────────────────────────────────────

        private void BuildTierGrid()
        {
            ClearTierGrid();

            if (BattlePassManager.Instance == null || BattlePassManager.Instance.SeasonData == null) return;

            BattlePassSeasonData seasonData = BattlePassManager.Instance.SeasonData;
            _totalPages = Mathf.CeilToInt((float)seasonData.TotalTiers / TiersPerPage);

            // Clamp the current player tier to find the current-page default
            int playerTier = BattlePassManager.Instance.CurrentTier;
            _currentPage = Mathf.Clamp(Mathf.FloorToInt((float)(playerTier - 1) / TiersPerPage), 0, _totalPages - 1);

            PopulatePage();
        }

        private void PopulatePage()
        {
            ClearTierGrid();

            if (BattlePassManager.Instance == null || BattlePassManager.Instance.SeasonData == null) return;
            if (_tierCardPrefab == null || _tierCardContainer == null) return;

            BattlePassSeasonData seasonData  = BattlePassManager.Instance.SeasonData;
            bool hasBattlePass               = BattlePassManager.Instance.HasBattlePass;
            int  playerTier                  = BattlePassManager.Instance.CurrentTier;

            int startTier = _currentPage * TiersPerPage + 1;
            int endTier   = Mathf.Min(startTier + TiersPerPage - 1, seasonData.TotalTiers);

            for (int t = startTier; t <= endTier; t++)
            {
                BattlePassTierEntry entry = seasonData.GetTierEntry(t);
                if (entry == null) continue;

                GameObject cardObj = Instantiate(_tierCardPrefab, _tierCardContainer);
                UIBattlePassTierCard card = cardObj.GetComponent<UIBattlePassTierCard>();
                if (card == null) continue;

                bool isCurrentTier = t == playerTier;
                bool isUnlocked    = t <= playerTier;

                card.Setup(entry, hasBattlePass, isCurrentTier, isUnlocked, OnTierCardClicked);
                _tierCards.Add(card);
            }

            UpdatePageNavigation();
        }

        private void ClearTierGrid()
        {
            foreach (UIBattlePassTierCard card in _tierCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _tierCards.Clear();
        }

        private void RefreshAllCards()
        {
            if (BattlePassManager.Instance == null) return;

            foreach (UIBattlePassTierCard card in _tierCards)
            {
                if (card != null)
                    card.Refresh(BattlePassManager.Instance.HasBattlePass, BattlePassManager.Instance.CurrentTier);
            }
        }

        // ── Pagination ─────────────────────────────────────────────────────────

        private void PrevPage()
        {
            if (_currentPage <= 0) return;
            _currentPage--;
            PopulatePage();
        }

        private void NextPage()
        {
            if (_currentPage >= _totalPages - 1) return;
            _currentPage++;
            PopulatePage();
        }

        private void UpdatePageNavigation()
        {
            if (_pageText != null)
                _pageText.text = $"PAGE {_currentPage + 1} / {_totalPages}";

            if (_prevPageButton != null)
                _prevPageButton.interactable = _currentPage > 0;

            if (_nextPageButton != null)
                _nextPageButton.interactable = _currentPage < _totalPages - 1;
        }

        // ── Reward Detail Panel ────────────────────────────────────────────────

        private void OnTierCardClicked(int tier, bool isPremium)
        {
            BattlePassTierEntry entry = BattlePassManager.Instance?.GetTierEntry(tier);
            BattlePassRewardData reward = isPremium ? entry?.PremiumReward : entry?.FreeReward;

            if (reward == null)
            {
                if (_rewardDetailPanel != null) _rewardDetailPanel.SetActive(false);
                return;
            }

            if (_rewardDetailPanel != null)   _rewardDetailPanel.SetActive(true);
            if (_rewardDetailIcon != null)    _rewardDetailIcon.sprite = reward.Icon;
            if (_rewardDetailName != null)    _rewardDetailName.text = reward.DisplayName;
            if (_rewardDetailDescription != null) _rewardDetailDescription.text = reward.Description;

            if (_rewardDetailRarity != null)
            {
                _rewardDetailRarity.text  = reward.GetRarityText();
                _rewardDetailRarity.color = reward.GetRarityColor();
            }
        }

        // ── Season Info ────────────────────────────────────────────────────────

        private void RefreshSeasonInfo()
        {
            if (BattlePassManager.Instance == null || BattlePassManager.Instance.SeasonData == null) return;

            BattlePassSeasonData season = BattlePassManager.Instance.SeasonData;

            SetGraphicText(_seasonNameGraphic, season.SeasonName.ToUpper());
            SetGraphicText(_tierLevelGraphic, $"TIER {BattlePassManager.Instance.CurrentTier}");

            if (_tierProgressBar != null)
                _tierProgressBar.value = BattlePassManager.Instance.TierProgress;

            UpdateSeasonTimer();
        }

        private void UpdateSeasonTimer()
        {
            if (_seasonTimerGraphic == null || BattlePassManager.Instance == null) return;

            TimeSpan remaining = BattlePassManager.Instance.SeasonTimeRemaining;
            string timerText = remaining.TotalSeconds > 0
                ? $"SEASON ENDS: {(int)remaining.TotalDays}d {remaining.Hours}h"
                : "SEASON ENDED";
            
            SetGraphicText(_seasonTimerGraphic, timerText);
        }

        // ── Purchase Pass ──────────────────────────────────────────────────────

        private void OnPurchasePassClicked()
        {
            if (BattlePassManager.Instance == null) return;

            if (BattlePassManager.Instance.HasBattlePass)
            {
                Debug.Log("[UIBattlePassView] Battle Pass already owned.");
                return;
            }

            int playerCoins = PlayerDataManager.Instance?.CurrentData?.CloudCoins ?? 0;
            int passCost = BattlePassManager.Instance.SeasonData?.BattlePassCost ?? 950;

            if (playerCoins < passCost)
            {
                Debug.LogWarning($"[UIBattlePassView] Not enough coins. Have {playerCoins}, need {passCost}");
                // Optional: Show a "Not enough coins" popup here
                return;
            }

            // Only play overlay and proceed if they have enough coins
            if (_purchaseOverlay != null)
            {
                // Animation first (Fortnite style), then the backend call happens on confirm or start?
                // Usually the backend call happens during the animation or at the end.
                // Based on existing logic, it plays and then calls the logic in the callback.
                _purchaseOverlay.Play(() =>
                {
                    BattlePassManager.Instance.PurchaseBattlePass((success, message) =>
                    {
                        if (success)
                        {
                            PopulatePage();
                            UpdatePurchaseButtonVisibility();
                            UpdateCoinDisplay();
                        }
                        else
                        {
                            Debug.LogWarning($"[UIBattlePassView] Purchase failed: {message}");
                        }
                    });
                });
            }
            else
            {
                BattlePassManager.Instance.PurchaseBattlePass((success, message) =>
                {
                    if (success)
                    {
                        PopulatePage();
                        UpdatePurchaseButtonVisibility();
                        UpdateCoinDisplay();
                    }
                });
            }
        }

        private void UpdatePurchaseButtonVisibility()
        {
            if (_purchasePassButton == null) return;
            bool hasPass = BattlePassManager.Instance != null && BattlePassManager.Instance.HasBattlePass;
            _purchasePassButton.gameObject.SetActive(!hasPass);
        }

        // ── Buy Tiers ──────────────────────────────────────────────────────────

        private void OnBuyTiersClicked()
        {
            if (_buyTiersPanel == null || BattlePassManager.Instance == null) return;

            BattlePassSeasonData season = BattlePassManager.Instance.SeasonData;
            if (season == null) return;

            int playerCoins = PlayerDataManager.Instance?.CurrentData?.CloudCoins ?? 0;

            _buyTiersPanel.Open(
                BattlePassManager.Instance.CurrentTier,
                season.TotalTiers,
                season.TierSkipCost,
                playerCoins,
                OnBuyTiersConfirmed
            );
        }

        private void OnBuyTiersConfirmed(int quantity)
        {
            BattlePassManager.Instance?.PurchaseTiers(quantity, (success, message) =>
            {
                if (success)
                {
                    // Refresh current page to update badges/locks
                    PopulatePage();
                    RefreshSeasonInfo();
                    UpdateCoinDisplay();
                }
                else
                {
                    Debug.LogWarning($"[UIBattlePassView] Tier purchase failed: {message}");
                }
            });
        }

        // ── Coin Display ───────────────────────────────────────────────────────

        private void UpdateCoinDisplay()
        {
            if (_coinBalanceText == null) return;

            int coins = 0;
            if (PlayerDataManager.Instance?.CurrentData != null)
                coins = PlayerDataManager.Instance.CurrentData.CloudCoins;

            _coinBalanceText.text = $"{coins}";
        }

        // ── Event Handlers ─────────────────────────────────────────────────────

        private void OnBattlePassPurchased()
        {
            // Fully rebuild or just refresh cards to update premium states
            PopulatePage();
            UpdatePurchaseButtonVisibility();
            UpdateCoinDisplay();
        }

        private void OnTierReached(int newTier)
        {
            RefreshSeasonInfo();
            PopulatePage();
        }

        private void OnXPChanged(int newXP)
        {
            if (_tierProgressBar != null && BattlePassManager.Instance != null)
                _tierProgressBar.value = BattlePassManager.Instance.TierProgress;

            SetGraphicText(_tierLevelGraphic, $"TIER {BattlePassManager.Instance.CurrentTier}");
        }

        private void OnCoinsChanged(int newAmount)
        {
            UpdateCoinDisplay();
        }

        private void OnDataLoaded(PlayerGameData data)
        {
            UpdateCoinDisplay();
            RefreshAllCards();
            RefreshSeasonInfo();
            UpdatePurchaseButtonVisibility();
        }
    }
}
