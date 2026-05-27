using System.Collections.Generic;
using UnityEngine;

namespace TPSBR
{
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "TPSBR/Shop Database", order = 1)]
    public class ShopDatabase : ScriptableObject
    {
        [Header("Shop Configuration")]
        [Tooltip("All available characters in the shop")]
        public List<CharacterData> characters = new List<CharacterData>();
        
        [Header("Starting Currency")]
        [Tooltip("How many CloudCoins players start with")]
        public int startingCloudCoins = 100;

        public CharacterData GetCharacter(string characterID)
        {
            return characters.Find(c => c.characterID == characterID);
        }

        public List<CharacterData> GetDefaultUnlockedCharacters()
        {
            return characters.FindAll(c => c.unlockedByDefault);
        }

        public void ValidateDatabase()
        {
            HashSet<string> ids = new HashSet<string>();
            
            foreach (var character in characters)
            {
                if (character == null)
                {
                    Debug.LogWarning("ShopDatabase contains a null character reference!", this);
                    continue;
                }
                
                character.Validate();
                
                if (!string.IsNullOrEmpty(character.characterID))
                {
                    if (ids.Contains(character.characterID))
                    {
                        Debug.LogError($"Duplicate character ID '{character.characterID}' found in ShopDatabase!", this);
                    }
                    else
                    {
                        ids.Add(character.characterID);
                    }
                }
            }
            
            var defaultUnlocked = GetDefaultUnlockedCharacters();
            if (defaultUnlocked.Count == 0)
            {
                Debug.LogWarning("ShopDatabase has no default unlocked characters! Players won't have any character to start with.", this);
            }
        }

        private void OnValidate()
        {
            ValidateDatabase();
        }
    }
}
