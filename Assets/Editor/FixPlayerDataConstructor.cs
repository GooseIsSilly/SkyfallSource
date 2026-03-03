using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace TPSBR.Editor
{
    public class FixPlayerDataConstructor
    {
        [MenuItem("TPSBR/🔧 Fix Shop System Initialization")]
        public static void Fix()
        {
            FixPlayerData();
            FixPlayerService();
            
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Shop system initialization fixed!");
            Debug.Log("✓ PlayerData constructor fixed");
            Debug.Log("✓ PlayerService initialization fixed");
            Debug.Log("\n📝 Next: Run 'TPSBR → 🔧 Connect Shop Database to Settings'");
        }
        
        private static void FixPlayerData()
        {
            string path = "Assets/TPSBR/Scripts/Player/PlayerData.cs";
            
            if (!File.Exists(path))
            {
                Debug.LogError($"PlayerData.cs not found at {path}");
                return;
            }

            string content = File.ReadAllText(path);
            
            string badPattern = @"public PlayerData\(string userID\)\s*\{[\s\S]*?\n\t\t\}\s*\n\s*\/\/ PUBLIC METHODS";
            
            string goodReplacement = @"public PlayerData(string userID)
	{
		_userID = userID;
		
		var shopDatabase = Global.Settings != null ? Global.Settings.ShopDatabase : null;
		if (shopDatabase != null)
		{
			_shopSystem.InitializeWithDatabase(shopDatabase);
			
			if (_coinSystem.CloudCoins == 100)
			{
				_coinSystem.CloudCoins = shopDatabase.startingCloudCoins;
			}
		}
		else
		{
			_shopSystem.Initialize();
		}
	}

	// PUBLIC METHODS";

            content = Regex.Replace(content, badPattern, goodReplacement);
            
            File.WriteAllText(path, content);
            Debug.Log("✓ Fixed PlayerData.cs");
        }
        
        private static void FixPlayerService()
        {
            string path = "Assets/TPSBR/Scripts/Player/PlayerService.cs";
            
            if (!File.Exists(path))
            {
                Debug.LogError($"PlayerService.cs not found at {path}");
                return;
            }

            string content = File.ReadAllText(path);
            
            string badPattern = @"playerData\.ShopSystem\.Initialize\(\);[\s\S]*?if \(!playerData\.ShopSystem\.OwnsAgent\(playerData\.AgentID\)\)";
            
            string goodReplacement = @"var shopDatabase = Global.Settings != null ? Global.Settings.ShopDatabase : null;
			if (shopDatabase != null)
			{
				playerData.ShopSystem.InitializeWithDatabase(shopDatabase);
				
				if (playerData.CoinSystem.CloudCoins == 100)
				{
					playerData.CoinSystem.CloudCoins = shopDatabase.startingCloudCoins;
				}
			}
			else
			{
				playerData.ShopSystem.Initialize();
			}
			
			if (!playerData.ShopSystem.OwnsAgent(playerData.AgentID))";

            content = Regex.Replace(content, badPattern, goodReplacement);
            
            File.WriteAllText(path, content);
            Debug.Log("✓ Fixed PlayerService.cs");
        }
    }
}
