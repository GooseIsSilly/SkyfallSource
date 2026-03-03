# Photon Friends System - Quick Start

## ⚡ 5-Minute Setup

### 1️⃣ Add to NetworkRunner (One-Time Setup)

```
1. Find: /Assets/TPSBR/Prefabs/Game/NetworkRunner.prefab
2. Add Component: PhotonFriendsInitializer
3. Set Auto Initialize: ✓
4. Done!
```

### 2️⃣ That's It!

The system auto-initializes when players connect to Photon. No additional setup required!

---

## 🎮 Usage in Code

### See Who's Online

```csharp
var partyManager = PartyLobbyManager.Instance;
var players = partyManager.GetOnlinePlayers();

foreach (var player in players)
{
    Debug.Log($"{player.Nickname} is online!");
}
```

### Add a Friend

```csharp
PartyLobbyManager.Instance.AddFriend("Player_456", "Bob");
```

### Invite Friend to Party

```csharp
PartyLobbyManager.Instance.InviteFriend("Player_456");
```

### Get Your Friends List

```csharp
var friends = PartyLobbyManager.Instance.GetFriends();

foreach (var friend in friends)
{
    string status = friend.IsOnline ? "ONLINE" : "Offline";
    Debug.Log($"{friend.Nickname} - {status}");
}
```

---

## 📡 Listen to Events

```csharp
using TPSBR;
using UnityEngine;

public class FriendsController : MonoBehaviour
{
    private void Start()
    {
        var photonFriends = PhotonFriendsManager.Instance;
        
        photonFriends.OnOnlinePlayersUpdated += (players) => {
            Debug.Log($"{players.Count} players online!");
        };
        
        photonFriends.OnFriendRequestReceived += (invite) => {
            Debug.Log($"Friend request from {invite.FromNickname}");
            photonFriends.AcceptFriendRequest(invite.FromUserID);
        };
        
        photonFriends.OnPartyInviteReceived += (invite) => {
            Debug.Log($"Party invite from {invite.FromNickname}");
        };
    }
}
```

---

## 🔑 Key Classes

| Class | Purpose |
|-------|---------|
| `PhotonFriendsManager` | Core networking (auto-created) |
| `PartyLobbyManager` | Your main API to call |
| `PhotonFriendsInitializer` | Auto-setup helper |
| `UIPhotonFriendsList` | Example UI controller |

---

## 📝 Important Notes

### ✅ What Works

- See who's online in real-time
- Send friend requests to online players
- Invite friends to party
- Friends list persists locally

### ⚠️ Limitations (Photon-Only)

- Players must be connected to Photon to appear online
- Friends list saved locally (not cloud)
- No real user accounts (uses generated IDs)
- Players must be in same session to see each other

### 💡 For Production

Consider upgrading to **Unity Gaming Services** for:
- Real user accounts
- Cloud-saved friends
- Global online status
- Cross-platform support

See `PHOTON_FRIENDS_SETUP.md` for full documentation.

---

## 🐛 Troubleshooting

**Problem:** Friends not showing online  
**Fix:** Ensure PhotonFriendsInitializer is on NetworkRunner

**Problem:** Can't send friend requests  
**Fix:** Make sure both players are connected to Photon

**Problem:** Not initialized  
**Fix:** Call `PartyLobbyManager.Instance.Initialize(userID, nickname)` manually

---

## 📚 Full Documentation

See `PHOTON_FRIENDS_SETUP.md` for complete API reference and advanced usage.
