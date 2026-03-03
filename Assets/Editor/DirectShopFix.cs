using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;

namespace TPSBR
{
    [InitializeOnLoad]
    public class DirectShopFix
    {
        static DirectShopFix()
        {
            EditorApplication.delayCall += TryAutoFix;
        }

        private static void TryAutoFix()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/Menu.unity");
            
            if (!menuScene.isLoaded)
                return;

            GameObject menuUIObj = GameObject.Find("MenuUI");
            if (menuUIObj == null)
                return;

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
                return;

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
                return;

            UIList uiList = shopItemsListTransform.GetComponent<UIList>();
            if (uiList == null)
                return;

            SerializedObject so = new SerializedObject(uiList);
            SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
            
            if (itemInstanceProp.objectReferenceValue != null)
                return;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (prefab == null)
                return;

            itemInstanceProp.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(uiList);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);
            
            Debug.Log("✅ AUTO-FIX APPLIED!");
            Debug.Log("✅ UIShopItem now inherits from UIListItemBase");
            Debug.Log("✅ UIShopItem prefab assigned to UIList");
            Debug.Log("✅ Scene saved");
            Debug.Log("🎮 Press Play to test the shop!");
        }

        [MenuItem("TPSBR/FIX SHOP NOW ⚡", priority = 0)]
        public static void FixShopNow()
        {
            Debug.Log("🔧 Applying Shop Fix...");

            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/Menu.unity");
            
            if (!menuScene.isLoaded)
            {
                EditorUtility.DisplayDialog("Scene Not Loaded", 
                    "Please open the Menu scene first!\n\nGo to: Assets/TPSBR/Scenes/Menu.unity", 
                    "OK");
                return;
            }

            GameObject menuUIObj = GameObject.Find("MenuUI");
            if (menuUIObj == null)
            {
                Debug.LogError("MenuUI not found!");
                return;
            }

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
            {
                Debug.LogError("UIShopView not found!");
                return;
            }

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogError("ShopItemsList not found!");
                return;
            }

            UIList uiList = shopItemsListTransform.GetComponent<UIList>();
            if (uiList == null)
            {
                Debug.LogError("UIList component not found!");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (prefab == null)
            {
                Debug.LogError("UIShopItem prefab not found at: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                return;
            }

            SerializedObject so = new SerializedObject(uiList);
            SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
            
            itemInstanceProp.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(uiList);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);
            
            Debug.Log("✅ SHOP FIXED!");
            Debug.Log("✅ UIShopItem prefab assigned to UIList");
            Debug.Log("✅ Scene saved");
            Debug.Log("🎮 Press PLAY to test!");
            
            EditorUtility.DisplayDialog("Shop Fixed!", 
                "✅ UIShopItem prefab has been assigned!\n✅ Scene saved!\n\n" +
                "Press Play ▶️ to test the shop!", 
                "Got It!");
        }
    }
}
