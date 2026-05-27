using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

namespace TPSBR
{
    public static class LiveEventSetupHelper
    {
        private const string PREFAB_PATH = "Assets/Prefabs/LiveEventCountdown.prefab";
        
        [MenuItem("Assets/Create/Skyfall/Live Event Countdown Prefab", false, 10)]
        private static void CreateCountdownPrefab()
        {
            GameObject countdownObject = CreateCountdownGameObject();
            
            string prefabDirectory = Path.GetDirectoryName(PREFAB_PATH);
            if (!Directory.Exists(prefabDirectory))
            {
                Directory.CreateDirectory(prefabDirectory);
            }
            
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(PREFAB_PATH);
            
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(countdownObject, uniquePath);
            
            Object.DestroyImmediate(countdownObject);
            
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"Created Live Event Countdown Prefab at: {uniquePath}");
            Debug.Log("Drag this prefab into your scene and position it where you want the countdown to appear!");
        }
        
        [MenuItem("GameObject/Skyfall/Create World Countdown Text", false, 10)]
        public static void CreateWorldCountdownText()
        {
            GameObject countdownObject = CreateCountdownGameObject();
            
            countdownObject.transform.position = Vector3.up * 10f;
            
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            
            Selection.activeGameObject = countdownObject;
            EditorGUIUtility.PingObject(countdownObject);
            
            Debug.Log("Created LiveEventCountdown at position (0, 10, 0). Drag it to your desired location on the map!");
        }
        
        private static GameObject CreateCountdownGameObject()
        {
            GameObject countdownObject = new GameObject("LiveEventCountdown");
            
            Undo.RegisterCreatedObjectUndo(countdownObject, "Create World Countdown Text");
            
            TextMeshPro tmp = countdownObject.AddComponent<TextMeshPro>();
            tmp.text = "00:00";
            tmp.fontSize = 10;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;
            tmp.fontStyle = FontStyles.Bold;
            tmp.margin = new Vector4(0, 0, 0, 0);
            
            RectTransform rectTransform = countdownObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(20, 5);
            }
            
            LiveEventWorldText worldText = countdownObject.AddComponent<LiveEventWorldText>();
            
            SerializedObject so = new SerializedObject(worldText);
            so.FindProperty("_billboardToCamera").boolValue = true;
            so.FindProperty("_normalColor").colorValue = Color.white;
            so.FindProperty("_urgentColor").colorValue = Color.red;
            so.FindProperty("_pulseSpeed").floatValue = 2f;
            so.FindProperty("_pulseIntensity").floatValue = 0.3f;
            so.FindProperty("_baseScale").vector3Value = Vector3.one * 5f;
            so.FindProperty("_showEventName").boolValue = true;
            so.FindProperty("_eventNameOffsetY").floatValue = 2f;
            so.FindProperty("_showGizmo").boolValue = true;
            so.FindProperty("_gizmoColor").colorValue = Color.yellow;
            so.ApplyModifiedProperties();
            
            return countdownObject;
        }
        
        [MenuItem("GameObject/Skyfall/Setup Satellite Event Object", false, 11)]
        private static void SetupSatelliteEventObject()
        {
            GameObject selected = Selection.activeGameObject;
            
            if (selected == null)
            {
                EditorUtility.DisplayDialog(
                    "No Object Selected", 
                    "Please select the satellite GameObject first, then run this command.", 
                    "OK"
                );
                return;
            }
            
            Undo.RecordObject(selected, "Setup Satellite Event Object");
            
            Animation anim = selected.GetComponent<Animation>();
            if (anim == null)
            {
                anim = Undo.AddComponent<Animation>(selected);
            }
            
            LiveEventAnimationTrigger trigger = selected.GetComponent<LiveEventAnimationTrigger>();
            if (trigger == null)
            {
                trigger = Undo.AddComponent<LiveEventAnimationTrigger>(selected);
            }
            
            AudioSource audioSource = selected.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = Undo.AddComponent<AudioSource>(selected);
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
            }
            
            EditorUtility.SetDirty(selected);
            
            Debug.Log($"Setup complete for '{selected.name}'! Now assign your animation clip and LiveEventData in the inspector.");
        }
    }
}
