using UnityEngine;
using UnityEditor;

namespace TPSBR.Editor
{
    public class AssignShopDatabase
    {
        [MenuItem("TPSBR/🔧 Connect Shop Database to Settings")]
        public static void ConnectDatabase()
        {
            string[] guids = AssetDatabase.FindAssets("t:ShopDatabase");
            if (guids.Length == 0)
            {
                Debug.LogError("No ShopDatabase found in project!");
                return;
            }

            string dbPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            ShopDatabase shopDatabase = AssetDatabase.LoadAssetAtPath<ShopDatabase>(dbPath);

            if (shopDatabase == null)
            {
                Debug.LogError($"Failed to load ShopDatabase at: {dbPath}");
                return;
            }

            string[] settingsGuids = AssetDatabase.FindAssets("t:GlobalSettings");
            if (settingsGuids.Length == 0)
            {
                Debug.LogError("No GlobalSettings found in project!");
                return;
            }

            string settingsPath = AssetDatabase.GUIDToAssetPath(settingsGuids[0]);
            GlobalSettings settings = AssetDatabase.LoadAssetAtPath<GlobalSettings>(settingsPath);

            if (settings == null)
            {
                Debug.LogError($"Failed to load GlobalSettings at: {settingsPath}");
                return;
            }

            settings.ShopDatabase = shopDatabase;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log("✅ Shop Database connected to Global Settings!");
            Debug.Log($"✓ Database: {dbPath}");
            Debug.Log($"✓ Settings: {settingsPath}");
            Debug.Log($"✓ Characters in database: {shopDatabase.characters.Count}");
            
            var defaultUnlocked = shopDatabase.GetDefaultUnlockedCharacters();
            Debug.Log($"✓ Default unlocked characters: {defaultUnlocked.Count}");
            
            foreach (var character in defaultUnlocked)
            {
                Debug.Log($"  • {character.displayName} ({character.agentID})");
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
