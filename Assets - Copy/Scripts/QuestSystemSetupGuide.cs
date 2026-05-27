using UnityEngine;

namespace TPSBR
{
    public class QuestSystemSetupGuide : MonoBehaviour
    {
        [Header("Quest System Setup Guide")]
        [TextArea(10, 20)]
        [SerializeField] private string _setupInstructions = 
            "QUEST SYSTEM SETUP GUIDE\n\n" +
            "1. Use the Setup Wizard:\n" +
            "   Menu: TPSBR > Quest System Setup Wizard\n\n" +
            "2. Read the documentation:\n" +
            "   - QUEST_SYSTEM_README.md (Complete guide)\n" +
            "   - QUEST_INTEGRATION_GUIDE.md (Code integration)\n" +
            "   - QUEST_SYSTEM_SUMMARY.md (Quick overview)\n\n" +
            "3. Quick Start:\n" +
            "   - Add QuestSystemInitializer to scene\n" +
            "   - Create quest UI\n" +
            "   - Add integration patches\n" +
            "   - Test in Play Mode\n\n" +
            "All quest functionality is ready to use!";

        [ContextMenu("Open Setup Wizard")]
        private void OpenSetupWizard()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExecuteMenuItem("TPSBR/Quest System Setup Wizard");
            #endif
        }

        [ContextMenu("Show Documentation")]
        private void ShowDocumentation()
        {
            Debug.Log(
                "=== QUEST SYSTEM DOCUMENTATION ===\n\n" +
                "Main Files:\n" +
                "- QUEST_SYSTEM_README.md - Complete setup guide\n" +
                "- QUEST_INTEGRATION_GUIDE.md - Integration examples\n" +
                "- QUEST_SYSTEM_SUMMARY.md - Quick reference\n\n" +
                "All files located in: /Assets/Scripts/\n\n" +
                "Use TPSBR > Quest System Setup Wizard for guided setup!"
            );
        }
    }
}
