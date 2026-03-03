using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR;

namespace TPSBREditor
{
	public class FixGameplayModePrefabs : EditorWindow
	{
		[MenuItem("TPSBR/Fix Gameplay Mode Prefabs in Scene")]
		public static void FixPrefabsInCurrentScene()
		{
			NetworkGame networkGame = Object.FindFirstObjectByType<NetworkGame>();
			
			if (networkGame == null)
			{
				Debug.LogError("NetworkGame not found in the current scene. Please open the Menu scene (Assets/TPSBR/Scenes/Menu.unity) or Game scene (Assets/TPSBR/Scenes/Game.unity) first.");
				return;
			}

			FixNetworkGamePrefabs(networkGame);

			EditorSceneManager.MarkSceneDirty(networkGame.gameObject.scene);
			Debug.Log("Scene NetworkGame fixed! Press Ctrl+S to save the scene.");
		}

		[MenuItem("TPSBR/Fix Gameplay Mode Prefabs in Gameplay Prefab")]
		public static void FixPrefabsInGameplayPrefab()
		{
			GameObject gameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/Prefabs/Game/Gameplay.prefab");
			
			if (gameplayPrefab == null)
			{
				Debug.LogError("Could not load Gameplay.prefab at Assets/TPSBR/Prefabs/Game/Gameplay.prefab");
				return;
			}

			NetworkGame networkGame = gameplayPrefab.GetComponentInChildren<NetworkGame>();
			
			if (networkGame == null)
			{
				Debug.LogError("NetworkGame not found in Gameplay.prefab");
				return;
			}

			FixNetworkGamePrefabs(networkGame);

			EditorUtility.SetDirty(gameplayPrefab);
			AssetDatabase.SaveAssets();
			Debug.Log("Gameplay.prefab NetworkGame fixed and saved!");
		}

		[MenuItem("TPSBR/Fix All Gameplay Mode Prefabs (Scene + Prefab)")]
		public static void FixAllPrefabs()
		{
			bool fixedScene = false;
			bool fixedPrefab = false;

			NetworkGame sceneNetworkGame = Object.FindFirstObjectByType<NetworkGame>();
			if (sceneNetworkGame != null)
			{
				FixNetworkGamePrefabs(sceneNetworkGame);
				EditorSceneManager.MarkSceneDirty(sceneNetworkGame.gameObject.scene);
				fixedScene = true;
				Debug.Log("✓ Scene NetworkGame fixed!");
			}

			GameObject gameplayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/Prefabs/Game/Gameplay.prefab");
			if (gameplayPrefab != null)
			{
				NetworkGame prefabNetworkGame = gameplayPrefab.GetComponentInChildren<NetworkGame>();
				if (prefabNetworkGame != null)
				{
					FixNetworkGamePrefabs(prefabNetworkGame);
					EditorUtility.SetDirty(gameplayPrefab);
					fixedPrefab = true;
					Debug.Log("✓ Gameplay.prefab NetworkGame fixed!");
				}
			}

			if (fixedScene || fixedPrefab)
			{
				AssetDatabase.SaveAssets();
				if (fixedScene)
				{
					Debug.Log("IMPORTANT: Press Ctrl+S to save the Game scene!");
				}
				Debug.Log("=== FIX COMPLETE! Try starting the game now. ===");
			}
			else
			{
				Debug.LogError("Could not find NetworkGame in scene or prefab. Please open the Game scene first.");
			}
		}

		private static void FixNetworkGamePrefabs(NetworkGame networkGame)
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
			modePrefabsProperty.arraySize = 2;
			
			modePrefabsProperty.GetArrayElementAtIndex(0).objectReferenceValue = null;
			modePrefabsProperty.GetArrayElementAtIndex(1).objectReferenceValue = battleRoyalePrefab;

			serializedNetworkGame.ApplyModifiedProperties();

			Debug.Log($"Fixed {networkGame.name}: _modePrefabs[0]=None, _modePrefabs[1]=BattleRoyale");
		}
	}
}
