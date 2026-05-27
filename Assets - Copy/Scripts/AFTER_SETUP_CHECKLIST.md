# After Setup - Complete These Steps! ✅

## 🎯 Your Current Status

✅ UIFortniteLobbyView created in scene  
✅ All UI elements created (buttons, panels, text)  
⚠️ Button references need to be connected  
⚠️ Need to integrate with main menu  
⚠️ Need to test

## 📋 Step-by-Step Checklist

### Step 1: Connect Button References (REQUIRED)

The wizard created the UI but some references need to be connected manually:

1. **Select** the `UIFortniteLobbyView` GameObject in the Hierarchy
2. **In the Inspector**, find the `UIFortniteLobbyView` script component
3. **Assign these references** by dragging from the Hierarchy:

```
Top Navigation Buttons:
├─ Shop Button       → Drag: TopNavigationBar/ShopButton
├─ Quest Button      → Drag: TopNavigationBar/QuestButton  
├─ Locker Button     → Drag: TopNavigationBar/LockerButton
├─ Battle Pass Button → Drag: TopNavigationBar/BattlePassButton
└─ Settings Button   → Drag: TopNavigationBar/SettingsButton

Main Action Buttons:
├─ Play Button       → Drag: PlayButton
└─ Play Button Text  → Already connected ✅

Player Info:
├─ Player Name Text  → Already connected ✅
└─ Level Text        → Already connected ✅
```

**How to drag references:**
- Click and hold on the GameObject in Hierarchy (e.g., `ShopButton`)
- Drag it to the corresponding field in the Inspector
- Release to assign

### Step 2: Verify Settings

Check these settings in the `UIFortniteLobbyView` component:

```
✅ Search Timeout: 5 (seconds)
✅ Gameplay Type: BattleRoyale  
✅ Max Players: 100
✅ Default Map Scene Path: "TPSBR/Scenes/Game"
```

These are already set correctly! 

### Step 3: Integrate with Your Main Menu

You need to tell your game to open this new lobby. Choose ONE option:

#### Option A: Replace Existing Main Menu (Recommended)

