using UnityEngine;
using TPSBR.UI;

namespace TPSBR
{
    [ExecuteInEditMode]
    public class ShopListFixer : MonoBehaviour
    {
        private void Awake()
        {
            UIList list = GetComponent<UIList>();
            if (list != null)
            {
                #if UNITY_EDITOR
                UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(list);
                UnityEditor.SerializedProperty prop = so.FindProperty("_itemInstance");
                
                if (prop.objectReferenceValue == null)
                {
                    GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
                    if (prefab != null)
                    {
                        prop.objectReferenceValue = prefab;
                        so.ApplyModifiedProperties();
                        Debug.Log("✅ ShopListFixer: Assigned UIShopItem prefab!");
                    }
                }
                #endif
            }
        }
    }
}
