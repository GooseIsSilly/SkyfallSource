using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class RandomTeamAssignment : NetworkBehaviour
    {
        [SerializeField]
        private bool _autoAssignTeams = true;
        [SerializeField]
        private TeamMode _defaultTeamMode = TeamMode.Duo;

        private Dictionary<PlayerRef, bool> _assignedPlayers = new Dictionary<PlayerRef, bool>();

        public override void Spawned()
        {
            if (HasStateAuthority && _autoAssignTeams)
            {
                AssignPlayersToRandomTeams();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_autoAssignTeams)
                return;

            AssignNewPlayers();
        }

        private void AssignNewPlayers()
        {
            if (TeamManager.Instance == null)
                return;

            var networkGame = Runner.SimulationUnityScene.GetComponent<NetworkGame>(true);
            if (networkGame == null)
                return;

            foreach (var player in networkGame.ActivePlayers)
            {
                if (player == null)
                    continue;

                var playerRef = player.Object.InputAuthority;
                if (_assignedPlayers.ContainsKey(playerRef))
                    continue;

                AssignPlayerToRandomTeam(player);
                _assignedPlayers[playerRef] = true;
            }
        }

        private void AssignPlayersToRandomTeams()
        {
            if (TeamManager.Instance == null)
                return;

            TeamManager.Instance.SetTeamMode(_defaultTeamMode);

            var networkGame = Runner.SimulationUnityScene.GetComponent<NetworkGame>(true);
            if (networkGame == null)
                return;

            foreach (var player in networkGame.ActivePlayers)
            {
                if (player != null)
                {
                    AssignPlayerToRandomTeam(player);
                    _assignedPlayers[player.Object.InputAuthority] = true;
                }
            }
        }

        private void AssignPlayerToRandomTeam(Player player)
        {
            if (_defaultTeamMode == TeamMode.Solo)
            {
                TeamManager.Instance.CreateOrJoinTeam(player.UserID, player.Nickname);
                return;
            }

            var availableTeam = FindAvailableTeam();
            if (availableTeam != null)
            {
                TeamManager.Instance.CreateOrJoinTeam(player.UserID, player.Nickname, availableTeam.TeamID.ToString());
                Debug.Log($"Assigned player {player.Nickname} to existing team {availableTeam.TeamID}");
            }
            else
            {
                var newTeam = TeamManager.Instance.CreateOrJoinTeam(player.UserID, player.Nickname);
                Debug.Log($"Created new team {newTeam?.TeamID} for player {player.Nickname}");
            }
        }

        private TeamData FindAvailableTeam()
        {
            if (TeamManager.Instance == null)
                return null;

            var networkGame = Runner.SimulationUnityScene.GetComponent<NetworkGame>(true);
            if (networkGame == null)
                return null;

            var teamsToCheck = new Dictionary<byte, TeamData>();

            foreach (var player in networkGame.ActivePlayers)
            {
                if (player == null)
                    continue;

                var team = TeamManager.Instance.GetPlayerTeam(player.UserID);
                if (team != null && !team.IsFull(_defaultTeamMode))
                {
                    teamsToCheck[team.TeamID] = team;
                }
            }

            foreach (var team in teamsToCheck.Values)
            {
                if (!team.IsFull(_defaultTeamMode))
                {
                    return team;
                }
            }

            return null;
        }

        public void SetTeamMode(TeamMode mode)
        {
            if (HasStateAuthority)
            {
                _defaultTeamMode = mode;
                if (TeamManager.Instance != null)
                {
                    TeamManager.Instance.SetTeamMode(mode);
                }
            }
        }
    }
}
