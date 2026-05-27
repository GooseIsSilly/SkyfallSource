using UnityEngine;
using UnityEditor;

namespace TPSBREditor
{
    public class ClearPlayerData
    {
        [MenuItem("TPSBR/🗑️ Clear Player Data")]
        public static void ClearAllPlayerData()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Clear Player Data",
                "Are you sure you want to delete all player data?\n\n" +
                "This will reset:\n" +
                "• Owned agents/skins\n" +
                "• CloudCoins balance\n" +
                "• Selected agent\n" +
                "• All other player preferences\n\n" +
                "This action cannot be undone!",
                "Clear All Data",
                "Cancel"
            );

            if (!confirmed)
            {
                Debug.Log("Player data clear cancelled.");
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            Debug.Log("✅ Player data cleared successfully!");
            Debug.Log("✓ All PlayerPrefs deleted");
            Debug.Log("✓ Next play will start fresh with:");
            Debug.Log("  - Default agent (soldier66)");
            Debug.Log("  - Starting coins from ShopDatabase");
            Debug.Log("  - No purchased items");
            Debug.Log("\n▶ Press Play to test with fresh player data!");

            EditorUtility.DisplayDialog(
                "Player Data Cleared",
                "All player data has been cleared!\n\n" +
                "Press Play to start fresh with default settings.",
                "OK"
            );
        }
    }
}
