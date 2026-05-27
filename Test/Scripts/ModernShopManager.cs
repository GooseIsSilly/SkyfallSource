using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using TPSBR.UI;
using TPSBR.Backend;

namespace TPSBR
{
    public class ModernShopManager : UICloseView
    {
        [Header("Shop Data")]
        [SerializeField] private ShopDatabase _shopDatabase;

        [Header("Daily UI")]
        [SerializeField] private Transform _shopItemsContainer;
        [SerializeField] private GameObject _shopCardPrefab;

        [Header("Weekly UI")]
        [SerializeField] private GameObject _weeklyScrollView;
        [SerializeField] private Transform _weeklyItemsContainer;

        [Header("Tab Buttons")]
        [SerializeField] private UIButton _dailyTabButton;
        [SerializeField] private UIButton _weeklyTabButton;

        [Header("Timers")]
        [SerializeField] private TextMeshProUGUI _dailyTimerText;
        [SerializeField] private TextMeshProUGUI _weeklyTimerText;

        [Header("Coins")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private string _coinsFormat = "{0}";

        [Header("Settings")]
        [SerializeField] private bool _useBackendSync = true;

        // Active tab colours
        private static readonly Color TabActiveColor   = new Color(0.15f, 0.55f, 0.95f, 1f);
        private static readonly Color TabInactiveColor = new Color(0.25f, 0.25f, 0.32f, 1f);

        private PlayerData _playerData;
        private List<ModernShopCard> _dailyCards  = new List<ModernShopCard>();
        private List<ModernShopCard> _weeklyCards = new List<ModernShopCard>();

        private DateTime _dailyResetTime;
        private DateTime _weeklyResetTime;
        private bool _weeklyTabActive;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _playerData = Context.PlayerData;

            if (_dailyTabButton != null)
                _dailyTabButton.onClick.AddListener(() => SetTab(false));

            if (_weeklyTabButton != null)
                _weeklyTabButton.onClick.AddListener(() => SetTab(true));

            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged += OnBackendCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded   += OnBackendDataLoaded;
            }

            InitializeDailyShop();
        }

        protected override void OnDeinitialize()
        {
            if (_dailyTabButton != null)
                _dailyTabButton.onClick.RemoveAllListeners();

            if (_weeklyTabButton != null)
                _weeklyTabButton.onClick.RemoveAllListeners();

            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged -= OnBackendCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded   -= OnBackendDataLoaded;
            }

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            SetTab(false);

