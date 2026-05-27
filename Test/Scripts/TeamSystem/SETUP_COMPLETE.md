# Team System + Revive System - Complete!

## ✅ What Was Fixed

### Compilation Errors Resolved
All 8 compilation errors have been fixed:

1. **TeamGameplayIntegration.cs** - Changed from `OnInitialize/OnDeinitialize` to `FixedUpdateNetwork/Despawned`
2. **UITeamPanel.cs** - Changed from `OnInitialize/OnDeinitialize` to `Awake/OnEnable/OnDisable`
3. **UITeammateIndicatorManager.cs** - Changed to `OnEnable/OnDisable`
4. **TeamSystemInitializer.cs** - Fixed base class from ContextBehaviour to MonoBehaviour

### Root Cause
The TPSBR project uses `Awake()` for initialization in UI components (not `OnInitialize()`), and ContextBehaviour doesn't have those override methods.

---

## 🎮 New Feature: Fortnite-Style Revive System

### What You Asked For
✅ **Downed State** - Players crouch when "killed" in team modes (can't shoot)
✅ **Hold E to Revive** - Teammates hold E for 5 seconds to revive
✅ **30-Second Timer** - Players bleed out if not revived
✅ **Team Only** - Only teammates within 2.5m can revive
✅ **Works Like Fortnite** - Exactly as requested!

### How It Works

**When Player Dies (Duo/Squad):**
1. Instead of dying, enters downed state
2. Character is forced into crouch
3. Weapons and actions disabled
4. 30-second bleed-out timer starts
5. Can still look around and see teammates

**Reviving Process:**
1. Teammate walks within 2.5 meters
2. UI shows "Hold [E] to Revive {PlayerName}"
3. Hold E for 5 seconds (progress bar fills)
4. Player is revived with 30 health
5. Can move and shoot again

**If Not Revived:**
- After 30 seconds, player dies permanently
- Counts as elimination

**Solo Mode:**
- Revive system disabled
- Death is instant (normal behavior)

---

## 📁 Files Created

### Team System (Original - Fixed)
```
/Assets/Scripts/TeamSystem/
├── TeamData.cs
├── TeamManager.cs
├── PartyLobbyManager.cs
├── TeamMatchmaker.cs
├── TeamGameplayIntegration.cs ✓ FIXED
├── RandomTeamAssignment.cs
├── PlayerTeamExtension.cs
├── TeamSystemInitializer.cs ✓ FIXED
├── UI/
│   ├── UITeamPanel.cs ✓ FIXED
│   ├── UITeamMemberWidget.cs
│   ├── UIPartyLobbyView.cs
│   ├── UIFriendWidget.cs
│   ├── UITeammateIndicator.cs
│   └── UITeammateIndicatorManager.cs ✓ FIXED
├── Examples/
│   └── TeamFriendlyFirePrevention.cs
└── README.md
```

### Revive System (NEW)
```
/Assets/Scripts/TeamSystem/Revive/
├── ReviveData.cs                    # Settings: duration, distance, health
├── ReviveSystem.cs                  # Core revive logic (NetworkBehaviour)
├── ReviveInteraction.cs             # E key detection and interaction
├── ReviveIntegration.cs             # Integrates with gameplay
├── PlayerReviveExtension.cs         # Helper methods
├── DownedPlayerMarker.cs            # Visual marker above downed players
├── UI/
│   ├── UIRevivePrompt.cs            # Shows "Hold E" for revivers
│   └── UIDownedState.cs             # Shows downed status & timer
├── Examples/
│   ├── CustomReviveBehavior.cs      # Add audio/VFX to events
│   └── ReviveSystemDebugger.cs      # Debug keys (K/L/P)
└── README_REVIVE.md                 # Complete documentation
```

---

## 🚀 Quick Setup (Follow These Steps)

### Step 1: Add to Game Scene

Open `Assets/TPSBR/Scenes/Game.unity`:

1. **Find or create "GameSystems" GameObject**
2. **Add these components:**
   - `ReviveIntegration` ✓
   - `ReviveInteraction` ✓
3. **Configure ReviveInteraction:**
   - Player Layer: Set to "Agent" or your player layer

### Step 2: Create Revive UI

You need to create 2 UI panels in your gameplay canvas:

#### A) UIRevivePrompt (For Players Reviving Others)

Create hierarchy:
```
Canvas (your gameplay UI)
└── Panel "RevivePrompt"
    ├── Text "PromptText"        → Set text: "Hold [E] to Revive"
    ├── Text "PlayerNameText"    → Shows downed player name
    └── Image "ProgressFill"     → Image Type: Filled, Fill Amount: 0
```

Add `UIRevivePrompt` component to "RevivePrompt" panel.
Assign all references in Inspector.

#### B) UIDownedState (For Downed Players)

Create hierarchy:
```
Canvas (your gameplay UI)
└── Panel "DownedState"
    ├── Text "StatusText"           → "You are down!"
    ├── Image "BleedOutFill"        → Shows time remaining (filled)
    ├── Text "BleedOutTimer"        → "30s"
    └── Panel "BeingRevivedPanel"
        ├── Text "ReviverNameText"  → "PlayerX is reviving you..."
        └── Image "ReviveProgress"  → Revive progress bar (filled)
```

Add `UIDownedState` component to "DownedState" panel.
Assign all references in Inspector.

### Step 3: (Optional) Add Debug Helper

Create GameObject "ReviveDebugger" in scene:
- Add `ReviveSystemDebugger` component
- Enable Debug Keys

**Debug Controls:**
- **K** = Force downed state
- **L** = Force revive
- **P** = Print status to console

### Step 4: Test It!

1. **Start game with 2+ players**
2. **Set team mode to Duo or Squad**
3. **Test downed state:**
   - Press K (debug) or take fatal damage
   - Should enter crouch, can't shoot
   - See 30-second bleed-out timer
4. **Test reviving:**
   - Teammate walks close
   - Sees "Hold [E] to Revive"
   - Holds E for 5 seconds
   - Player revives with 30 health

---

## ⚙️ Customization

### Change Revive Settings

Edit `/Assets/Scripts/TeamSystem/Revive/ReviveData.cs`:

```csharp
public const float REVIVE_DURATION = 5.0f;           // Time to revive (seconds)
public const float BLEED_OUT_DURATION = 30.0f;       // Time until permanent death
public const float REVIVE_INTERACTION_DISTANCE = 2.5f; // Range to revive (meters)
public const float REVIVE_HEALTH_RESTORED = 30f;     // Health after revive
```

### Change Revive Key

Edit `/Assets/Scripts/TeamSystem/Revive/ReviveInteraction.cs` line ~105:

```csharp
// Change from E to F:
if (Keyboard.current.fKey.isPressed)

// Or use your Input Action:
if (_reviveInputAction.IsPressed())
```

### Add Custom Events

```csharp
var reviveSystem = player.GetComponent<ReviveSystem>();

reviveSystem.OnPlayerDowned += (downedPlayer) =>
{
    Debug.Log($"{downedPlayer.Nickname} is down!");
    // Play sound, spawn VFX, etc
};

reviveSystem.OnReviveCompleted += (revivedPlayer, reviver) =>
{
    Debug.Log($"{reviver.Nickname} saved {revivedPlayer.Nickname}!");
    // Award points, play sound, etc
};
```

See `Examples/CustomReviveBehavior.cs` for audio/VFX example.

---

## 💡 Usage Examples

### Check if Player is Downed

```csharp
using TPSBR;

if (player.IsDown())
{
    Debug.Log("Player is downed!");
}

if (player.IsBeingRevived())
{
    Debug.Log("Someone is reviving them!");
}

float timeLeft = player.GetBleedOutProgress();
```

### Prevent Damage to Downed Players

```csharp
public void ApplyDamage(Player victim, float damage)
{
    if (victim.IsDown())
    {
        // Don't damage downed players
        return;
    }
    
    // Apply damage normally
}
```

### Custom Down Behavior

```csharp
var reviveSystem = player.GetComponent<ReviveSystem>();

reviveSystem.OnPlayerDowned += (player) =>
{
    // Play downed animation
    // Show help UI
    // Notify teammates
};
```

---

## 📖 Full Documentation

- **Team System**: `/Assets/Scripts/TeamSystem/README.md`
- **Revive System**: `/Assets/Scripts/TeamSystem/Revive/README_REVIVE.md`

---

## ✅ Testing Checklist

### Team System Tests
- [ ] Solo mode: Only your name shows
- [ ] Duo mode: 2 players per team
- [ ] Squad mode: 4 players per team
- [ ] Team panel displays all teammates
- [ ] Friendly fire prevention works

### Revive System Tests
- [ ] Solo mode: Death is instant (no revive)
- [ ] Duo/Squad: Death enters downed state
- [ ] Downed player is crouching
- [ ] Downed player cannot shoot
- [ ] 30-second timer counts down
- [ ] Teammate can see "Hold E" prompt within range
- [ ] Holding E for 5 seconds revives player
- [ ] Revived player has 30 health
- [ ] Bleed-out after 30s kills player permanently
- [ ] UI shows correct status (downed, being revived, timer)

---

## 🎯 Summary

**Fixed:** All compilation errors in team system
**Added:** Complete Fortnite-style revive system with:
- Downed state (crouch, can't shoot)
- Hold E for 5 seconds to revive
- 30-second bleed-out timer
- Team-only reviving (2.5m range)
- Full UI for revive prompts and status
- Debug tools for testing
- Extension methods for easy integration

**Next Steps:**
1. Add `ReviveIntegration` and `ReviveInteraction` to Game scene
2. Create the 2 UI panels (UIRevivePrompt, UIDownedState)
3. Test with multiple players in Duo/Squad mode
4. Customize settings in `ReviveData.cs`
5. Add custom audio/VFX with `CustomReviveBehavior.cs`

Everything is ready to go! 🚀
