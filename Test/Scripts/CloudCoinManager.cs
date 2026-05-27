using UnityEngine;

namespace TPSBR
{
    public class CloudCoinManager : MonoBehaviour
    {
        private static CloudCoinManager _instance;
        public static CloudCoinManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CloudCoinManager");
                    _instance = go.AddComponent<CloudCoinManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public CloudCoinSystem CoinSystem { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeCoinSystem();
        }

        private void InitializeCoinSystem()
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData != null)
            {
                CoinSystem = playerData.CoinSystem;
            }
            else
            {
                CoinSystem = new CloudCoinSystem();
            }
        }

        private void Update()
        {
            if (CoinSystem == null)
            {
                InitializeCoinSystem();
            }
        }

        public void AddCoins(int amount)
        {
            if (CoinSystem != null)
            {
                CoinSystem.AddCoins(amount);
            }
        }

        public bool TryPurchase(int cost)
        {
            if (CoinSystem != null)
            {
                return CoinSystem.TryPurchase(cost);
            }
            return false;
        }

        public int GetCurrentCoins()
        {
            return CoinSystem?.CloudCoins ?? 0;
        }
    }
}
