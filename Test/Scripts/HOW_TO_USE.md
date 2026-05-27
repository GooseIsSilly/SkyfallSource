# How to Use Quick Match - Visual Guide

## 🎯 Goal
Add a single "PLAY" button that automatically finds games or creates one if none exist.

---

## 🚀 FASTEST METHOD (30 seconds)

### Step 1: Find Your Play Button
In your Menu scene, locate the button you want to use for Quick Match

### Step 2: Add the Component
1. Select the button in the Hierarchy
2. In the Inspector, click "Add Component"
3. Type "UIQuickMatchButton"
4. Click to add it

### Step 3: Test!
1. Click Play in Unity Editor
2. Click your button
3. Watch it automatically search and create/join a session!

**DONE!** ✅

---

## 📋 ALTERNATIVE METHOD (1 minute)

If you have existing button code:

### Your Existing Code:
```csharp
public void OnPlayButtonClick()
{
    // Your old code here
}
```

### Update to This:
```csharp
using TPSBR.UI;

public void OnPlayButtonClick()
{
    FindObjectOfType<UIMultiplayerView>().StartQuickMatch();
}
```

**DONE!** ✅

---

## 🎨 CUSTOM UI METHOD (5 minutes)

Want status messages and a cancel button?

### Step 1: Create UI GameObject
1. In Menu scene, right-click Hierarchy
2. Create Empty → Name it "QuickMatchView"

### Step 2: Add Component
1. Select "QuickMatchView"
2. Add Component → "UIQuickMatchView"

### Step 3: Create Status Text (Optional)
1. Right-click QuickMatchView → UI → Text - TextMeshPro
2. Name it "StatusText"
3. Drag it to the "Status Text" field in UIQuickMatchView

### Step 4: Create Cancel Button (Optional)
1. Right-click QuickMatchView → UI → Button - TextMeshPro  
2. Name it "CancelButton"
3. Drag it to the "Cancel Button" field in UIQuickMatchView

### Step 5: Configure Settings
In the UIQuickMatchView component:
- Search Timeout: 5 (seconds to search)
- Gameplay Type: BattleRoyale
- Max Players: 100
- Default Map Scene Path: TPSBR/Scenes/Game

### Step 6: Call It
```csharp
FindObjectOfType<UIMultiplayerView>().StartQuickMatch();
```

**DONE!** ✅

---

## ⚡ WHAT HAPPENS WHEN USER CLICKS PLAY

```
┌─────────────────────────────────────┐
│  User Clicks "PLAY"                 │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│  Connecting to lobby...             │
│  (1-2 seconds)                      │
└──────────────┬──────────────────────┘
               ↓
┌─────────────────────────────────────┐
│  Searching for session... (5s)      │
│  (Countdown: 5, 4, 3, 2, 1...)     │
└──────────────┬──────────────────────┘
               ↓
        ┌──────┴──────┐
        ↓             ↓
┌───────────────┐  ┌────────────────────┐
│ Found Session │  │ No Session Found   │
└───────┬───────┘  └─────────┬──────────┘
        ↓                    ↓
┌───────────────┐  ┌────────────────────┐
│ Join Game!    │  │ Create New Game!   │
│ (as Client)   │  │ (as Host)          │
└───────────────┘  └────────────────────┘
```

---

## 🧪 TESTING

### Test 1: Create New Game
1. Make sure no games are running
2. Click PLAY
3. Wait 5 seconds
4. ✅ You should become host of new game

### Test 2: Join Existing Game  
1. Start a game on another PC/build
2. On first PC, click PLAY
3. ✅ Should join immediately (no 5 second wait)

### Test 3: Cancel
1. Click PLAY
2. During the "Searching..." phase, click Cancel
3. ✅ Should return to menu

---

## ❓ TROUBLESHOOTING

### Nothing happens when I click the button
**Fix**: Make sure UIMultiplayerView exists in your Menu scene

### Always creates new game (never finds existing ones)
**Fix**: 
- Check both builds are using same Photon App ID
- Verify same region is selected
- Ensure games are set to "visible" and "open"

### Get errors in console
**Fix**: Make sure you've added the `using TPSBR.UI;` line at the top of your script

---

## 📝 COMPLETE CODE EXAMPLE

Here's a complete, copy-paste-ready example:

```csharp
using UnityEngine;
using TPSBR.UI;

public class MenuController : MonoBehaviour
{
    // Attach this to your Play button's onClick event
    public void OnPlayButtonPressed()
    {
        var multiplayerView = FindObjectOfType<UIMultiplayerView>();
        
        if (multiplayerView != null)
        {
            multiplayerView.StartQuickMatch();
        }
        else
        {
            Debug.LogWarning("UIMultiplayerView not found!");
        }
    }
}
```

**To use this:**
1. Create new script called `MenuController.cs`
2. Copy the code above
3. Attach script to any GameObject in Menu scene
4. In your Play button's Inspector:
   - Find "On Click ()" event
   - Click "+"
   - Drag the GameObject with MenuController
   - Select MenuController → OnPlayButtonPressed

**DONE!** ✅

---

## 🎉 Summary

You now have **automatic matchmaking** that:
- ✅ Finds available games
- ✅ Joins them automatically  
- ✅ Creates new games if none exist
- ✅ Works with 1 line of code
- ✅ No setup required

**Just call `StartQuickMatch()` and everything else is automatic!**
