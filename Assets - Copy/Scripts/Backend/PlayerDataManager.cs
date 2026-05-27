using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace TPSBR.Backend
{
    [Serializable]
    public class PlayerGameData
    {
        public int Level;
        public int XP;
        public int CloudCoins;
        public string[] OwnedSkins;
        public string EquippedSkin;
        public bool HasBattlePass;
        public int BattlePassTier;
        public int BattlePassXP;
    }

    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        [Header("Current Player Data")]
        [SerializeField] private PlayerGameData currentData;

        [Header("Character Data References")]
        [Tooltip("All available characters in the game")]
        [SerializeField] private CharacterData[] allCharacters;

        public PlayerGameData CurrentData => currentData;

        public event Action<PlayerGameData> OnDataLoaded;
        public event Action<int> OnLevelChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<CharacterData> OnSkinUnlocked;
        public event Action OnBattlePassPurchased;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Auto-load all CharacterData assets if not assigned
                if (allCharacters == null || allCharacters.Length == 0)
                {
                    LoadAllCharacters();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadAllCharacters()
        {
            // Load all CharacterData assets from Resources or entire project
            allCharacters = Resources.LoadAll<CharacterData>("CharacterData");
            
            if (allCharacters.Length == 0)
            {
                Debug.LogWarning("[PlayerDataManager] No CharacterData found in Resources/CharacterData. Loading from entire project...");
                #if UNITY_EDITOR
                allCharacters = UnityEditor.AssetDatabase.FindAssets("t:CharacterData")
                    .Select(guid => UnityEditor.AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path))
                    .Where(cd => cd != null)
                    .ToArray();
                #endif
            }
            
            Debug.Log($"[PlayerDataManager] Loaded {allCharacters.Length} characters");
        }

        public void LoadPlayerData(Action<bool, PlayerGameData> callback)
        {
            if (BackendServiceManager.Instance == null)
            {
                Debug.LogError("[PlayerDataManager] BackendServiceManager not found!");
                callback?.Invoke(false, null);
                return;
            }

            StartCoroutine(BackendServiceManager.Instance.GetPlayerGameData((success, data) =>
            {
                if (success)
                {
                    currentData = data;
                    
                    // Auto-unlock default characters on first login
                    UnlockDefaultCharacters();
                    
                    OnDataLoaded?.Invoke(currentData);
                    Debug.Log($"[PlayerDataManager] Loaded data: Level {data.Level}, {data.CloudCoins} coins, {data.OwnedSkins.Length} skins");
                }
                else
                {
                    Debug.LogWarning("[PlayerDataManager] Failed to load player data");
                }

                callback?.Invoke(success, data);
            }));
        }

        private void UnlockDefaultCharacters()
        {
            if (allCharacters == null || currentData == null)
                return;

            bool needsSave = false;

            foreach (var character in allCharacters)
            {
                if (character != null && character.unlockedByDefault && !HasCharacter(character.characterID))
                {
                    Debug.Log($"[PlayerDataManager] Auto-unlocking default character: {character.displayName}");
                    
                    var newSkins = new List<string>(currentData.OwnedSkins) { character.characterID };
                    currentData.OwnedSkins = newSkins.ToArray();
                    needsSave = true;
                }
            }

            // Save if we unlocked any defaults
            if (needsSave)
            {
                SaveAllData();
            }
        }

        public void SaveAllData(Action<bool> callback = null)
        {
            if (BackendServiceManager.Instance == null || currentData == null)
            {
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(BackendServiceManager.Instance.UpdatePlayerGameData(currentData, (success) =>
            {
                if (success)
                {
                    Debug.Log("[PlayerDataManager] Data saved successfully");
                }
                else
                {
                    Debug.LogWarning("[PlayerDataManager] Failed to save data");
                }

                callback?.Invoke(success);
            }));
        }

        public void AddCoins(int amount, Action<bool, int> callback = null)
        {
            if (BackendServiceManager.Instance == null)
            {
                callback?.Invoke(false, 0);
                return;
            }

            StartCoroutine(BackendServiceManager.Instance.AddCoins(amount, (success, newTotal) =>
            {
                if (success)
                {
                    if (currentData != null)
                    {
                        currentData.CloudCoins = newTotal;
                    }
                    OnCoinsChanged?.Invoke(newTotal);
                    Debug.Log($"[PlayerDataManager] Added {amount} coins. New total: {newTotal}");
                }

                callback?.Invoke(success, newTotal);
            }));
        }

        public void AddXP(int amount, Action<bool, int, int> callback = null)
        {
            if (BackendServiceManager.Instance == null)
            {
                callback?.Invoke(false, 0, 0);
                return;
            }

            StartCoroutine(BackendServiceManager.Instance.AddXP(amount, (success, level, xp) =>
            {
                if (success)
                {
                    if (currentData != null)
                    {
                        int oldLevel = currentData.Level;
                        currentData.Level = level;
                        currentData.XP = xp;

                        if (level > oldLevel)
                        {
                            OnLevelChanged?.Invoke(level);
                            Debug.Log($"[PlayerDataManager] LEVEL UP! Now level {level}");
                        }
                    }
                }

                callback?.Invoke(success, level, xp);
            }));
        }

        public void UnlockCharacter(CharacterData character, Action<bool, string> callback = null)
        {
            if (character == null)
            {
                callback?.Invoke(false, "Invalid character");
                return;
            }

            UnlockCharacterByID(character.characterID, (success, message) =>
            {
                if (success)
                {
                    OnSkinUnlocked?.Invoke(character);
                }
                callback?.Invoke(success, message);
            });
        }

        public void UnlockCharacterByID(string characterID, Action<bool, string> callback = null)
        {
            if (BackendServiceManager.Instance == null)
            {
                callback?.Invoke(false, "Backend not available");
                return;
            }

            StartCoroutine(BackendServiceManager.Instance.UnlockSkin(characterID, (success, message) =>
            {
                if (success)
                {
                    if (currentData != null && !HasCharacter(characterID))
                    {
                        var newSkins = new List<string>(currentData.OwnedSkins) { characterID };
                        currentData.OwnedSkins = newSkins.ToArray();
                    }

                    Debug.Log($"[PlayerDataManager] Unlocked character: {characterID}");
                }

                callback?.Invoke(success, message);
            }));
        }

        public bool HasCharacter(string characterID)
        {
            if (currentData == null || currentData.OwnedSkins == null)
                return false;

            return Array.Exists(currentData.OwnedSkins, s => s == characterID);
        }

        public bool HasCharacter(CharacterData character)
        {
            if (character == null)
                return false;

            return HasCharacter(character.characterID);
        }

        public void SetEquippedCharacter(CharacterData character)
        {
            if (character != null && currentData != null)
            {
                currentData.EquippedSkin = character.characterID;
                Debug.Log($"[PlayerDataManager] Equipped character: {character.displayName}");
            }
        }

        public void SetEquippedCharacterByID(string characterID)
        {
            if (currentData != null)
            {
                currentData.EquippedSkin = characterID;
            }
        }

        public CharacterData GetEquippedCharacter()
        {
            if (currentData == null || string.IsNullOrEmpty(currentData.EquippedSkin))
                return null;

            return GetCharacterByID(currentData.EquippedSkin);
        }

        public CharacterData GetCharacterByID(string characterID)
        {
            if (allCharacters == null || allCharacters.Length == 0)
                return null;

            return Array.Find(allCharacters, c => c != null && c.characterID == characterID);
        }

        public CharacterData[] GetAllCharacters()
        {
            return allCharacters;
        }

        public CharacterData[] GetOwnedCharacters()
        {
            if (currentData == null || currentData.OwnedSkins == null || allCharacters == null)
                return new CharacterData[0];

            return allCharacters
                .Where(c => c != null && HasCharacter(c.characterID))
                .ToArray();
        }

        public CharacterData[] GetLockedCharacters()
        {
            if (currentData == null || currentData.OwnedSkins == null || allCharacters == null)
                return allCharacters;

            return allCharacters
                .Where(c => c != null && !HasCharacter(c.characterID))
                .ToArray();
        }

        public bool CanAfford(CharacterData character)
        {
            if (character == null || currentData == null)
                return false;

            return currentData.CloudCoins >= character.price;
        }

        public void PurchaseCharacter(CharacterData character, Action<bool, string> callback = null)
        {
            if (character == null)
            {
                callback?.Invoke(false, "Invalid character");
                return;
            }

            if (HasCharacter(character))
            {
                callback?.Invoke(false, "Already owned");
                return;
            }

            if (!CanAfford(character))
            {
                callback?.Invoke(false, "Not enough CloudCoins");
                return;
            }

            // Deduct coins
            AddCoins(-character.price, (success, newTotal) =>
            {
                if (success)
                {
                    // Unlock character
                    UnlockCharacter(character, (unlocked, message) =>
                    {
                        if (unlocked)
                        {
                            Debug.Log($"[PlayerDataManager] Purchased {character.displayName} for {character.price} coins!");
                            callback?.Invoke(true, $"Purchased {character.displayName}!");
                        }
                        else
                        {
                            // Refund coins if unlock failed
                            AddCoins(character.price);
                            callback?.Invoke(false, "Purchase failed");
                        }
                    });
                }
                else
                {
                    callback?.Invoke(false, "Failed to deduct coins");
                }
            });
        }

        /// <summary>Purchases the battle pass for 950 CloudCoins, marks it in the backend, and fires OnBattlePassPurchased.</summary>
        public void PurchaseBattlePass(Action<bool, string> callback = null)
        {
            const int battlePassPrice = 950;

            if (currentData == null)
            {
                callback?.Invoke(false, "Player data not loaded");
                return;
            }

            if (currentData.HasBattlePass)
            {
                callback?.Invoke(false, "Battle Pass already owned");
                return;
            }

            if (currentData.CloudCoins < battlePassPrice)
            {
                callback?.Invoke(false, $"Not enough CloudCoins (need {battlePassPrice})");
                return;
            }

            // Delegate the purchase entirely to the server so coins and pass status stay in sync.
            BackendServiceManager.Instance.PurchaseBattlePassFromServer((success, message) =>
            {
                if (success)
                {
                    currentData.HasBattlePass = true;
                    currentData.CloudCoins -= battlePassPrice;
                    OnBattlePassPurchased?.Invoke();
                    Debug.Log("[PlayerDataManager] Battle Pass purchased.");
                }
                else
                {
                    Debug.LogWarning($"[PlayerDataManager] Battle Pass purchase failed: {message}");
                }
                callback?.Invoke(success, message);
            });
        }

        /// <summary>Awards battle pass XP and tiers up via the server. Fires OnBattlePassTierUp when the tier increases.</summary>
        public void AddBattlePassXP(int amount, Action<bool, int, int> callback = null)
        {
            if (BackendServiceManager.Instance == null)
            {
                callback?.Invoke(false, 0, 0);
                return;
            }

            BackendServiceManager.Instance.AddBattlePassXP(amount, (success, newTier, newXP) =>
            {
                if (success && currentData != null)
                {
                    int oldTier = currentData.BattlePassTier;
                    currentData.BattlePassTier = newTier;
                    currentData.BattlePassXP = newXP;

                    if (newTier > oldTier)
                    {
                        Debug.Log($"[PlayerDataManager] Battle Pass tier up! Now tier {newTier}.");
                    }
                }
                callback?.Invoke(success, newTier, newXP);
            });
        }

        /// <summary>Fetches fresh battle pass status from the server and updates local data.</summary>
        public void LoadBattlePassStatus(Action<bool, bool, int, int> callback = null)
        {
            if (BackendServiceManager.Instance == null)
            {
                callback?.Invoke(false, false, 0, 0);
                return;
            }

            BackendServiceManager.Instance.GetBattlePassStatus((success, data) =>
            {
                if (success && data != null && currentData != null)
                {
                    currentData.HasBattlePass = data.HasBattlePass;
                    currentData.BattlePassTier = data.CurrentTier;
                    currentData.BattlePassXP = data.BattlePassXP;
                    Debug.Log($"[PlayerDataManager] BP status: hasPass={data.HasBattlePass}, tier={data.CurrentTier}, xp={data.BattlePassXP}");
                }
                callback?.Invoke(success, data?.HasBattlePass ?? false, data?.CurrentTier ?? 0, data?.BattlePassXP ?? 0);
            });
        }
    }
}
