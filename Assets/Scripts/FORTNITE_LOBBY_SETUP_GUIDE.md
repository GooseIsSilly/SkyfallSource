# Fortnite-Style Lobby Setup Guide

This guide will help you create a Fortnite-inspired lobby UI with a top navigation bar and a Play button that acts as quickplay.

## Overview

The new lobby features:
- **Top Navigation Bar**: Shop, Quest, Locker, Battle Pass, Settings buttons
- **Prominent Play Button**: Large button on the right that handles quickplay
- **Smart Matchmaking**: Automatically searches for games and shows create game UI if none found
- **Player Info**: Display player name and level (optional)

## Files Created

1. `/Assets/Scripts/UIFortniteLobbyView.cs` - Main lobby controller script

## How It Works

### Play Button Functionality

When the player clicks the **PLAY** button:

1. **Search for Games** (5 seconds by default)
   - Connects to lobby
   - Searches for available sessions matching the gameplay type
   - Shows "SEARCHING..." on the button

2. **Join Best Session** (if found)
   - Automatically joins the session with the most players
   - Shows "JOINING..." on the button
   - Loads into the game

3. **Show Create Game UI** (if no sessions found after timeout)
   - Opens the `UICreateSessionView` popup
   - Player can configure and create a new game

### Top Navigation Buttons

- **Shop Button**: Opens shop UI (placeholder for now)
- **Quest Button**: Opens quest UI (placeholder for now)
- **Locker Button**: Opens agent selection view
- **Battle Pass Button**: Opens battle pass UI (placeholder for now)
- **Settings Button**: Opens settings view

## Unity Scene Setup Instructions

### Step 1: Create the Lobby UI GameObject

1. Open the `Menu.unity` scene (`Assets/TPSBR/Scenes/Menu.unity`)
2. In the Hierarchy, locate the `MenuUI` GameObject
3. Create a new UI GameObject:
   - Right-click `MenuUI` → Create Empty
   - Name it: `UIFortniteLobbyView`
   - Add components:
     - `Canvas` (set to Screen Space - Overlay)
     - `Canvas Group`
     - `Graphic Raycaster`
     - `UIFortniteLobbyView` script (from `/Assets/Scripts/UIFortniteLobbyView.cs`)
     - `UIFader` (optional, for fade effects)

### Step 2: Create Top Navigation Bar

1. Create a UI Panel for the top bar:
   - Right-click `UIFortniteLobbyView` → UI → Panel
   - Name it: `TopNavigationBar`
   - Set RectTransform:
     - Anchor: Top stretch (Alt+Shift+Click top anchor)
     - Height: 80-100 pixels
     - Left/Right: 20 (padding)

2. Add navigation buttons (create 5 buttons):
   - Shop Button
   - Quest Button
   - Locker Button
   - Battle Pass Button
   - Settings Button

   For each button:
   - Right-click `TopNavigationBar` → UI → Button - TextMeshPro
   - Set size: 60x60 pixels (or similar)
   - Add an icon sprite
   - Space them horizontally with ~20px gaps

### Step 3: Create Play Button

1. Create the Play button:
   - Right-click `UIFortniteLobbyView` → UI → Button - TextMeshPro
   - Name it: `PlayButton`
   - Set RectTransform:
     - Anchor: Bottom-Right
     - Position: X: -150, Y: 100 (adjust to taste)
     - Width: 280, Height: 80

2. Style the button (Fortnite-style):
   - Set a bright color (yellow/green gradient recommended)
   - Make the text bold: "PLAY"
   - Add a shadow or glow effect
   - Font size: 36-42

### Step 4: Create Player Info Panel (Optional)

1. Create a panel for player info:
   - Right-click `UIFortniteLobbyView` → UI → Panel
   - Name it: `PlayerInfoPanel`
   - Set RectTransform:
     - Anchor: Top-Left
     - Position: X: 150, Y: -150
     - Width: 250, Height: 150

2. Add text elements:
   - Player Name (TextMeshPro)
   - Level/XP (TextMeshPro)
   - Progress Bar (UI Slider or Image)

### Step 5: Wire Up Components

