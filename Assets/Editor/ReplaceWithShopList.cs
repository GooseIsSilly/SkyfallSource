using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;
using System.Linq;

namespace TPSBR
{
    public class ReplaceWithShopList
    {
        [MenuItem("TPSBR/🎯 FINAL FIX - Replace UIList with UIShopList")]
        public static void ReplaceComponent()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode!", 
                    "Please exit Play Mode first!", 
                    "OK");
                return;
            }

            Debug.Log("🔧 Starting component replacement...");

            var menuScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity", OpenSceneMode.Single);
            
            if (!menuScene.IsValid())
            {
                Debug.LogError("❌ Could not open Menu scene!");
                return;
            }

            var rootObjects = menuScene.GetRootGameObjects();
            GameObject menuUIObj = rootObjects.FirstOrDefault(obj => obj.name == "MenuUI");

            if (menuUIObj == null)
            {
                Debug.LogError("❌ MenuUI not found!");
                return;
            }

            Transform shopItemsListTransform = menuUIObj.transform.Find("UIShopView/Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogError("❌ ShopItemsList not found!");
                return;
            }

            GameObject shopItemsListObj = shopItemsListTransform.gameObject;

            UIList oldList = shopItemsListObj.GetComponent<UIList>();
            if (oldList == null)
            {
                Debug.Log("✓ UIList not found, checking if UIShopList already exists...");
                
                UIShopList existing = shopItemsListObj.GetComponent<UIShopList>();
                if (existing != null)
                {
                    Debug.Log("✓ UIShopList already exists!");
                    AssignPrefab(existing, menuScene);
                    return;
                }
                
                Debug.LogError("❌ Neither UIList nor UIShopList found!");
                return;
            }

            Debug.Log("✓ Found UIList, preparing to replace...");

            bool allowSelection = oldList.GetType().GetProperty("AllowSelection")?.GetValue(oldList) as bool? ?? true;
            bool allowDeselection = oldList.GetType().GetProperty("AllowDeselection")?.GetValue(oldList) as bool? ?? true;

            Undo.DestroyObjectImmediate(oldList);
            Debug.Log("✓ Removed old UIList component");

            UIShopList newList = Undo.AddComponent<UIShopList>(shopItemsListObj);
            Debug.Log("✓ Added UIShopList component");

            var allowSelProp = newList.GetType().GetProperty("AllowSelection");
            var allowDesProp = newList.GetType().GetProperty("AllowDeselection");
            
            if (allowSelProp != null) allowSelProp.SetValue(newList, allowSelection);
            if (allowDesProp != null) allowDesProp.SetValue(newList, allowDeselection);

            AssignPrefab(newList, menuScene);
        }

        private static void AssignPrefab(UIShopList shopList, UnityEngine.SceneManagement.Scene scene)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (prefab == null)
            {
                Debug.LogError("❌ UIShopItem prefab not found!");
                return;
            }

            UIShopItem shopItemComponent = prefab.GetComponent<UIShopItem>();
            if (shopItemComponent == null)
            {
                Debug.LogError("❌ UIShopItem component not found on prefab!");
                return;
            }

            Debug.Log("✓ Found UIShopItem prefab and component");

            SerializedObject so = new SerializedObject(shopList);
            SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
            
            if (itemInstanceProp == null)
            {
                Debug.LogError("❌ _itemInstance property not found!");
                return;
            }

            itemInstanceProp.objectReferenceValue = shopItemComponent;
            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log("✓ Assigned UIShopItem to _itemInstance");

            EditorUtility.SetDirty(shopList);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("");
            Debug.Log("═══════════════════════════════════");
            Debug.Log("✅✅✅ SHOP FIXED FOR REAL! ✅✅✅");
            Debug.Log("═══════════════════════════════════");
            Debug.Log("✅ Replaced UIList with UIShopList");
            Debug.Log("✅ Assigned UIShopItem prefab");
            Debug.Log("✅ Scene saved");
            Debug.Log("");
            Debug.Log("🎮 Press Play and test the shop NOW!");
            Debug.Log("");

            EditorUtility.DisplayDialog("✅ FIXED!", 
                "The shop is now properly configured!\n\n" +
                "✓ UIShopList component added\n" +
                "✓ UIShopItem prefab assigned\n" +
                "✓ Scene saved\n\n" +
                "Press Play ▶️ and test!", 
                "Awesome!");
        }
    }
}
