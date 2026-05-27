using UnityEngine;
using UnityEditor;
using System.IO;

namespace TPSBR
{
    public class MapScreenshotTool : EditorWindow
    {
        private int _screenshotSize = 2048;
        private float _cameraHeight = 100f;
        private Vector3 _mapCenter = Vector3.zero;
        private float _cameraSize = 50f;

        [MenuItem("Tools/Map Screenshot Tool")]
        public static void ShowWindow()
        {
            GetWindow<MapScreenshotTool>("Map Screenshot");
        }

        private void OnGUI()
        {
            GUILayout.Label("Automatic Map Screenshot", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool creates a perfect top-down screenshot of your map!\n\n" +
                "1. Set the camera position and size\n" +
                "2. Click 'Take Screenshot'\n" +
                "3. Screenshot saved to Assets/Textures/MapImage.png",
                MessageType.Info
            );

            GUILayout.Space(10);

            _mapCenter = EditorGUILayout.Vector3Field("Map Center", _mapCenter);
            _cameraHeight = EditorGUILayout.FloatField("Camera Height", _cameraHeight);
            _cameraSize = EditorGUILayout.FloatField("Camera Size (Zoom)", _cameraSize);
            
            GUILayout.Space(5);
            
            _screenshotSize = EditorGUILayout.IntPopup(
                "Screenshot Size",
                _screenshotSize,
                new string[] { "512x512", "1024x1024", "2048x2048", "4096x4096" },
                new int[] { 512, 1024, 2048, 4096 }
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Preview Camera Position", GUILayout.Height(30)))
            {
                PreviewCameraPosition();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Take Screenshot", GUILayout.Height(40)))
            {
                TakeScreenshot();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Tip: Use 'Preview Camera Position' to check if your camera captures the whole map before taking the screenshot!",
                MessageType.None
            );
        }

        private void PreviewCameraPosition()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.LookAt(_mapCenter, Quaternion.Euler(90f, 0f, 0f), _cameraSize);
                sceneView.Repaint();
                Debug.Log("Scene view positioned at map camera location. Check if it captures your whole map!");
            }
        }

        private void TakeScreenshot()
        {
            GameObject tempCameraObj = new GameObject("TempScreenshotCamera");
            Camera tempCamera = tempCameraObj.AddComponent<Camera>();
            
            tempCamera.transform.position = new Vector3(_mapCenter.x, _cameraHeight, _mapCenter.z);
            tempCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = _cameraSize;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = new Color(0.2f, 0.25f, 0.2f);
            tempCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));

            RenderTexture rt = new RenderTexture(_screenshotSize, _screenshotSize, 24);
            tempCamera.targetTexture = rt;
            
            Texture2D screenshot = new Texture2D(_screenshotSize, _screenshotSize, TextureFormat.RGB24, false);
            tempCamera.Render();
            
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, _screenshotSize, _screenshotSize), 0, 0);
            screenshot.Apply();
            
            RenderTexture.active = null;
            tempCamera.targetTexture = null;
            DestroyImmediate(rt);
            DestroyImmediate(tempCameraObj);

            string directory = "Assets/Textures";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string path = $"{directory}/MapImage.png";
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            
            DestroyImmediate(screenshot);
            
            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            EditorUtility.DisplayDialog(
                "Screenshot Saved!",
                $"Map screenshot saved to:\n{path}\n\nYou can now use this image in your SimpleMapSystem!",
                "OK"
            );

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            EditorGUIUtility.PingObject(Selection.activeObject);

            Debug.Log($"Map screenshot saved to: {path}");
        }
    }
}
