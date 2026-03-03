# Photon Friends System - Architecture

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         YOUR GAME                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌─────────────────┐      ┌──────────────────┐                 │
│  │  Your UI Code   │◄────►│ PartyLobbyManager│ ◄── Main API    │
│  │ (MenuController)│      │   (Singleton)     │                 │
│  └─────────────────┘      └────────┬──────────┘                 │
│                                    │                              │
│                                    ├─► Local Friends List        │
│                                    │   (PlayerPrefs)              │
│                                    │                              │
│                                    ▼                              │
│                         ┌──────────────────────┐                 │
│                         │ PhotonFriendsManager │                 │
│                         │    (Singleton)       │                 │
│                         └──────────┬───────────┘                 │
│                                    │                              │
├────────────────────────────────────┼──────────────────────────────┤
│                    PHOTON FUSION   │                              │
├────────────────────────────────────┼──────────────────────────────┤
│                                    │                              │
│                         ┌──────────▼───────────┐                 │
│                         │   NetworkRunner      │                 │
│                         │  (Photon Fusion)     │                 │
│                         └──────────┬───────────┘                 │
│                                    │                              │
└────────────────────────────────────┼──────────────────────────────┘
                                     │
                            ┌────────▼─────────┐
                            │  Photon Cloud    │
                            │    Servers       │
                            └──────────────────┘
```

---

## 🔄 Data Flow

### 1. Player Joins Game

```
Player Starts Game
    │
    ├─► NetworkRunner connects to Photon
    │
    ├─► PhotonFriendsInitializer detects connection
    │
    ├─► Initializes PartyLobbyManager with userID
    │
    ├─► Initializes PhotonFriendsManager with NetworkRunner
    │
    └─► System ready to track online players
```

### 2. Friend Request Flow

```
Player A                  PhotonFriendsManager              Player B
   │                              │                            │
   ├─ AddFriend("Player_B") ──────►                           │
   │                              │                            │
   │                       Save to local                       │
   │                       PlayerPrefs                         │
   │                              │                            │
   │                       SendFriendRequest()                 │
   │                              │                            │
   │                       Serialize to JSON                   │
   │                              │                            │
   │                              ├─── Custom Event ─────────► │
   │                              │                            │
   │                              │        OnFriendRequestReceived
   │                              │                            │
   │ ◄──── Custom Event ──────────┤◄─── AcceptFriendRequest() │
   │                              │                            │
   OnFriendAdded                  │                  Save to local
   │                              │                  PlayerPrefs
   │                              │                            │
```

### 3. Online Status Tracking

```
NetworkRunner
    │
    ├─► OnPlayerJoined(PlayerRef)
    │       │
    │       └─► PhotonFriendsManager.OnPlayerJoined()
    │               │
    │               ├─► Add to _onlinePlayers dictionary
    │               │
    │               └─► Fire OnOnlinePlayersUpdated event
    │                       │
    │                       └─► PartyLobbyManager.OnPhotonPlayersUpdated()
    │                               │
    │                               └─► Update friend IsOnline status
    │                                       │
    │                                       └─► Fire OnFriendStatusChanged
    │                                               │
    │                                               └─► UI refreshes
```

### 4. Party Invite Flow

```
Party Leader              PartyLobbyManager           Friend
     │                           │                       │
     ├─ InviteFriend("Bob") ────►                       │
     │                           │                       │
     │                    Check if online                │
     │                           │                       │
     │                    PhotonFriendsManager           │
     │                    .SendPartyInvite()             │
     │                           │                       │
     │                    Serialize PartyInvite          │
     │                           │                       │
     │                           ├──── Custom Event ───► │
     │                           │                       │
     │                           │          OnPartyInviteReceived
     │                           │                       │
     │                           │          Show accept/reject UI
     │                           │                       │
     │ ◄──────────────────────────┼───── AcceptInvite() │
     │                           │                       │
     OnPartyUpdated              │          OnPartyUpdated
     │                           │                       │
```

---

## 📦 Component Responsibilities

### PhotonFriendsManager (Core Networking)

**Responsibilities:**
- Track all online players via NetworkRunner
- Send/receive friend requests via Photon events
- Send/receive party invites via Photon events
- Maintain friends list in PlayerPrefs
- Fire events for UI updates

**Does NOT:**
- Handle UI
- Manage party state
- Make gameplay decisions

### PartyLobbyManager (Business Logic)

**Responsibilities:**
- Provide unified API for UI
- Manage party creation/joining
- Integrate PhotonFriendsManager with existing systems
- Fallback to local mode if Photon unavailable
- Coordinate between friends and party systems

**Does NOT:**
- Handle networking directly
- Render UI
- Store network state

### PhotonFriendsInitializer (Auto-Setup)

**Responsibilities:**
- Detect when NetworkRunner is ready
- Initialize PhotonFriendsManager
- Pass user credentials
- One-time setup

**Does NOT:**
- Handle ongoing networking
- Manage state
- Process events

### UI Controllers (Your Code)

**Responsibilities:**
- Listen to manager events
- Display data to user
- Handle user input
- Call manager APIs

**Does NOT:**
- Store state
- Handle networking
- Manage business logic

---

## 🗄️ Data Storage

### Local Storage (PlayerPrefs)

```
Key: "PhotonFriends"
Value: JSON { friends: ["Player_123", "Player_456", ...] }

