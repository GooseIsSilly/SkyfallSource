using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using TPSBR.UI;
using TPSBR.Backend;

namespace TPSBR
{
    public class ModernShopManager : UICloseView
    {
        [Header("Shop Data")]
        [SerializeField] private ShopDatabase _shopDatabase;
        
        [Header("UI References")]
        [SerializeField] private Transform _shopItemsContainer;
        [SerializeField] private GameObject _shopCardPrefab;
        [SerializeField] private TMPro.TextMeshProUGUI _coinsText;
        
        [Header("Settings")]
        [SerializeField] private string _coinsFormat = "{0}";
        [SerializeField] private bool _useBackendSync = true;
        
        private PlayerData _playerData;
        private List<ModernShopCard> _spawnedCards = new List<ModernShopCard>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            _playerData = Context.PlayerData;
            
            // Subscribe to backend coin changes
            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged += OnBackendCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded += OnBackendDataLoaded;
            }
            
            InitializeShop();
        }

        protected override void OnDeinitialize()
        {
            // Unsubscribe from backend coin changes
            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged -= OnBackendCoinsChanged;
                PlayerDataManager.Instance.OnDataLoaded -= OnBackendDataLoaded;
            }
            
            base.OnDeinitialize();
        }
        
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

        private void InitializeShop()
        {
            if (_shopDatabase == null)
            {
                Debug.LogError("ShopDatabase is not assigned!");
                return;
            }

            if (_shopCardPrefab == null)
            {
                Debug.LogError("Shop card prefab is not assigned!");
                return;
            }

            if (_shopItemsContainer == null)
            {
                Debug.LogError("Shop items container is not assigned!");
                return;
            }

            ClearShopItems();
            
            var sortedCharacters = _shopDatabase.characters
                .OrderByDescending(c => c.rarity)
                .ThenBy(c => c.price)
                .ToList();

            foreach (var character in sortedCharacters)
            {
                if (character == null) continue;
                
                GameObject cardObj = Instantiate(_shopCardPrefab, _shopItemsContainer);
                ModernShopCard card = cardObj.GetComponent<ModernShopCard>();
                
                if (card != null)
                {
                    card.Setup(character, _playerData, OnPurchaseClicked);
                    _spawnedCards.Add(card);
                }
            }

            UpdateCoinsDisplay();
        }

        private void ClearShopItems()
        {
            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            _spawnedCards.Clear();
        }

        private void OnPurchaseClicked(CharacterData character)
        {
            if (_playerData == null) return;
            
            // Use backend sync if enabled
            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PurchaseWithBackend(character);
            }
            else
            {
                PurchaseWithLocalSystem(character);
            }
        }
        
        private void PurchaseWithBackend(CharacterData character)
        {
            // Check if already owned
            if (PlayerDataManager.Instance.HasCharacter(character))
            {
                Debug.Log($"{character.displayName} is already owned!");
                return;
            }
            
            // Use the backend purchase system
            PlayerDataManager.Instance.PurchaseCharacter(character, (success, message) =>
            {
                if (success)
                {
                    Debug.Log($"[ModernShop] Purchased {character.displayName} for {character.price} coins!");
                    
                    // Also add to local system for this session
                    if (!_playerData.ShopSystem.OwnsAgent(character.agentID))
                    {
                        _playerData.ShopSystem.OwnedSkins.Add(character.agentID);
                    }
                    
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
            bool isOwned = _playerData.ShopSystem.OwnsAgent(character.agentID);
            if (isOwned)
            {
                Debug.Log($"{character.displayName} is already owned!");
                return;
            }

            bool purchaseSuccessful = _playerData.ShopSystem.TryUnlockAgent(
                character.agentID, 
                character.price, 
                _playerData.CoinSystem
            );

            if (purchaseSuccessful)
            {
                Debug.Log($"Purchased {character.displayName} for {character.price} coins!");
                RefreshAllCards();
                UpdateCoinsDisplay();
            }
            else
            {
                int currentCoins = _playerData.CoinSystem.CloudCoins;
                Debug.Log($"Not enough coins! Need {character.price}, have {currentCoins}");
            }
        }

        private void RefreshAllCards()
        {
            foreach (var card in _spawnedCards)
            {
                if (card != null)
                {
                    card.Refresh(_playerData);
                }
            }
        }

        private void UpdateCoinsDisplay()
        {
            if (_coinsText == null) return;
            
            int coins = 0;
            
            // Use backend coins if enabled and available
            if (_useBackendSync && PlayerDataManager.Instance != null && PlayerDataManager.Instance.CurrentData != null)
            {
                coins = PlayerDataManager.Instance.CurrentData.CloudCoins;
            }
            else if (_playerData != null)
            {
                coins = _playerData.CoinSystem.CloudCoins;
            }
            
            _coinsText.text = string.Format(_coinsFormat, coins);
        }

        private void OnEnable()
        {
            if (_playerData != null)
            {
                RefreshAllCards();
                UpdateCoinsDisplay();
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            // Load backend data when shop opens
            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.LoadPlayerData((success, data) =>
                {
                    if (success)
                    {
                        Debug.Log($"[ModernShop] Loaded player data: {data.CloudCoins} coins");
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
    }
}