1. Select the `UIFortniteLobbyView` GameObject
2. In the Inspector, find the `UIFortniteLobbyView` script component
3. Assign references:
   - **Top Navigation Buttons**:
     - Shop Button → Your shop button
     - Quest Button → Your quest button
     - Locker Button → Your locker button
     - Battle Pass Button → Your battle pass button
     - Settings Button → Your settings button
   - **Main Action Buttons**:
     - Play Button → Your play button
     - Play Button Text → The TextMeshPro component inside the play button
   - **Player Info** (optional):
     - Player Name Text → Your player name text
     - Level Text → Your level text
     - Level Progress Bar → Your progress bar image

4. Configure Quick Play Settings:
   - Search Timeout: 5 (seconds to wait before showing create game)
   - Gameplay Type: BattleRoyale (or your preferred type)
   - Max Players: 100 (or your game's max)
   - Default Map Scene Path: "TPSBR/Scenes/Game" (your main game scene)

### Step 6: Configure UI View System

1. Make sure your `UIFortniteLobbyView` is registered in the UI system
2. You may need to add it to the Context or MenuUI manager
3. Ensure it can open other views like `UICreateSessionView` and `UISettingsView`

### Step 7: Set as Default Menu

To make this the default lobby screen:

1. Open your main menu script (likely `UIMainMenuView.cs`)
2. Add a button or modify the existing Play button to open the Fortnite lobby:
   ```csharp
   Open<UIFortniteLobbyView>();
   ```

OR replace the existing main menu with the Fortnite lobby in the scene hierarchy.

## Customization

### Adjust Search Timeout

In the `UIFortniteLobbyView` component, change the `Search Timeout` value:
- **Lower value** (3s): Faster fallback to create game (good for testing)
- **Higher value** (10s): More time to find existing games (better for production)

### Change Gameplay Type

Set the `Gameplay Type` field to match your game mode:
- BattleRoyale
- TeamDeathmatch
- Etc.

### Modify Button Actions

You can customize what each top navigation button does by editing the script:

```csharp
private void OnShopButtonClicked()
{
    // Open your shop system
    Open<UIShopView>();  // Replace with your shop view
}

private void OnQuestButtonClicked()
{
    // Open your quest system
    Open<UIQuestView>();  // Replace with your quest view
}
```

### Style the UI

Match the Fortnite aesthetic:
1. Use bright, vibrant colors (yellow, blue, purple)
2. Add glow effects and shadows to buttons
3. Use bold, clear fonts
4. Add icons to navigation buttons
5. Consider animated backgrounds or particle effects

## Testing

1. Enter Play Mode
2. The lobby should appear with all buttons visible
3. Click the **PLAY** button
4. Watch the console for debug logs:
   - "Starting quick play search"
   - "Found session" or "No sessions found"
5. If no sessions are found after the timeout, the Create Game UI should appear
6. If a session is found, the game should automatically join it

## Troubleshooting

### Play Button Does Nothing
- Check that the button's OnClick listener is connected
- Verify the `UIFortniteLobbyView` script is attached
- Check the console for error messages

### Create Game UI Doesn't Appear
- Ensure `UICreateSessionView` exists in your project
- Check that the view is properly registered in the UI system
- Verify the timeout is being reached (check console logs)

### Can't Find Sessions
- Make sure you're connected to the Photon lobby
- Check your network settings and region
- Verify Fusion App ID is configured

### Buttons Don't Open Views
- Ensure the target views (UISettingsView, UIAgentSelectionView, etc.) exist
- Check that the UI view system is properly initialized
- Verify Context is available

## Next Steps

1. **Add Icons**: Replace text-only buttons with icons
2. **Add Animations**: Animate button hovers and clicks
3. **Add Shop System**: Implement the shop UI
4. **Add Quest System**: Implement the quest/challenge UI
5. **Add Battle Pass**: Implement season pass progression
6. **Add Player Preview**: Show 3D character in lobby
7. **Add Friends List**: Add social features (not included in this guide as per your request)

## Notes

- The system uses your existing TPSBR networking infrastructure
- It integrates with Fusion Photon Realtime for matchmaking
- The Create Session view is reused from your existing UI
- All placeholder buttons log to console so you can implement features incrementally
