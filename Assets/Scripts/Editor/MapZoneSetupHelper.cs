using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace TPSBR
{
    public class MapZoneSetupHelper : EditorWindow
    {
        private SimpleMapSystem mapSystem;
        private GameObject mapPanel;

        [MenuItem("Tools/Map Zone Setup Helper")]
        public static void ShowWindow()
        {
            GetWindow<MapZoneSetupHelper>("Map Zone Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Map Zone Circle Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool will automatically create and setup the zone circles for your map system.",
                MessageType.Info);

            GUILayout.Space(10);

            mapSystem = (SimpleMapSystem)EditorGUILayout.ObjectField(
                "Map System", mapSystem, typeof(SimpleMapSystem), true);

            mapPanel = (GameObject)EditorGUILayout.ObjectField(
                "Map Panel", mapPanel, typeof(GameObject), true);

            GUILayout.Space(20);

            GUI.enabled = mapSystem != null && mapPanel != null;

            if (GUILayout.Button("Auto-Setup Zone Circles", GUILayout.Height(40)))
            {
                SetupZoneCircles();
            }

            GUI.enabled = true;

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "Steps:\n" +
                "1. Assign your SimpleMapSystem component\n" +
                "2. Assign your Map Panel GameObject\n" +
                "3. Click 'Auto-Setup Zone Circles'\n\n" +
                "The tool will create CurrentZoneCircle and NextZoneCircle images and link them automatically!",
                MessageType.None);
        }

        private void SetupZoneCircles()
        {
            if (mapSystem == null || mapPanel == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Map System and Map Panel!", "OK");
                return;
            }

            Undo.RecordObject(mapSystem, "Setup Zone Circles");

            GameObject currentZone = CreateZoneCircle(mapPanel.transform, "CurrentZoneCircle", new Color(1f, 0f, 0f, 0.3f));
            GameObject nextZone = CreateZoneCircle(mapPanel.transform, "NextZoneCircle", new Color(1f, 1f, 1f, 0.5f));

            SerializedObject so = new SerializedObject(mapSystem);
            so.FindProperty("_currentZoneCircle").objectReferenceValue = currentZone.GetComponent<Image>();
            so.FindProperty("_nextZoneCircle").objectReferenceValue = nextZone.GetComponent<Image>();
            so.FindProperty("_showZones").boolValue = true;
            so.ApplyModifiedProperties();

            EditorUtility.DisplayDialog(
                "Success!",
                "Zone circles created and linked successfully!\n\n" +
                "CurrentZoneCircle (Red) and NextZoneCircle (White) have been added to your Map Panel.\n\n" +
                "Press M in Play mode to see the zones on your map!",
                "Awesome!");

            Selection.activeGameObject = mapSystem.gameObject;
        }

        private GameObject CreateZoneCircle(Transform parent, string name, Color color)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }

            GameObject circleObj = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(circleObj, "Create " + name);

            circleObj.transform.SetParent(parent, false);

            Image image = circleObj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            RectTransform rect = circleObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(200f, 200f);
            rect.anchoredPosition = Vector2.zero;

            circleObj.SetActive(false);

            return circleObj;
        }
    }
}
