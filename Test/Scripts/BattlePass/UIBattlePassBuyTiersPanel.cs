using System;
using TMPro;
using UnityEngine;
using TPSBR.UI;

namespace TPSBR
{
    /// <summary>Slide-up panel that lets the player buy 1, 5, 10, or a custom number of tiers.</summary>
    public class UIBattlePassBuyTiersPanel : MonoBehaviour
    {
        // ── Serialized ─────────────────────────────────────────────────────────

        [Header("Quantity Controls")]
        [SerializeField] private UIButton _decreaseButton;
        [SerializeField] private UIButton _increaseButton;
        [SerializeField] private TextMeshProUGUI _quantityText;

        [Header("Preset Buttons")]
        [SerializeField] private UIButton _preset1Button;
        [SerializeField] private UIButton _preset5Button;
        [SerializeField] private UIButton _preset10Button;

        [Header("Cost Display")]
        [SerializeField] private TextMeshProUGUI _totalCostText;
        [SerializeField] private TextMeshProUGUI _playerCoinsText;

        [Header("Actions")]
        [SerializeField] private UIButton _confirmButton;
        [SerializeField] private UIButton _cancelButton;

        // ── Private State ──────────────────────────────────────────────────────

        private int _currentQuantity = 1;
        private int _maxTier;
        private int _currentTier;
        private int _costPerTier;
        private int _playerCoins;
        private Action<int> _onConfirm;

        private const int MinQuantity = 1;

        // ── Lifecycle ──────────────────────────────────────────────────────────

        private void Awake()
        {
            gameObject.SetActive(false);

            if (_decreaseButton != null) _decreaseButton.onClick.AddListener(OnDecrease);
            if (_increaseButton != null) _increaseButton.onClick.AddListener(OnIncrease);

            if (_preset1Button  != null) _preset1Button.onClick.AddListener( () => SetQuantity(1));
            if (_preset5Button  != null) _preset5Button.onClick.AddListener( () => SetQuantity(5));
            if (_preset10Button != null) _preset10Button.onClick.AddListener(() => SetQuantity(10));

            if (_confirmButton != null) _confirmButton.onClick.AddListener(OnConfirm);
            if (_cancelButton  != null) _cancelButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (_decreaseButton != null) _decreaseButton.onClick.RemoveAllListeners();
            if (_increaseButton != null) _increaseButton.onClick.RemoveAllListeners();
            if (_preset1Button  != null) _preset1Button.onClick.RemoveAllListeners();
            if (_preset5Button  != null) _preset5Button.onClick.RemoveAllListeners();
            if (_preset10Button != null) _preset10Button.onClick.RemoveAllListeners();
            if (_confirmButton  != null) _confirmButton.onClick.RemoveAllListeners();
            if (_cancelButton   != null) _cancelButton.onClick.RemoveAllListeners();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>Opens the panel and configures it for the player's current state.</summary>
        public void Open(int currentTier, int maxTier, int costPerTier, int playerCoins, Action<int> onConfirm)
        {
            _currentTier  = currentTier;
            _maxTier      = maxTier;
            _costPerTier  = costPerTier;
            _playerCoins  = playerCoins;
            _onConfirm    = onConfirm;

            gameObject.SetActive(true);

            if (_playerCoinsText != null)
                _playerCoinsText.text = $"{playerCoins} COINS";

            SetQuantity(1);
        }

        /// <summary>Hides the panel.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        // ── Private Handlers ───────────────────────────────────────────────────

        private void OnDecrease() => SetQuantity(_currentQuantity - 1);
        private void OnIncrease() => SetQuantity(_currentQuantity + 1);

        private void SetQuantity(int qty)
        {
            int maxPurchasable = _maxTier - _currentTier;
            _currentQuantity = Mathf.Clamp(qty, MinQuantity, Mathf.Max(MinQuantity, maxPurchasable));

            if (_quantityText != null)
                _quantityText.text = _currentQuantity.ToString();

            int totalCost = _currentQuantity * _costPerTier;

            if (_totalCostText != null)
                _totalCostText.text = $"{totalCost} COINS";

            // Visually indicate unaffordable purchase
            if (_confirmButton != null)
            {
                bool canAfford = _playerCoins >= totalCost;
                _confirmButton.interactable = canAfford;
            }

            if (_decreaseButton != null)
                _decreaseButton.interactable = _currentQuantity > MinQuantity;

            if (_increaseButton != null)
                _increaseButton.interactable = _currentQuantity < maxPurchasable;
        }

        private void OnConfirm()
        {
            int quantityToConfirm = _currentQuantity;
            Close();
            _onConfirm?.Invoke(quantityToConfirm);
        }
    }
}
