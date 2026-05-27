using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;

namespace TPSBR
{
    [InitializeOnLoad]
    public static class ApplyShopFixNow
    {
        private static bool hasRunOnce = false;

        static ApplyShopFixNow()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || hasRunOnce)
                    return;

                UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/Menu.unity");
                if (!menuScene.isLoaded)
                    return;

                hasRunOnce = true;
                ApplyFix();
            };
        }

        [MenuItem("TPSBR/Apply Shop Fix Now")]
        public static void ApplyFixMenuItem()
        {
            ApplyFix();
        }

        private static void ApplyFix()
        {
            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/TPSBR/Scenes/Menu.unity");
            
            if (!menuScene.isLoaded)
            {
                Debug.LogWarning("Menu scene not loaded. Open it first.");
                return;
            }

            GameObject menuUIObj = GameObject.Find("MenuUI");
            if (menuUIObj == null)
            {
                Debug.LogWarning("MenuUI not found in scene");
                return;
            }

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
            {
                Debug.LogWarning("UIShopView not found");
                return;
            }

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogWarning("ShopItemsList not found");
                return;
            }

            UIList uiList = shopItemsListTransform.GetComponent<UIList>();
            if (uiList == null)
            {
                Debug.LogWarning("UIList component not found");
                return;
            }

            SerializedObject so = new SerializedObject(uiList);
            SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
            
            if (itemInstanceProp.objectReferenceValue == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                if (prefab != null)
                {
                    itemInstanceProp.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    EditorSceneManager.MarkSceneDirty(menuScene);
                    EditorSceneManager.SaveScene(menuScene);
                    Debug.Log("✅ SHOP FIXED! UIShopItem prefab assigned to UIList! Press Play to test!");
                }
                else
                {
                    Debug.LogError("UIShopItem prefab not found at Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                }
            }
            else
            {
                Debug.Log("✅ Shop already fixed! UIShopItem prefab is assigned. Press Play to test!");
            }
        }
    }
}
