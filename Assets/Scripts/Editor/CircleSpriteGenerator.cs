using UnityEngine;
using UnityEditor;
using System.IO;

namespace TPSBR
{
    public class CircleSpriteGenerator : EditorWindow
    {
        private int textureSize = 512;
        private float borderThickness = 0.05f;
        private bool fillCircle = false;

        [MenuItem("Tools/Generate Circle Sprite for Map")]
        public static void ShowWindow()
        {
            GetWindow<CircleSpriteGenerator>("Circle Sprite Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Circle Sprite Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Generate a circle sprite with a transparent center for the zone visualization.",
                MessageType.Info);

            GUILayout.Space(10);

            textureSize = EditorGUILayout.IntSlider("Texture Size", textureSize, 128, 2048);
            borderThickness = EditorGUILayout.Slider("Border Thickness", borderThickness, 0.01f, 0.5f);
            fillCircle = EditorGUILayout.Toggle("Fill Circle", fillCircle);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate Circle Sprite", GUILayout.Height(40)))
            {
                GenerateCircleSprite();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This will create a circle sprite at:\nAssets/Textures/ZoneCircle.png\n\n" +
                "Use this sprite for better-looking zone circles on your map!",
                MessageType.None);
        }

        private void GenerateCircleSprite()
        {
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            
            Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
            float outerRadius = textureSize / 2f;
            float innerRadius = fillCircle ? 0f : outerRadius * (1f - borderThickness);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    
                    Color color = Color.clear;
                    
                    if (distance <= outerRadius)
                    {
                        if (fillCircle)
                        {
                            float alpha = 1f - Mathf.Clamp01((distance - innerRadius) / 2f);
                            color = new Color(1f, 1f, 1f, alpha);
                        }
                        else
                        {
                            if (distance >= innerRadius)
                            {
                                float alpha = 1f;
                                if (distance > outerRadius - 2f)
                                {
                                    alpha = 1f - (distance - (outerRadius - 2f)) / 2f;
                                }
                                else if (distance < innerRadius + 2f)
                                {
                                    alpha = (distance - innerRadius) / 2f;
                                }
                                color = new Color(1f, 1f, 1f, alpha);
                            }
                        }
                    }
                    
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            string directory = "Assets/Textures";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string path = directory + "/ZoneCircle.png";
            byte[] bytes = texture.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            EditorUtility.DisplayDialog(
                "Success!",
                $"Circle sprite created at:\n{path}\n\n" +
                "You can now assign this sprite to your zone circle images for better visuals!",
                "Great!");

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
