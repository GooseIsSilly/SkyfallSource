using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class GameplayQuestHooks : MonoBehaviour
    {
        private static GameplayQuestHooks _instance;
        
        [Header("Automatic Quest Tracking")]
        [SerializeField] private bool _trackMatchEvents = true;
        [SerializeField] private bool _trackCombatEvents = true;

        private GameplayMode _currentGameplayMode;
        private NetworkGame _networkGame;
        private bool _isMatchActive = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            FindGameSystems();
            RegisterEventListeners();
        }

        private void FindGameSystems()
        {
            _networkGame = FindObjectOfType<NetworkGame>();
            _currentGameplayMode = FindObjectOfType<GameplayMode>();

            if (_networkGame != null && QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.SetNetworkGame(_networkGame);
            }

            if (_currentGameplayMode != null && QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.SetGameplayMode(_currentGameplayMode);
            }
        }

        private void RegisterEventListeners()
        {
            if (_currentGameplayMode != null && _trackMatchEvents)
            {
                _currentGameplayMode.OnAgentDeath += OnAgentKilled;
                _currentGameplayMode.OnPlayerEliminated += OnPlayerEliminated;
            }
        }

        private void Update()
        {
            // Survival time and travel distance are tracked by QuestTracker via QuestEventIntegration.
        }

        public void OnMatchStarted()
        {
            if (!_trackMatchEvents) return;

            _isMatchActive = true;

            QuestIntegrationPatches.PatchGameplayModeActivated();
            Debug.Log("[Quest Hooks] Match started - Quest tracking active");
        }

        public void OnMatchEnded(int playerPosition, int totalPlayers, bool isWinner)
        {
            if (!_trackMatchEvents) return;

            _isMatchActive = false;

            QuestIntegrationPatches.PatchGameplayModeFinished(playerPosition, totalPlayers, isWinner);
            Debug.Log($"[Quest Hooks] Match ended - Position: {playerPosition}/{totalPlayers}, Winner: {isWinner}");
        }

        private void OnAgentKilled(KillData killData)
        {
            if (!_trackCombatEvents) return;

            Debug.Log($"[Quest Hooks] Agent killed - Killer: {killData.KillerRef}, Victim: {killData.VictimRef}, Headshot: {killData.Headshot}");
            
            if (QuestEventIntegration.Instance != null)
            {
                QuestEventIntegration.Instance.OnAgentKilled(killData);
            }
        }

        private void OnPlayerEliminated(PlayerRef playerRef)
        {
            Debug.Log($"[Quest Hooks] Player eliminated: {playerRef}");
        }

        public static void NotifyItemPickup(Agent agent)
        {
            QuestIntegrationPatches.PatchItemPickup(agent);
        }

        public static void NotifyHealingUsed(Agent agent)
        {
            QuestIntegrationPatches.PatchHealingItemUsed(agent);
        }

        public static void NotifyStormDamage(Agent agent)
        {
            QuestIntegrationPatches.PatchStormDamage(agent);
        }

        public static void NotifyDamageDealt(Health health, HitData hitData)
        {
            QuestIntegrationPatches.PatchHealthDamage(health, hitData);
        }

        public static void NotifyPlayerLanded(Agent agent, Vector3 position)
        {
            QuestIntegrationPatches.PatchPlayerLanded(agent, position);
        }

        public static void NotifyStormCircleChanged(int circleNumber)
        {
            QuestIntegrationPatches.PatchShrinkingAreaChanged(circleNumber);
        }

        private void OnDestroy()
        {
            if (_currentGameplayMode != null)
            {
                _currentGameplayMode.OnAgentDeath -= OnAgentKilled;
                _currentGameplayMode.OnPlayerEliminated -= OnPlayerEliminated;
            }
        }
    }
}
