using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TPSBR.Backend;

namespace TPSBR.UI
{
    public class UIShopModernView : UICloseView
    {
        [Header("References")]
        [SerializeField] private UIShopList _dailyList;
        [SerializeField] private UIShopList _weeklyList;
        [SerializeField] private TextMeshProUGUI _dailyTimer;
        [SerializeField] private TextMeshProUGUI _weeklyTimer;
        [SerializeField] private UIButton _dailyTabButton;
        [SerializeField] private UIButton _weeklyTabButton;
        [SerializeField] private TextMeshProUGUI _cloudCoinsText;

        [Header("Shop Database")]
        [SerializeField] private ShopDatabase _shopDatabase;

        private DateTime _dailyResetTime;
        private DateTime _weeklyResetTime;
        private bool _isWeeklyActive;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            _dailyTabButton.onClick.AddListener(() => SetTab(false));
            _weeklyTabButton.onClick.AddListener(() => SetTab(true));
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            RefreshShop();
            SetTab(false);
            UpdateCoins();
        }

        private void SetTab(bool weekly)
        {
            _isWeeklyActive = weekly;
            _dailyList.gameObject.SetActive(!weekly);
            _weeklyList.gameObject.SetActive(weekly);
            
            // Visual feedback for tabs could be added here
        }

        private void RefreshShop()
        {
            BackendServiceManager.Instance.GetShopCatalog((success, response) =>
            {
                if (success)
                {
                    PopulateList(_dailyList, response.daily);
                    PopulateList(_weeklyList, response.weekly);
                    
                    DateTime.TryParse(response.daily_reset, out _dailyResetTime);
                    DateTime.TryParse(response.weekly_reset, out _weeklyResetTime);
                }
            });
        }

        private void PopulateList(UIShopList list, List<BackendServiceManager.ShopItemData> items)
        {
            // Simple mapping from backend IDs to ScriptableObject data
            var characters = new List<CharacterData>();
            foreach (var item in items)
            {
                var data = _shopDatabase.GetCharacter(item.id);
                if (data != null)
                {
                    characters.Add(data);
                }
            }
            
            list.Refresh(characters.Count, false);
            // The UIShopList uses UpdateContent event which needs to be hooked up
        }

        private void Update()
        {
            if (_dailyTimer != null && _dailyResetTime != default)
            {
                var span = _dailyResetTime - DateTime.UtcNow;
                _dailyTimer.text = span.TotalSeconds > 0 ? $"Daily Reset: {span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}" : "Refreshing...";
            }

            if (_weeklyTimer != null && _weeklyResetTime != default)
            {
                var span = _weeklyResetTime - DateTime.UtcNow;
                _weeklyTimer.text = span.TotalSeconds > 0 ? $"Weekly Reset: {span.Days}d {span.Hours:D2}h" : "Refreshing...";
            }
        }

        private void UpdateCoins()
        {
            if (_cloudCoinsText != null)
                _cloudCoinsText.text = Context.PlayerData.CoinSystem.CloudCoins.ToString();
        }
    }
}