1. Find your current `UIMainMenuView` in the Hierarchy
2. Right-click it → Disable (or delete if you don't need it)
3. Set `UIFortniteLobbyView` to open by default when the menu scene loads

#### Option B: Add Button to Open Fortnite Lobby

Add this to your existing main menu script (like `UIMainMenuView.cs`):

```csharp
private void OnPlayButton()
{
    // Open the new Fortnite-style lobby
    Open<UIFortniteLobbyView>();
}
```

#### Option C: Test Standalone First

Just disable your other UI views temporarily and enable `UIFortniteLobbyView` in the scene to test it.

### Step 4: Set Initial State

1. Select `UIFortniteLobbyView` in Hierarchy
2. In Inspector, set the GameObject to **Active** (checkbox should be ON)
3. Or, if using the UI system, set it to open on scene load

### Step 5: Test It! 🎮

1. **Enter Play Mode**
2. You should see:
   - Top navigation bar with 5 buttons
   - Player info panel on the left
   - Large PLAY button on the right

3. **Click the PLAY button**
4. Watch the console logs:
   ```
   [UIFortniteLobbyView] Play button clicked - Starting Quick Play
   [UIFortniteLobbyView] Starting quick play search
   ```

5. Wait 5 seconds (or your search timeout)

6. **Expected behavior:**
   - Button text changes: "PLAY" → "SEARCHING..." → "JOINING..." (if game found)
   - If no games found: Create Game UI should appear
   - If game found: Automatically joins the session

### Step 6: Test Navigation Buttons

Click each top button and check console:
- Shop button → "[UIFortniteLobbyView] Shop button clicked"
- Quest button → "[UIFortniteLobbyView] Quest button clicked"  
- Locker button → Should open Agent Selection
- Battle Pass button → "[UIFortniteLobbyView] Battle Pass button clicked"
- Settings button → Should open Settings

## 🎨 Optional Customization

Once it's working, customize it:

### Add Icons to Buttons

1. Import icon sprites to your project
2. For each button in TopNavigationBar:
   - Add an `Image` component to the button GameObject
   - Assign your icon sprite
   - Adjust size (recommended: 40x40 pixels)
3. You can hide or shrink the text labels

### Style the Play Button

1. Select the `PlayButton` GameObject
2. Modify the `Image` component:
   - Change color to bright yellow/gold
   - Add outline effect (Component → UI → Effects → Outline)
   - Add shadow effect (Component → UI → Effects → Shadow)
3. Select `PlayButton/Text`:
   - Make text bold
   - Increase font size (48-60pt)
   - Change color to black or white

### Add Button Animation

1. Select the `PlayButton` GameObject
2. Add Component → `UIPlayButtonAnimator`
3. Configure:
   - Enable Pulse: ✓
   - Pulse Speed: 2.0
   - Normal Color: Yellow
   - Searching Color: Gray
   - Joining Color: Green

### Adjust Layout

Move things around to match your game's style:
- **TopNavigationBar**: Change height, padding, button sizes
- **PlayButton**: Reposition (right-click → Anchor Presets)
- **PlayerInfoPanel**: Move, resize, add background image

## 🐛 Troubleshooting

### "Nothing happens when I click Play"
✅ **Fix:** Make sure all button references are assigned in Step 1

### "Create Game UI doesn't appear"
✅ **Fix:** Check that `UICreateSessionView` exists in your UI system and is registered

### "Buttons are missing or invisible"
✅ **Fix:** Check Canvas Scaler settings and make sure Canvas is set to Screen Space

### "Console shows errors"
✅ **Fix:** Share the error message - check if UICreateSessionView or Context is missing

### "Can't find sessions"
✅ **Fix:** 
- Make sure Photon App ID is configured
- Check network connection
- Verify you're connected to the lobby

## 📊 Quick Test Checklist

Before considering it "done", test these:

- [ ] Lobby appears when scene loads
- [ ] All 5 top buttons are visible
- [ ] Play button is visible and large
- [ ] Player name displays (should show your nickname)
- [ ] Click Play → Text changes to "SEARCHING..."
- [ ] After 5 seconds → Create Game UI appears OR joins a game
- [ ] Click Settings → Settings menu opens
- [ ] Click Locker → Agent selection opens
- [ ] All buttons respond to clicks
- [ ] No errors in console

## 🎯 What to Do After Testing

### If Everything Works ✅

Congratulations! Now you can:

1. **Customize the look** - Add your icons, colors, fonts
2. **Implement Shop** - Replace the placeholder with your shop system
3. **Add Quests** - Create a quest/challenge UI
4. **Add Battle Pass** - Create progression system
5. **Polish animations** - Add button effects, transitions
6. **Add sounds** - Hook up button click sounds

### If Something Doesn't Work ❌

1. Check the troubleshooting section above
2. Verify all button references are assigned
3. Check the console for error messages
4. Make sure your networking is set up (Photon App ID)
5. Test with a fresh scene if needed

## 🚀 Next Features to Add

### Priority 1: Core Features
1. **Shop System** - Let players buy items/skins
2. **Quest System** - Daily/weekly challenges
3. **Better Matchmaking** - Region selection, game mode selection

### Priority 2: Polish
1. **Button Icons** - Replace text with professional icons
2. **Animations** - Smooth transitions and button effects
3. **Sound Effects** - Button clicks, whooshes
4. **Background** - Animated background or 3D scene

### Priority 3: Advanced
1. **Battle Pass** - Season progression and rewards
2. **Player Stats** - Display wins, kills, matches played
3. **Cosmetics Preview** - Show equipped items
4. **News Feed** - Display updates and events

## 💡 Pro Tips

- **Faster Testing**: Set Search Timeout to 1-2 seconds during development
- **Debug Logs**: Keep console open to see what's happening
- **Safe Area**: The UI respects safe areas for mobile devices
- **UI System**: This integrates with your TPSBR UI framework automatically

## 📚 Reference Files

- **Quick Start**: `FORTNITE_LOBBY_QUICKSTART.md`
- **Full Setup**: `FORTNITE_LOBBY_SETUP_GUIDE.md`
- **Layout Guide**: `FORTNITE_LOBBY_LAYOUT_REFERENCE.md`
- **Main Script**: `/Assets/Scripts/UIFortniteLobbyView.cs`

## ✨ You're Almost Done!

Complete Steps 1-5 above and you'll have a working Fortnite-style lobby with quickplay functionality! The Play button will automatically search for games and show the create game UI if none are found.

Need help? Check the console for debug messages - they'll tell you exactly what's happening!
