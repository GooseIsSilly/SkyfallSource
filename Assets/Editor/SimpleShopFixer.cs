using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TPSBR.UI;

namespace TPSBR
{
    public class SimpleShopFixer
    {
        [MenuItem("TPSBR/✨ Simple Shop Fix ✨")]
        public static void FixShop()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("⚠️ EXIT PLAY MODE FIRST!");
                return;
            }

            Debug.Log("Starting simple fix...");

            GameObject menuUI = GameObject.Find("MenuUI");
            if (menuUI == null)
            {
                Debug.LogError("MenuUI not found. Is Menu scene open?");
                return;
            }

            Transform shopItemsList = menuUI.transform.Find("UIShopView/Content/ShopItemsList");
            if (shopItemsList == null)
            {
                Debug.LogError("ShopItemsList not found!");
                return;
            }

            UIList list = shopItemsList.GetComponent<UIList>();
            if (list == null)
            {
                Debug.LogError("UIList component not found!");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
            if (prefab == null)
            {
                Debug.LogError("UIShopItem prefab not found!");
                return;
            }

            UIShopItem shopItemComponent = prefab.GetComponent<UIShopItem>();
            if (shopItemComponent == null)
            {
                Debug.LogError("UIShopItem component not found on prefab!");
                return;
            }

            Debug.Log("All components found. Attempting assignment...");

            Undo.RecordObject(list, "Assign UIShopItem");
            
            SerializedObject so = new SerializedObject(list);
            SerializedProperty prop = so.FindProperty("_itemInstance");
            
            if (prop == null)
            {
                Debug.LogError("_itemInstance property not found!");
                return;
            }

            Debug.Log($"Property type: {prop.propertyType}");
            Debug.Log($"Current value: {prop.objectReferenceValue}");

            prop.objectReferenceValue = shopItemComponent;
            
            bool applied = so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"ApplyModifiedProperties result: {applied}");

            if (applied)
            {
                EditorUtility.SetDirty(list);
                EditorSceneManager.MarkSceneDirty(shopItemsList.gameObject.scene);
                EditorSceneManager.SaveOpenScenes();

                Debug.Log("═══════════════════════════════════");
                Debug.Log("✅ SUCCESS!");
                Debug.Log("═══════════════════════════════════");
                Debug.Log("Press Play and test the shop!");
                
                EditorUtility.DisplayDialog("Success!", "Shop fixed! Press Play to test.", "OK");
            }
            else
            {
                Debug.LogError("Failed to apply changes.");
                Debug.LogError("═══════════════════════════════════");
                Debug.LogError("MANUAL FIX REQUIRED:");
                Debug.LogError("═══════════════════════════════════");
                Debug.LogError("1. Select: MenuUI/UIShopView/Content/ShopItemsList");
                Debug.LogError("2. Find UIList component in Inspector");
                Debug.LogError("3. Drag UIShopItem prefab to '_itemInstance' field");
                Debug.LogError("4. Save scene (Ctrl+S)");
                
                EditorUtility.DisplayDialog("Manual Fix Needed",
                    "Automatic fix failed. Please:\n\n" +
                    "1. Select: MenuUI/UIShopView/Content/ShopItemsList\n" +
                    "2. In Inspector, find UIList component\n" +
                    "3. Drag UIShopItem prefab to '_itemInstance' field\n" +
                    "4. Save scene (Ctrl+S)\n\n" +
                    "Or open: TPSBR → Manual Shop Fix Guide",
                    "OK");
            }
        }
    }
}
