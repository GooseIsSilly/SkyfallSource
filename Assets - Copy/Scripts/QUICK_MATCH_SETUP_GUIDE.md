# Quick Match Setup Guide

## What I Created for You

I've implemented a complete Quick Match system that:

✅ **Searches for available sessions** - Automatically looks for joinable games  
✅ **Joins the best session** - Prioritizes active games with available slots  
✅ **Creates a session if none found** - Automatically becomes host if no games exist  
✅ **Simple one-button solution** - Easy to integrate into your UI  

## Files Created

### Core Components

1. **`/Assets/TPSBR/Scripts/UI/MenuViews/UIQuickMatchView.cs`**
   - Main matchmaking logic
   - Handles lobby connection, session search, and session creation
   - Configurable timeout, gameplay type, and max players

2. **`/Assets/TPSBR/Scripts/UI/MenuViews/UIMultiplayerView.cs`** (Modified)
   - Added `StartQuickMatch()` method
   - Easy integration point for existing menu

3. **`/Assets/TPSBR/Scripts/UI/UIQuickMatchButton.cs`**
   - Helper component for buttons
   - Just attach to any button for instant quick match

4. **`/Assets/Scripts/QuickMatchExample.cs`**
   - Example usage script
   - Shows how to call quick match from your code

## Quick Start - 3 Easy Ways to Use

### Option 1: Simplest - Use Existing Quick Play Button

If you already have a Quick Play button in your menu, just change it to use the new system:

```csharp
// Instead of:
multiplayerView.StartQuickPlay();

// Use:
multiplayerView.StartQuickMatch();
```

### Option 2: Add to Any Button

1. Select any button in your Menu scene
2. Add Component → `UIQuickMatchButton`
3. Done! The button now triggers quick match

### Option 3: Call from Code

```csharp
using TPSBR.UI;

public class MyMenuController : MonoBehaviour
{
    public void OnPlayClicked()
    {
        var multiplayerView = FindObjectOfType<UIMultiplayerView>();
        multiplayerView.StartQuickMatch();
    }
}
```

## How It Works - The Flow

```
User Clicks Play
    ↓
Connect to Lobby (shows: "Connecting to lobby...")
    ↓
Search for Sessions for 5 seconds (shows: "Searching for session... (5s)")
    ↓
    ├─→ Sessions Found → Join Best Session (shows: "Joining session: [Name]...")
    │
    └─→ No Sessions Found → Create New Session (shows: "No sessions found. Creating new session...")
```

## Configuration

### UIQuickMatchView Settings

When you set up the UIQuickMatchView component, you can configure:

| Setting | Default | Description |
|---------|---------|-------------|
| Search Timeout | 5 seconds | How long to search before creating a session |
| Gameplay Type | Battle Royale | Type of game to search for/create |
| Max Players | 100 | Maximum players for new sessions |
| Default Map Scene Path | TPSBR/Scenes/Game | Map to use for new sessions |

## Setting Up the UI (Optional)

If you want a dedicated Quick Match screen with status messages:

1. **Create the UI GameObject:**
   - Right-click in Menu scene hierarchy
   - Create Empty GameObject, name it "QuickMatchView"

2. **Add the Component:**
   - Add Component → `UIQuickMatchView`

3. **Create UI Elements:**
   - Add a TextMeshProUGUI for status messages
   - Add a UIButton for cancel functionality

4. **Wire it up:**
   - Assign Status Text field to your text element
   - Assign Cancel Button field to your button

5. **Configure settings** as needed

## Testing

### Test Scenario 1: No Sessions Available
1. Make sure no other game sessions are running
2. Click your Play/Quick Match button
3. **Expected**: After 5 seconds, creates a new session as host

### Test Scenario 2: Sessions Available
1. Start a game session on another device/build
2. Return to menu on first device
3. Click Play/Quick Match button
4. **Expected**: Finds and joins the existing session immediately

### Test Scenario 3: Multiple Sessions
1. Have multiple sessions running
2. Click Play/Quick Match button
3. **Expected**: Joins the session with the most players (but not full)

## Differences from Existing Quick Play

| Feature | StartQuickPlay() (Old) | StartQuickMatch() (New) |
|---------|----------------------|------------------------|
| **Backend** | Unity Gaming Services | Photon Fusion Sessions |
| **Requires UGS Setup** | Yes | No |
| **Auto-Create Session** | No | Yes |
| **Fallback Behavior** | Shows error | Creates session |
| **Configuration** | Unity Dashboard | In-Editor settings |

## Customization Examples

### Change Search Timeout
```csharp
// In UIQuickMatchView component inspector
Search Timeout = 10  // Wait 10 seconds instead of 5
```

### Use Different Gameplay Type
```csharp
// In UIQuickMatchView component inspector
Gameplay Type = BattleRoyale  // Or any other EGameplayType
```

### Change Max Players for New Sessions
```csharp
// In UIQuickMatchView component inspector
Max Players = 50  // Create smaller sessions
```

### Use Different Map
```csharp
// In UIQuickMatchView component inspector
Default Map Scene Path = "TPSBR/Scenes/CustomMap"
```

## Troubleshooting

### "UIMultiplayerView not found"
- Make sure you're calling this from the Menu scene
- Ensure UIMultiplayerView component exists in your scene

### "Failed to connect to lobby"
- Check your Photon App Settings
- Verify your internet connection
- Check the region is valid

### Creates session immediately without searching
- Check Search Timeout isn't set to 0
- Verify lobby connection is successful
- Ensure Photon App ID is configured

### Joins wrong session type
- Check Gameplay Type setting matches your game type
- Verify session properties are set correctly

## Advanced: Modify Session Selection Logic

The session selection logic is in `UIQuickMatchView.JoinBestSession()`:

```csharp
private void JoinBestSession()
{
    var bestSession = _availableSessions
        .OrderByDescending(s => s.PlayerCount)     // Prefer active games
        .ThenBy(s => s.MaxPlayers - s.PlayerCount) // Prefer games with space
        .First();
    
    // ... join session
}
```

You can modify this to prefer:
- **Empty sessions**: Change `OrderByDescending` to `OrderBy`
- **Smallest sessions**: Sort by `MaxPlayers` ascending
- **Newest sessions**: Add `.ThenBy(s => s.Name)` or custom property

## Need Help?

- Check the console for detailed logs
- Verify Photon connection status
- Ensure matchmaking is enabled in your scene
- Check that all required components are assigned

## Summary

You now have a **fully automatic matchmaking system** that:
- ✅ Searches for games
- ✅ Joins available games
- ✅ Creates new games if needed
- ✅ Works with one button click
- ✅ No Unity Gaming Services required

Just call `multiplayerView.StartQuickMatch()` and you're done! 🎮
