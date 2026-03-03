using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TPSBR
{
    public class PartyLobbyManager : MonoBehaviour
    {
        public static PartyLobbyManager Instance { get; private set; }

        public Action<TeamData> OnPartyUpdated;
        public Action<FriendData> OnFriendStatusChanged;
        public Action<bool> OnAllPlayersReady;

        private TeamData _currentParty;
        private FriendsList _friendsList;
        private Dictionary<string, bool> _readyStates = new Dictionary<string, bool>();
        private string _localUserID;
        private string _localNickname;

        private PhotonFriendsManager _photonFriends;
        private bool _usePhotonFriends = true;

        private const float FRIEND_STATUS_UPDATE_INTERVAL = 5.0f;
        private float _friendStatusUpdateTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _friendsList = new FriendsList();
            _friendsList.Load();

            if (_usePhotonFriends && PhotonFriendsManager.Instance == null)
            {
                var photonObj = new GameObject("PhotonFriendsManager");
                _photonFriends = photonObj.AddComponent<PhotonFriendsManager>();
                DontDestroyOnLoad(photonObj);
            }
            else
            {
                _photonFriends = PhotonFriendsManager.Instance;
            }

            if (_photonFriends != null)
            {
                _photonFriends.OnOnlinePlayersUpdated += OnPhotonPlayersUpdated;
                _photonFriends.OnFriendRequestReceived += OnPhotonFriendRequest;
                _photonFriends.OnPartyInviteReceived += OnPhotonPartyInvite;
                _photonFriends.OnFriendAdded += OnPhotonFriendAdded;
            }
        }

        private void Update()
        {
            _friendStatusUpdateTimer += Time.deltaTime;
            if (_friendStatusUpdateTimer >= FRIEND_STATUS_UPDATE_INTERVAL)
            {
                _friendStatusUpdateTimer = 0f;
                UpdateFriendStatuses();
            }
        }

        public void Initialize(string userID, string nickname = null)
        {
            _localUserID = userID;
            _localNickname = nickname ?? userID;

            Debug.Log($"[PartyLobbyManager] Initialized: {_localUserID} ({_localNickname})");
        }

        public void InitializeWithRunner(Fusion.NetworkRunner runner)
        {
            if (_photonFriends != null && runner != null)
            {
                _photonFriends.Initialize(runner, _localUserID, _localNickname);
                Debug.Log("[PartyLobbyManager] Initialized Photon friends with NetworkRunner");
            }
        }

        public void CreateParty()
        {
            if (_currentParty != null)
            {
                LeaveParty();
            }

            _currentParty = new TeamData
            {
                TeamID = 1,
                PartyLeaderUserID = _localUserID
            };
            _currentParty.AddMember(_localUserID);

            _readyStates.Clear();
            _readyStates[_localUserID] = false;

            OnPartyUpdated?.Invoke(_currentParty);
        }

        public bool InviteFriend(string friendUserID)
        {
            if (_currentParty == null)
            {
                CreateParty();
            }

            if (!_currentParty.IsPartyLeader(_localUserID))
                return false;

            if (_currentParty.IsFull(TeamMode.Squad))
                return false;

            if (_photonFriends != null && _usePhotonFriends)
            {
                var onlineFriend = _photonFriends.GetOnlinePlayers()
                    .FirstOrDefault(p => p.UserID == friendUserID);

                if (onlineFriend == null)
                {
                    Debug.LogWarning($"[PartyLobbyManager] Friend {friendUserID} is not online");
                    return false;
                }

                string partyID = _currentParty.TeamID.ToString();
                _photonFriends.SendPartyInvite(friendUserID, partyID);
                Debug.Log($"[PartyLobbyManager] Sent Photon party invite to {friendUserID}");
                return true;
            }
            else
            {
                var friend = _friendsList.GetFriend(friendUserID);
                if (friend == null || !friend.IsOnline)
                    return false;

                Debug.Log($"[PartyLobbyManager] Sending local party invite to {friend.Nickname}");
                return true;
            }
        }

        public void AcceptInvite(string partyLeaderUserID)
        {
            if (_currentParty != null)
            {
                LeaveParty();
            }

            _currentParty = new TeamData
            {
                TeamID = 1,
                PartyLeaderUserID = partyLeaderUserID
            };

            _readyStates.Clear();

            Debug.Log($"Accepted invite from {partyLeaderUserID}");
        }

        public void LeaveParty()
        {
            if (_currentParty == null)
                return;

            _currentParty.RemoveMember(_localUserID);

            if (_currentParty.MemberUserIDs.Count == 0 || _currentParty.PartyLeaderUserID == _localUserID)
            {
                _currentParty = null;
            }

            _readyStates.Clear();
            OnPartyUpdated?.Invoke(_currentParty);
        }

        public void SetReady(bool ready)
        {
            if (_currentParty == null)
                return;

            _readyStates[_localUserID] = ready;

            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.SetPlayerReady(_localUserID, ready);
            }

            CheckAllPlayersReady();
        }

        public bool IsReady(string userID)
        {
            return _readyStates.TryGetValue(userID, out bool ready) && ready;
        }

        public bool IsPartyLeader()
        {
            return _currentParty != null && _currentParty.IsPartyLeader(_localUserID);
        }

        public TeamData GetCurrentParty()
        {
            return _currentParty;
        }

        public void StartMatchmaking()
        {
            if (!IsPartyLeader())
            {
                Debug.LogWarning("Only party leader can start matchmaking");
                return;
            }

            if (!AreAllPlayersReady())
            {
                Debug.LogWarning("Not all players are ready");
                return;
            }

            Debug.Log("Starting matchmaking for party...");
        }

        public bool AddFriend(string userID, string nickname)
        {
            bool added = _friendsList.AddFriend(userID, nickname);

            if (added && _photonFriends != null && _usePhotonFriends)
            {
                _photonFriends.SendFriendRequest(userID);
                Debug.Log($"[PartyLobbyManager] Sent Photon friend request to {userID}");
            }

            return added;
        }

        public bool RemoveFriend(string userID)
        {
            bool removed = _friendsList.RemoveFriend(userID);

            if (removed && _photonFriends != null && _usePhotonFriends)
            {
                _photonFriends.RemoveFriend(userID);
            }

            return removed;
        }

        public List<FriendData> GetFriends()
        {
            if (_photonFriends != null && _usePhotonFriends)
            {
                return GetPhotonFriends();
            }
            return _friendsList.Friends;
        }

        private List<FriendData> GetPhotonFriends()
        {
            var photonFriends = new List<FriendData>();
            var onlinePlayers = _photonFriends.GetOnlinePlayers();

            foreach (var localFriend in _friendsList.Friends)
            {
                var onlinePlayer = onlinePlayers.FirstOrDefault(p => p.UserID == localFriend.UserID);

                photonFriends.Add(new FriendData
                {
                    UserID = localFriend.UserID,
                    Nickname = onlinePlayer?.Nickname ?? localFriend.Nickname,
                    IsOnline = onlinePlayer != null,
                    InLobby = onlinePlayer != null,
                    CurrentTeamID = onlinePlayer?.InParty == true ? "InParty" : null
                });
            }

            return photonFriends;
        }

        public List<OnlinePlayer> GetOnlinePlayers()
        {
            if (_photonFriends != null && _usePhotonFriends)
            {
                return _photonFriends.GetOnlinePlayers();
            }
            return new List<OnlinePlayer>();
        }

        public FriendData GetFriend(string userID)
        {
            return _friendsList.GetFriend(userID);
        }

        private void CheckAllPlayersReady()
        {
            bool allReady = AreAllPlayersReady();
            OnAllPlayersReady?.Invoke(allReady);
        }

        private bool AreAllPlayersReady()
        {
            if (_currentParty == null || _currentParty.MemberUserIDs.Count == 0)
                return false;

            foreach (var userID in _currentParty.MemberUserIDs)
            {
                if (!_readyStates.TryGetValue(userID, out bool ready) || !ready)
                    return false;
            }

            return true;
        }

        private void UpdateFriendStatuses()
        {
            foreach (var friend in _friendsList.Friends)
            {
                OnFriendStatusChanged?.Invoke(friend);
            }
        }

        private void OnPhotonPlayersUpdated(List<OnlinePlayer> onlinePlayers)
        {
            Debug.Log($"[PartyLobbyManager] Photon players updated: {onlinePlayers.Count} online");

            foreach (var friend in _friendsList.Friends)
            {
                var onlinePlayer = onlinePlayers.FirstOrDefault(p => p.UserID == friend.UserID);
                friend.IsOnline = onlinePlayer != null;
                friend.InLobby = onlinePlayer != null;
                OnFriendStatusChanged?.Invoke(friend);
            }
        }

        private void OnPhotonFriendRequest(FriendInvite invite)
        {
            Debug.Log($"[PartyLobbyManager] Received friend request from {invite.FromNickname} ({invite.FromUserID})");
        }

        private void OnPhotonPartyInvite(PartyInvite invite)
        {
            Debug.Log($"[PartyLobbyManager] Received party invite from {invite.FromNickname} ({invite.FromUserID})");
        }

        private void OnPhotonFriendAdded(string userID)
        {
            Debug.Log($"[PartyLobbyManager] Friend added via Photon: {userID}");

            var onlinePlayer = _photonFriends?.GetOnlinePlayers()
                .FirstOrDefault(p => p.UserID == userID);

            if (onlinePlayer != null)
            {
                _friendsList.AddFriend(userID, onlinePlayer.Nickname);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (_photonFriends != null)
            {
                _photonFriends.OnOnlinePlayersUpdated -= OnPhotonPlayersUpdated;
                _photonFriends.OnFriendRequestReceived -= OnPhotonFriendRequest;
                _photonFriends.OnPartyInviteReceived -= OnPhotonPartyInvite;
                _photonFriends.OnFriendAdded -= OnPhotonFriendAdded;
            }
        }
    }
}
