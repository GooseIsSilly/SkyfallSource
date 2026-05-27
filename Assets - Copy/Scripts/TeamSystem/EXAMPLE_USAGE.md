# Photon Friends System - Example Usage

## 📖 Real-World Examples

### Example 1: Display Friends List in Menu

```csharp
using TPSBR;
using UnityEngine;
using TMPro;

public class MenuFriendsList : MonoBehaviour
{
    [SerializeField] private Transform friendsContainer;
    [SerializeField] private GameObject friendEntryPrefab;
    
    private void Start()
    {
        RefreshFriendsList();
        
        var partyManager = PartyLobbyManager.Instance;
        if (partyManager != null)
        {
            partyManager.OnFriendStatusChanged += OnFriendUpdated;
        }
    }
    
    private void RefreshFriendsList()
    {
        ClearList();
        
        var friends = PartyLobbyManager.Instance.GetFriends();
        
        foreach (var friend in friends)
        {
            var entry = Instantiate(friendEntryPrefab, friendsContainer);
            
            var nameText = entry.GetComponentInChildren<TextMeshProUGUI>();
            nameText.text = friend.Nickname;
            nameText.color = friend.IsOnline ? Color.green : Color.gray;
            
            var inviteBtn = entry.GetComponentInChildren<Button>();
            inviteBtn.interactable = friend.IsOnline;
            inviteBtn.onClick.AddListener(() => InviteFriend(friend.UserID));
        }
    }
    
    private void InviteFriend(string userID)
    {
        PartyLobbyManager.Instance.InviteFriend(userID);
        Debug.Log($"Invited {userID} to party!");
    }
    
    private void OnFriendUpdated(FriendData friend)
    {
        RefreshFriendsList();
    }
    
    private void ClearList()
    {
        foreach (Transform child in friendsContainer)
            Destroy(child.gameObject);
    }
}
```

---

### Example 2: Add Friend by Player ID

```csharp
using TPSBR;
using UnityEngine;
using TMPro;

public class AddFriendDialog : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerIDInput;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button addButton;
    [SerializeField] private TextMeshProUGUI statusText;
    
    private void Start()
    {
        addButton.onClick.AddListener(OnAddFriendClicked);
    }
    
    private void OnAddFriendClicked()
    {
        string userID = playerIDInput.text.Trim();
        string nickname = nicknameInput.text.Trim();
        
        if (string.IsNullOrEmpty(userID))
        {
            statusText.text = "Please enter Player ID";
            return;
        }
        
        if (string.IsNullOrEmpty(nickname))
            nickname = userID;
        
        var partyManager = PartyLobbyManager.Instance;
        bool added = partyManager.AddFriend(userID, nickname);
        
        if (added)
        {
            statusText.text = $"Friend request sent to {nickname}!";
            statusText.color = Color.green;
            
            playerIDInput.text = "";
            nicknameInput.text = "";
        }
        else
        {
            statusText.text = "Already friends or invalid ID";
            statusText.color = Color.red;
        }
    }
}
```

---

### Example 3: Show Online Players

```csharp
using TPSBR;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OnlinePlayersList : MonoBehaviour
{
    [SerializeField] private Transform playersContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    
    private void Start()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnOnlinePlayersUpdated += OnPlayersUpdated;
        }
        
        RefreshPlayersList();
    }
    
    private void OnDestroy()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnOnlinePlayersUpdated -= OnPlayersUpdated;
        }
    }
    
    private void OnPlayersUpdated(List<OnlinePlayer> players)
    {
        RefreshPlayersList();
    }
    
    private void RefreshPlayersList()
    {
        ClearList();
        
        var players = PartyLobbyManager.Instance.GetOnlinePlayers();
        
        foreach (var player in players)
        {
            var entry = Instantiate(playerEntryPrefab, playersContainer);
            
            var nameText = entry.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            nameText.text = player.Nickname;
            
            var statusText = entry.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
            statusText.text = player.InParty ? "In Party" : "Available";
            
            bool isFriend = PhotonFriendsManager.Instance.IsFriend(player.UserID);
            
            var addBtn = entry.transform.Find("AddButton").GetComponent<Button>();
            addBtn.interactable = !isFriend;
            addBtn.GetComponentInChildren<TextMeshProUGUI>().text = isFriend ? "Friend" : "Add";
            addBtn.onClick.AddListener(() => AddPlayer(player));
        }
    }
    
    private void AddPlayer(OnlinePlayer player)
    {
        PartyLobbyManager.Instance.AddFriend(player.UserID, player.Nickname);
    }
    
    private void ClearList()
    {
        foreach (Transform child in playersContainer)
            Destroy(child.gameObject);
    }
}
```

