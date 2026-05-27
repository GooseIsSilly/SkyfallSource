using UnityEngine;

namespace TPSBR
{
    public class CloudCoinReward : MonoBehaviour
    {
        public void AddCloudCoins(int amount)
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                playerData.CoinSystem.AddCoins(amount);
                Debug.Log($"Added {amount} CloudCoins. Total: {playerData.CoinSystem.CloudCoins}");
            }
        }

        public void AddCloudCoins100()
        {
            AddCloudCoins(100);
        }

        public void AddCloudCoins500()
        {
            AddCloudCoins(500);
        }

        public void AddCloudCoins1000()
        {
            AddCloudCoins(1000);
        }
    }
}
