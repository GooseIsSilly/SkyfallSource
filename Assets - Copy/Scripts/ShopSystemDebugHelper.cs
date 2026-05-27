using UnityEngine;

namespace TPSBR
{
    public class ShopSystemDebugHelper : MonoBehaviour
    {
        [Header("Debug Actions")]
        [SerializeField]
        private bool _logCurrentCoins = false;
        [SerializeField]
        private bool _logOwnedAgents = false;
        [SerializeField]
        private bool _resetShopData = false;

        private void Update()
        {
            if (_logCurrentCoins)
            {
                _logCurrentCoins = false;
                LogCurrentCoins();
            }

            if (_logOwnedAgents)
            {
                _logOwnedAgents = false;
                LogOwnedAgents();
            }

            if (_resetShopData)
            {
                _resetShopData = false;
                ResetShopData();
            }
        }

        [ContextMenu("Log Current CloudCoins")]
        public void LogCurrentCoins()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                Debug.Log($"Current CloudCoins: {playerData.CoinSystem.CloudCoins}");
            }
            else
            {
                Debug.LogWarning("PlayerData not available yet");
            }
        }

        [ContextMenu("Log Owned Agents")]
        public void LogOwnedAgents()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                string ownedAgents = "Owned Agents: ";
                foreach (var agentID in playerData.ShopSystem.OwnedSkins)
                {
                    ownedAgents += agentID + ", ";
                }
                Debug.Log(ownedAgents);
                Debug.Log($"Currently Selected: {playerData.AgentID}");
            }
            else
            {
                Debug.LogWarning("PlayerData not available yet");
            }
        }

        [ContextMenu("Add 100 CloudCoins")]
        public void Add100Coins()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                playerData.CoinSystem.AddCoins(100);
                Debug.Log($"Added 100 CloudCoins. New total: {playerData.CoinSystem.CloudCoins}");
            }
        }

        [ContextMenu("Add 500 CloudCoins")]
        public void Add500Coins()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                playerData.CoinSystem.AddCoins(500);
                Debug.Log($"Added 500 CloudCoins. New total: {playerData.CoinSystem.CloudCoins}");
            }
        }

        [ContextMenu("Reset Shop Data (Clear All Owned Skins)")]
        public void ResetShopData()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                playerData.ShopSystem.OwnedSkins.Clear();
                playerData.ShopSystem.Initialize();
                Debug.Log("Shop data reset. Only Soldier skin is owned now.");
            }
        }
    }
}
