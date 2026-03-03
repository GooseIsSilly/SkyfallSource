using UnityEngine;
using UnityEditor;
using TPSBR;

namespace TPSBREditor
{
	public class FixBattleRoyaleType : EditorWindow
	{
		[MenuItem("TPSBR/Fix BattleRoyale Type Value")]
		public static void FixBattleRoyalePrefabType()
		{
			GameObject battleRoyalePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/Prefabs/Gameplay/Modes/BattleRoyale.prefab");

			if (battleRoyalePrefab == null)
			{
				Debug.LogError("Could not load BattleRoyale.prefab at Assets/TPSBR/Prefabs/Gameplay/Modes/BattleRoyale.prefab");
				return;
			}

			GameplayMode gameplayMode = battleRoyalePrefab.GetComponent<GameplayMode>();

			if (gameplayMode == null)
			{
				Debug.LogError("BattleRoyale.prefab does not have a GameplayMode component!");
				return;
			}

			SerializedObject serializedObject = new SerializedObject(gameplayMode);
			SerializedProperty typeProperty = serializedObject.FindProperty("_type");

			if (typeProperty == null)
			{
				Debug.LogError("Could not find _type property on GameplayMode!");
				return;
			}

			Debug.Log($"Current _type value: {typeProperty.enumValueIndex} ({(EGameplayType)typeProperty.enumValueIndex})");

			typeProperty.enumValueIndex = (int)EGameplayType.BattleRoyale;

			serializedObject.ApplyModifiedProperties();

			EditorUtility.SetDirty(battleRoyalePrefab);
			AssetDatabase.SaveAssets();

			Debug.Log($"✓ Fixed! BattleRoyale.prefab _type is now set to: {typeProperty.enumValueIndex} ({(EGameplayType)typeProperty.enumValueIndex})");
			Debug.Log("=== FIX COMPLETE! Try starting the game now. ===");
		}
	}
}
