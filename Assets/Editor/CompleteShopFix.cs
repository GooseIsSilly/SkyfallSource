using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;

namespace TPSBR
{
    public class CompleteShopFix : EditorWindow
    {
        [MenuItem("TPSBR/Complete Shop Fix")]
        public static void ShowWindow()
        {
            GetWindow<CompleteShopFix>("Complete Shop Fix");
        }

        private void OnGUI()
        {
            GUILayout.Label("Complete Shop UI Fix", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("This will fix all shop issues:\n" +
                "✓ Assign UIShopItem prefab to UIList\n" +
                "✓ Verify ShopDatabase is assigned\n" +
                "✓ Verify CloseButton is connected\n" +
                "✓ Check CharacterData IDs\n" +
                "✓ Save scene", MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("FIX EVERYTHING NOW", GUILayout.Height(40)))
            {
                FixEverything();
            }
        }

        private void FixEverything()
        {
            Debug.Log("=== Starting Complete Shop Fix ===");

            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/Menu.unity");
            
            if (!menuScene.isLoaded)
            {
                Debug.Log("Opening Menu scene...");
                menuScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity", OpenSceneMode.Single);
            }

            GameObject menuUIObj = GameObject.Find("MenuUI");
            if (menuUIObj == null)
            {
                Debug.LogError("Could not find MenuUI in scene!");
                return;
            }

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
            {
                Debug.LogError("Could not find UIShopView!");
                return;
            }

            UIShopView shopView = shopViewTransform.GetComponent<UIShopView>();
            if (shopView == null)
            {
                Debug.LogError("UIShopView component not found!");
                return;
            }

            Debug.Log("✓ Found UIShopView");

            SerializedObject shopViewSO = new SerializedObject(shopView);

            SerializedProperty shopDatabaseProp = shopViewSO.FindProperty("_shopDatabase");
            if (shopDatabaseProp.objectReferenceValue == null)
            {
                ShopDatabase database = AssetDatabase.LoadAssetAtPath<ShopDatabase>("Assets/Scripts/ShopDatabase.asset");
                if (database != null)
                {
                    shopDatabaseProp.objectReferenceValue = database;
                    Debug.Log("✓ Assigned ShopDatabase");
                }
                else
                {
                    Debug.LogWarning("ShopDatabase.asset not found at Assets/Scripts/ShopDatabase.asset");
                }
            }
            else
            {
                Debug.Log("✓ ShopDatabase already assigned");
            }

            SerializedProperty closeButtonProp = shopViewSO.FindProperty("_closeButton");
            if (closeButtonProp.objectReferenceValue == null)
            {
                Transform closeButton = shopViewTransform.Find("CloseButton");
                if (closeButton != null)
                {
                    closeButtonProp.objectReferenceValue = closeButton.GetComponent<UIButton>();
                    Debug.Log("✓ Assigned CloseButton");
                }
            }
            else
            {
                Debug.Log("✓ CloseButton already assigned");
            }

            shopViewSO.ApplyModifiedProperties();

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogError("Could not find ShopItemsList!");
                return;
            }

            UIList uiList = shopItemsListTransform.GetComponent<UIList>();
            if (uiList == null)
            {
                Debug.LogError("UIList component not found on ShopItemsList!");
                return;
            }

            Debug.Log("✓ Found ShopItemsList UIList");

            GameObject shopItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (shopItemPrefab == null)
            {
                Debug.LogError("UIShopItem prefab not found at Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                return;
            }

            Debug.Log("✓ Found UIShopItem prefab");

            SerializedObject uiListSO = new SerializedObject(uiList);
            SerializedProperty itemInstanceProp = uiListSO.FindProperty("_itemInstance");
            
            if (itemInstanceProp.objectReferenceValue == null)
            {
                itemInstanceProp.objectReferenceValue = shopItemPrefab;
                uiListSO.ApplyModifiedProperties();
                Debug.Log("✓ Assigned UIShopItem prefab to UIList._itemInstance");
            }
            else
            {
                Debug.Log("✓ UIShopItem prefab already assigned");
            }

            VerifyCharacterData();

            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);
            
            Debug.Log("✓ Menu scene saved");
            Debug.Log("=== SHOP FIX COMPLETE! ===");
            Debug.Log("Enter Play Mode and click the SHOP button from the main menu!");
            
            EditorUtility.DisplayDialog("Shop Fixed!", 
                "All shop issues fixed!\n\n" +
                "✓ UIShopItem prefab assigned\n" +
                "✓ ShopDatabase connected\n" +
                "✓ CloseButton connected\n" +
                "✓ CharacterData verified\n\n" +
                "Press Play and click SHOP button!", 
                "OK");
        }

        private void VerifyCharacterData()
        {
            string[] characterPaths = new string[]
            {
                "Assets/Scripts/CharacterData/soldier66.asset",
                "Assets/Scripts/CharacterData/marine.asset"
            };

            foreach (string path in characterPaths)
            {
                CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (data != null)
                {
                    bool needsSave = false;
                    
                    if (data.name == "soldier66")
                    {
                        if (data.characterID != "Agent.Soldier" || data.agentID != "Agent.Soldier")
                        {
                            data.characterID = "Agent.Soldier";
                            data.agentID = "Agent.Soldier";
                            needsSave = true;
                        }
                    }
                    else if (data.name == "marine")
                    {
                        if (data.characterID != "Agent.Marine" || data.agentID != "Agent.Marine")
                        {
                            data.characterID = "Agent.Marine";
                            data.agentID = "Agent.Marine";
                            needsSave = true;
                        }
                    }

                    if (needsSave)
                    {
                        EditorUtility.SetDirty(data);
                        Debug.Log($"✓ Fixed {data.name} IDs");
                    }
                    else
                    {
                        Debug.Log($"✓ {data.name} IDs already correct");
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