---

### Example 4: Handle Friend Requests

```csharp
using TPSBR;
using UnityEngine;
using TMPro;

public class FriendRequestHandler : MonoBehaviour
{
    [SerializeField] private GameObject requestPopupPrefab;
    [SerializeField] private Transform popupContainer;
    
    private void Start()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnFriendRequestReceived += OnFriendRequest;
        }
    }
    
    private void OnDestroy()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnFriendRequestReceived -= OnFriendRequest;
        }
    }
    
    private void OnFriendRequest(FriendInvite invite)
    {
        ShowFriendRequestPopup(invite);
    }
    
    private void ShowFriendRequestPopup(FriendInvite invite)
    {
        var popup = Instantiate(requestPopupPrefab, popupContainer);
        
        var messageText = popup.transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        messageText.text = $"{invite.FromNickname} wants to be friends!";
        
        var acceptBtn = popup.transform.Find("AcceptButton").GetComponent<Button>();
        acceptBtn.onClick.AddListener(() => {
            AcceptFriendRequest(invite.FromUserID);
            Destroy(popup);
        });
        
        var rejectBtn = popup.transform.Find("RejectButton").GetComponent<Button>();
        rejectBtn.onClick.AddListener(() => {
            Destroy(popup);
        });
    }
    
    private void AcceptFriendRequest(string userID)
    {
        PhotonFriendsManager.Instance.AcceptFriendRequest(userID);
        Debug.Log($"Accepted friend request from {userID}");
    }
}
```

---

### Example 5: Party Invite Handler

```csharp
using TPSBR;
using UnityEngine;
using TMPro;

public class PartyInviteHandler : MonoBehaviour
{
    [SerializeField] private GameObject invitePopupPrefab;
    [SerializeField] private Transform popupContainer;
    
    private void Start()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnPartyInviteReceived += OnPartyInvite;
        }
    }
    
    private void OnDestroy()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        if (photonFriends != null)
        {
            photonFriends.OnPartyInviteReceived -= OnPartyInvite;
        }
    }
    
    private void OnPartyInvite(PartyInvite invite)
    {
        ShowPartyInvitePopup(invite);
    }
    
    private void ShowPartyInvitePopup(PartyInvite invite)
    {
        var popup = Instantiate(invitePopupPrefab, popupContainer);
        
        var messageText = popup.transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        messageText.text = $"{invite.FromNickname} invited you to join their party!";
        
        var joinBtn = popup.transform.Find("JoinButton").GetComponent<Button>();
        joinBtn.onClick.AddListener(() => {
            JoinParty(invite);
            Destroy(popup);
        });
        
        var declineBtn = popup.transform.Find("DeclineButton").GetComponent<Button>();
        declineBtn.onClick.AddListener(() => {
            Destroy(popup);
        });
    }
    
    private void JoinParty(PartyInvite invite)
    {
        var partyManager = PartyLobbyManager.Instance;
        partyManager.AcceptInvite(invite.FromUserID);
        Debug.Log($"Joined {invite.FromNickname}'s party!");
    }
}
```

---

### Example 6: Initialize on Game Start

