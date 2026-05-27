using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

public class QuickFixGameplayMode
{
    [MenuItem("TPSBR/Quick Fix GameplayMode")]
    public static void Fix()
    {
        string path = "Assets/TPSBR/Scripts/Gameplay/GameplayModes/GameplayMode.cs";
        string content = File.ReadAllText(path);
        
        content = Regex.Replace(
            content,
            @"State = EState\.Active;\r\n\r\n\t\tQuestIntegrationPatches\.PatchGameplayModeActivated\(\);\r\n\t\t\tOnActivate\(\);",
            "State = EState.Active;\r\n\r\n\t\t\tOnActivate();\r\n\r\n\t\t\tQuestIntegrationPatches.PatchGameplayModeActivated();"
        );
        
        content = Regex.Replace(
            content,
            @"Context\.Backfill\.BackfillEnabled = false;\r\n\r\n\t\t\tif \(Application\.isBatchMode == true\)\r\n\t\t\t\{\r\n\t\t\t\tStartCoroutine\(ShutdownCoroutine\(\)\);\r\n\r\n\t\tvar localPlayer.*?\r\n\t\t\t\}",
            "Context.Backfill.BackfillEnabled = false;\r\n\r\n\t\t\tvar localPlayer = Context.NetworkGame.GetPlayer(Runner.LocalPlayer);\r\n\t\t\tif (localPlayer != null)\r\n\t\t\t{\r\n\t\t\t\tvar stats = localPlayer.Statistics;\r\n\t\t\t\tint playerPosition = stats.Position > 0 ? stats.Position : 1;\r\n\t\t\t\tint totalPlayers = Context.NetworkGame.ActivePlayerCount;\r\n\t\t\t\tbool isWinner = stats.Position == 1;\r\n\t\t\t\t\r\n\t\t\t\tQuestIntegrationPatches.PatchGameplayModeFinished(playerPosition, totalPlayers, isWinner);\r\n\t\t\t}\r\n\r\n\t\t\tif (Application.isBatchMode == true)\r\n\t\t\t{\r\n\t\t\t\tStartCoroutine(ShutdownCoroutine());\r\n\t\t\t}",
            RegexOptions.Singleline
        );
        
        File.WriteAllText(path, content);
        AssetDatabase.Refresh();
        
        Debug.Log("[Quest Fix] GameplayMode.cs has been fixed! Quest hooks are now properly positioned.");
    }
}
