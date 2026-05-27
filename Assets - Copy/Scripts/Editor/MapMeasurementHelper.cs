using UnityEngine;
using UnityEditor;

namespace TPSBR
{
    public class MapMeasurementHelper : EditorWindow
    {
        private Vector3 _point1;
        private Vector3 _point2;
        private bool _hasPoint1 = false;
        private bool _hasPoint2 = false;

        [MenuItem("Tools/Map Measurement Helper")]
        public static void ShowWindow()
        {
            GetWindow<MapMeasurementHelper>("Map Measurement");
        }

        private void OnGUI()
        {
            GUILayout.Label("Map Bounds Measurement", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps you measure your map size for the SimpleMapSystem.\n\n" +
                "1. Click 'Set Point 1' below\n" +
                "2. Click anywhere in Scene view (one corner of your map)\n" +
                "3. Click 'Set Point 2'\n" +
                "4. Click another corner of your map\n" +
                "5. See the calculated Map World Size and Center!",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Set Point 1 (Click in Scene)", GUILayout.Height(30)))
            {
                SceneView.duringSceneGui += OnSceneGUIPoint1;
                EditorGUIUtility.SetWantsMouseJumping(1);
            }

            if (_hasPoint1)
            {
                EditorGUILayout.LabelField("Point 1:", $"X: {_point1.x:F2}, Z: {_point1.z:F2}");
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Set Point 2 (Click in Scene)", GUILayout.Height(30)))
            {
                SceneView.duringSceneGui += OnSceneGUIPoint2;
                EditorGUIUtility.SetWantsMouseJumping(1);
            }

            if (_hasPoint2)
            {
                EditorGUILayout.LabelField("Point 2:", $"X: {_point2.x:F2}, Z: {_point2.z:F2}");
            }

            GUILayout.Space(10);

            if (_hasPoint1 && _hasPoint2)
            {
                GUILayout.Label("Results:", EditorStyles.boldLabel);
                
                float sizeX = Mathf.Abs(_point2.x - _point1.x);
                float sizeZ = Mathf.Abs(_point2.z - _point1.z);
                float centerX = (_point1.x + _point2.x) / 2f;
                float centerZ = (_point1.z + _point2.z) / 2f;

                EditorGUILayout.HelpBox(
                    $"Map World Size:\n  X: {sizeX:F2}\n  Y: {sizeZ:F2}\n\n" +
                    $"Map World Center:\n  X: {centerX:F2}\n  Y: {centerZ:F2}",
                    MessageType.None
                );

                if (GUILayout.Button("Copy Map World Size", GUILayout.Height(25)))
                {
                    EditorGUIUtility.systemCopyBuffer = $"{sizeX:F2}, {sizeZ:F2}";
                    Debug.Log($"Copied: {sizeX:F2}, {sizeZ:F2}");
                }

                if (GUILayout.Button("Copy Map World Center", GUILayout.Height(25)))
                {
                    EditorGUIUtility.systemCopyBuffer = $"{centerX:F2}, {centerZ:F2}";
                    Debug.Log($"Copied: {centerX:F2}, {centerZ:F2}");
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Reset"))
            {
                _hasPoint1 = false;
                _hasPoint2 = false;
            }
        }

        private void OnSceneGUIPoint1(SceneView sceneView)
        {
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _point1 = hit.point;
                    _hasPoint1 = true;
                    SceneView.duringSceneGui -= OnSceneGUIPoint1;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Repaint();
                    e.Use();
                }
            }
        }

        private void OnSceneGUIPoint2(SceneView sceneView)
        {
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _point2 = hit.point;
                    _hasPoint2 = true;
                    SceneView.duringSceneGui -= OnSceneGUIPoint2;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Repaint();
                    e.Use();
                }
            }
        }

        private void OnDestroy()
        {
            SceneView.duringSceneGui -= OnSceneGUIPoint1;
            SceneView.duringSceneGui -= OnSceneGUIPoint2;
            EditorGUIUtility.SetWantsMouseJumping(0);
        }
    }
}