```csharp
using TPSBR;
using UnityEngine;
using Fusion;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private string playerNickname = "Player";
    
    private NetworkRunner _runner;
    
    private void Start()
    {
        _runner = FindObjectOfType<NetworkRunner>();
        
        if (_runner != null)
        {
            InitializeFriendsSystem();
        }
    }
    
    private void InitializeFriendsSystem()
    {
        string userID = GenerateUserID();
        
        var partyManager = PartyLobbyManager.Instance;
        if (partyManager != null)
        {
            partyManager.Initialize(userID, playerNickname);
            
            if (_runner.IsRunning)
            {
                partyManager.InitializeWithRunner(_runner);
                Debug.Log($"Friends system initialized for {userID}");
            }
        }
    }
    
    private string GenerateUserID()
    {
        if (Context.PlayerData != null)
            return Context.PlayerData.UserID;
        
        if (_runner != null && _runner.LocalPlayer.IsValid)
            return $"Player_{_runner.LocalPlayer.PlayerId}";
        
        return $"Player_{Random.Range(1000, 9999)}";
    }
}
```

---

## 🎨 UI Prefab Structure Examples

### Friend Entry Prefab

```
FriendEntry (GameObject)
├── NameText (TextMeshProUGUI)
├── StatusText (TextMeshProUGUI)
├── InviteButton (Button)
│   └── Text (TextMeshProUGUI) "Invite"
└── RemoveButton (Button)
    └── Text (TextMeshProUGUI) "Remove"
```

### Player Entry Prefab

```
PlayerEntry (GameObject)
├── NameText (TextMeshProUGUI)
├── StatusText (TextMeshProUGUI)
└── AddButton (Button)
    └── Text (TextMeshProUGUI) "Add Friend"
```

### Friend Request Popup Prefab

```
FriendRequestPopup (GameObject)
├── Background (Image)
├── MessageText (TextMeshProUGUI)
├── AcceptButton (Button)
│   └── Text (TextMeshProUGUI) "Accept"
└── RejectButton (Button)
    └── Text (TextMeshProUGUI) "Reject"
```

---

## 🔧 Testing Locally

### Setup Two Players Locally

1. **Build Player 1:**
   - Build and Run (creates standalone build)
   
2. **Player 2:**
   - Press Play in Unity Editor

3. **Connect Both:**
   - Both join same Photon session
   - They will appear in each other's online players list

4. **Test:**
   - Send friend request from Player 1 to Player 2
   - Accept on Player 2
   - Try party invite
   - See real-time online status updates

---

## 💡 Tips & Best Practices

### DO ✅

- Check if managers exist before using
- Listen to events for real-time updates
- Show online/offline status visually
- Display friend nicknames
- Provide feedback for actions

### DON'T ❌

- Don't call GetFriends() every frame
- Don't forget to unsubscribe from events
- Don't assume players are always online
- Don't hard-code user IDs
- Don't skip error checking

---

## 🎯 Common Patterns

### Refresh UI on Data Change

```csharp
void Start()
{
    PhotonFriendsManager.Instance.OnOnlinePlayersUpdated += _ => RefreshUI();
    PartyLobbyManager.Instance.OnFriendStatusChanged += _ => RefreshUI();
}
```

### Check Online Before Action

```csharp
void InviteFriend(string userID)
{
    var friend = PartyLobbyManager.Instance.GetFriend(userID);
    if (friend != null && friend.IsOnline)
    {
        PartyLobbyManager.Instance.InviteFriend(userID);
    }
    else
    {
        Debug.Log("Friend is offline!");
    }
}
```

### Safe Manager Access

```csharp
T GetManager<T>() where T : MonoBehaviour
{
    var manager = FindObjectOfType<T>();
    if (manager == null)
    {
        Debug.LogWarning($"{typeof(T).Name} not found!");
    }
    return manager;
}
```

---

These examples should cover most common use cases. Mix and match as needed for your game!
