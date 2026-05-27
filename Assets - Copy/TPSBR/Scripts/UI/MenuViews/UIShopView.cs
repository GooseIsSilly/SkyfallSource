using System;
using TMPro;
using UnityEngine;

namespace TPSBR.UI
{
	public class UIShopView : UICloseView
	{
		[Header("Shop Configuration")]
		[SerializeField]
		private ShopDatabase _shopDatabase;

		[Header("UI References")]
		[SerializeField]
		private UIShopList _shopItemsList;
		[SerializeField]
		private TextMeshProUGUI _cloudCoinsText;

		[Header("Display Settings")]
		[SerializeField]
		private string _cloudCoinsFormat = "CloudCoins: {0}";

		[Header("Audio")]
		[SerializeField]
		private AudioSetup _purchaseSound;
		[SerializeField]
		private AudioSetup _insufficientFundsSound;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			if (_shopItemsList != null)
			{
				_shopItemsList.UpdateContent += OnListUpdateContent;
			}

			if (_shopDatabase == null)
			{
				Debug.LogError("UIShopView: ShopDatabase is not assigned! Please assign it in the inspector.", this);
			}
		}

		private void OnListUpdateContent(int index, MonoBehaviour content)
		{
			if (_shopDatabase == null || index >= _shopDatabase.characters.Count)
				return;

			var shopItem = content as UIShopItem;
			var characterData = _shopDatabase.characters[index];

			shopItem.SetData(characterData, Context.PlayerData, OnPurchaseClicked);
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			if (_shopDatabase != null && _shopItemsList != null)
			{
				_shopItemsList.Refresh(_shopDatabase.characters.Count, false);
			}
			else if (_shopDatabase == null)
			{
				Debug.LogError("UIShopView: Cannot open shop - ShopDatabase is not assigned!", this);
			}

			UpdateCloudCoinsDisplay();

			Context.PlayerData.CoinSystem.OnCloudCoinsChanged += OnCloudCoinsChanged;
		}

		protected override void OnClose()
		{
			Context.PlayerData.CoinSystem.OnCloudCoinsChanged -= OnCloudCoinsChanged;

			base.OnClose();
		}

		protected override void OnDeinitialize()
		{
			if (_shopItemsList != null)
			{
				_shopItemsList.UpdateContent -= OnListUpdateContent;
			}

			base.OnDeinitialize();
		}

		private void OnPurchaseClicked(CharacterData characterData)
		{
			if (characterData == null)
				return;

			bool success = Context.PlayerData.ShopSystem.TryUnlockAgent(
				characterData.characterID,
				characterData.price,
				Context.PlayerData.CoinSystem
			);

			if (success)
			{
				Context.PlayerData.AgentID = characterData.agentID;
				
				PlaySound(_purchaseSound);
				RefreshShopItems();
				UpdateCloudCoinsDisplay();
			}
			else
			{
				PlaySound(_insufficientFundsSound);
			}
		}

		private void OnCloudCoinsChanged(int newAmount)
		{
			UpdateCloudCoinsDisplay();
			RefreshShopItems();
		}

		private void UpdateCloudCoinsDisplay()
		{
			if (_cloudCoinsText != null)
			{
				_cloudCoinsText.text = string.Format(_cloudCoinsFormat, Context.PlayerData.CoinSystem.CloudCoins);
			}
		}

		private void RefreshShopItems()
		{
			if (_shopDatabase == null || _shopItemsList == null)
				return;

			foreach (Transform child in _shopItemsList.transform)
			{
				var shopItem = child.GetComponent<UIShopItem>();
				if (shopItem != null)
				{
					shopItem.RefreshState();
				}
			}
		}
	}
}
