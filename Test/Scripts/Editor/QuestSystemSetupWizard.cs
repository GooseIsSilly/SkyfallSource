using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace TPSBREditor
{
    public class QuestSystemSetupWizard : EditorWindow
    {
        private bool _setupComplete = false;
        private Vector2 _scrollPosition;

        [MenuItem("TPSBR/Quest System Setup Wizard")]
        public static void ShowWindow()
        {
            QuestSystemSetupWizard window = GetWindow<QuestSystemSetupWizard>("Quest System Setup");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(10);
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 18;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            
            EditorGUILayout.LabelField("Quest System Setup Wizard", titleStyle);
            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox("This wizard will help you set up the Quest System in your project.", MessageType.Info);
            EditorGUILayout.Space(10);

            DrawSetupSteps();
            DrawQuickActions();
            DrawTroubleshooting();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupSteps()
        {
            EditorGUILayout.LabelField("Setup Steps", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox("STEP 1: Initialize Core Systems", MessageType.None);
            if (GUILayout.Button("Create Quest System Initializer in Current Scene", GUILayout.Height(30)))
            {
                CreateQuestSystemInitializer();
            }
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox("STEP 2: Setup UI (Automated!)", MessageType.None);
            if (GUILayout.Button("AUTO-GENERATE QUEST UI (One Click!)", GUILayout.Height(40)))
            {
                QuestUIGenerator.GenerateQuestUI();
            }
            EditorGUILayout.LabelField("This will automatically create:", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• UIQuestView in Menu scene", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Quest Item Prefab", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• Quests button in main menu", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("Open QUEST_SYSTEM_README.md"))
            {
                OpenReadme();
            }
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox("STEP 3: Add Gameplay Hooks (Manual)", MessageType.None);
            EditorGUILayout.LabelField("• Add quest tracking calls to game events", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField("• See Integration section in README", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(10);
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (GUILayout.Button("Find Quest System Components", GUILayout.Height(25)))
            {
                FindQuestComponents();
            }

            if (GUILayout.Button("Test Quest System (Play Mode Only)", GUILayout.Height(25)))
            {
                TestQuestSystem();
            }

            if (GUILayout.Button("Clear Quest Save Data", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear Quest Data", 
                    "Are you sure you want to clear all saved quest data?", 
                    "Yes", "No"))
                {
                    ClearQuestData();
                }
            }

            EditorGUILayout.Space(10);
        }

        private void DrawTroubleshooting()
        {
            EditorGUILayout.LabelField("Troubleshooting", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Common Issues:\n\n" +
                "• Quests not tracking? Check quest integration patches are called in game code.\n" +
                "• UI not showing? Verify UIQuestView component and references.\n" +
                "• Coins not adding? Ensure CloudCoinManager is initialized.\n\n" +
                "See README for detailed troubleshooting.",
                MessageType.Info
            );

            if (GUILayout.Button("Validate Quest System Setup"))
            {
                ValidateSetup();
            }
        }

        private void CreateQuestSystemInitializer()
        {
            GameObject existing = GameObject.Find("QuestSystemInitializer");
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Already Exists", 
                    "QuestSystemInitializer already exists in the scene!", 
                    "OK");
                Selection.activeGameObject = existing;
                return;
            }

            GameObject questSystem = new GameObject("QuestSystemInitializer");
            questSystem.AddComponent<TPSBR.QuestSystemInitializer>();
            
            Undo.RegisterCreatedObjectUndo(questSystem, "Create Quest System Initializer");
            Selection.activeGameObject = questSystem;
            
            EditorUtility.DisplayDialog("Success", 
                "QuestSystemInitializer created successfully!\n\n" +
                "The system will auto-initialize on scene load.", 
                "OK");

            Debug.Log("[Quest System] QuestSystemInitializer created in scene: " + SceneManager.GetActiveScene().name);
        }

        private void FindQuestComponents()
        {
            Debug.Log("=== Quest System Components ===");
            
            var initializer = FindObjectOfType<TPSBR.QuestSystemInitializer>();
            Debug.Log($"QuestSystemInitializer: {(initializer != null ? "Found" : "NOT FOUND")}");
            
            if (Application.isPlaying)
            {
                Debug.Log($"QuestManager: {(TPSBR.QuestManager.Instance != null ? "Active" : "Not Active")}");
                Debug.Log($"CloudCoinManager: {(TPSBR.CloudCoinManager.Instance != null ? "Active" : "Not Active")}");
                Debug.Log($"QuestEventIntegration: {(TPSBR.QuestEventIntegration.Instance != null ? "Active" : "Not Active")}");
            }
            else
            {
                Debug.Log("Enter Play Mode to check runtime managers.");
            }

            var questViews = FindObjectsOfType<TPSBR.UI.UIQuestView>(true);
            Debug.Log($"UIQuestView components found: {questViews.Length}");

            Debug.Log("=== End Components ===");
        }

        private void TestQuestSystem()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Not in Play Mode", 
                    "Enter Play Mode to test the quest system.", 
                    "OK");
                return;
            }

            if (TPSBR.QuestManager.Instance != null)
            {
                TPSBR.QuestManager.Instance.UpdateQuestProgress(
                    TPSBR.QuestRequirementType.PlayMatches, 1);
                Debug.Log("[Quest Test] Simulated 'Play 1 Match' quest progress");
            }

            if (TPSBR.CloudCoinManager.Instance != null)
            {
                TPSBR.CloudCoinManager.Instance.AddCoins(100);
                Debug.Log($"[Quest Test] Added 100 test coins. Total: {TPSBR.CloudCoinManager.Instance.GetCurrentCoins()}");
            }
        }

        private void ClearQuestData()
        {
            PlayerPrefs.DeleteKey("PlayerQuestData");
            PlayerPrefs.Save();
            Debug.Log("[Quest System] Quest save data cleared!");
            
            EditorUtility.DisplayDialog("Data Cleared", 
                "Quest save data has been cleared from PlayerPrefs.", 
                "OK");
        }

        private void ValidateSetup()
        {
            bool allGood = true;
            string report = "=== Quest System Validation ===\n\n";

            var initializer = FindObjectOfType<TPSBR.QuestSystemInitializer>();
            if (initializer != null)
            {
                report += "✓ QuestSystemInitializer found in scene\n";
            }
            else
            {
                report += "✗ QuestSystemInitializer NOT found - Create it!\n";
                allGood = false;
            }

            var questViews = FindObjectsOfType<TPSBR.UI.UIQuestView>(true);
            if (questViews.Length > 0)
            {
                report += $"✓ UIQuestView found ({questViews.Length})\n";
            }
            else
            {
                report += "⚠ UIQuestView not found - Create quest UI\n";
            }

            if (Application.isPlaying)
            {
                if (TPSBR.QuestManager.Instance != null)
                    report += "✓ QuestManager initialized\n";
                else
                {
                    report += "✗ QuestManager not initialized\n";
                    allGood = false;
                }

                if (TPSBR.CloudCoinManager.Instance != null)
                    report += "✓ CloudCoinManager initialized\n";
                else
                {
                    report += "✗ CloudCoinManager not initialized\n";
                    allGood = false;
                }

                if (TPSBR.QuestEventIntegration.Instance != null)
                    report += "✓ QuestEventIntegration initialized\n";
                else
                {
                    report += "✗ QuestEventIntegration not initialized\n";
                    allGood = false;
                }
            }
            else
            {
                report += "\n⚠ Enter Play Mode to validate runtime systems\n";
            }

            report += "\n" + (allGood && Application.isPlaying ? 
                "=== All systems operational! ===" : 
                "=== Issues detected - check above ===");

            Debug.Log(report);
            EditorUtility.DisplayDialog("Validation Report", report, "OK");
        }

        private void OpenReadme()
        {
            string readmePath = "Assets/Scripts/QUEST_SYSTEM_README.md";
            var readme = AssetDatabase.LoadAssetAtPath<TextAsset>(readmePath);
            
            if (readme != null)
            {
                AssetDatabase.OpenAsset(readme);
            }
            else
            {
                EditorUtility.DisplayDialog("Not Found", 
                    "Could not find QUEST_SYSTEM_README.md\n\nExpected location: " + readmePath, 
                    "OK");
            }
        }
    }
}
