using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SocketIOClient;
using TPSBR.Backend;

namespace TPSBR
{
    public class PartyLobbyManager : MonoBehaviour
    {
        public static PartyLobbyManager Instance { get; private set; }

        public Action<TeamData> OnPartyUpdated;
        public Action<FriendData> OnFriendStatusChanged;
        public Action<bool> OnAllPlayersReady;
        public Action<string> OnMatchFound;

        private TeamData _currentParty;
        private FriendsList _friendsList;
        private Dictionary<string, bool> _readyStates = new Dictionary<string, bool>();
        private string _localUserID;
        private string _localNickname;

        private SocketIOUnity _socket;
        private bool _isConnected;

        private const string SOCKET_URL = "http://localhost:3551";
        private const int PROTOCOL_VERSION = 1;

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
        }

        public void Initialize(string userID, string nickname = null)
        {
            _localUserID = userID;
            _localNickname = nickname ?? userID;

            Debug.Log($"[PartyLobbyManager] Initialized: {_localUserID} ({_localNickname})");

            // Initialize a local party state so the player is considered a leader of their own party immediately
            _currentParty = new TeamData
            {
                TeamID = 1,
                PartyLeaderUserID = _localNickname
            };
            _currentParty.AddMember(_localNickname);

            ConnectToServer();
        }

        public void InitializeWithRunner(Fusion.NetworkRunner runner)
        {
            // Stub for compatibility with existing scripts
            Debug.Log("[PartyLobbyManager] InitializeWithRunner called - Socket.IO version doesn't require NetworkRunner for initialization.");
        }

        private void ConnectToServer()
        {
            if (_socket != null)
            {
                _socket.Disconnect();
            }

            // Ensure UnityThread is initialized for safe cross-thread calls
            UnityThread.initUnityThread();

            var uri = new Uri(SOCKET_URL);
            _socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Query = new Dictionary<string, string>
                {
                    {"token", "UNITY"}
                },
                EIO = EngineIO.V4,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            _socket.OnConnected += (sender, e) =>
            {
                UnityThread.executeInUpdate(() => {
                    Debug.Log("[PartyLobbyManager] Connected to Socket.IO server at " + SOCKET_URL);
                    _isConnected = true;
                    
                    string token = BackendServiceManager.Instance != null ? BackendServiceManager.Instance.GetStoredToken() : "no-token";
                    
                    // Login format: userId|username|protocol|token=partyID
                    string loginMessage = $"{_localUserID}|{_localNickname}|{PROTOCOL_VERSION}|{token}={_localUserID}";
                    Debug.Log("[PartyLobbyManager] Emitting login request: " + loginMessage);
                    _socket.Emit("requestLogin", new { message = loginMessage });
                });
            };

            _socket.OnDisconnected += (sender, e) =>
            {
                Debug.Log("[PartyLobbyManager] Disconnected from Socket.IO server. Reason: " + e);
                _isConnected = false;
            };

            _socket.OnError += (sender, e) =>
            {
                Debug.LogError("[PartyLobbyManager] Socket.IO Error: " + e);
            };

            _socket.OnUnityThread("Lsuccess", (response) =>
            {
                Debug.Log("[PartyLobbyManager] Login successful on Party Backend!");
            });

            _socket.OnUnityThread("ServerError", (response) =>
            {
                string error = response.GetValue<string>();
                Debug.LogError("[PartyLobbyManager] Server Error: " + error);
            });

            _socket.OnUnityThread("requestPlayers", (response) =>
            {
                string data = response.GetValue<string>();
                UpdatePartyFromData(data);
            });

            _socket.OnUnityThread("inviteToParty", (response) =>
            {
                string data = response.GetValue<string>();
                string[] parts = data.Split('@');
                if (parts.Length == 2 && parts[0] == _localNickname)
                {
                    Debug.Log($"[PartyLobbyManager] Received party invite from {parts[1]}");
                }
            });

            _socket.OnUnityThread("travelToLaunchZone", (response) =>
            {
                string owner = response.GetValue<string>();
                Debug.Log($"[PartyLobbyManager] Party traveling to Launch Zone! Owner: {owner}");
                OnMatchFound?.Invoke(owner);
            });

            _socket.OnUnityThread("notifyPlayerJoined", (response) =>
            {
                string username = response.GetValue<string>();
                Debug.Log($"[PartyLobbyManager] {username} joined the party");
            });

            _socket.OnUnityThread("notifyPlayerLeft", (response) =>
            {
                string username = response.GetValue<string>();
                Debug.Log($"[PartyLobbyManager] {username} left the party");
            });

            _socket.Connect();
        }

