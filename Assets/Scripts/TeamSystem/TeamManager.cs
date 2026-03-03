using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class TeamManager : NetworkBehaviour
    {
        public static TeamManager Instance { get; private set; }

        public Action<TeamData> OnTeamUpdated;
        public Action<string> OnPlayerReadyChanged;
        public Action<TeamMode> OnTeamModeChanged;

        [Networked, Capacity(TeamData.MAX_TEAMS * TeamData.MAX_TEAM_SIZE)]
        private NetworkArray<TeamMember> NetworkedTeamMembers { get; }

        [Networked]
        private TeamMode CurrentTeamMode { get; set; }

        [Networked, Capacity(TeamData.MAX_TEAMS * TeamData.MAX_TEAM_SIZE)]
        private NetworkArray<NetworkBool> ReadyStates { get; }

        private Dictionary<string, TeamData> _localTeams = new Dictionary<string, TeamData>();
        private Dictionary<string, byte> _userIDToTeamID = new Dictionary<string, byte>();
        private byte _nextTeamID = 1;

        public TeamMode GetTeamMode() => CurrentTeamMode;

        public override void Spawned()
        {
            if (Instance != null && Instance != this)
            {
                Runner.Despawn(Object);
                return;
            }

            Instance = this;

            if (HasStateAuthority)
            {
                CurrentTeamMode = TeamMode.Solo;
            }

            Runner.SetIsSimulated(Object, true);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetTeamMode(TeamMode mode)
        {
            if (HasStateAuthority)
            {
                CurrentTeamMode = mode;
                OnTeamModeChanged?.Invoke(mode);
            }
            else
            {
                RPC_SetTeamMode(mode);
            }
        }

        public TeamData CreateOrJoinTeam(string userID, string nickname, string teamID = null)
        {
            if (!HasStateAuthority)
            {
                RPC_CreateOrJoinTeam(userID, nickname, teamID);
                return null;
            }

            TeamData team;

            if (!string.IsNullOrEmpty(teamID) && _localTeams.TryGetValue(teamID, out team))
            {
                if (!team.IsFull(CurrentTeamMode) && team.AddMember(userID))
                {
                    _userIDToTeamID[userID] = team.TeamID;
                    SyncTeamToNetwork(team);
                    OnTeamUpdated?.Invoke(team);
                    return team;
                }
                return null;
            }

            team = new TeamData
            {
                TeamID = _nextTeamID++,
                PartyLeaderUserID = userID
            };
            team.AddMember(userID);

            _localTeams[team.TeamID.ToString()] = team;
            _userIDToTeamID[userID] = team.TeamID;

            SyncTeamToNetwork(team);
            OnTeamUpdated?.Invoke(team);

            return team;
        }

        public bool LeaveTeam(string userID)
        {
            if (!HasStateAuthority)
            {
                RPC_LeaveTeam(userID);
                return false;
            }

            if (!_userIDToTeamID.TryGetValue(userID, out byte teamID))
                return false;

            string teamKey = teamID.ToString();
            if (!_localTeams.TryGetValue(teamKey, out TeamData team))
                return false;

            team.RemoveMember(userID);
            _userIDToTeamID.Remove(userID);

            if (team.MemberUserIDs.Count == 0)
            {
                _localTeams.Remove(teamKey);
            }
            else
            {
                SyncTeamToNetwork(team);
            }

            OnTeamUpdated?.Invoke(team);
            return true;
        }

        public void SetPlayerReady(string userID, bool ready)
        {
            if (!HasStateAuthority)
            {
                RPC_SetPlayerReady(userID, ready);
                return;
            }

            if (_userIDToTeamID.TryGetValue(userID, out byte teamID))
            {
                int index = GetMemberIndex(teamID, userID);
                if (index >= 0 && index < ReadyStates.Length)
                {
                    ReadyStates.Set(index, ready);
                    OnPlayerReadyChanged?.Invoke(userID);
                }
            }
        }

        public bool IsPlayerReady(string userID)
        {
            if (_userIDToTeamID.TryGetValue(userID, out byte teamID))
            {
                int index = GetMemberIndex(teamID, userID);
                if (index >= 0 && index < ReadyStates.Length)
                {
                    return ReadyStates[index];
                }
            }
            return false;
        }

        public bool IsTeamReady(byte teamID)
        {
            string teamKey = teamID.ToString();
            if (!_localTeams.TryGetValue(teamKey, out TeamData team))
                return false;

            foreach (var userID in team.MemberUserIDs)
            {
                if (!IsPlayerReady(userID))
                    return false;
            }

            return team.MemberUserIDs.Count > 0;
        }

        public TeamData GetPlayerTeam(string userID)
        {
            if (_userIDToTeamID.TryGetValue(userID, out byte teamID))
            {
                string teamKey = teamID.ToString();
                if (_localTeams.TryGetValue(teamKey, out TeamData team))
                {
                    return team;
                }
            }
            return null;
        }

        public List<TeamMember> GetTeamMembers(byte teamID)
        {
            var members = new List<TeamMember>();
            for (int i = 0; i < NetworkedTeamMembers.Length; i++)
            {
                var member = NetworkedTeamMembers[i];
                if (member.TeamID == teamID && member.PlayerRef.IsRealPlayer)
                {
                    members.Add(member);
                }
            }
            return members;
        }

        public byte GetPlayerTeamID(string userID)
        {
            return _userIDToTeamID.TryGetValue(userID, out byte teamID) ? teamID : (byte)0;
        }

        public bool AreTeammates(string userID1, string userID2)
        {
            byte team1 = GetPlayerTeamID(userID1);
            byte team2 = GetPlayerTeamID(userID2);
            return team1 != 0 && team1 == team2;
        }

        private void SyncTeamToNetwork(TeamData team)
        {
            int baseIndex = (team.TeamID - 1) * TeamData.MAX_TEAM_SIZE;

            for (int i = 0; i < TeamData.MAX_TEAM_SIZE; i++)
            {
                int networkIndex = baseIndex + i;
                if (networkIndex >= NetworkedTeamMembers.Length)
                    break;

                if (i < team.MemberUserIDs.Count)
                {
                    var member = new TeamMember
                    {
                        TeamID = team.TeamID,
                        IsPartyLeader = team.MemberUserIDs[i] == team.PartyLeaderUserID
                    };

                    NetworkedTeamMembers.Set(networkIndex, member);
                }
                else
                {
                    NetworkedTeamMembers.Set(networkIndex, default);
                }
            }
        }

        private int GetMemberIndex(byte teamID, string userID)
        {
            int baseIndex = (teamID - 1) * TeamData.MAX_TEAM_SIZE;
            string teamKey = teamID.ToString();

            if (_localTeams.TryGetValue(teamKey, out TeamData team))
            {
                int memberIndex = team.MemberUserIDs.IndexOf(userID);
                if (memberIndex >= 0)
                {
                    return baseIndex + memberIndex;
                }
            }

            return -1;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_CreateOrJoinTeam(string userID, string nickname, string teamID = null)
        {
            CreateOrJoinTeam(userID, nickname, teamID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_LeaveTeam(string userID)
        {
            LeaveTeam(userID);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_SetPlayerReady(string userID, bool ready)
        {
            SetPlayerReady(userID, ready);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
        private void RPC_SetTeamMode(TeamMode mode)
        {
            if (HasStateAuthority)
            {
                CurrentTeamMode = mode;
                OnTeamModeChanged?.Invoke(mode);
            }
        }
    }
}
