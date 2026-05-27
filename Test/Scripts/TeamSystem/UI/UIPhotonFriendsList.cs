using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UIPhotonFriendsList : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _friendsListContainer;
        [SerializeField] private Transform _onlinePlayersContainer;
        [SerializeField] private GameObject _friendEntryPrefab;
        [SerializeField] private GameObject _playerEntryPrefab;

        [Header("Buttons")]
        [SerializeField] private Button _refreshButton;

        private PartyLobbyManager _partyManager;
        private PhotonFriendsManager _photonFriends;

        private void Start()
        {
            _partyManager = PartyLobbyManager.Instance;
            _photonFriends = PhotonFriendsManager.Instance;

            if (_refreshButton != null)
            {
                _refreshButton.onClick.AddListener(RefreshLists);
            }

            if (_photonFriends != null)
            {
                _photonFriends.OnOnlinePlayersUpdated += OnOnlinePlayersUpdated;
                _photonFriends.OnFriendRequestReceived += OnFriendRequestReceived;
                _photonFriends.OnPartyInviteReceived += OnPartyInviteReceived;
            }

            if (_partyManager != null)
            {
                _partyManager.OnFriendStatusChanged += OnFriendStatusChanged;
            }

            RefreshLists();
        }

        private void OnDestroy()
        {
            if (_refreshButton != null)
            {
                _refreshButton.onClick.RemoveListener(RefreshLists);
            }

            if (_photonFriends != null)
            {
                _photonFriends.OnOnlinePlayersUpdated -= OnOnlinePlayersUpdated;
                _photonFriends.OnFriendRequestReceived -= OnFriendRequestReceived;
                _photonFriends.OnPartyInviteReceived -= OnPartyInviteReceived;
            }

            if (_partyManager != null)
            {
                _partyManager.OnFriendStatusChanged -= OnFriendStatusChanged;
            }
        }

        private void RefreshLists()
        {
            RefreshFriendsList();
            RefreshOnlinePlayersList();
        }

        private void RefreshFriendsList()
        {
            if (_friendsListContainer == null || _partyManager == null)
                return;

            ClearContainer(_friendsListContainer);

            var friends = _partyManager.GetFriends();
            foreach (var friend in friends)
            {
                CreateFriendEntry(friend);
            }
        }

        private void RefreshOnlinePlayersList()
        {
            if (_onlinePlayersContainer == null || _partyManager == null)
                return;

            ClearContainer(_onlinePlayersContainer);

            var onlinePlayers = _partyManager.GetOnlinePlayers();
            foreach (var player in onlinePlayers)
            {
                bool isFriend = _photonFriends?.IsFriend(player.UserID) ?? false;
                CreatePlayerEntry(player, isFriend);
            }
        }

        private void CreateFriendEntry(FriendData friend)
        {
            if (_friendEntryPrefab == null)
                return;

            var entry = Instantiate(_friendEntryPrefab, _friendsListContainer);

            var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = friend.Nickname;
                nameText.color = friend.IsOnline ? Color.green : Color.gray;
            }

            var statusText = entry.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null)
            {
                statusText.text = friend.IsOnline ? "Online" : "Offline";
            }

            var inviteButton = entry.transform.Find("InviteButton")?.GetComponent<Button>();
            if (inviteButton != null)
            {
                inviteButton.interactable = friend.IsOnline;
                inviteButton.onClick.AddListener(() => InviteToParty(friend.UserID));
            }

            var removeButton = entry.transform.Find("RemoveButton")?.GetComponent<Button>();
            if (removeButton != null)
            {
                removeButton.onClick.AddListener(() => RemoveFriend(friend.UserID));
            }
        }

        private void CreatePlayerEntry(OnlinePlayer player, bool isFriend)
        {
            if (_playerEntryPrefab == null)
                return;

            var entry = Instantiate(_playerEntryPrefab, _onlinePlayersContainer);

            var nameText = entry.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = player.Nickname;
            }

            var addButton = entry.transform.Find("AddButton")?.GetComponent<Button>();
            if (addButton != null)
            {
                addButton.interactable = !isFriend;
                var buttonText = addButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = isFriend ? "Friend" : "Add Friend";
                }
                addButton.onClick.AddListener(() => SendFriendRequest(player.UserID, player.Nickname));
            }
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        private void SendFriendRequest(string userID, string nickname)
        {
            if (_partyManager != null)
            {
                _partyManager.AddFriend(userID, nickname);
                Debug.Log($"[UIPhotonFriendsList] Sent friend request to {nickname}");
            }
        }

        private void InviteToParty(string userID)
        {
            if (_partyManager != null)
            {
                _partyManager.InviteFriend(userID);
                Debug.Log($"[UIPhotonFriendsList] Invited {userID} to party");
            }
        }

        private void RemoveFriend(string userID)
        {
            if (_partyManager != null)
            {
                _partyManager.RemoveFriend(userID);
                RefreshFriendsList();
                RefreshOnlinePlayersList();
            }
        }

        private void OnOnlinePlayersUpdated(List<OnlinePlayer> players)
        {
            Debug.Log($"[UIPhotonFriendsList] Online players updated: {players.Count}");
            RefreshOnlinePlayersList();
        }

        private void OnFriendStatusChanged(FriendData friend)
        {
            RefreshFriendsList();
        }

        private void OnFriendRequestReceived(FriendInvite invite)
        {
            Debug.Log($"[UIPhotonFriendsList] Friend request from {invite.FromNickname}");

            if (_photonFriends != null)
            {
                _photonFriends.AcceptFriendRequest(invite.FromUserID);
            }
        }

        private void OnPartyInviteReceived(PartyInvite invite)
        {
            Debug.Log($"[UIPhotonFriendsList] Party invite from {invite.FromNickname}");
        }
    }
}
