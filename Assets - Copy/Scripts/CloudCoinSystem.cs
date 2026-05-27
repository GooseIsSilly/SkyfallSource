using System;
using UnityEngine;

namespace TPSBR
{
    [Serializable]
    public class CloudCoinSystem
    {
        public int CloudCoins
        {
            get => _cloudCoins;
            set
            {
                _cloudCoins = Mathf.Max(0, value);
                IsDirty = true;
                OnCloudCoinsChanged?.Invoke(_cloudCoins);
            }
        }

        public bool IsDirty { get; private set; }

        public event Action<int> OnCloudCoinsChanged;

        [SerializeField]
        private int _cloudCoins = 0;

        public bool CanAfford(int cost)
        {
            return _cloudCoins >= cost;
        }

        public bool TryPurchase(int cost)
        {
            if (!CanAfford(cost))
                return false;

            CloudCoins -= cost;
            return true;
        }

        public void AddCoins(int amount)
        {
            CloudCoins += amount;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }
    }
}
