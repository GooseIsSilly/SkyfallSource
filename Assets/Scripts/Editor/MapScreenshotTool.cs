using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace TPSBR
{
    public class MapScreenshotTool : EditorWindow
    {
        private int _screenshotSize = 2048;
        private float _cameraHeight = 5000f; // High default to clear mountains
        private Vector3 _mapCenter = new Vector3(0, 0, 0);
        private float _cameraSize = 5000f; // Wide default to see the whole map

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
                "NOTE: If water is missing, ensure your Camera Height is high enough to be ABOVE your mountains, and your Map Center Y is at the water level (usually 0-5).",
                MessageType.Info
            );

            GUILayout.Space(10);

            _mapCenter = EditorGUILayout.Vector3Field("Map Center", _mapCenter);
            _cameraHeight = EditorGUILayout.FloatField("Camera Offset Height", _cameraHeight);
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
            
            // Add URP specific data
            var cameraData = tempCameraObj.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderShadows = false;
            cameraData.requiresColorTexture = true;
            cameraData.requiresDepthTexture = true;
            cameraData.renderPostProcessing = false;

            // Use Map Center Y as the baseline for the camera offset
            Vector3 targetPos = new Vector3(_mapCenter.x, _mapCenter.y + _cameraHeight, _mapCenter.z);
            tempCamera.transform.position = targetPos;
            tempCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            
            // Toggle between Ortho and Perspective to see if Perspective works better for depth
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = _cameraSize;
            
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = Color.black; 
            tempCamera.cullingMask = -1; // Render all layers
            
            tempCamera.nearClipPlane = 0.1f;
            tempCamera.farClipPlane = _cameraHeight + 20000f; 
            tempCamera.depthTextureMode = DepthTextureMode.Depth;

            Debug.Log($"Map Screenshot: Cam at {targetPos}, View Size {_cameraSize}, Target Y {_mapCenter.y}");

            RenderTexture rt = new RenderTexture(_screenshotSize, _screenshotSize, 24, RenderTextureFormat.ARGB32);
            tempCamera.targetTexture = rt;
            
            Texture2D screenshot = new Texture2D(_screenshotSize, _screenshotSize, TextureFormat.RGB24, false);
            
            // Temporarily disable fog
            bool oldFog = RenderSettings.fog;
            RenderSettings.fog = false;
            
            // Standard render call
            tempCamera.Render();
            
            RenderSettings.fog = oldFog;
            
            RenderTexture.active = rt;
            screenshot.ReadPixels(new Rect(0, 0, _screenshotSize, _screenshotSize), 0, 0);
            screenshot.Apply();
            
            RenderTexture.active = null;
            tempCamera.targetTexture = null;
            
            // Cleanup
            if (Application.isPlaying) {
                Destroy(tempCameraObj);
                Destroy(rt);
            } else {
                DestroyImmediate(tempCameraObj);
                DestroyImmediate(rt);
            }

            string directory = "Assets/Textures";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string path = $"{directory}/MapImage.png";
            byte[] bytes = screenshot.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            
            if (Application.isPlaying) {
                Destroy(screenshot);
            } else {
                DestroyImmediate(screenshot);
            }
            
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
