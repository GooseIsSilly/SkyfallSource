# Revive System - Fortnite Style

This system adds a Fortnite-style "Down But Not Out" (DBNO) revive mechanic to your battle royale game.

## Features

- **Downed State**: When a player dies in Duo/Squad mode, they enter a downed state instead of dying
- **Crouch Locked**: Downed players are forced into crouch and cannot shoot or perform actions
- **Hold E to Revive**: Teammates can hold E for 5 seconds to revive
- **Bleed Out Timer**: 30 seconds to get revived or permanently die
- **Team-Only**: Only teammates can revive each other
- **Visual Feedback**: UI shows revive progress and bleed-out timer
- **Solo Mode Disabled**: Revive system only works in Duo/Squad modes

## How It Works

### Player Gets Downed
1. Player takes fatal damage
2. Instead of dying, enters downed state (if in Duo/Squad mode)
3. Player is forced to crouch
4. Weapons and actions are disabled
5. 30-second bleed-out timer starts

### Reviving Process
1. Teammate walks within 2.5 meters
2. UI prompt shows "Hold [E] to Revive"
3. Teammate holds E key for 5 seconds
4. Progress bar fills up
5. Player is revived with 30 health
6. Downed player can move again and use weapons

### Bleed Out
- If not revived within 30 seconds, player dies permanently
- Counts as elimination in match statistics

## Setup Instructions

### Step 1: Add Revive System to Player Prefab

1. Open your Player prefab (`/Assets/Prefabs/Player.prefab`)
2. Add `ReviveSystem` component
3. The component will automatically configure itself

### Step 2: Add Revive Integration to Game Scene

1. Open your game scene
2. Find or create "GameSystems" GameObject
3. Add `ReviveIntegration` component
4. Configure:
   - Enable Revive System: ✅

### Step 3: Add Revive Interaction to Game Scene

1. On same "GameSystems" or create new GameObject: "ReviveInteraction"
2. Add `ReviveInteraction` component
3. Configure:
   - Player Layer: Set to your Agent/Player layer

### Step 4: Create UI Elements

#### Revive Prompt UI (for reviving players)
1. Create UI Panel in gameplay UI
2. Add children:
   - PromptText: "Hold [E] to Revive"
   - PlayerNameText: Shows downed player's name
   - ProgressFill: Image with fill type
3. Add `UIRevivePrompt` component
4. Assign all references
5. Position: Center of screen or bottom

#### Downed State UI (for downed players)
1. Create UI Panel in gameplay UI
2. Add children:
   - StatusText: "You are down!"
   - BleedOutFill: Image showing time remaining
   - BleedOutTimerText: Countdown in seconds
   - BeingRevivedIndicator: Shows when being revived
   - ReviverNameText: Shows who is reviving
   - ReviveProgressFill: Revive progress bar
3. Add `UIDownedState` component
4. Assign all references
5. Position: Center or top of screen

### Step 5: (Optional) Add Visual Marker

To show an icon above downed players:

1. Create a sprite/icon GameObject (e.g., exclamation mark)
2. Add `DownedPlayerMarker` component
3. Assign Marker Visual
4. Configure colors and pulse speed
5. Add as child to Player prefab

## Customization

### Change Revive Duration
Edit `ReviveSettings.cs`:
```csharp
public const float REVIVE_DURATION = 5.0f; // Change to desired seconds
```

### Change Bleed Out Time
```csharp
public const float BLEED_OUT_DURATION = 30.0f; // Change to desired seconds
```

### Change Revive Distance
```csharp
public const float REVIVE_INTERACTION_DISTANCE = 2.5f; // In meters
```

### Change Revived Health Amount
```csharp
public const float REVIVE_HEALTH_RESTORED = 30f; // Health after revive
```

### Change Revive Input
Edit `ReviveInteraction.cs`, line with `Keyboard.current.eKey`:
```csharp
if (Keyboard.current.eKey.isPressed) // Change to different key
```

Or use the new Input System:
```csharp
// In ReviveInteraction.cs, add InputAction reference
[SerializeField]
private InputActionReference _reviveActionRef;

// Then in HandleReviveInput:
if (_reviveActionRef != null && _reviveActionRef.action.IsPressed())
{
    // Revive logic
}
```

## Integration with Existing Systems

The revive system automatically integrates with:
- **Team System**: Only teammates can revive
- **Health System**: Uses existing Agent.Health
- **Character Controller**: Locks to crouch state
- **Weapon System**: Disables weapons when downed
- **Statistics**: Maintains alive state while downed

## Events You Can Listen To

```csharp
var reviveSystem = player.GetComponent<ReviveSystem>();

reviveSystem.OnPlayerDowned += (downedPlayer) =>
{
    Debug.Log($"{downedPlayer.Nickname} is down!");
};

reviveSystem.OnReviveStarted += (downedPlayer, reviver) =>
{
    Debug.Log($"{reviver.Nickname} started reviving {downedPlayer.Nickname}");
};

reviveSystem.OnReviveCompleted += (revivedPlayer, reviver) =>
{
    Debug.Log($"{reviver.Nickname} revived {revivedPlayer.Nickname}");
};

reviveSystem.OnReviveCancelled += (downedPlayer) =>
{
    Debug.Log("Revive was cancelled");
};

reviveSystem.OnPlayerBledOut += (deadPlayer) =>
{
    Debug.Log($"{deadPlayer.Nickname} bled out");
};
```

## Testing

### Test Downed State
1. Set team mode to Duo or Squad
2. Take damage until health reaches 0
3. Should enter downed state (crouching, can't shoot)
4. Bleed-out timer should start (30 seconds)

### Test Reviving
1. Have teammate walk close (within 2.5m)
2. Hold E key
3. Progress bar should fill over 5 seconds
4. Player should be revived with 30 health

### Test Bleed Out
1. Enter downed state
2. Wait 30 seconds without revive
3. Should die permanently

### Test Solo Mode
1. Set team mode to Solo
2. Take fatal damage
3. Should die immediately (no downed state)

## Troubleshooting

**Issue: Not entering downed state**
- Check team mode is Duo or Squad (not Solo)
- Verify ReviveIntegration is enabled
- Ensure ReviveSystem component exists on Player

**Issue: Can't revive teammate**
- Check distance (must be within 2.5m)
- Verify players are on same team
- Ensure E key is being held (not just pressed)

**Issue: Revive not completing**
- Hold E for full 5 seconds
- Don't move too far away during revive
- Check for network lag/desync

**Issue: UI not showing**
- Verify UI components have references assigned
- Check Context is set on UI widgets
- Ensure Canvas is active

## Files

```
/Assets/Scripts/TeamSystem/Revive/
├── ReviveData.cs               # Data structures and settings
├── ReviveSystem.cs             # Core revive logic
├── ReviveInteraction.cs        # Player interaction handler
├── ReviveIntegration.cs        # Game integration
├── DownedPlayerMarker.cs       # Visual marker (optional)
├── UI/
│   ├── UIRevivePrompt.cs       # Revive prompt for reviver
│   └── UIDownedState.cs        # Downed state UI for victim
└── README_REVIVE.md            # This file
```

## Advanced Features to Add

Consider implementing:
- Self-revive items (rare pickup)
- Faster revive with specific items
- Crawling while downed
- Voice lines when downed
- Kill credit if bleed out
- Revive statistics tracking
- Different animations for downed state
