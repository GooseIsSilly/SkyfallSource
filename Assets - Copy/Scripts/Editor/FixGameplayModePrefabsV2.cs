using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TPSBR
{
    public class FixGameplayModePrefabsV2
    {
        [MenuItem("Tools/Fix Gameplay Mode Prefabs v2")]
        public static void FixPrefabs()
        {
            Debug.Log("[Fix] Starting Gameplay Mode Prefabs fix v2...");
            
            var battleRoyalePrefab = AssetDatabase.LoadAssetAtPath<GameplayMode>("Assets/TPSBR/Prefabs/Gameplay/Modes/BattleRoyale.prefab");
            
            if (battleRoyalePrefab == null)
            {
                Debug.LogError("[Fix] Could not find BattleRoyale.prefab at expected path!");
                return;
            }
            
            Debug.Log($"[Fix] Loaded BattleRoyale prefab: {battleRoyalePrefab.name}");
            
            UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
            string originalScenePath = currentScene.path;
            
            bool wasGameSceneLoaded = currentScene.path == "Assets/TPSBR/Scenes/Game.unity";
            
            if (!wasGameSceneLoaded)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    Debug.LogWarning("[Fix] User cancelled scene save. Aborting fix.");
                    return;
                }
                
                Debug.Log("[Fix] Opening Game.unity scene...");
                EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Game.unity");
            }
            
            var networkGame = Object.FindAnyObjectByType<NetworkGame>();
            
            if (networkGame == null)
            {
                Debug.LogError("[Fix] Could not find NetworkGame in Game.unity scene!");
                return;
            }
            
            Debug.Log($"[Fix] Found NetworkGame: {networkGame.name}");
            
            var serializedNetworkGame = new SerializedObject(networkGame);
            var modePrefabsProperty = serializedNetworkGame.FindProperty("_modePrefabs");
            
            Debug.Log($"[Fix] Current array size: {modePrefabsProperty.arraySize}");
            
            for (int i = 0; i < modePrefabsProperty.arraySize; i++)
            {
                var element = modePrefabsProperty.GetArrayElementAtIndex(i);
                Debug.Log($"[Fix] Element {i}: {(element.objectReferenceValue != null ? element.objectReferenceValue.name : "null")}");
            }
            
            modePrefabsProperty.ClearArray();
            modePrefabsProperty.arraySize = 2;
            
            modePrefabsProperty.GetArrayElementAtIndex(0).objectReferenceValue = null;
            modePrefabsProperty.GetArrayElementAtIndex(1).objectReferenceValue = battleRoyalePrefab;
            
            serializedNetworkGame.ApplyModifiedProperties();
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            
            Debug.Log("[Fix] ✅ NetworkGame._modePrefabs array fixed and saved!");
            Debug.Log("[Fix]   - Element 0: None (null)");
            Debug.Log($"[Fix]   - Element 1: {battleRoyalePrefab.name}");
            
            if (!wasGameSceneLoaded && !string.IsNullOrEmpty(originalScenePath))
            {
                Debug.Log($"[Fix] Returning to original scene: {originalScenePath}");
                EditorSceneManager.OpenScene(originalScenePath);
            }
        }
    }
}