Key: "FRIENDS_LIST" (legacy)
Value: JSON { friends: [{UserID, Nickname, IsOnline, ...}] }
```

### In-Memory State

```
PhotonFriendsManager:
├── _onlinePlayers: Dictionary<PlayerRef, OnlinePlayer>
│   └── Updated when players join/leave
├── _friendUserIDs: HashSet<string>
│   └── Loaded from PlayerPrefs on start
└── _localUserID: string
    └── Set during Initialize()

PartyLobbyManager:
├── _currentParty: TeamData
│   └── Current party state
├── _friendsList: FriendsList
│   └── Local friends with metadata
└── _readyStates: Dictionary<string, bool>
    └── Party ready status
```

---

## 🔌 Integration Points

### With Existing Systems

```
PhotonFriendsManager
    │
    ├─► TeamManager (for team assignments)
    │
    ├─► Context.PlayerData (for user info)
    │
    └─► NetworkRunner (for online tracking)

PartyLobbyManager
    │
    ├─► PhotonFriendsManager (for networking)
    │
    ├─► TeamManager (for party-to-team conversion)
    │
    └─► Context.PlayerData (for local player info)
```

### Event System

```
PhotonFriendsManager Events:
├─► OnOnlinePlayersUpdated
│   └─► Listeners: PartyLobbyManager, UI Controllers
│
├─► OnFriendRequestReceived
│   └─► Listeners: PartyLobbyManager, UI Dialogs
│
├─► OnPartyInviteReceived
│   └─► Listeners: PartyLobbyManager, UI Dialogs
│
├─► OnFriendAdded
│   └─► Listeners: UI Lists
│
└─► OnFriendRemoved
    └─► Listeners: UI Lists

PartyLobbyManager Events:
├─► OnPartyUpdated
│   └─► Listeners: Party UI, TeamManager
│
├─► OnFriendStatusChanged
│   └─► Listeners: Friends List UI
│
└─► OnAllPlayersReady
    └─► Listeners: Matchmaking UI
```

---

## 🔐 Security Considerations

### Current Implementation

⚠️ **Not production-ready for security**

**Issues:**
- No authentication (anyone can claim any userID)
- No validation of friend requests
- No rate limiting
- No anti-cheat measures
- PlayerPrefs easily modifiable

### Recommended for Production

For a secure system:

1. **Use Unity Gaming Services Authentication**
   - Real user accounts
   - Secure player IDs
   - Token-based auth

2. **Server-side validation**
   - Validate all friend requests
   - Rate limit requests
   - Prevent spam/abuse

3. **Encrypted storage**
   - Don't use PlayerPrefs for sensitive data
   - Use Cloud Save

---

## 🚀 Performance Characteristics

### Memory Usage

```
PhotonFriendsManager:
- ~1KB per online player
- ~100 bytes per friend
- Typical: <100KB for 100 friends

PartyLobbyManager:
- ~500 bytes per party member
- ~200 bytes per friend
- Typical: <50KB
```

### Network Bandwidth

```
Friend Request: ~200 bytes
Party Invite: ~300 bytes
Player Join/Leave: Handled by Photon (minimal)

Typical Usage:
- 10 friend requests/hour: ~2KB/hour
- 5 party invites/hour: ~1.5KB/hour
- Negligible impact on bandwidth
```

### CPU Usage

```
Update Cycles:
- PhotonFriendsManager: Minimal (event-driven)
- PartyLobbyManager: ~5 seconds (friend status updates)

Event Processing:
- Friend requests: <1ms
- Party invites: <1ms
- Player updates: <5ms for 100 players
```

---

## 📈 Scalability

### Current Limits

```
Max Friends: Unlimited (stored locally)
Max Online Players: Limited by NetworkRunner (typically 100)
Max Party Size: 4 (TeamMode.Squad)
Concurrent Requests: No limit (no rate limiting)
```

### To Scale Up

1. **Backend service** - Move to cloud storage
2. **Database** - Store friends in database
3. **Caching** - Cache online status
4. **Load balancing** - Distribute across servers

---

## 🔧 Extension Points

### Easy to Add

- Voice chat integration
- Friend presence status
- Custom friend nicknames
- Block/mute system
- Friend groups/categories

### Requires More Work

- Cross-platform friends
- Global leaderboards
- Clan/guild system
- Persistent world state
- Anti-cheat integration

---

This architecture is designed to be:
- ✅ Easy to understand
- ✅ Simple to extend
- ✅ Loosely coupled
- ✅ Event-driven
- ✅ Testable

But remember: **For production, use a proper backend!**
