# Team System Setup for Testing (2 Players Minimum)

## What Was Fixed

1. **Team Assignment for 2 Players**: Modified `RandomTeamAssignment.cs` to ensure that even with just 2 players, they will be placed on the same team in Duo/Squad modes.

2. **Revive System Always Active**: Updated `ReviveIntegration.cs` to ensure players ALWAYS enter downed state when they die (including fall damage), regardless of team configuration - perfect for testing!

3. **Debug Logging**: Added logging to track team assignments and player deaths.

## Automatic Setup

The easiest way to set up the system is to add the `TeamSystemNetworkSetup` component to your GameplayMode prefabs:

1. Open your GameplayMode prefab (e.g., `BattleRoyale.prefab`, `Deathmatch.prefab`)
2. Add the `TeamSystemNetworkSetup` component
3. Configure the settings:
   - **Enable Team System**: ✓ (checked)
   - **Team Mode**: Duo (or Squad)
   - **Auto Assign Teams**: ✓ (checked)
   - **Enable Revive System**: ✓ (checked)
   - **Enable Debug Logs**: ✓ (checked for testing)

This will automatically:
- Spawn the TeamManager
- Add RandomTeamAssignment to assign players to teams
- Add ReviveIntegration to handle the downed state

## Manual Setup (Alternative)

If you prefer manual setup, add these components to your GameplayMode prefab:

1. `RandomTeamAssignment`:
   - Auto Assign Teams: ✓
   - Default Team Mode: Duo

2. `ReviveIntegration`:
   - Enable Revive System: ✓

You'll also need to ensure TeamManager is spawned in your scene or network prefabs.

## Testing the System

### Test Scenario 1: Two Players - Team Assignment
1. Start a game with 2 players
2. Check console logs - you should see:
   ```
   Assigned player [Name] to existing team [ID]
   ```
3. Both players should be on team ID 1

### Test Scenario 2: Fall Damage - Downed State
1. Have one player jump from a high location
2. When health reaches 0, the player should:
   - Enter downed state (not die)
   - See bleed-out timer (30 seconds)
   - Be able to be revived by teammate
3. Console should show:
   ```
   Player [Name] entering downed state from [HitType]
   ```

### Test Scenario 3: Revive Mechanic
1. Get downed (fall damage or combat)
2. Teammate approaches within 2.5 units
3. Press revive button (default: E or interact key)
4. After 5 seconds, downed player is revived with 30 HP

## Revive System Settings

You can modify these constants in `ReviveData.cs`:

```csharp
public const float REVIVE_DURATION = 5.0f;           // Time to revive
public const float BLEED_OUT_DURATION = 30.0f;       // Time before permanent death
public const float REVIVE_INTERACTION_DISTANCE = 2.5f; // Max distance
public const float REVIVE_HEALTH_RESTORED = 30f;     // HP after revive
```

## Troubleshooting

### Players aren't on the same team
- Check that Team Mode is set to Duo or Squad (not Solo)
- Verify `RandomTeamAssignment` has "Auto Assign Teams" enabled
- Look for debug logs about team assignment

### Players die instantly instead of getting downed
- Ensure `ReviveIntegration` component is present
- Check that "Enable Revive System" is checked
- Verify `ReviveSystem` is added to Player objects
- Look for the debug log "Player [Name] entering downed state"

### Cannot revive teammate
- Check distance (must be within 2.5 units)
- Ensure `ReviveInteraction` component exists
- Verify players are actually teammates (check team IDs)

## Component Hierarchy

```
GameplayMode (e.g., BattleRoyale)
├── TeamSystemNetworkSetup ✓ NEW (recommended)
│   └── Automatically adds:
│       ├── RandomTeamAssignment
│       └── ReviveIntegration
│
Or manually add:
├── RandomTeamAssignment ✓
└── ReviveIntegration ✓

Spawned at Runtime:
└── TeamManager (network object)

Added to Players:
└── ReviveSystem (added automatically)
```

## Key Changes Made

### RandomTeamAssignment.cs
- Added debug logging to track team assignments
- Ensures players always try to join existing teams before creating new ones

### ReviveIntegration.cs  
- Added debug logging to show when players enter downed state
- Always triggers downed state on death (no team checks for testing)

### New: TeamSystemNetworkSetup.cs
- Single component to initialize entire team system
- Spawns TeamManager as network object
- Adds all required components automatically
- Provides runtime configuration methods
