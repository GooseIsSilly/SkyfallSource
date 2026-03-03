using UnityEngine;
using UnityEditor;
using TPSBR;

namespace TPSBR.UI.Editor
{
    public class ShopSystemSetupHelper : EditorWindow
    {
        [MenuItem("TPSBR/Shop System Setup Helper")]
        public static void ShowWindow()
        {
            GetWindow<ShopSystemSetupHelper>("Shop Setup Helper");
        }

        private int soldierCost = 0;
        private int marineCost = 500;

        private void OnGUI()
        {
            GUILayout.Label("Shop System Setup Helper", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps you quickly configure the shop system settings.",
                MessageType.Info
            );

            GUILayout.Space(10);

            GUILayout.Label("Agent Pricing Configuration", EditorStyles.boldLabel);
            soldierCost = EditorGUILayout.IntField("Soldier Cost (CloudCoins)", soldierCost);
            marineCost = EditorGUILayout.IntField("Marine Cost (CloudCoins)", marineCost);

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Agent Prices", GUILayout.Height(40)))
            {
                ApplyAgentPrices();
            }

            GUILayout.Space(20);

            EditorGUILayout.HelpBox(
                "Other setup tasks:\n" +
                "• Create UI prefabs using 'TPSBR/Create Shop UI'\n" +
                "• Add shop button to main menu manually\n" +
                "• Test using ShopSystemDebugHelper component",
                MessageType.Info
            );
        }

        private void ApplyAgentPrices()
        {
            AgentSettings agentSettings = Resources.Load<AgentSettings>("Settings/Gameplay/AgentSettings");
            
            if (agentSettings == null)
            {
                agentSettings = AssetDatabase.LoadAssetAtPath<AgentSettings>("Assets/TPSBR/Resources/Settings/Gameplay/AgentSettings.asset");
            }

            if (agentSettings == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:AgentSettings");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    agentSettings = AssetDatabase.LoadAssetAtPath<AgentSettings>(path);
                }
            }

            if (agentSettings == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find AgentSettings asset. Please locate it manually in the Project window and configure prices directly.", 
                    "OK");
                return;
            }

            SerializedObject so = new SerializedObject(agentSettings);
            SerializedProperty agentsArray = so.FindProperty("_agents");

            if (agentsArray == null || !agentsArray.isArray)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find agents array in AgentSettings. The asset structure may have changed.", 
                    "OK");
                return;
            }

            int updatedCount = 0;
            for (int i = 0; i < agentsArray.arraySize; i++)
            {
                SerializedProperty agentProp = agentsArray.GetArrayElementAtIndex(i);
                SerializedProperty idProp = agentProp.FindPropertyRelative("_agentID");
                SerializedProperty costProp = agentProp.FindPropertyRelative("_cloudCoinCost");

                if (idProp == null || costProp == null)
                    continue;

                string agentId = idProp.stringValue;

                if (agentId.Equals("Soldier", System.StringComparison.OrdinalIgnoreCase))
                {
                    costProp.intValue = soldierCost;
                    updatedCount++;
                    Debug.Log($"Set Soldier CloudCoinCost to {soldierCost}");
                }
                else if (agentId.Equals("Marine", System.StringComparison.OrdinalIgnoreCase))
                {
                    costProp.intValue = marineCost;
                    updatedCount++;
                    Debug.Log($"Set Marine CloudCoinCost to {marineCost}");
                }
            }

            if (updatedCount > 0)
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(agentSettings);
                AssetDatabase.SaveAssets();

                EditorUtility.DisplayDialog("Success", 
                    $"Updated {updatedCount} agent price(s) in AgentSettings.\n\n" +
                    $"Soldier: {soldierCost} CloudCoins\n" +
                    $"Marine: {marineCost} CloudCoins", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Warning", 
                    "No agents were updated. Make sure your AgentSettings has agents with IDs 'Soldier' and/or 'Marine'.", 
                    "OK");
            }
        }
    }
}
