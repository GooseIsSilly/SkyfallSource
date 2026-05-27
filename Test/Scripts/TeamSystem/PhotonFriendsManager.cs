using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace TPSBR
{
    public class PhotonFriendsManager : SimulationBehaviour, INetworkRunnerCallbacks
    {
        public static PhotonFriendsManager Instance { get; private set; }

        public Action<List<OnlinePlayer>> OnOnlinePlayersUpdated;
        public Action<FriendInvite> OnFriendRequestReceived;
        public Action<PartyInvite> OnPartyInviteReceived;
        public Action<string> OnFriendAdded;
        public Action<string> OnFriendRemoved;

        private NetworkRunner _runner;
        private Dictionary<PlayerRef, OnlinePlayer> _onlinePlayers = new Dictionary<PlayerRef, OnlinePlayer>();
        private HashSet<string> _friendUserIDs = new HashSet<string>();
        private string _localUserID;
        private string _localNickname;

        private const string PROPERTY_USER_ID = "uid";
        private const string PROPERTY_NICKNAME = "nick";
        private const string PROPERTY_IN_PARTY = "party";

        private const byte EVENT_FRIEND_REQUEST = 1;
        private const byte EVENT_FRIEND_ACCEPT = 2;
        private const byte EVENT_PARTY_INVITE = 3;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadFriendsFromDisk();
        }

        public void Initialize(NetworkRunner runner, string userID, string nickname)
        {
            _runner = runner;
            _localUserID = userID;
            _localNickname = nickname;

            if (_runner != null)
            {
                _runner.AddCallbacks(this);
            }
        }

        public void SetPlayerProperties()
        {
            if (_runner == null || !_runner.IsRunning)
                return;

            Debug.Log($"[PhotonFriends] Setting player properties: {_localUserID} - {_localNickname}");
        }

        public List<OnlinePlayer> GetOnlinePlayers()
        {
            return _onlinePlayers.Values.ToList();
        }

        public List<OnlinePlayer> GetOnlineFriends()
        {
            return _onlinePlayers.Values
                .Where(p => _friendUserIDs.Contains(p.UserID))
                .ToList();
        }

        public bool IsFriend(string userID)
        {
            return _friendUserIDs.Contains(userID);
        }

        public void SendFriendRequest(string targetUserID)
        {
            var targetPlayer = _onlinePlayers.Values.FirstOrDefault(p => p.UserID == targetUserID);
            if (targetPlayer == null)
            {
                Debug.LogWarning($"[PhotonFriends] Player {targetUserID} not found online");
                return;
            }

            var invite = new FriendInvite
            {
                FromUserID = _localUserID,
                FromNickname = _localNickname,
                ToUserID = targetUserID
            };

            SendCustomEvent(EVENT_FRIEND_REQUEST, invite, targetPlayer.PlayerRef);
            Debug.Log($"[PhotonFriends] Sent friend request to {targetUserID}");
        }

        public void AcceptFriendRequest(string fromUserID)
        {
            var fromPlayer = _onlinePlayers.Values.FirstOrDefault(p => p.UserID == fromUserID);
            if (fromPlayer == null)
            {
                Debug.LogWarning($"[PhotonFriends] Player {fromUserID} not found online");
                return;
            }

            _friendUserIDs.Add(fromUserID);
            SaveFriendsToDisk();

            var response = new FriendInvite
            {
                FromUserID = _localUserID,
                FromNickname = _localNickname,
                ToUserID = fromUserID
            };

            SendCustomEvent(EVENT_FRIEND_ACCEPT, response, fromPlayer.PlayerRef);
            OnFriendAdded?.Invoke(fromUserID);
            Debug.Log($"[PhotonFriends] Accepted friend request from {fromUserID}");
        }

        public void RemoveFriend(string userID)
        {
            if (_friendUserIDs.Remove(userID))
            {
                SaveFriendsToDisk();
                OnFriendRemoved?.Invoke(userID);
                Debug.Log($"[PhotonFriends] Removed friend {userID}");
            }
        }

        public void SendPartyInvite(string targetUserID, string partyID)
        {
            var targetPlayer = _onlinePlayers.Values.FirstOrDefault(p => p.UserID == targetUserID);
            if (targetPlayer == null)
            {
                Debug.LogWarning($"[PhotonFriends] Player {targetUserID} not found online");
                return;
            }

            var invite = new PartyInvite
            {
                FromUserID = _localUserID,
                FromNickname = _localNickname,
                ToUserID = targetUserID,
                PartyID = partyID
            };

            SendCustomEvent(EVENT_PARTY_INVITE, invite, targetPlayer.PlayerRef);
            Debug.Log($"[PhotonFriends] Sent party invite to {targetUserID}");
        }

        private void SendCustomEvent(byte eventCode, object data, PlayerRef target)
        {
            if (_runner == null || !_runner.IsRunning)
            {
                Debug.LogWarning("[PhotonFriends] Cannot send event, runner not active");
                return;
            }

            string json = JsonUtility.ToJson(data);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

            Debug.Log($"[PhotonFriends] Sending event {eventCode} to {target}: {json}");
        }

        private void LoadFriendsFromDisk()
        {
            string json = PlayerPrefs.GetString("PhotonFriends", "");
            if (!string.IsNullOrEmpty(json))
            {
                var wrapper = JsonUtility.FromJson<FriendsWrapper>(json);
                _friendUserIDs = new HashSet<string>(wrapper.friends ?? new string[0]);
                Debug.Log($"[PhotonFriends] Loaded {_friendUserIDs.Count} friends from disk");
            }
        }

        private void SaveFriendsToDisk()
        {
            var wrapper = new FriendsWrapper { friends = _friendUserIDs.ToArray() };
            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString("PhotonFriends", json);
            PlayerPrefs.Save();
            Debug.Log($"[PhotonFriends] Saved {_friendUserIDs.Count} friends to disk");
        }

        private void UpdateOnlinePlayers()
        {
            if (_runner == null || !_runner.IsRunning)
                return;

            _onlinePlayers.Clear();

            foreach (var player in _runner.ActivePlayers)
            {
                if (_runner.TryGetPlayerObject(player, out var playerObject))
                {
                    var userID = $"Player_{player.PlayerId}";
                    var nickname = $"Player {player.PlayerId}";

                    _onlinePlayers[player] = new OnlinePlayer
                    {
                        PlayerRef = player,
                        UserID = userID,
                        Nickname = nickname,
                        InParty = false
                    };
                }
            }

            OnOnlinePlayersUpdated?.Invoke(GetOnlinePlayers());
            Debug.Log($"[PhotonFriends] Updated online players: {_onlinePlayers.Count}");
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[PhotonFriends] Player joined: {player}");
            UpdateOnlinePlayers();
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"[PhotonFriends] Player left: {player}");
            _onlinePlayers.Remove(player);
            UpdateOnlinePlayers();
        }

        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectedToServer(NetworkRunner runner) 
        {
            Debug.Log("[PhotonFriends] Connected to server");
            UpdateOnlinePlayers();
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
        {
            Debug.Log($"[PhotonFriends] Disconnected from server: {reason}");
            _onlinePlayers.Clear();
        }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (_runner != null)
            {
                _runner.RemoveCallbacks(this);
            }
        }

        [Serializable]
        private class FriendsWrapper
        {
            public string[] friends;
        }
    }

    [Serializable]
    public class OnlinePlayer
    {
        public PlayerRef PlayerRef;
        public string UserID;
        public string Nickname;
        public bool InParty;
    }

    [Serializable]
    public class FriendInvite
    {
        public string FromUserID;
        public string FromNickname;
        public string ToUserID;
    }

    [Serializable]
    public class PartyInvite
    {
        public string FromUserID;
        public string FromNickname;
        public string ToUserID;
        public string PartyID;
    }
}
