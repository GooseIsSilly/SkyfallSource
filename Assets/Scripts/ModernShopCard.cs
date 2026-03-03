using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using TPSBR.Backend;

namespace TPSBR
{
    public class ModernShopCard : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image _characterIcon;
        [SerializeField] private Image _rarityBorder;
        [SerializeField] private Image _rarityGlow;
        [SerializeField] private TextMeshProUGUI _characterName;
        [SerializeField] private TextMeshProUGUI _rarityText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _actionButton;
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [SerializeField] private GameObject _ownedBadge;

        [Header("Visual Settings")]
        [SerializeField] private float _glowIntensity = 0.5f;
        
        [Header("Backend Sync")]
        [SerializeField] private bool _useBackendSync = true;

        private CharacterData _character;
        private PlayerData _playerData;
        private Action<CharacterData> _onPurchase;

        public void Setup(CharacterData character, PlayerData playerData, Action<CharacterData> onPurchase)
        {
            _character = character;
            _playerData = playerData;
            _onPurchase = onPurchase;

            if (_characterIcon != null && character.icon != null)
            {
                _characterIcon.sprite = character.icon;
            }

            if (_characterName != null)
            {
                _characterName.text = character.displayName;
            }

            if (_rarityText != null)
            {
                _rarityText.text = character.GetRarityText();
                _rarityText.color = character.GetRarityColor();
            }

            Color rarityColor = character.GetRarityColor();
            
            if (_rarityBorder != null)
            {
                _rarityBorder.color = rarityColor;
            }

            if (_rarityGlow != null)
            {
                Color glowColor = rarityColor;
                glowColor.a = _glowIntensity;
                _rarityGlow.color = glowColor;
            }

            if (_actionButton != null)
            {
                _actionButton.onClick.RemoveAllListeners();
                _actionButton.onClick.AddListener(OnActionButtonClicked);
            }

            Refresh(playerData);
        }

        public void Refresh(PlayerData playerData)
        {
            if (_character == null || playerData == null) return;

            _playerData = playerData;

            // Check ownership from backend if enabled
            bool isOwned = false;
            int currentCoins = 0;
            
            if (_useBackendSync && PlayerDataManager.Instance != null && PlayerDataManager.Instance.CurrentData != null)
            {
                isOwned = PlayerDataManager.Instance.HasCharacter(_character);
                currentCoins = PlayerDataManager.Instance.CurrentData.CloudCoins;
            }
            else
            {
                isOwned = playerData.ShopSystem.OwnsAgent(_character.agentID);
                currentCoins = playerData.CoinSystem.CloudCoins;
            }

            if (_ownedBadge != null)
            {
                _ownedBadge.SetActive(isOwned);
            }

            if (isOwned)
            {
                if (_actionButtonText != null)
                {
                    _actionButtonText.text = "OWNED";
                }

                if (_priceText != null)
                {
                    _priceText.gameObject.SetActive(false);
                }

                if (_actionButton != null)
                {
                    _actionButton.interactable = false;
                }
            }
            else
            {
                if (_actionButtonText != null)
                {
                    _actionButtonText.text = "BUY";
                }

                if (_priceText != null)
                {
                    _priceText.gameObject.SetActive(true);
                    _priceText.text = _character.price == 0 ? "FREE" : $"{_character.price}";
                }

                if (_actionButton != null)
                {
                    _actionButton.interactable = currentCoins >= _character.price;
                }
            }
        }

        private void OnActionButtonClicked()
        {
            if (_character == null || _playerData == null) return;

            // Check ownership from backend if enabled
            bool isOwned = false;
            
            if (_useBackendSync && PlayerDataManager.Instance != null)
            {
                isOwned = PlayerDataManager.Instance.HasCharacter(_character);
            }
            else
            {
                isOwned = _playerData.ShopSystem.OwnsAgent(_character.agentID);
            }

            if (!isOwned)
            {
                _onPurchase?.Invoke(_character);
            }
        }
    }
}
