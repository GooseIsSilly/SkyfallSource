using UnityEngine;
using UnityEditor;

namespace TPSBR
{
    public class ManualShopGuide : EditorWindow
    {
        [MenuItem("TPSBR/📋 Manual Shop Fix Guide")]
        public static void ShowGuide()
        {
            var window = GetWindow<ManualShopGuide>("Manual Fix Guide");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            GUILayout.Label("🔧 MANUAL SHOP FIX GUIDE", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "If the automatic fix isn't working, follow these steps to manually fix the shop:", 
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 1: Open Menu Scene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. In Project window, navigate to:\n" +
                "   Assets/TPSBR/Scenes/\n" +
                "2. Double-click Menu.unity to open it", 
                MessageType.None);

            if (GUILayout.Button("Open Menu Scene Now"))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity");
                Debug.Log("✓ Opened Menu scene");
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 2: Find ShopItemsList", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. In Hierarchy window, expand:\n" +
                "   MenuUI → UIShopView → Content → ShopItemsList\n" +
                "2. Click on ShopItemsList to select it", 
                MessageType.None);

            if (GUILayout.Button("Select ShopItemsList Now"))
            {
                var obj = GameObject.Find("MenuUI");
                if (obj != null)
                {
                    var shopItemsList = obj.transform.Find("UIShopView/Content/ShopItemsList");
                    if (shopItemsList != null)
                    {
                        Selection.activeGameObject = shopItemsList.gameObject;
                        EditorGUIUtility.PingObject(shopItemsList.gameObject);
                        Debug.Log("✓ Selected ShopItemsList");
                    }
                    else
                    {
                        Debug.LogError("Could not find ShopItemsList!");
                    }
                }
                else
                {
                    Debug.LogError("Menu scene not loaded!");
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 3: Locate UIShopItem Prefab", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. In Project window, navigate to:\n" +
                "   Assets/TPSBR/UI/Prefabs/Widgets/\n" +
                "2. Find UIShopItem prefab (should have blue cube icon)", 
                MessageType.None);

            if (GUILayout.Button("Find UIShopItem Prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                if (prefab != null)
                {
                    EditorGUIUtility.PingObject(prefab);
                    Debug.Log("✓ Found UIShopItem prefab");
                }
                else
                {
                    Debug.LogError("UIShopItem prefab not found!");
                }
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 4: Assign Prefab to UIList", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. With ShopItemsList selected in Hierarchy\n" +
                "2. Look at Inspector window\n" +
                "3. Find 'UI List' component\n" +
                "4. Look for '_itemInstance' field (might say 'None')\n" +
                "5. Drag UIShopItem prefab from Project window\n" +
                "   into the '_itemInstance' field\n" +
                "6. You should see it change to 'UIShopItem'", 
                MessageType.None);

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 5: Save Scene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Press Ctrl+S (Cmd+S on Mac) to save\n" +
                "2. Or File → Save\n" +
                "3. Asterisk (*) next to scene name should disappear", 
                MessageType.None);

            if (GUILayout.Button("Save Scene Now", GUILayout.Height(30)))
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                Debug.Log("✓ Scene saved");
            }

            GUILayout.Space(10);

            EditorGUILayout.LabelField("STEP 6: Test", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Press Play ▶️\n" +
                "2. Click SHOP button\n" +
                "3. You should see Soldier66 and Marine cards!", 
                MessageType.None);

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "⚠️ IMPORTANT: You must be in EDIT MODE (not playing) when doing steps 1-5!", 
                MessageType.Warning);
        }
    }
}
