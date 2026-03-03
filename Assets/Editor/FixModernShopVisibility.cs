using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace TPSBR.Editor
{
    public class FixModernShopVisibility
    {
        [MenuItem("TPSBR/🔧 Fix Modern Shop Visibility")]
        public static void Fix()
        {
            UnityEngine.SceneManagement.Scene menuScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Menu");
            if (!menuScene.IsValid() || !menuScene.isLoaded)
            {
                Debug.LogError("Menu scene is not loaded!");
                return;
            }

            GameObject modernShopObj = GameObject.Find("/MenuUI/ModernShop");
            
            if (modernShopObj == null)
            {
                Debug.LogWarning("ModernShop not found. Run 'TPSBR → 🎨 Create Modern Shop UI' first.");
                return;
            }

            CanvasGroup canvasGroup = modernShopObj.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log("✓ Fixed CanvasGroup settings");
            }

            modernShopObj.SetActive(false);
            Debug.Log("✓ Set ModernShop to inactive");

            EditorUtility.SetDirty(modernShopObj);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);

            Debug.Log("✅ Modern Shop visibility fixed!");
            Debug.Log("The UIView system will now properly show/hide the shop.");
            Debug.Log("Press Play and click SHOP button to test!");
        }
    }
}
