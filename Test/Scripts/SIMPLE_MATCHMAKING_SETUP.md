# Simple Matchmaking Setup Guide

This guide shows how to set up the simplified matchmaking system with just a gamemode selector and Play button.

## What This Does

- **No manual session creation/browsing** - Players just select a gamemode and click Play
- **Auto-matchmaking** - Automatically finds available sessions or creates one
- **Simple UI** - Just gamemode dropdown + Play button
- **Smart session selection** - Joins the most populated available session

## Setup Steps

### 1. Create the UI in Your Menu Scene

1. Open your Menu scene (`Assets/TPSBR/Scenes/Menu.unity`)

2. **Duplicate the existing UIMultiplayerView** GameObject to use as a template:
   - Find `MenuUI/UIMultiplayerView` in the hierarchy
   - Duplicate it (Ctrl+D)
   - Rename the duplicate to `UISimpleMatchmakingView`

3. **Clean up the duplicated GameObject**:
   - Remove all child objects except:
     - A background panel
     - Any header/title elements you want to keep
   - Delete all the session list, buttons panels, etc.

4. **Add the UI elements**:

   Create these children under `UISimpleMatchmakingView`:

   ```
   UISimpleMatchmakingView
   ├── Background (Panel)
   ├── Title (TextMeshPro)
   ├── GamemodeDropdown (TMP_Dropdown)
   ├── PlayButton (Button with UIButton component)
   ├── StatusText (TextMeshPro)
   └── LoadingIndicator (GameObject with UIBehaviour)
   ```

5. **Configure the UISimpleMatchmakingView component**:
   - Remove the `UIMultiplayerView` component
   - Add the `UISimpleMatchmakingView` component
   - Assign the references:
     - **Gamemode Dropdown** → Your TMP_Dropdown
     - **Play Button** → Your UIButton
     - **Status Text** → Your TextMeshPro status text
     - **Loading Indicator** → Your loading spinner/indicator
   - Set matchmaking settings:
     - **Max Players**: 20 (or your preferred max)
     - **Dedicated Server**: false (use Host mode)

### 2. Update Your Main Menu to Open This View

Replace the multiplayer menu button to open the simple matchmaking view:

Find the script that opens `UIMultiplayerView` and change it to open `UISimpleMatchmakingView` instead.

Example:
```csharp
// Old code:
Open<UIMultiplayerView>();

// New code:
Open<UISimpleMatchmakingView>();
```

### 3. Optional: Customize the Gamemode List

By default, only Battle Royale is shown. To add more gamemodes:

1. Open `UISimpleMatchmakingView.cs`
2. Find the `PrepareGamemodeDropdown()` method
3. Add more options:

```csharp
private void PrepareGamemodeDropdown()
{
    var options = new List<TMP_Dropdown.OptionData>();

    options.Add(new TMP_Dropdown.OptionData("Battle Royale"));
    // Add more gamemodes here when you create them
    // options.Add(new TMP_Dropdown.OptionData("Team Deathmatch"));
    // options.Add(new TMP_Dropdown.OptionData("Capture the Flag"));

    _gamemodeDropdown.ClearOptions();
    _gamemodeDropdown.AddOptions(options);
}
```

4. Update the `OnGamemodeChanged()` method to handle the new modes

## How It Works

1. **Player opens the menu** → Automatically connects to Photon lobby
2. **Player selects gamemode** → Choose from available game modes
3. **Player clicks Play** → System searches for available sessions
4. **If session found** → Joins the most populated session
5. **If no session found** → Creates a new session on a random map
6. **Game starts!** → Player is in the game

## Features

✅ Automatic session search  
✅ Smart session selection (joins most populated)  
✅ Automatic session creation if none available  
✅ Random map selection for new sessions  
✅ Loading indicators and status messages  
✅ Photon Fusion matchmaking (no Unity Gaming Services needed)  

## Customization Options

### Change Max Players
In the Inspector on `UISimpleMatchmakingView`:
- Set **Max Players** to your desired value (default: 20)

### Use Dedicated Server Mode
In the Inspector on `UISimpleMatchmakingView`:
- Check **Dedicated Server** to create Server mode sessions instead of Host mode

### Filter Maps by Gamemode
Edit the `PrepareMapData()` method to filter maps based on selected gamemode:

```csharp
private void PrepareMapData()
{
    _availableMaps.Clear();
    
    foreach (var mapSetup in Context.Settings.Maps)
    {
        if (mapSetup != null && mapSetup.IsValid)
        {
            // Add filtering logic here
            // if (mapSetup.SupportedGamemodes.Contains(_selectedGamemode))
            _availableMaps.Add(mapSetup);
        }
    }
}
```

## Troubleshooting

### "No Photon App ID configured"
- Open PhotonAppSettings asset
- Add your Photon Fusion App ID

### "No maps available"
- Check that your Settings asset has maps configured
- Verify maps have valid ScenePath set

### Play button stays disabled
- Check Unity Console for connection errors
- Verify Photon App ID is correct
- Check network connectivity

## Removing Old UI

Once your simple matchmaking is working, you can remove:
- `UIMultiplayerView` (if not used elsewhere)
- `UICreateSessionView` (no longer needed)
- `UIQuickMatchView` (replaced by simple matchmaking)
- Session browser UI elements

This gives you a clean, streamlined matchmaking experience!
