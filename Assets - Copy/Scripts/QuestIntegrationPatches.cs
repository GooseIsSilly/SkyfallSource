using UnityEngine;
using Fusion;

namespace TPSBR
{
    public static class QuestIntegrationPatches
    {
        public static void PatchHealthDamage(Health health, HitData hitData)
        {
            if (health == null || QuestEventIntegration.Instance == null)
                return;

            if (hitData.Amount > 0 && hitData.InstigatorRef != PlayerRef.None)
            {
                int damage = Mathf.FloorToInt(hitData.Amount);
                QuestEventIntegration.Instance.OnDamageDealt(damage, hitData.InstigatorRef);
            }
        }

        public static void PatchStormDamage(Agent agent)
        {
            if (agent == null || QuestEventIntegration.Instance == null)
                return;

            if (agent.Object != null)
            {
                var playerRef = agent.Object.InputAuthority;
                QuestEventIntegration.Instance.OnStormDamageTaken(playerRef);
            }
        }

        public static void PatchItemPickup(Agent agent)
        {
            if (agent == null || QuestEventIntegration.Instance == null)
                return;

            if (agent.Object != null)
            {
                var playerRef = agent.Object.InputAuthority;
                QuestEventIntegration.Instance.OnItemPickedUp(playerRef);
            }
        }

        public static void PatchHealingItemUsed(Agent agent)
        {
            if (agent == null || QuestEventIntegration.Instance == null)
                return;

            if (agent.Object != null)
            {
                var playerRef = agent.Object.InputAuthority;
                QuestEventIntegration.Instance.OnHealingItemUsed(playerRef);
            }
        }

        public static void PatchPlayerLanded(Agent agent, Vector3 position)
        {
            if (agent == null || QuestEventIntegration.Instance == null)
                return;

            if (agent.Object != null)
            {
                var playerRef = agent.Object.InputAuthority;
                QuestEventIntegration.Instance.OnPlayerLanded(playerRef, position);
            }
        }

        public static void PatchGameplayModeActivated()
        {
            if (QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.OnGameplayModeActivated();
            }
        }

        public static void PatchGameplayModeFinished(int playerPosition, int totalPlayers, bool isWinner)
        {
            if (QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.OnGameplayModeFinished(playerPosition, totalPlayers, isWinner);
            }
        }

        public static void PatchShrinkingAreaChanged(int circleNumber)
        {
            if (QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.OnStormCircleChanged(circleNumber);
            }
        }
    }
}
