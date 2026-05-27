using System;
using System.Collections.Generic;
using UnityEngine;

namespace TPSBR
{
    [Serializable]
    public class BattlePassTierEntry
    {
        /// <summary>1-based tier index.</summary>
        public int Tier;

        /// <summary>Free reward for this tier. May be null if no free reward is defined.</summary>
        public BattlePassRewardData FreeReward;

        /// <summary>Premium reward for this tier. May be null if no premium reward is defined.</summary>
        public BattlePassRewardData PremiumReward;
    }

    [CreateAssetMenu(fileName = "BattlePassSeasonData", menuName = "TPSBR/Battle Pass/Season Data", order = 0)]
    public class BattlePassSeasonData : ScriptableObject
    {
        [Header("Season Info")]
        /// <summary>Human-readable season label, e.g. "Season 9".</summary>
        public string SeasonName = "Season 1";

        /// <summary>ISO-8601 end date string. Parse with DateTime and DateTimeStyles.RoundtripKind.</summary>
        public string SeasonEndDateISO = "2025-12-31T00:00:00Z";

        [Header("Progression")]
        /// <summary>Total number of tiers in this season (e.g. 100).</summary>
        public int TotalTiers = 100;

        /// <summary>XP required to advance one tier (e.g. 1000).</summary>
        public int XPPerTier = 1000;

        [Header("Pricing")]
        /// <summary>Cost to purchase the Battle Pass in CloudCoins.</summary>
        public int BattlePassCost = 950;

        /// <summary>Cost in CloudCoins to skip a single tier.</summary>
        public int TierSkipCost = 150;

        [Header("Tiers")]
        /// <summary>Per-tier entries. Index 0 = Tier 1. Length should match TotalTiers.</summary>
        public List<BattlePassTierEntry> Tiers = new List<BattlePassTierEntry>();

        /// <summary>Returns the tier entry for the given 1-based tier number, or null if out of range.</summary>
        public BattlePassTierEntry GetTierEntry(int tier)
        {
            int index = tier - 1;
            if (index < 0 || index >= Tiers.Count)
                return null;

            return Tiers[index];
        }
    }
}
