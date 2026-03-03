using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class TeamGameplayIntegration : ContextBehaviour
    {
        [SerializeField]
        private TeamManager _teamManagerPrefab;

        private TeamManager _teamManager;
        private bool _initialized;

        public override void FixedUpdateNetwork()
        {
            if (!_initialized && Context != null && Context.Runner != null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            _initialized = true;

            if (Context.Runner != null && Context.Runner.IsServer)
            {
                SpawnTeamManager();
            }

            if (Context.GameplayMode != null)
            {
                Context.GameplayMode.OnAgentDeath += OnAgentDeath;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Context != null && Context.GameplayMode != null)
            {
                Context.GameplayMode.OnAgentDeath -= OnAgentDeath;
            }

            base.Despawned(runner, hasState);
        }

        private void SpawnTeamManager()
        {
            if (TeamManager.Instance != null)
            {
                _teamManager = TeamManager.Instance;
                return;
            }
            
            if (_teamManagerPrefab != null && Context.Runner != null)
            {
                var managerObject = Context.Runner.Spawn(_teamManagerPrefab);
                _teamManager = managerObject.GetComponent<TeamManager>();
            }
        }

        private void OnAgentDeath(KillData killData)
        {
            if (_teamManager == null)
                return;

            UpdateTeamMemberStatus(killData.VictimRef, false);

            CheckTeamElimination(killData.VictimRef);
        }

        private void UpdateTeamMemberStatus(PlayerRef playerRef, bool isAlive)
        {
            if (Context.NetworkGame == null || _teamManager == null)
                return;

            var player = Context.NetworkGame.GetPlayer(playerRef);
            if (player == null)
                return;

            byte teamID = _teamManager.GetPlayerTeamID(player.UserID);
            if (teamID != 0)
            {
                var teamMembers = _teamManager.GetTeamMembers(teamID);
                foreach (var member in teamMembers)
                {
                }
            }
        }

        private void CheckTeamElimination(PlayerRef playerRef)
        {
            if (Context.NetworkGame == null || _teamManager == null)
                return;

            var player = Context.NetworkGame.GetPlayer(playerRef);
            if (player == null)
                return;

            byte teamID = _teamManager.GetPlayerTeamID(player.UserID);
            if (teamID == 0)
                return;

            var teamMembers = _teamManager.GetTeamMembers(teamID);
            bool anyAlive = false;

            foreach (var member in teamMembers)
            {
                if (member.PlayerRef.IsRealPlayer)
                {
                    var memberPlayer = Context.NetworkGame.GetPlayer(member.PlayerRef);
                    if (memberPlayer != null && memberPlayer.Statistics.IsAlive)
                    {
                        anyAlive = true;
                        break;
                    }
                }
            }

            if (!anyAlive)
            {
                Debug.Log($"Team {teamID} has been eliminated!");
            }
        }

        public bool AreTeammates(PlayerRef playerRef1, PlayerRef playerRef2)
        {
            if (_teamManager == null || Context.NetworkGame == null)
                return false;

            var player1 = Context.NetworkGame.GetPlayer(playerRef1);
            var player2 = Context.NetworkGame.GetPlayer(playerRef2);

            if (player1 == null || player2 == null)
                return false;

            return _teamManager.AreTeammates(player1.UserID, player2.UserID);
        }

        public byte GetPlayerTeamID(PlayerRef playerRef)
        {
            if (_teamManager == null || Context.NetworkGame == null)
                return 0;

            var player = Context.NetworkGame.GetPlayer(playerRef);
            if (player == null)
                return 0;

            return _teamManager.GetPlayerTeamID(player.UserID);
        }
    }
}
