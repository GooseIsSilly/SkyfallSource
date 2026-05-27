using UnityEngine;

namespace TPSBR
{
    public enum SkinRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
        Mythic = 4
    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "TPSBR/Character Data", order = 0)]
    public class CharacterData : ScriptableObject
    {
        [Header("Character Identity")]
        [Tooltip("Unique identifier for this character (e.g., 'soldier', 'marine')")]
        public string characterID = "";
        
        [Tooltip("Display name shown in the shop UI")]
        public string displayName = "Character Name";
        
        [TextArea(2, 4)]
        [Tooltip("Optional description of the character")]
        public string description = "";

        [Header("Shop Settings")]
        [Tooltip("Character icon displayed in the shop")]
        public Sprite icon;
        
        [Tooltip("Rarity of this skin")]
        public SkinRarity rarity = SkinRarity.Common;
        
        [Tooltip("Price in CloudCoins (0 = free/default character)")]
        public int price = 0;
        
        [Tooltip("Is this character unlocked by default?")]
        public bool unlockedByDefault = false;

        [Header("Character Model")]
        [Tooltip("The agent prefab ID that matches AgentSettings")]
        public string agentID = "";

        public Color GetRarityColor()
        {
            return rarity switch
            {
                SkinRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                SkinRarity.Rare => new Color(0.2f, 0.5f, 1f),
                SkinRarity.Epic => new Color(0.6f, 0.2f, 0.9f),
                SkinRarity.Legendary => new Color(1f, 0.6f, 0f),
                SkinRarity.Mythic => new Color(1f, 0.2f, 0.3f),
                _ => Color.white
            };
        }

        public string GetRarityText()
        {
            return rarity.ToString().ToUpper();
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(characterID))
            {
                Debug.LogWarning($"CharacterData '{name}' has no Character ID set!", this);
            }
            
            if (icon == null)
            {
                Debug.LogWarning($"CharacterData '{characterID}' has no icon assigned!", this);
            }
        }
    }
}