            if (_useBackendSync && BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.GetShopCatalog(OnShopCatalogReceived);
            }

            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.LoadPlayerData((success, data) =>
                {
                    if (success)
                    {
                        Debug.Log($"[ModernShop] Player data loaded: {data.CloudCoins} coins");
                        RefreshAllCards();
                        UpdateCoinsDisplay();
                    }
                });
            }
            else
            {
                RefreshAllCards();
                UpdateCoinsDisplay();
            }
        }

        private void OnEnable()
        {
            if (_playerData != null)
            {
                RefreshAllCards();
                UpdateCoinsDisplay();
            }
        }

        private void Update()
        {
            UpdateTimerDisplay(_dailyTimerText,  _dailyResetTime,  "Daily Reset: {0:hh\\:mm\\:ss}",  "Daily Refreshing...");
            UpdateTimerDisplay(_weeklyTimerText, _weeklyResetTime, "Weekly Reset: {0:%d}d {0:hh\\:mm}", "Weekly Refreshing...");
        }

        // ── Tab switching ──────────────────────────────────────────────────────

        /// <summary>Activates either the Daily (false) or Weekly (true) tab.</summary>
        private void SetTab(bool weekly)
        {
            _weeklyTabActive = weekly;

            bool hasDailyContainer  = _shopItemsContainer  != null;
            bool hasWeeklyContainer = _weeklyScrollView     != null;

            if (hasDailyContainer)
                _shopItemsContainer.gameObject.SetActive(!weekly);

            if (hasWeeklyContainer)
                _weeklyScrollView.SetActive(weekly);

            SetTabButtonVisual(_dailyTabButton,  !weekly);
            SetTabButtonVisual(_weeklyTabButton,  weekly);
        }

        private static void SetTabButtonVisual(UIButton button, bool active)
        {
            if (button == null) return;

            var img = button.GetComponent<Image>();
            if (img != null)
                img.color = active ? TabActiveColor : TabInactiveColor;

            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.color = active ? Color.white : new Color(0.75f, 0.75f, 0.75f, 1f);
        }

        // ── Shop catalog (backend) ─────────────────────────────────────────────

        private void OnShopCatalogReceived(bool success, BackendServiceManager.ShopCatalogResponse response)
        {
            if (!success || response == null)
            {
                Debug.LogWarning("[ModernShop] Failed to load shop catalog from backend.");
                return;
            }

            if (DateTime.TryParse(response.daily_reset,  null, System.Globalization.DateTimeStyles.RoundtripKind, out var daily))
                _dailyResetTime = daily.ToUniversalTime();

            if (DateTime.TryParse(response.weekly_reset, null, System.Globalization.DateTimeStyles.RoundtripKind, out var weekly))
                _weeklyResetTime = weekly.ToUniversalTime();

            PopulateWeeklyCards(response.weekly);

            Debug.Log($"[ModernShop] Catalog loaded — {response.daily?.Count ?? 0} daily, {response.weekly?.Count ?? 0} weekly items.");
        }

        // ── Card population ────────────────────────────────────────────────────

        /// <summary>Populates the daily grid from ShopDatabase (existing behaviour).</summary>
        private void InitializeDailyShop()
        {
            if (_shopDatabase == null || _shopCardPrefab == null || _shopItemsContainer == null)
            {
                Debug.LogError("[ModernShop] Missing daily shop references.");
                return;
            }

            ClearCards(_dailyCards);

            var sorted = _shopDatabase.characters
                .Where(c => c != null)
                .OrderByDescending(c => c.rarity)
                .ThenBy(c => c.price)
                .ToList();

            foreach (var character in sorted)
                SpawnCard(character, _shopItemsContainer, _dailyCards);

            UpdateCoinsDisplay();
        }

        /// <summary>Populates the weekly grid from the backend catalog response.</summary>
        private void PopulateWeeklyCards(List<BackendServiceManager.ShopItemData> items)
        {
            if (_weeklyItemsContainer == null || _shopCardPrefab == null || _shopDatabase == null)
                return;

            ClearCards(_weeklyCards);

            if (items == null) return;

            foreach (var item in items)
            {
                var character = _shopDatabase.GetCharacter(item.id);
                if (character != null)
                    SpawnCard(character, _weeklyItemsContainer, _weeklyCards);
            }
        }

        private void SpawnCard(CharacterData character, Transform container, List<ModernShopCard> list)
        {
            var cardObj = Instantiate(_shopCardPrefab, container);
            var card    = cardObj.GetComponent<ModernShopCard>();
            if (card != null)
            {
                card.Setup(character, _playerData, OnPurchaseClicked);
                list.Add(card);
            }
        }

        private static void ClearCards(List<ModernShopCard> list)
        {
            foreach (var card in list)
            {
                if (card != null) Destroy(card.gameObject);
            }
            list.Clear();
        }

        // ── Purchase ───────────────────────────────────────────────────────────

        private void OnPurchaseClicked(CharacterData character)
        {
            if (_playerData == null) return;

            if (_useBackendSync && PlayerDataManager.Instance != null)
                PurchaseWithBackend(character);
            else
                PurchaseWithLocalSystem(character);
        }

        private void PurchaseWithBackend(CharacterData character)
        {
            if (PlayerDataManager.Instance.HasCharacter(character))
            {
                Debug.Log($"[ModernShop] {character.displayName} is already owned.");
                return;
            }

            PlayerDataManager.Instance.PurchaseCharacter(character, (success, message) =>
            {
                if (success)
                {
                    Debug.Log($"[ModernShop] Purchased {character.displayName} for {character.price} coins.");

                    if (!_playerData.ShopSystem.OwnsAgent(character.agentID))
                        _playerData.ShopSystem.OwnedSkins.Add(character.agentID);

                    RefreshAllCards();
                    UpdateCoinsDisplay();
                }
                else
                {
                    Debug.Log($"[ModernShop] Purchase failed: {message}");
                }
            });
        }

        private void PurchaseWithLocalSystem(CharacterData character)
        {
            if (_playerData.ShopSystem.OwnsAgent(character.agentID))
            {
                Debug.Log($"[ModernShop] {character.displayName} is already owned.");
                return;
            }

            bool ok = _playerData.ShopSystem.TryUnlockAgent(character.agentID, character.price, _playerData.CoinSystem);

            if (ok)
            {
                Debug.Log($"[ModernShop] Purchased {character.displayName} for {character.price} coins.");
                RefreshAllCards();
                UpdateCoinsDisplay();
            }
            else
            {
                Debug.Log($"[ModernShop] Not enough coins. Need {character.price}, have {_playerData.CoinSystem.CloudCoins}.");
            }
        }

        // ── Refresh & display ──────────────────────────────────────────────────

        private void RefreshAllCards()
        {
            foreach (var card in _dailyCards)  { if (card != null) card.Refresh(_playerData); }
            foreach (var card in _weeklyCards) { if (card != null) card.Refresh(_playerData); }
        }

        private void UpdateCoinsDisplay()
        {
            if (_coinsText == null) return;

            int coins = 0;

            if (_useBackendSync && PlayerDataManager.Instance != null && PlayerDataManager.Instance.CurrentData != null)
                coins = PlayerDataManager.Instance.CurrentData.CloudCoins;
            else if (_playerData != null)
                coins = _playerData.CoinSystem.CloudCoins;

            _coinsText.text = string.Format(_coinsFormat, coins);
        }

        private static void UpdateTimerDisplay(TextMeshProUGUI label, DateTime resetTime, string format, string expiredText)
        {
            if (label == null || resetTime == default) return;

            var remaining = resetTime - DateTime.UtcNow;
            label.text = remaining.TotalSeconds > 0
                ? string.Format(format, remaining)
                : expiredText;
        }

        // ── Backend event handlers ─────────────────────────────────────────────

        private void OnBackendCoinsChanged(int newAmount)
        {
            UpdateCoinsDisplay();
            RefreshAllCards();
        }

        private void OnBackendDataLoaded(PlayerGameData data)
        {
            UpdateCoinsDisplay();
            RefreshAllCards();
        }
    }
}
