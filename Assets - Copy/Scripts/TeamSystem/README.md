# Team System for Battle Royale

This comprehensive team system adds support for **Duos**, **Squads**, **Party System**, **Friend List**, **Ready-up**, and **Party Leader** functionality to your battle royale game.

## Features

### 1. Team Modes
- **Solo**: 1 player per team
- **Duo**: 2 players per team
- **Squad**: 4 players per team

### 2. Party Lobby System
- Create and manage parties before matchmaking
- Invite friends to your party
- Party leader can control team settings
- Ready-up system ensures all players are prepared
- Visual indicators for ready status

### 3. Friend System
- Add friends by their User ID
- See friend online status
- Invite friends to your party
- Friends list persists between sessions

### 4. Teammate Display
- Shows teammate names on the left side of the screen
- Color-coded status indicators:
  - **Green**: Alive and ready
  - **Red**: Dead/eliminated
  - **Gray**: Not ready (in lobby)
- Party leader crown icon
- Ready checkmark indicators

### 5. Random Matchmaking
- Automatically assigns random teammates
- Fills incomplete teams
- Balances team sizes

## Setup Instructions

### Step 1: Create Network Prefab for TeamManager

1. Create an empty GameObject in your scene
2. Add the `TeamManager` component
3. Create a prefab from it in `/Assets/Prefabs/`
4. Delete the instance from the scene

### Step 2: Add Team System to Your Game Scene

1. Open your main game scene (`Assets/TPSBR/Scenes/Game.unity`)
2. Find or create a GameObject called "GameSystems"
3. Add the `TeamGameplayIntegration` component
4. Assign the TeamManager prefab to the component
5. Add the `RandomTeamAssignment` component if you want automatic team assignment

### Step 3: Create UI Widgets

#### Create UITeamMemberWidget Prefab
1. Create a new UI GameObject (Right-click in Hierarchy → UI → Panel)
2. Add the following children:
   - TextMeshProUGUI for nickname (`_nicknameText`)
   - Image for background (`_background`)
   - Image for status icon (`_statusIcon`)
   - GameObject with Image for leader icon (`_leaderIcon`)
   - GameObject for ready indicator (`_readyIndicator`)
3. Add the `UITeamMemberWidget` component to the root
4. Assign all references
5. Save as prefab in `/Assets/Prefabs/UI/`

#### Create UIFriendWidget Prefab
1. Create a new UI GameObject
2. Add children:
   - TextMeshProUGUI for nickname
   - Image for status indicator
   - Button for invite (`_inviteButton`)
   - Button for remove (`_removeButton`)
3. Add the `UIFriendWidget` component
4. Assign references
5. Save as prefab

### Step 4: Add Team Panel to Gameplay UI

1. Open your gameplay UI prefab
2. Create a new Panel on the left side of the screen
3. Add the `UITeamPanel` component
4. Assign the `UITeamMemberWidget` prefab
5. Create a vertical layout group for the members container
6. Set max members to 4

### Step 5: Create Party Lobby View

1. Create a new Canvas or UI Panel
2. Add sections for:
   - Party members list
   - Team mode selection buttons (Solo/Duo/Squad)
   - Ready button
   - Start matchmaking button
   - Friends list
   - Add friend input field
3. Add the `UIPartyLobbyView` component
4. Assign all UI references
5. Save as prefab in `/Assets/UI/Prefabs/MenuViews/`

### Step 6: Add Managers to Menu Scene

1. Open your menu scene (`Assets/TPSBR/Scenes/Menu.unity`)
2. Create an empty GameObject called "TeamSystems"
3. Add components:
   - `PartyLobbyManager`
   - `TeamMatchmaker`
4. Make sure this persists with `DontDestroyOnLoad`

### Step 7: Initialize the System

Add this code to your game initialization (modify your existing player spawn/join logic):

```csharp
// In your player join handler
if (PartyLobbyManager.Instance != null)
{
    PartyLobbyManager.Instance.Initialize(playerData.UserID);
}

// When creating a team (if using manual team creation)
if (TeamManager.Instance != null)
{
    TeamManager.Instance.CreateOrJoinTeam(
        playerData.UserID, 
        playerData.Nickname
    );
}
```

## Usage Examples

### Creating a Party
```csharp
if (PartyLobbyManager.Instance != null)
{
    PartyLobbyManager.Instance.CreateParty();
}
```

