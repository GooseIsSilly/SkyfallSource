using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;
using System.Linq;

namespace TPSBR
{
    public class FinalCompleteShopFix
    {
        [MenuItem("TPSBR/⭐ COMPLETE SHOP FIX (RUN THIS!) ⭐")]
        public static void FixEverything()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode!", 
                    "Please exit Play Mode before running this fix!", 
                    "OK");
                return;
            }

            Debug.Log("🔧 Starting COMPLETE shop fix...");
            Debug.Log("");

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
                Debug.LogError("❌ MenuUI not found!");
                return;
            }

            Debug.Log("✓ MenuUI found");

            Transform shopViewTransform = menuUIObj.transform.Find("UIShopView");
            if (shopViewTransform == null)
            {
                Debug.LogError("❌ UIShopView not found!");
                return;
            }

            UIShopView shopView = shopViewTransform.GetComponent<UIShopView>();
            if (shopView == null)
            {
                Debug.LogError("❌ UIShopView component not found!");
                return;
            }

            Debug.Log("✓ UIShopView found");

            Transform shopItemsListTransform = shopViewTransform.Find("Content/ShopItemsList");
            if (shopItemsListTransform == null)
            {
                Debug.LogError("❌ ShopItemsList not found!");
                return;
            }

            GameObject shopItemsListObj = shopItemsListTransform.gameObject;
            Debug.Log("✓ ShopItemsList found");

            UIShopList shopList = shopItemsListObj.GetComponent<UIShopList>();
            if (shopList == null)
            {
                Debug.Log("⚠️ UIShopList not found, checking for UIList...");
                
                UIList oldList = shopItemsListObj.GetComponent<UIList>();
                if (oldList != null)
                {
                    Debug.Log("✓ Found UIList, will replace with UIShopList");
                    
                    Undo.DestroyObjectImmediate(oldList);
                    Debug.Log("✓ Removed UIList component");
                }

                shopList = Undo.AddComponent<UIShopList>(shopItemsListObj);
                Debug.Log("✓ Added UIShopList component");
            }
            else
            {
                Debug.Log("✓ UIShopList already exists");
            }

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

            Debug.Log("✓ UIShopItem prefab found");

            SerializedObject listSO = new SerializedObject(shopList);
            SerializedProperty itemInstanceProp = listSO.FindProperty("_itemInstance");
            
            if (itemInstanceProp != null)
            {
                itemInstanceProp.objectReferenceValue = shopItemComponent;
                listSO.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("✓ Assigned UIShopItem prefab to UIShopList");
            }
            else
            {
                Debug.LogError("❌ Could not find _itemInstance property!");
                return;
            }

            SerializedObject shopViewSO = new SerializedObject(shopView);
            SerializedProperty shopItemsListProp = shopViewSO.FindProperty("_shopItemsList");
            
            if (shopItemsListProp != null)
            {
                Debug.Log($"Current _shopItemsList value: {shopItemsListProp.objectReferenceValue}");
                
                shopItemsListProp.objectReferenceValue = shopList;
                shopViewSO.ApplyModifiedPropertiesWithoutUndo();
                
                Debug.Log($"New _shopItemsList value: {shopItemsListProp.objectReferenceValue}");
                Debug.Log("✓ Linked UIShopView to UIShopList");
            }
            else
            {
                Debug.LogError("❌ Could not find _shopItemsList property on UIShopView!");
                return;
            }

            EditorUtility.SetDirty(shopList);
            EditorUtility.SetDirty(shopView);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);

            Debug.Log("");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("✅✅✅ SHOP COMPLETELY FIXED! ✅✅✅");
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("✅ UIShopList component configured");
            Debug.Log("✅ UIShopItem prefab assigned");
            Debug.Log("✅ UIShopView linked to list");
            Debug.Log("✅ Scene saved");
            Debug.Log("");
            Debug.Log("🎮 Press Play ▶️ and click SHOP!");
            Debug.Log("You should now see Soldier66 and Marine!");
            Debug.Log("");

            EditorUtility.DisplayDialog("✅ COMPLETE FIX APPLIED!", 
                "The shop is now fully configured!\n\n" +
                "✓ UIShopList component\n" +
                "✓ UIShopItem prefab assigned\n" +
                "✓ UIShopView → UIShopList linked\n" +
                "✓ Scene saved\n\n" +
                "Press Play ▶️ and test the shop!\n" +
                "You should see character cards!", 
                "Let's Go!");
        }
    }
}
