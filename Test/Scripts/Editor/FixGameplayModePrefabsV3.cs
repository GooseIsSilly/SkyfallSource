using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR;

namespace TPSBREditor
{
	public class FixGameplayModePrefabsV3 : EditorWindow
	{
		[MenuItem("TPSBR/Fix Gameplay Mode Prefabs V3 (RECOMMENDED)")]
		public static void FixAllPrefabsV3()
		{
			bool fixedAnything = false;

			NetworkGame sceneNetworkGame = Object.FindFirstObjectByType<NetworkGame>();
			if (sceneNetworkGame != null)
			{
				FixNetworkGamePrefabsV3(sceneNetworkGame);
				EditorSceneManager.MarkSceneDirty(sceneNetworkGame.gameObject.scene);
				fixedAnything = true;
				Debug.Log("✓ Scene NetworkGame fixed!");
			}

			GameObject gameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/Prefabs/Game/Gameplay.prefab");
			if (gameplayPrefab != null)
			{
				NetworkGame prefabNetworkGame = gameplayPrefab.GetComponentInChildren<NetworkGame>();
				if (prefabNetworkGame != null)
				{
					FixNetworkGamePrefabsV3(prefabNetworkGame);
					EditorUtility.SetDirty(gameplayPrefab);
					fixedAnything = true;
					Debug.Log("✓ Gameplay.prefab NetworkGame fixed!");
				}
			}

			if (fixedAnything)
			{
				AssetDatabase.SaveAssets();
				Debug.Log("IMPORTANT: Press Ctrl+S to save the scene if you had one open!");
				Debug.Log("=== FIX COMPLETE! Try starting the game now. ===");
			}
			else
			{
				Debug.LogError("Could not find NetworkGame in scene or prefab. Please open the Game scene first.");
			}
		}

		private static void FixNetworkGamePrefabsV3(NetworkGame networkGame)
		{
			SerializedObject serializedNetworkGame = new SerializedObject(networkGame);
			SerializedProperty modePrefabsProperty = serializedNetworkGame.FindProperty("_modePrefabs");

			if (modePrefabsProperty == null || !modePrefabsProperty.isArray)
			{
				Debug.LogError("Could not find _modePrefabs array on NetworkGame.");
				return;
			}

			GameplayMode battleRoyalePrefab = AssetDatabase.LoadAssetAtPath<GameplayMode>("Assets/TPSBR/Prefabs/Gameplay/Modes/BattleRoyale.prefab");

			if (battleRoyalePrefab == null)
			{
				Debug.LogError("Could not load BattleRoyale.prefab from Assets/TPSBR/Prefabs/Gameplay/Modes/BattleRoyale.prefab");
				return;
			}

			modePrefabsProperty.ClearArray();
			modePrefabsProperty.InsertArrayElementAtIndex(0);
			modePrefabsProperty.GetArrayElementAtIndex(0).objectReferenceValue = null;
			
			modePrefabsProperty.InsertArrayElementAtIndex(1);
			modePrefabsProperty.GetArrayElementAtIndex(1).objectReferenceValue = battleRoyalePrefab;

			serializedNetworkGame.ApplyModifiedProperties();

			Debug.Log($"Fixed {networkGame.name}: _modePrefabs array properly cleared and set with BattleRoyale at index 1");
		}
	}
}
