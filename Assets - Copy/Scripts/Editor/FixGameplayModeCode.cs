using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace TPSBREditor
{
    public class FixGameplayModeCode : EditorWindow
    {
        [MenuItem("TPSBR/Fix GameplayMode Quest Hooks")]
        public static void FixGameplayMode()
        {
            string filePath = "Assets/TPSBR/Scripts/Gameplay/GameplayModes/GameplayMode.cs";
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return;
            }
            
            string content = File.ReadAllText(filePath);
            
            content = content.Replace("\\t\\t\\t", "\t\t\t");
            content = content.Replace("\\t\\t\\t\\t", "\t\t\t\t");
            
            content = Regex.Replace(content, @"State = EState\.Active;\r\n\r\n\r\n.*QuestIntegrationPatches\.PatchGameplayModeActivated\(\);\r\n\s*OnActivate\(\);", 
                                  "State = EState.Active;\r\n\r\n\t\t\tOnActivate();\r\n\r\n\t\t\tQuestIntegrationPatches.PatchGameplayModeActivated();");
            
            content = Regex.Replace(content, @"if \(Application\.isBatchMode == true\)\r\n\s*\{\r\n\s*StartCoroutine\(ShutdownCoroutine\(\)\);\r\n\r\n.*?var localPlayer.*?\r\n.*?if \(localPlayer.*?\r\n.*?\{.*?\r\n.*?var stats.*?\r\n.*?int playerPosition.*?\r\n.*?int totalPlayers.*?\r\n.*?bool isWinner.*?\r\n.*?\r\n.*?QuestIntegrationPatches\.PatchGameplayModeFinished.*?\r\n.*?\}", 
                                  "var localPlayer = Context.NetworkGame.GetPlayer(Runner.LocalPlayer);\r\n\t\t\tif (localPlayer != null)\r\n\t\t\t{\r\n\t\t\t\tvar stats = localPlayer.Statistics;\r\n\t\t\t\tint playerPosition = stats.Position > 0 ? stats.Position : 1;\r\n\t\t\t\tint totalPlayers = Context.NetworkGame.ActivePlayerCount;\r\n\t\t\t\tbool isWinner = stats.Position == 1;\r\n\t\t\t\t\r\n\t\t\t\tQuestIntegrationPatches.PatchGameplayModeFinished(playerPosition, totalPlayers, isWinner);\r\n\t\t\t}\r\n\r\n\t\t\tif (Application.isBatchMode == true)\r\n\t\t\t{\r\n\t\t\t\tStartCoroutine(ShutdownCoroutine());\r\n\t\t\t}", RegexOptions.Singleline);
            
            File.WriteAllText(filePath, content);
            AssetDatabase.Refresh();
            
            Debug.Log("[Quest Integration] Fixed GameplayMode.cs - quest hooks properly added!");
        }
    }
}
