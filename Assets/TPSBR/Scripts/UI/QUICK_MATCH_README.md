# Quick Match System

This system provides an automated matchmaking solution that attempts to find and join available sessions, and creates a new session if none are found.

## Overview

The Quick Match system consists of three main components:

1. **UIQuickMatchView** - The main matchmaking logic that handles the search and fallback
2. **UIMultiplayerView.StartQuickMatch()** - A method to trigger the quick match from the existing multiplayer view
3. **UIQuickMatchButton** - A simple component that can be attached to any button to trigger quick match

## How It Works

1. **Connects to Lobby** - First connects to the Photon lobby for the selected region
2. **Searches for Sessions** - Looks for available sessions that match the criteria:
   - Same gameplay type (Battle Royale by default)
   - Has available player slots
   - Is visible and open
   - Has a valid map
3. **Joins Best Session** - If sessions are found, joins the one with the most players (but not full)
4. **Creates New Session** - If no suitable session is found within the timeout (5 seconds), creates a new host session
5. **Automatic Fallback** - The system automatically handles the entire flow without user intervention

## Usage

### Option 1: Using UIMultiplayerView (Recommended)

Call the `StartQuickMatch()` method from any script that has access to `UIMultiplayerView`:

```csharp
var multiplayerView = FindObjectOfType<UIMultiplayerView>();
multiplayerView.StartQuickMatch();
```

### Option 2: Using UIQuickMatchButton Component

1. Select any button in your UI
2. Add the `UIQuickMatchButton` component to it
3. The button will automatically trigger quick match when clicked

### Option 3: Direct View Access

Open the quick match view directly from any UI context:

```csharp
using TPSBR.UI;

public class YourUIView : UIView
{
    public void OnPlayButtonClicked()
    {
        Open<UIQuickMatchView>();
    }
}
```

## Configuration

You can configure the `UIQuickMatchView` component with the following settings:

- **Search Timeout** (default: 5 seconds) - How long to search before creating a new session
- **Gameplay Type** (default: Battle Royale) - The type of game to search for
- **Max Players** (default: 100) - Maximum players when creating a new session
- **Default Map Scene Path** (default: "TPSBR/Scenes/Game") - The map to use for new sessions

## UI Setup

To use the Quick Match view, you'll need to create a UI prefab or add it to your menu:

1. Create a new GameObject in your Menu scene
2. Add the `UIQuickMatchView` component
3. Assign the following in the Inspector:
   - **Status Text** - A TextMeshProUGUI component to display status messages
   - **Cancel Button** - A UIButton to allow users to cancel the search
4. Configure the optional settings as needed

## Status Messages

The system provides clear status feedback:

- "Connecting to lobby..."
- "Searching for available sessions..."
- "Searching for session... (Xs)" - Countdown timer
- "Joining session: [Name]..."
- "No sessions found. Creating new session..."
- "Failed to connect to lobby in [region]"
- "Cancelled"

## Integration with Existing Quick Play

The existing `StartQuickPlay()` method uses Unity Gaming Services matchmaker. The new `StartQuickMatch()` method provides a simpler alternative that:

- Uses Photon Fusion's built-in session browser
- Doesn't require Unity Gaming Services configuration
- Has automatic fallback to session creation
- Works out of the box with your existing setup

## Example: Adding Quick Match to Main Menu

```csharp
using UnityEngine;
using TPSBR.UI;

public class MainMenuController : MonoBehaviour
{
    public void OnQuickMatchButtonPressed()
    {
        var multiplayerView = FindObjectOfType<UIMultiplayerView>();
        if (multiplayerView != null)
        {
            multiplayerView.StartQuickMatch();
        }
    }
}
```

## Technical Details

### Session Selection Logic

When multiple sessions are available, the system selects the best one using:

1. **Primary Sort**: Sessions with more players (to join active games)
2. **Secondary Sort**: Sessions with more available slots (to ensure you can join)

This ensures you join the most active session while avoiding full games.

### Error Handling

- Lobby connection failures are reported to the user
- Timeout protection prevents infinite searching
- Graceful fallback to session creation
- Cancel support at any time during the process