        public void CreateParty()
        {
            if (_socket != null && _isConnected)
            {
                _socket.Emit("leaveParty", new { message = "leave" });
            }
        }

        public bool InviteFriend(string friendNickname)
        {
            if (_socket == null || !_isConnected) return false;

            _socket.Emit("inviteToParty", new { message = friendNickname });
            Debug.Log($"[PartyLobbyManager] Invited {friendNickname} to party");
            return true;
        }

        public void AcceptInvite(string targetPartyID)
        {
            if (_socket == null || !_isConnected) return;

            _socket.Emit("joinParty", new { message = targetPartyID });
        }

        public void LeaveParty()
        {
            if (_socket == null || !_isConnected) return;

            _socket.Emit("leaveParty", new { message = "leave" });
        }

        public void StartMatchmaking()
        {
            Debug.Log($"[PartyLobbyManager] StartMatchmaking called. Connected: {_isConnected}");
            if (!IsPartyLeader())
            {
                Debug.LogWarning("[PartyLobbyManager] Only party leader can start matchmaking");
                return;
            }

            if (_socket != null && _isConnected)
            {
                _socket.Emit("partyTravelToLaunchZone", new { message = "start" });
                Debug.Log("[PartyLobbyManager] Emitting travel signal to party backend...");
            }
            else
            {
                Debug.LogWarning("[PartyLobbyManager] Cannot start matchmaking: Not connected to Socket.IO server");
            }
        }

        public bool AddFriend(string userID, string nickname)
        {
            return _friendsList.AddFriend(userID, nickname);
        }

        public bool RemoveFriend(string userID)
        {
            return _friendsList.RemoveFriend(userID);
        }

        public FriendData GetFriend(string userID)
        {
            return _friendsList.GetFriend(userID);
        }

        public List<FriendData> GetFriends()
        {
            return _friendsList.Friends;
        }

        public List<OnlinePlayer> GetOnlinePlayers()
        {
            return new List<OnlinePlayer>();
        }

        public void SetReady(bool ready)
        {
            _readyStates[_localUserID] = ready;
            OnAllPlayersReady?.Invoke(ready);
        }

        public bool IsReady(string userID)
        {
            return _readyStates.TryGetValue(userID, out bool ready) && ready;
        }

        public bool IsPartyLeader()
        {
            return _currentParty != null && _currentParty.PartyLeaderUserID == _localNickname;
        }

        public TeamData GetCurrentParty()
        {
            return _currentParty;
        }

        private void UpdatePartyFromData(string data)
        {
            if (string.IsNullOrEmpty(data)) return;

            string[] members = data.Split(',');
            if (members.Length == 0) return;

            string[] firstParts = members[0].Split('&');
            if (firstParts.Length < 3) return;

            string ownerUsername = firstParts[2];

            _currentParty = new TeamData
            {
                TeamID = 1,
                PartyLeaderUserID = ownerUsername
            };

            foreach (var member in members)
            {
                string[] parts = member.Split('&');
                if (parts.Length >= 2)
                {
                    _currentParty.AddMember(parts[1]); 
                }
            }

            OnPartyUpdated?.Invoke(_currentParty);
            CheckAllPlayersReady();
        }

        private void CheckAllPlayersReady()
        {
            OnAllPlayersReady?.Invoke(true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (_socket != null)
            {
                _socket.Disconnect();
            }
        }
    }
}
