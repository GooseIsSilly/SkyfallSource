using Fusion;
using UnityEngine;
using TPSBR.UI;

namespace TPSBR
{
    public class TeamSystemNetworkSetup : NetworkBehaviour
    {
        [Header("Team Settings")]
        [SerializeField]
        private bool _enableTeamSystem = true;
        [SerializeField]
        private TeamMode _teamMode = TeamMode.Duo;
        [SerializeField]
        private bool _autoAssignTeams = true;

        [Header("Revive Settings")]
        [SerializeField]
        private bool _enableReviveSystem = true;

        [Header("Debug")]
        [SerializeField]
        private bool _enableDebugLogs = true;

        private TeamManager _teamManager;
        private RandomTeamAssignment _randomTeamAssignment;
        private ReviveIntegration _reviveIntegration;
        private ReviveInteraction _reviveInteraction;
        private bool _initialized;
        private static bool _uiCreated = false;

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;

            if (!_enableTeamSystem)
            {
                LogDebug("Team System is disabled");
                return;
            }

            InitializeTeamSystem();
        }

        private void InitializeTeamSystem()
        {
            if (_initialized)
                return;

            _initialized = true;
            LogDebug($"Initializing Team System (Mode: {_teamMode}, Revive: {_enableReviveSystem})");

            SpawnTeamManager();
            
            if (_autoAssignTeams)
            {
                AddRandomTeamAssignment();
            }

            if (_enableReviveSystem)
            {
                AddReviveIntegration();
                AddReviveInteraction();
            }

            Debug.Log($"[TeamSystemNetworkSetup] ✅ Initialization complete! Revive system is {"enabled".ToUpper()}!");
        }

        private void SpawnTeamManager()
        {
            if (TeamManager.Instance != null)
            {
                _teamManager = TeamManager.Instance;
                LogDebug("Using existing TeamManager instance");
                return;
            }

            var teamManagerPrefab = Resources.Load<GameObject>("TeamManager");
            if (teamManagerPrefab != null)
            {
                var spawned = Runner.Spawn(teamManagerPrefab);
                _teamManager = spawned.GetComponent<TeamManager>();
                LogDebug("Spawned TeamManager from Resources");
            }
            else
            {
                var go = new GameObject("TeamManager");
                var networkObject = go.AddComponent<NetworkObject>();
                _teamManager = go.AddComponent<TeamManager>();
                Runner.Spawn(networkObject);
                LogDebug("Created and spawned new TeamManager");
            }

            if (_teamManager != null)
            {
                _teamManager.SetTeamMode(_teamMode);
                LogDebug($"TeamManager set to {_teamMode} mode");
            }
        }

        private void AddRandomTeamAssignment()
        {
            _randomTeamAssignment = gameObject.GetComponent<RandomTeamAssignment>();
            
            if (_randomTeamAssignment == null)
            {
                _randomTeamAssignment = gameObject.AddComponent<RandomTeamAssignment>();
                LogDebug("Added RandomTeamAssignment component");
            }
            else
            {
                LogDebug("RandomTeamAssignment component already exists");
            }
        }

        private void AddReviveIntegration()
        {
            _reviveIntegration = gameObject.GetComponent<ReviveIntegration>();
            
            if (_reviveIntegration == null)
            {
                _reviveIntegration = gameObject.AddComponent<ReviveIntegration>();
                LogDebug("Added ReviveIntegration component");
            }
            else
            {
                LogDebug("ReviveIntegration component already exists");
            }

            if (_reviveIntegration != null)
            {
                _reviveIntegration.SetReviveSystemEnabled(true);
            }
        }

        private void AddReviveInteraction()
        {
            _reviveInteraction = FindObjectOfType<ReviveInteraction>();
            
            if (_reviveInteraction == null)
            {
                var go = new GameObject("ReviveInteraction");
                go.transform.SetParent(transform);
                _reviveInteraction = go.AddComponent<ReviveInteraction>();
                
                var layerMask = LayerMask.GetMask("Agent");
                var playerLayerField = _reviveInteraction.GetType().GetField("_playerLayer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (playerLayerField != null)
                {
                    playerLayerField.SetValue(_reviveInteraction, layerMask);
                }
                
                LogDebug("Created ReviveInteraction component");
            }
            else
            {
                LogDebug("ReviveInteraction component already exists");
            }
        }

        public void SetTeamMode(TeamMode mode)
        {
            _teamMode = mode;
            
            if (_teamManager != null)
            {
                _teamManager.SetTeamMode(mode);
                LogDebug($"Team mode changed to {mode}");
            }
        }

        public void SetReviveSystemEnabled(bool enabled)
        {
            _enableReviveSystem = enabled;
            
            if (_reviveIntegration != null)
            {
                _reviveIntegration.SetReviveSystemEnabled(enabled);
                LogDebug($"Revive system {(enabled ? "enabled" : "disabled")}");
            }
        }

        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[TeamSystemNetworkSetup] {message}");
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _initialized = false;
            base.Despawned(runner, hasState);
        }
    }
}
