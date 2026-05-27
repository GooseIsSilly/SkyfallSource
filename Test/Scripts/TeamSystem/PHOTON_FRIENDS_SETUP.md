# Photon Friends System - Setup Guide

## Overview

This Photon-based friends system allows players to:
- See who's online in real-time
- Send friend requests to other players
- Invite friends to parties
- Track friend status across the network

## How It Works

The system uses Photon Fusion's existing network infrastructure:
- **Online Detection**: Monitors active players via NetworkRunner
- **Friend Requests**: Sent using custom events between players
- **Persistent Storage**: Friends list saved locally via PlayerPrefs
- **Real-time Updates**: Friend online status updates automatically

## Components

### 1. PhotonFriendsManager
The core networking component that handles:
- Tracking online players
- Sending/receiving friend requests
- Sending/receiving party invites
- Managing friends list

### 2. PartyLobbyManager (Updated)
Now integrates with PhotonFriendsManager to:
- Show real online status of friends
- Send actual network party invites
- Handle friend requests through Photon

### 3. PhotonFriendsInitializer
Helper component to automatically initialize the system when NetworkRunner connects.

### 4. UIPhotonFriendsList
Example UI controller demonstrating how to:
- Display friends list with online status
- Show all online players
- Send friend requests
- Invite friends to party

## Setup Instructions

### Step 1: Add to NetworkRunner

1. Select your NetworkRunner prefab (usually at `/Assets/TPSBR/Prefabs/Game/NetworkRunner.prefab`)
2. Add the `PhotonFriendsInitializer` component
3. Configure settings:
   - **Auto Initialize**: ✓ (enabled)
   - **Default Nickname**: "Player" (or leave default)

### Step 2: Initialize in Your Menu Scene

The system auto-initializes when a player connects to Photon. However, you can manually initialize:

```csharp
using TPSBR;

public class MenuController : MonoBehaviour
{
    private void Start()
    {
        var partyManager = PartyLobbyManager.Instance;
        if (partyManager != null)
        {
            string userID = Context.PlayerData.UserID;
            string nickname = Context.PlayerData.Nickname;
            partyManager.Initialize(userID, nickname);
        }
    }
}
```

### Step 3: Connect UI (Optional)

If you want to create a friends list UI:

1. Create a Canvas with UI elements for friends list
2. Add `UIPhotonFriendsList` component
3. Assign references:
   - Friends List Container
   - Online Players Container
   - Friend Entry Prefab
   - Player Entry Prefab

## Usage Examples

### Getting Online Players

```csharp
var partyManager = PartyLobbyManager.Instance;
var onlinePlayers = partyManager.GetOnlinePlayers();

foreach (var player in onlinePlayers)
{
    Debug.Log($"{player.Nickname} is online (ID: {player.UserID})");
}
```

### Sending Friend Request

```csharp
var partyManager = PartyLobbyManager.Instance;
partyManager.AddFriend("Player_123", "John");
```

This will:
1. Add to local friends list
2. Send a Photon event to the target player
3. Target player can accept/reject

### Inviting to Party

```csharp
var partyManager = PartyLobbyManager.Instance;
partyManager.InviteFriend("Player_123");
```

This will:
1. Check if friend is online
2. Send party invite via Photon
3. Friend receives `OnPartyInviteReceived` event

### Listening to Events

```csharp
private void Start()
{
    var photonFriends = PhotonFriendsManager.Instance;
    if (photonFriends != null)
    {
        photonFriends.OnOnlinePlayersUpdated += OnPlayersUpdated;
        photonFriends.OnFriendRequestReceived += OnFriendRequest;
        photonFriends.OnPartyInviteReceived += OnPartyInvite;
    }
}

private void OnPlayersUpdated(List<OnlinePlayer> players)
{
    Debug.Log($"{players.Count} players online");
}

private void OnFriendRequest(FriendInvite invite)
{
    Debug.Log($"Friend request from {invite.FromNickname}");
    
    PhotonFriendsManager.Instance.AcceptFriendRequest(invite.FromUserID);
}

private void OnPartyInvite(PartyInvite invite)
{
    Debug.Log($"Party invite from {invite.FromNickname}");
}
```

## API Reference

### PhotonFriendsManager

**Methods:**
- `Initialize(NetworkRunner, string userID, string nickname)` - Initialize with network runner
- `GetOnlinePlayers()` - Get all online players
- `GetOnlineFriends()` - Get online friends only
- `IsFriend(string userID)` - Check if user is a friend
- `SendFriendRequest(string targetUserID)` - Send friend request
- `AcceptFriendRequest(string fromUserID)` - Accept friend request
- `RemoveFriend(string userID)` - Remove friend
- `SendPartyInvite(string targetUserID, string partyID)` - Invite to party

**Events:**
- `OnOnlinePlayersUpdated` - Fired when players join/leave
- `OnFriendRequestReceived` - Fired when receiving friend request
- `OnPartyInviteReceived` - Fired when receiving party invite
- `OnFriendAdded` - Fired when friend is added
- `OnFriendRemoved` - Fired when friend is removed

### PartyLobbyManager (Updated Methods)

**New/Updated Methods:**
- `Initialize(string userID, string nickname)` - Initialize with user info
- `InitializeWithRunner(NetworkRunner)` - Connect to Photon
- `GetOnlinePlayers()` - Get all online players from Photon
- `InviteFriend(string friendUserID)` - Now sends real Photon invite

## Limitations

Since this is a Photon-only solution (no backend):

1. **Session-based**: Friends are only "online" when connected to Photon
2. **Local storage**: Friends list stored in PlayerPrefs (not cloud)
3. **No persistence**: Online status resets each session
4. **No authentication**: Uses generated Player IDs, not real accounts
5. **Same session**: Players must be in the same Photon region/lobby to see each other

## Recommended: Upgrade to Full Backend

For a production-ready friends system, consider using:
- **Unity Gaming Services** (Authentication + Cloud Save)
- **PlayFab**
- **Custom backend**

These provide:
- Real user accounts
- Persistent friends across devices
- Global online status
- Better security
- Cross-platform support

## Troubleshooting

**Friends not showing as online:**
- Ensure both players are connected to Photon
- Check that PhotonFriendsInitializer is on NetworkRunner
- Verify Initialize was called with correct userID

**Friend requests not working:**
- Currently auto-accepts all requests (see `OnFriendRequestReceived`)
- You can add UI to show accept/reject dialog

**Can't see other players:**
- Players must be in the same Photon session/room
- Check NetworkRunner is active and connected
- Verify both have PhotonFriendsManager initialized

## Debug Logging

All components log to console with `[ComponentName]` prefix:
- `[PhotonFriends]` - Core friend system
- `[PartyLobbyManager]` - Party management
- `[PhotonFriendsInitializer]` - Initialization
- `[UIPhotonFriendsList]` - UI events

Enable these logs to debug issues.
