using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

namespace TPSBREditor
{
	public class FixNetworkGameCode : EditorWindow
	{
		[MenuItem("TPSBR/Fix NetworkGame Initialize Method")]
		public static void FixInitializeMethod()
		{
			string filePath = "Assets/TPSBR/Scripts/Core/NetworkGame.cs";
			
			if (!File.Exists(filePath))
			{
				Debug.LogError($"File not found: {filePath}");
				return;
			}

			string fileContent = File.ReadAllText(filePath);
			
			string brokenPattern = @"if \(HasStateAuthority == true\)\s*\{\s+if \(prefab == null\)";
			
			if (!Regex.IsMatch(fileContent, brokenPattern))
			{
				Debug.LogWarning("The Initialize method appears to already be fixed or the pattern was not found.");
				return;
			}

			string searchPattern = @"(if \(HasStateAuthority == true\)\s*\{\s+)if \(prefab == null\)\s*\{\s*Debug\.LogError\(\$""[^""]*""\);\s*return;\s*\}\s*(var prefab = _modePrefabs\.Find\([^;]+\);)\s*(_gameplayMode = Runner\.Spawn\(prefab\);)";
			
			string replacement = "$1$2\r\n\t\t\t\r\n\t\t\tif (prefab == null)\r\n\t\t\t{\r\n\t\t\t\tDebug.LogError($\"[NetworkGame] No gameplay mode prefab found for type: {gameplayType}. Please check that mode prefabs are assigned in the NetworkGame component.\");\r\n\t\t\t\treturn;\r\n\t\t\t}\r\n\r\n\t\t\t$3";
			
			string fixedContent = Regex.Replace(fileContent, searchPattern, replacement, RegexOptions.Singleline);
			
			if (fixedContent == fileContent)
			{
				Debug.LogError("Failed to apply the fix. The code structure may have changed.");
				return;
			}

			File.WriteAllText(filePath, fixedContent);
			AssetDatabase.Refresh();
			
			Debug.Log("✓ NetworkGame.cs Initialize method has been fixed! The file will recompile automatically.");
		}
	}
}
