using UnityEngine;
using UnityEditor;
using TPSBR;
using TPSBR.UI;
using UnityEditor.SceneManagement;

public class FixShopSetup : EditorWindow
{
    [MenuItem("TPSBR/Fix Shop Setup")]
    public static void ShowWindow()
    {
        GetWindow<FixShopSetup>("Fix Shop Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Shop Setup Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This will fix the following issues:\n" +
            "1. Set correct Agent IDs in CharacterData\n" +
            "2. Assign UIShopItem prefab to UIList\n" +
            "3. Save the Menu scene",
            MessageType.Info
        );

        EditorGUILayout.Space();

        if (GUILayout.Button("Fix All Issues", GUILayout.Height(40)))
        {
            FixAllIssues();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Fix Character Agent IDs Only", GUILayout.Height(30)))
        {
            FixCharacterAgentIDs();
        }

        if (GUILayout.Button("Fix UIList Prefab Only", GUILayout.Height(30)))
        {
            FixUIListPrefab();
        }
    }

    private void FixAllIssues()
    {
        FixCharacterAgentIDs();
        FixUIListPrefab();
        Debug.Log("All shop issues fixed!");
    }

    private void FixCharacterAgentIDs()
    {
        string soldier66Path = "Assets/Scripts/CharacterData/soldier66.asset";
        CharacterData soldier66 = AssetDatabase.LoadAssetAtPath<CharacterData>(soldier66Path);
        
        if (soldier66 != null)
        {
            soldier66.characterID = "Agent.Soldier";
            soldier66.agentID = "Agent.Soldier";
            EditorUtility.SetDirty(soldier66);
            Debug.Log($"Fixed soldier66 - characterID: {soldier66.characterID}, agentID: {soldier66.agentID}");
        }
        else
        {
            Debug.LogWarning($"Could not find CharacterData at: {soldier66Path}");
        }

        string marinePath = "Assets/Scripts/CharacterData/marine.asset";
        CharacterData marine = AssetDatabase.LoadAssetAtPath<CharacterData>(marinePath);
        
        if (marine != null)
        {
            marine.characterID = "Agent.Marine";
            marine.agentID = "Agent.Marine";
            EditorUtility.SetDirty(marine);
            Debug.Log($"Fixed marine - characterID: {marine.characterID}, agentID: {marine.agentID}");
        }
        else
        {
            Debug.LogWarning($"Could not find CharacterData at: {marinePath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Character IDs fixed! Now they match AgentSettings.");
        Debug.Log("IMPORTANT: Exit Play Mode and re-enter to test!");
    }

    private void FixUIListPrefab()
    {
        var menuScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity", OpenSceneMode.Single);
        
        GameObject shopView = GameObject.Find("/MenuUI/UIShopView");
        if (shopView == null)
        {
            Debug.LogError("Could not find UIShopView in scene!");
            return;
        }

        Transform shopItemsList = shopView.transform.Find("Content/ShopItemsList");
        if (shopItemsList == null)
        {
            Debug.LogError("Could not find ShopItemsList!");
            return;
        }

        UIList uiList = shopItemsList.GetComponent<UIList>();
        if (uiList == null)
        {
            Debug.LogError("UIList component not found!");
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab");
        if (prefab == null)
        {
            Debug.LogError("Could not find UIShopItem prefab!");
            return;
        }

        SerializedObject so = new SerializedObject(uiList);
        SerializedProperty itemInstanceProp = so.FindProperty("_itemInstance");
        
        if (itemInstanceProp != null)
        {
            itemInstanceProp.objectReferenceValue = prefab;
            so.ApplyModifiedProperties();
            Debug.Log("Assigned UIShopItem prefab to UIList!");
        }

        EditorSceneManager.MarkSceneDirty(menuScene);
        EditorSceneManager.SaveScene(menuScene);
        Debug.Log("Menu scene saved!");
    }
}