### Inviting a Friend
```csharp
PartyLobbyManager.Instance.InviteFriend(friendUserID);
```

### Setting Ready Status
```csharp
PartyLobbyManager.Instance.SetReady(true);
```

### Starting Matchmaking
```csharp
if (PartyLobbyManager.Instance.IsPartyLeader())
{
    var party = PartyLobbyManager.Instance.GetCurrentParty();
    TeamMatchmaker.Instance.StartMatchmaking(party, TeamMode.Squad);
}
```

### Checking if Players are Teammates
```csharp
bool areTeammates = player1.IsTeammateWith(player2);

// or using extension method
if (TeamManager.Instance.AreTeammates(userID1, userID2))
{
    // They're on the same team
}
```

### Getting Team Information
```csharp
var team = player.GetTeam();
if (team != null)
{
    Debug.Log($"Team has {team.MemberUserIDs.Count} members");
    Debug.Log($"Party leader: {team.PartyLeaderUserID}");
}
```

### Setting Team Mode
```csharp
// Party leader only
TeamManager.Instance.SetTeamMode(TeamMode.Duo);
```

## Integration with Existing Game Systems

### Friendly Fire Prevention
Modify your damage system to check for teammates:

```csharp
// In your hit/damage processing code
var shooter = GetPlayer(hitData.InstigatorRef);
var target = GetPlayer(victimRef);

if (shooter != null && target != null && shooter.IsTeammateWith(target))
{
    // Prevent friendly fire
    return;
}
```

### Team-based Spawning
```csharp
// Get team members for spawn positioning
var teamMembers = TeamManager.Instance.GetTeamMembers(teamID);
Vector3 spawnCenter = CalculateTeamSpawnPosition(teamMembers);
```

### Team Elimination Check
The system automatically tracks team status. Listen to elimination events:

```csharp
if (Context.GameplayMode != null)
{
    Context.GameplayMode.OnAgentDeath += (killData) =>
    {
        // Check if entire team is eliminated
        CheckTeamElimination(killData.VictimRef);
    };
}
```

## Customization

### Changing Team Sizes
Edit `TeamData.cs`:
```csharp
public const byte MAX_TEAM_SIZE = 4; // Change to your desired max
```

### Custom Team Colors
Modify `UITeamMemberWidget.cs`:
```csharp
[SerializeField]
private Color _aliveColor = Color.green;
[SerializeField]
private Color _deadColor = Color.red;
```

### Matchmaking Timeout
Edit `TeamMatchmaker.cs`:
```csharp
private const float MATCHMAKING_TIMEOUT = 60.0f; // seconds
```

## Troubleshooting

### Teams not syncing across network
- Ensure `TeamManager` is spawned as a networked object
- Verify it has state authority on the server
- Check that RPCs are being called correctly

### UI not updating
- Ensure UI components are subscribed to events in `OnInitialize()`
- Verify the `Context` is properly set
- Check that `OnTick()` is being called

### Friends list not saving
- Check PlayerPrefs permissions
- Verify `Save()` is being called after modifications
- Ensure the game has write permissions

### Ready system not working
- Confirm `TeamManager` instance exists
- Verify network synchronization is working
- Check that ready states are being properly networked

## File Structure

```
/Assets/Scripts/TeamSystem/
├── TeamData.cs                    # Core data structures
├── TeamManager.cs                 # Network team management
├── PartyLobbyManager.cs          # Pre-game party system
├── TeamMatchmaker.cs             # Matchmaking logic
├── TeamGameplayIntegration.cs    # Game integration
├── RandomTeamAssignment.cs       # Auto team assignment
├── PlayerTeamExtension.cs        # Player helper methods
└── UI/
    ├── UITeamPanel.cs            # In-game team display
    ├── UITeamMemberWidget.cs     # Individual teammate widget
    ├── UIPartyLobbyView.cs       # Party lobby screen
    └── UIFriendWidget.cs         # Friend list item
```

## Notes

- This system is designed for Photon Fusion networking
- Compatible with Unity 6
- Requires TextMeshPro for UI
- Uses the existing TPSBR project structure
- Follows the project's coding guidelines

## Future Enhancements

Consider adding:
- Voice chat integration for team communication
- Team-specific objectives or bonuses
- Spectate teammates after death
- Team revival mechanics
- Squad markers/pings on the map
- Team statistics and leaderboards
