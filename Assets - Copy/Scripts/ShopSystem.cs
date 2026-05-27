using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSBR
{
    [Serializable]
    public class ShopSystem
    {
        public HashSet<string> OwnedSkins => _ownedSkins;
        public bool IsDirty { get; private set; }

        [SerializeField]
        private List<string> _ownedSkinsList = new List<string>();

        private HashSet<string> _ownedSkins = new HashSet<string>();

        public void Initialize()
        {
            _ownedSkins.Clear();
            foreach (var skin in _ownedSkinsList)
            {
                _ownedSkins.Add(skin);
            }

            if (_ownedSkins.Count == 0)
            {
                _ownedSkins.Add("Agent.Soldier");
                _ownedSkinsList.Add("Agent.Soldier");
                IsDirty = true;
            }
        }

        public void InitializeWithDatabase(ShopDatabase shopDatabase)
        {
            _ownedSkins.Clear();
            foreach (var skin in _ownedSkinsList)
            {
                _ownedSkins.Add(skin);
            }

            if (shopDatabase != null && _ownedSkins.Count == 0)
            {
                var defaultCharacters = shopDatabase.GetDefaultUnlockedCharacters();
                foreach (var character in defaultCharacters)
                {
                    _ownedSkins.Add(character.agentID);
                    _ownedSkinsList.Add(character.agentID);
                }
                
                if (_ownedSkins.Count > 0)
                {
                    IsDirty = true;
                }
                else
                {
                    Debug.LogWarning("ShopDatabase has no default unlocked characters! Unlocking fallback Agent.Soldier");
                    _ownedSkins.Add("Agent.Soldier");
                    _ownedSkinsList.Add("Agent.Soldier");
                    IsDirty = true;
                }
            }
        }

        public bool OwnsAgent(string agentID)
        {
            return _ownedSkins.Contains(agentID);
        }

        public bool TryUnlockAgent(string agentID, int cost, CloudCoinSystem coinSystem)
        {
            if (OwnsAgent(agentID))
                return false;

            if (!coinSystem.TryPurchase(cost))
                return false;

            _ownedSkins.Add(agentID);
            _ownedSkinsList.Add(agentID);
            IsDirty = true;
            return true;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }
    }
}
