# Fortnite Lobby - Quick Start Guide

## 🎯 What You Get

A complete Fortnite-inspired lobby system with:
- ✅ Top navigation bar (Shop, Quest, Locker, Battle Pass, Settings)
- ✅ Large "PLAY" button that acts as quickplay
- ✅ Automatic game search and join
- ✅ Auto-show create game UI when no sessions found
- ✅ Player info display
- ✅ Button animations and visual feedback

## 🚀 Quick Setup (5 Minutes)

### Option A: Use the Setup Wizard (Easiest!)

1. **Open the Wizard**
   - In Unity, go to: `Tools → Fortnite Lobby → Setup Wizard`

2. **Select Parent Canvas**
   - Find your MenuUI Canvas in the scene
   - Drag it to the "Parent Canvas" field

3. **Click "Create Fortnite Lobby UI"**
   - The wizard creates everything automatically!
   - All components are connected and ready

4. **Customize** (Optional)
   - Adjust colors, sizes, and positions
   - Add your own icons to buttons
   - Modify text and fonts

5. **Test It**
   - Enter Play Mode
   - Click the PLAY button
   - Watch it search and either join or show create game UI

### Option B: Manual Setup

See the full guide: `FORTNITE_LOBBY_SETUP_GUIDE.md`

## 📁 Files Created

| File | Purpose |
|------|---------|
| `UIFortniteLobbyView.cs` | Main lobby controller |
| `UIPlayButtonAnimator.cs` | Play button animations |
| `FortniteLobbySetupWizard.cs` | Editor tool for easy setup |
| `FORTNITE_LOBBY_SETUP_GUIDE.md` | Detailed setup instructions |

## 🎮 How The Play Button Works

```
Player Clicks PLAY
        ↓
[Search for Games - 5 seconds]
        ↓
   ┌────────────────┐
   │ Game Found?    │
   └────────────────┘
         ↙     ↘
      YES       NO
       ↓         ↓
  [Join Game]  [Show Create Game UI]
```

### Configuration

In the `UIFortniteLobbyView` component:

- **Search Timeout**: How long to search (default: 5 seconds)
- **Gameplay Type**: What game mode to search for
- **Max Players**: Maximum players for created games
- **Default Map Scene Path**: Which map to use for new games

## 🎨 Customization Tips

### Change Button Layout

Modify positions in the hierarchy:
- `TopNavigationBar` - Position the top bar
- `PlayButton` - Move the play button
- `PlayerInfoPanel` - Adjust player info location

### Add Icons to Buttons

1. Import your icon sprites
2. Add an Image component to each button
3. Assign the sprite
4. Adjust the layout

### Style the Play Button

Add these components to the Play Button:
- `Outline` - For borders
- `Shadow` - For depth
- `UIPlayButtonAnimator` - For pulsing animation

### Change Colors

Colors are defined in `UIPlayButtonAnimator.cs`:
- `_normalColor` - Default yellow/gold
- `_searchingColor` - Gray when searching
- `_joiningColor` - Green when joining

## 🔧 Integration with Your Game

### Connect to Main Menu

In your existing main menu script:

```csharp
public void OnPlayClicked()
{
    // Open the Fortnite lobby
    Open<UIFortniteLobbyView>();
}
```

### Add Shop Functionality

In `UIFortniteLobbyView.cs`, modify:

```csharp
private void OnShopButtonClicked()
{
    Debug.Log("[UIFortniteLobbyView] Shop button clicked");
    // Add your shop system here
    Open<UIShopView>();  // Replace with your shop
}
```

### Add Quest/Challenge System

```csharp
private void OnQuestButtonClicked()
{
    Debug.Log("[UIFortniteLobbyView] Quest button clicked");
    // Add your quest system here
    Open<UIQuestView>();  // Replace with your quest view
}
```

## 🐛 Troubleshooting

### "No sessions found" immediately
- **Cause**: Not connected to lobby yet
- **Fix**: Wait 1-2 seconds after lobby opens before clicking Play

### Create Game UI doesn't appear
- **Cause**: UICreateSessionView not found
- **Fix**: Ensure UICreateSessionView exists in your UI system

### Button clicks do nothing
- **Cause**: Component references not assigned
- **Fix**: Select UIFortniteLobbyView in hierarchy and assign all fields

### Can't compile
- **Cause**: Missing TPSBR.UI namespace or dependencies
- **Fix**: Ensure you have the TPSBR UI framework in your project

## 📊 Testing Checklist

- [ ] Lobby appears when opened
- [ ] All buttons are visible
- [ ] Play button shows correct text
- [ ] Clicking Play starts search
- [ ] Button text changes to "SEARCHING..."
- [ ] After timeout, Create Game UI appears OR joins found session
- [ ] Settings button opens settings
- [ ] Locker button opens agent selection
- [ ] Player name displays correctly

## 🎯 Next Steps

1. **Add Visual Polish**
   - Import Fortnite-style icons
   - Add gradient backgrounds
   - Add particle effects

2. **Implement Shop System**
   - Create UIShopView
   - Add items and currency
   - Connect to Shop button

3. **Add Quest/Challenge System**
   - Create UIQuestView
   - Add daily/weekly challenges
   - Connect to Quest button

4. **Add Battle Pass**
   - Create UIBattlePassView
   - Add progression system
   - Connect to Battle Pass button

5. **Add Player Customization**
   - Expand locker functionality
   - Add character customization
   - Add cosmetics

## 💡 Pro Tips

- **Faster Testing**: Set Search Timeout to 1-2 seconds during development
- **Better UX**: Add sound effects to button clicks
- **Visual Feedback**: Use the UIPlayButtonAnimator for button pulse effect
- **Matchmaking**: The system uses your existing TPSBR matchmaking infrastructure
- **Networking**: All networking uses Fusion Photon Realtime

## 📚 Additional Resources

- **Full Setup Guide**: `FORTNITE_LOBBY_SETUP_GUIDE.md`
- **Main Script**: `/Assets/Scripts/UIFortniteLobbyView.cs`
- **Editor Tool**: `/Assets/Scripts/Editor/FortniteLobbySetupWizard.cs`
- **Animation Helper**: `/Assets/Scripts/UIPlayButtonAnimator.cs`

## 🙋 Common Questions

**Q: Can I use this with my existing UI?**
A: Yes! It integrates with your existing TPSBR UI system.

**Q: Does this replace my current menu?**
A: No, it's an additional view. You can open it from your main menu or replace the main menu entirely.

**Q: Can I customize the search timeout?**
A: Yes, adjust the "Search Timeout" field in the component inspector.

**Q: Does it work with dedicated servers?**
A: Yes, it works with both Host and Dedicated Server modes.

**Q: Can I add more buttons to the top bar?**
A: Yes, just add more buttons to the TopNavigationBar GameObject and wire them up in the script.

**Q: How do I add icons instead of text buttons?**
A: Add Image components to the buttons and assign your icon sprites.

**Q: Can I change the gameplay type?**
A: Yes, change the "Gameplay Type" field to match your game mode (BattleRoyale, TeamDeathmatch, etc.)

## 🎉 You're Done!

Your Fortnite-style lobby is ready! Test it out and start customizing to match your game's style.

Need help? Check the full setup guide or the inline comments in the scripts.
