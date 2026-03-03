using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
	public class UIShopItem : UIListItemBase<MonoBehaviour>
	{
		[Header("UI References")]
		[SerializeField]
		private Image _agentIcon;
		[SerializeField]
		private TextMeshProUGUI _agentName;
		[SerializeField]
		private TextMeshProUGUI _costText;
		[SerializeField]
		private UIButton _purchaseButton;
		[SerializeField]
		private TextMeshProUGUI _purchaseButtonText;
		[SerializeField]
		private GameObject _ownedIndicator;

		[Header("Display Settings")]
		[SerializeField]
		private string _costFormat = "{0} CloudCoins";
		[SerializeField]
		private string _freeText = "FREE";
		[SerializeField]
		private string _purchaseText = "BUY";
		[SerializeField]
		private string _ownedText = "OWNED";
		[SerializeField]
		private string _selectedText = "SELECTED";
		[SerializeField]
		private Color _canAffordColor = Color.white;
		[SerializeField]
		private Color _cannotAffordColor = Color.red;

		private CharacterData _characterData;
		private PlayerData _playerData;
		private Action<CharacterData> _onPurchaseCallback;

		private void Awake()
		{
			if (_purchaseButton != null)
				_purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
		}

		private void OnDestroy()
		{
			if (_purchaseButton != null)
				_purchaseButton.onClick.RemoveListener(OnPurchaseButtonClicked);
		}

		public void SetData(CharacterData characterData, PlayerData playerData, Action<CharacterData> onPurchaseCallback)
		{
			_characterData = characterData;
			_playerData = playerData;
			_onPurchaseCallback = onPurchaseCallback;

			if (_agentIcon != null)
				_agentIcon.sprite = characterData.icon;

			if (_agentName != null)
				_agentName.text = characterData.displayName;

			if (_costText != null)
			{
				if (characterData.price == 0)
					_costText.text = _freeText;
				else
					_costText.text = string.Format(_costFormat, characterData.price);
			}

			UpdateButtonState();
		}

		private void OnPurchaseButtonClicked()
		{
			if (_characterData == null || _playerData == null)
				return;

			bool isOwned = _playerData.ShopSystem.OwnsAgent(_characterData.characterID);
			if (isOwned)
			{
				_playerData.AgentID = _characterData.agentID;
				UpdateButtonState();
			}
			else
			{
				_onPurchaseCallback?.Invoke(_characterData);
			}
		}

		private void UpdateButtonState()
		{
			if (_characterData == null || _playerData == null)
				return;

			bool isOwned = _playerData.ShopSystem.OwnsAgent(_characterData.characterID);
			bool isSelected = _playerData.AgentID == _characterData.agentID;
			bool canAfford = _playerData.CoinSystem.CanAfford(_characterData.price);

			if (_ownedIndicator != null)
				_ownedIndicator.SetActive(isOwned);

			if (_purchaseButton != null)
			{
				_purchaseButton.interactable = isOwned || canAfford;
			}

			if (_purchaseButtonText != null)
			{
				if (isSelected)
					_purchaseButtonText.text = _selectedText;
				else if (isOwned)
					_purchaseButtonText.text = _ownedText;
				else
					_purchaseButtonText.text = _purchaseText;
			}

			if (_costText != null)
			{
				_costText.color = (isOwned || canAfford) ? _canAffordColor : _cannotAffordColor;
			}
		}

		public void RefreshState()
		{
			UpdateButtonState();
		}
	}
}
