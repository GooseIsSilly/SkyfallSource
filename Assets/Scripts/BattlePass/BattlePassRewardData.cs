using UnityEngine;

namespace TPSBR
{
    public enum RewardType
    {
        Skin,
        CloudCoins,
        Emote,
        Spray,
        XPBoost
    }

    public enum Rarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    [CreateAssetMenu(fileName = "BattlePassRewardData", menuName = "TPSBR/Battle Pass/Reward Data", order = 1)]
    public class BattlePassRewardData : ScriptableObject
    {
        [Header("Identity")]
        /// <summary>Unique string key for this reward.</summary>
        public string RewardID;

        /// <summary>Display name shown in the UI.</summary>
        public string DisplayName;

        [TextArea(2, 4)]
        /// <summary>Description shown in the reward detail panel.</summary>
        public string Description;

        [Header("Visuals")]
        /// <summary>Icon displayed in the tier card and detail panel.</summary>
        public Sprite Icon;

        [Header("Reward Settings")]
        /// <summary>Category of this reward.</summary>
        public RewardType Type;

        /// <summary>Amount of CloudCoins awarded when Type == CloudCoins.</summary>
        public int CloudCoinAmount;

        /// <summary>Visual rarity classification of this reward.</summary>
        public Rarity Rarity;

        /// <summary>False = free row (all players); true = premium row (Battle Pass owners only).</summary>
        public bool IsPremium;

        /// <summary>Returns the color associated with this reward's rarity.</summary>
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                Rarity.Common    => new Color(0.7f, 0.7f, 0.7f),
                Rarity.Rare      => new Color(0.2f, 0.5f, 1f),
                Rarity.Epic      => new Color(0.6f, 0.2f, 0.9f),
                Rarity.Legendary => new Color(1f, 0.6f, 0f),
                _                => Color.white
            };
        }

        /// <summary>Returns the display string for this reward's rarity.</summary>
        public string GetRarityText() => Rarity.ToString().ToUpper();
    }
}
