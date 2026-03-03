using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class INTEGRATION_EXAMPLES : MonoBehaviour
    {
        [Header("Quest Integration Examples")]
        [TextArea(5, 10)]
        [SerializeField] private string _note = 
            "This file contains code examples for integrating the quest system.\n" +
            "See QUEST_INTEGRATION_GUIDE.md for complete integration instructions.";

        public void ExampleMatchStart()
        {
            QuestIntegrationPatches.PatchGameplayModeActivated();
        }

        public void ExampleMatchEnd(int playerPosition, int totalPlayers, bool isWinner)
        {
            QuestIntegrationPatches.PatchGameplayModeFinished(playerPosition, totalPlayers, isWinner);
        }

        public void ExampleDamageDealt(Health health, HitData hitData)
        {
            QuestIntegrationPatches.PatchHealthDamage(health, hitData);
        }

        public void ExampleItemPickup(Agent agent)
        {
            QuestIntegrationPatches.PatchItemPickup(agent);
        }

        public void ExampleHealingUsed(Agent agent)
        {
            QuestIntegrationPatches.PatchHealingItemUsed(agent);
        }

        public void ExampleStormDamage(Agent agent)
        {
            QuestIntegrationPatches.PatchStormDamage(agent);
        }

        public void ExamplePlayerLanded(Agent agent, Vector3 position)
        {
            QuestIntegrationPatches.PatchPlayerLanded(agent, position);
        }

        public void ExampleStormCircleChanged(int circleNumber)
        {
            QuestIntegrationPatches.PatchShrinkingAreaChanged(circleNumber);
        }

        [ContextMenu("Test Quest System")]
        private void TestQuestSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Enter Play Mode to test the quest system!");
                return;
            }

            if (QuestManager.Instance != null)
            {
                Debug.Log("=== Testing Quest System ===");
                
                QuestManager.Instance.UpdateQuestProgress(QuestRequirementType.PlayMatches, 1);
                Debug.Log("✓ Simulated: Play 1 match");
                
                QuestManager.Instance.OnEliminationObtained(true, 15f, "AssaultRifle");
                Debug.Log("✓ Simulated: Headshot elimination");
                
                QuestManager.Instance.OnDamageDealt(100);
                Debug.Log("✓ Simulated: 100 damage dealt");
                
                var activeQuests = QuestManager.Instance.GetActiveQuests();
                Debug.Log($"✓ Active Quests: {activeQuests.Count}");
                
                Debug.Log("=== Test Complete ===");
            }
            else
            {
                Debug.LogError("QuestManager not initialized!");
            }
        }
    }
}
