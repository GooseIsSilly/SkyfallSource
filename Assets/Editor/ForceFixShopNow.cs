using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;
using System.Linq;
using System.Reflection;

namespace TPSBR
{
    public class ForceFixShopNow : EditorWindow
    {
        [MenuItem("TPSBR/🔥 FORCE FIX SHOP (Click This!) 🔥")]
        public static void ShowWindow()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode First!", 
                    "⚠️ You must EXIT Play Mode before applying the fix!\n\n" +
                    "1. Click the STOP button (⏹️)\n" +
                    "2. Then run this menu item again", 
                    "OK");
                return;
            }

            FixShopManually();
        }

        private static void FixShopManually()
        {
            Debug.Log("🔧 Starting FORCE FIX...");

            var menuScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity", OpenSceneMode.Single);
            
            if (!menuScene.IsValid())
            {
                Debug.LogError("❌ Could not open Menu scene!");
                return;
            }

            Debug.Log("✓ Menu scene opened");

            var rootObjects = menuScene.GetRootGameObjects();
            GameObject menuUIObj = rootObjects.FirstOrDefault(obj => obj.name == "MenuUI");

            if (menuUIObj == null)
            {
                Debug.LogError("❌ MenuUI not found in scene!");
                return;
            }

            Debug.Log("✓ MenuUI found");

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
            {
                Debug.LogError("❌ UIShopView not found!");
                return;
            }

            Debug.Log("✓ UIShopView found");

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogError("❌ ShopItemsList not found!");
                return;
            }

            Debug.Log("✓ ShopItemsList found");

            UIList uiList = shopItemsListTransform.GetComponent<UIList>();
            if (uiList == null)
            {
                Debug.LogError("❌ UIList component not found!");
                return;
            }

            Debug.Log("✓ UIList component found");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (prefab == null)
            {
                Debug.LogError("❌ UIShopItem prefab not found!");
                Debug.LogError("Looking at: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                return;
            }

            Debug.Log("✓ UIShopItem prefab found");

            UIListItem listItemComponent = prefab.GetComponent<UIListItem>();
            if (listItemComponent == null)
            {
                Debug.LogError("❌ Prefab doesn't have UIListItem component!");
                Debug.Log("Trying to find UIShopItem component instead...");
                
                var shopItemComponent = prefab.GetComponent<UIShopItem>();
                if (shopItemComponent == null)
                {
                    Debug.LogError("❌ Prefab doesn't have UIShopItem component either!");
                    return;
                }
                
                Debug.Log("✓ Found UIShopItem component on prefab");
            }

            try
            {
                FieldInfo field = typeof(UIList).BaseType.GetField("_itemInstance", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (field == null)
                {
                    Debug.LogError("❌ Could not find _itemInstance field via reflection!");
                    Debug.Log("Trying SerializedObject approach...");
                    
                    SerializedObject so = new SerializedObject(uiList);
                    SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
                    
                    if (itemInstanceProp != null)
                    {
                        Debug.Log($"Current value: {itemInstanceProp.objectReferenceValue}");
                        itemInstanceProp.objectReferenceValue = listItemComponent != null ? listItemComponent : (Object)prefab;
                        so.ApplyModifiedPropertiesWithoutUndo();
                        Debug.Log($"New value: {itemInstanceProp.objectReferenceValue}");
                    }
                }
                else
                {
                    Debug.Log("✓ Found _itemInstance field via reflection");
                    object currentValue = field.GetValue(uiList);
                    Debug.Log($"Current value: {currentValue}");
                    
                    field.SetValue(uiList, listItemComponent != null ? listItemComponent : prefab.GetComponent<UIShopItem>());
                    
                    object newValue = field.GetValue(uiList);
                    Debug.Log($"New value: {newValue}");
                }

                EditorUtility.SetDirty(uiList);
                EditorUtility.SetDirty(shopItemsListTransform.gameObject);
                EditorSceneManager.MarkSceneDirty(menuScene);
                
                if (!EditorSceneManager.SaveScene(menuScene))
                {
                    Debug.LogError("❌ Failed to save scene!");
                    return;
                }

                Debug.Log("✓ Scene saved");

                Debug.Log("");
                Debug.Log("═══════════════════════════════════");
                Debug.Log("✅ SHOP FIXED SUCCESSFULLY!");
                Debug.Log("═══════════════════════════════════");
                Debug.Log("✅ UIShopItem prefab assigned");
                Debug.Log("✅ Scene saved");
                Debug.Log("");
                Debug.Log("🎮 NOW: Press Play and test the shop!");
                Debug.Log("");

                EditorUtility.DisplayDialog("✅ Shop Fixed!", 
                    "The shop has been fixed!\n\n" +
                    "✓ UIShopItem prefab assigned\n" +
                    "✓ Scene saved\n\n" +
                    "Press Play ▶️ and click SHOP to test!", 
                    "Great!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Exception during fix: {e.Message}");
                Debug.LogError(e.StackTrace);
            }
        }
    }
}
