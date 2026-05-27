using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TPSBR;
using TPSBR.UI;

namespace TPSBREditor
{
    public class UpdateModernShopCloseButton
    {
        [MenuItem("TPSBR/🔧 Fix Modern Shop Close Button")]
        public static void FixCloseButton()
        {
            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Menu");
            
            if (!menuScene.isLoaded)
            {
                Debug.LogError("Menu scene is not loaded!");
                return;
            }

            GameObject modernShopObj = null;
            foreach (GameObject root in menuScene.GetRootGameObjects())
            {
                Transform found = root.transform.Find("MenuUI/ModernShop");
                if (found == null)
                {
                    found = FindInChildren(root.transform, "ModernShop");
                }
                
                if (found != null)
                {
                    modernShopObj = found.gameObject;
                    break;
                }
            }

            if (modernShopObj == null)
            {
                Debug.LogError("ModernShop GameObject not found in Menu scene!");
                Debug.Log("Please create it first using: TPSBR → 🎨 Create Modern Shop UI");
                return;
            }

            RectTransform shopRect = modernShopObj.GetComponent<RectTransform>();
            if (shopRect != null && shopRect.localScale != Vector3.one)
            {
                shopRect.localScale = Vector3.one;
                Debug.Log($"✓ Fixed scale from {shopRect.localScale} to (1, 1, 1)");
            }

            Transform closeButtonTransform = modernShopObj.transform.Find("Content/Header/CloseButton");
            
            if (closeButtonTransform == null)
            {
                Debug.LogError("CloseButton not found in ModernShop/Content/Header!");
                return;
            }

            GameObject closeButtonObj = closeButtonTransform.gameObject;
            
            Button oldButton = closeButtonObj.GetComponent<Button>();
            if (oldButton != null)
            {
                Object.DestroyImmediate(oldButton);
                Debug.Log("✓ Removed old Unity Button component");
            }

            UIButton uiButton = closeButtonObj.GetComponent<UIButton>();
            if (uiButton == null)
            {
                uiButton = closeButtonObj.AddComponent<UIButton>();
                Debug.Log("✓ Added UIButton component");
            }

            ModernShopManager shopManager = modernShopObj.GetComponent<ModernShopManager>();
            if (shopManager != null)
            {
                SerializedObject so = new SerializedObject(shopManager);
                so.FindProperty("_closeButton").objectReferenceValue = uiButton;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("✓ Assigned UIButton to ModernShopManager");
            }

            EditorUtility.SetDirty(modernShopObj);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);

            Debug.Log("✅ Modern Shop fixed!");
            Debug.Log("✓ Scale set to (1, 1, 1)");
            Debug.Log("✓ Close button now uses UIButton");
            Debug.Log("✓ ModernShopManager updated");
            Debug.Log("\n▶ Test: Press Play and click the close button (✕)");
        }

        private static Transform FindInChildren(Transform parent, string name)
        {
            if (parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                Transform result = FindInChildren(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
