# Quest System Integration Guide

This guide shows **exactly** how to integrate quest tracking into your existing game code.

---

## Quick Integration Checklist

- [ ] Add QuestSystemInitializer to Menu scene
- [ ] Hook match start/end events
- [ ] Hook damage/combat events  
- [ ] Hook pickup/item events
- [ ] Hook storm/zone events
- [ ] Test in Play Mode

---

## 1. Health Component Integration

**File:** `/Assets/TPSBR/Scripts/Gameplay/Components/Health.cs`

Find the method where damage is applied (usually `ProcessHit`, `ApplyDamage`, or similar):

```csharp
// Around line 100-150, in the hit processing method
public void ProcessHit(ref HitData hitData)
{
    // ... existing damage code ...
    
    // ADD THIS: Track damage for quests
    QuestIntegrationPatches.PatchHealthDamage(this, hitData);
    
    // ... rest of method ...
}
```

**Alternative location** - If damage is in a different method:
```csharp
private void ApplyDamage(HitData hitData)
{
    float actualDamage = CalculateDamage(hitData);
    CurrentHealth -= actualDamage;
    
    // ADD THIS: Track damage dealt
    QuestIntegrationPatches.PatchHealthDamage(this, hitData);
}
```

---

## 2. GameplayMode Integration

**File:** `/Assets/TPSBR/Scripts/Gameplay/GameplayModes/BattleRoyaleGameplayMode.cs`

### A. Match Start

In the `OnActivate()` method:

```csharp
protected override void OnActivate()
{
    base.OnActivate();
    
    PrepareAirplane();
    
    // ... existing code ...
    
    // ADD THIS: Notify quest system match started
    QuestIntegrationPatches.PatchGameplayModeActivated();
}
```

### B. Match End

Find the method that ends the match (usually `FinishGameplay()` or `CheckWinCondition()`):

```csharp
protected override void FinishGameplay()
{
    // ... existing finish code ...
    
    // ADD THIS: Calculate and report match results
    if (HasInputAuthority || HasStateAuthority)
    {
        var localPlayer = Context?.NetworkGame?.GetPlayer(Runner.LocalPlayer);
        if (localPlayer != null)
        {
            var stats = localPlayer.Statistics;
            int position = stats.Position;
            int totalPlayers = Context.NetworkGame.ActivePlayers.Count;
            bool isWinner = position == 1;
            
            QuestIntegrationPatches.PatchGameplayModeFinished(position, totalPlayers, isWinner);
        }
    }
    
    base.FinishGameplay();
}
```

### C. Setup References

In `Spawned()` method:

```csharp
public override void Spawned()
{
    base.Spawned();
    
    // ADD THIS: Register with quest system
    if (QuestEventIntegration.Instance != null)
    {
        QuestEventIntegration.Instance.SetGameplayMode(this);
    }
}
```

---

## 3. NetworkGame Integration

**File:** `/Assets/TPSBR/Scripts/Core/NetworkGame.cs`

In the `Spawned()` or initialization method:

```csharp
public override void Spawned()
{
    base.Spawned();
    
    // ... existing initialization ...
    
    // ADD THIS: Register with quest system
    if (QuestEventIntegration.Instance != null)
    {
        QuestEventIntegration.Instance.SetNetworkGame(this);
    }
}
```

---

## 4. Pickup System Integration

**File:** `/Assets/TPSBR/Scripts/Gameplay/Interactions/StaticPickup.cs` (or similar)

In the pickup method:

```csharp
public void OnPickup(Agent agent)
{
    // ... existing pickup code ...
    
    // ADD THIS: Track item pickup for quests
    QuestIntegrationPatches.PatchItemPickup(agent);
    
    // ... rest of pickup logic ...
}
```

**For DynamicPickup.cs:**
```csharp
protected override void OnPickup(Agent agent)
{
    base.OnPickup(agent);
    
    // ADD THIS: Track pickup
    QuestIntegrationPatches.PatchItemPickup(agent);
}
```

**For weapon pickups specifically:**
```csharp
// In WeaponPickup.cs or similar
public override void OnPickup(Agent agent)
{
    base.OnPickup(agent);
    
    // Track pickup
    QuestIntegrationPatches.PatchItemPickup(agent);
    
    // ... weapon equip logic ...
}
```

---

## 5. Healing System Integration

**File:** Health consumable or healing item script

```csharp
public void UseHealingItem(Agent agent)
{
    // ... apply healing ...
    
    // ADD THIS: Track healing item usage
    QuestIntegrationPatches.PatchHealingItemUsed(agent);
}
```

**If using Health.AddHealth():**
```csharp
// In Health.cs
public void AddHealth(float amount)
{
    CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    
    // ADD THIS: If this was from a healing item, track it
    // You may need to add a parameter to distinguish healing sources
    var agent = GetComponent<Agent>();
    if (agent != null)
    {
        QuestIntegrationPatches.PatchHealingItemUsed(agent);
    }
}
```

---

## 6. Storm/Zone Damage Integration

**File:** `/Assets/TPSBR/Scripts/Gameplay/ShrinkingArea.cs` or `/Assets/TPSBR/Scripts/Gameplay/DamageArea.cs`

### A. Storm Damage

```csharp
private void ApplyStormDamage(Agent agent)
{
    // ... apply damage ...
    
    // ADD THIS: Track storm damage taken
    QuestIntegrationPatches.PatchStormDamage(agent);
}
```

**In the FixedUpdateNetwork or damage tick:**
```csharp
public override void FixedUpdateNetwork()
{
    // ... existing storm logic ...
    
    foreach (var agent in _agentsInStorm)
    {
        agent.Health.ApplyDamage(_damagePerTick);
        
        // ADD THIS: Track storm damage
        QuestIntegrationPatches.PatchStormDamage(agent);
    }
}
```

### B. Storm Circle Changes

```csharp
private void OnShrinkingComplete()
{
    _currentStage++;
    
    // ADD THIS: Notify quest system of circle change
    QuestIntegrationPatches.PatchShrinkingAreaChanged(_currentStage);
}
```

---

## 7. Player Landing Integration

**File:** `/Assets/TPSBR/Scripts/Gameplay/Jetpack/Jetpack.cs` or Agent landing detection

```csharp
private void OnLanded()
{
    // ... existing landing code ...
    
    // ADD THIS: Track where player landed
    var agent = GetComponent<Agent>();
    if (agent != null && !_hasLandedBefore)
    {
        _hasLandedBefore = true;
        Vector3 landingPosition = transform.position;
        QuestIntegrationPatches.PatchPlayerLanded(agent, landingPosition);
    }
}
```

**Or in Character Controller:**
```csharp
// When character first touches ground after drop
private void CheckFirstLanding()
{
    if (_character.CharacterController.IsGrounded && !_hasLanded)
    {
        _hasLanded = true;
        var agent = GetComponent<Agent>();
        QuestIntegrationPatches.PatchPlayerLanded(agent, transform.position);
    }
}
```

---

## 8. Alternative: Use GameplayQuestHooks (Simpler)

Instead of manual integration, add `GameplayQuestHooks` to your gameplay scene:

1. Create GameObject named "GameplayQuestHooks" in Game scene
2. Add `GameplayQuestHooks` component
3. It will auto-register most events
4. Use static methods for specific events:

```csharp
// In any script where events occur:

// Item pickup
GameplayQuestHooks.NotifyItemPickup(agent);

// Healing used
GameplayQuestHooks.NotifyHealingUsed(agent);

// Storm damage
GameplayQuestHooks.NotifyStormDamage(agent);

// Damage dealt
GameplayQuestHooks.NotifyDamageDealt(health, hitData);

// Player landed
GameplayQuestHooks.NotifyPlayerLanded(agent, position);

// Storm circle changed
GameplayQuestHooks.NotifyStormCircleChanged(circleNumber);
```

---

## 9. Testing Your Integration

### A. Console Logs
Enable debug logging to verify events are firing:

```csharp
// Add to any integration point temporarily:
Debug.Log($"[Quest Integration] Event fired: {eventName}");
```

### B. Test in Play Mode

1. Enter Play Mode
2. Open Console (Ctrl+Shift+C)
3. Look for `[Quest System]` and `[Quest Hooks]` messages
4. Perform actions (get kills, loot items, etc.)
5. Check quest progress updates

### C. Quest Menu Testing

1. Open quest menu in-game
2. Verify quests are shown
3. Complete quest objectives
4. Claim rewards
5. Check CloudCoins increase

---

## 10. Common Integration Patterns

### Pattern 1: Event-Based
```csharp
// Use when you have events already set up
public class MyGameSystem : MonoBehaviour
{
    public event Action<Agent> OnItemCollected;
    
    private void Start()
    {
        OnItemCollected += (agent) => {
            QuestIntegrationPatches.PatchItemPickup(agent);
        };
    }
}
```

### Pattern 2: Direct Call
```csharp
// Use for immediate tracking
public void PerformAction(Agent agent)
{
    DoActionLogic();
    QuestIntegrationPatches.PatchItemPickup(agent);
}
```

### Pattern 3: Conditional Tracking
```csharp
// Use when tracking needs conditions
public void OnDamageDealt(HitData hitData)
{
    if (hitData.Instigator.IsValid && IsLocalPlayer(hitData.Instigator))
    {
        QuestIntegrationPatches.PatchHealthDamage(health, hitData);
    }
}
```

---

## 11. Minimum Required Integration

**At minimum, integrate these 3 things for basic functionality:**

1. **Match Start/End** (GameplayMode)
   ```csharp
   QuestIntegrationPatches.PatchGameplayModeActivated();
   QuestIntegrationPatches.PatchGameplayModeFinished(pos, total, isWinner);
   ```

2. **Combat Events** (Health/Damage)
   ```csharp
   QuestIntegrationPatches.PatchHealthDamage(this, hitData);
   ```

3. **Item Pickups** (Pickup system)
   ```csharp
   QuestIntegrationPatches.PatchItemPickup(agent);
   ```

**This will enable most quests to work!**

---

## 12. Troubleshooting Integration

### Issue: Events not firing

**Check:**
- Is QuestSystemInitializer in the scene?
- Is the integration code being executed? (Add Debug.Log)
- Is the method being called? (Check with breakpoints)

**Solution:**
```csharp
// Add temporary logging:
Debug.Log($"[Quest] Calling patch at {Time.time}");
QuestIntegrationPatches.PatchItemPickup(agent);
```

### Issue: Quest progress not updating

**Check:**
- Is QuestManager initialized? (Check console for init message)
- Are you in Play Mode?
- Is the quest active for today?

**Solution:**
```csharp
// Verify quest manager exists:
if (QuestManager.Instance != null)
{
    Debug.Log("Quest Manager is active!");
}
else
{
    Debug.LogError("Quest Manager is NULL!");
}
```

### Issue: Wrong player getting credit

**Check:**
- Are you filtering for local player?
- Is InputAuthority correct?

**Solution:**
```csharp
// Add local player check:
if (agent.Object.InputAuthority == Runner.LocalPlayer)
{
    QuestIntegrationPatches.PatchItemPickup(agent);
}
```

---

## 13. Performance Considerations

The quest system is lightweight, but follow these best practices:

### ✅ DO:
- Call integration patches only for local player events
- Use existing event systems when available
- Cache quest manager references

### ❌ DON'T:
- Call patches every frame unnecessarily
- Track events for all players (only local)
- Add integration in hot paths (Update loops)

**Good Example:**
```csharp
private void OnKillObtained(KillData killData)
{
    // Only track for local player
    if (IsLocalPlayer(killData.KillerRef))
    {
        TrackQuestProgress();
    }
}
```

**Bad Example:**
```csharp
private void Update()
{
    // DON'T call every frame!
    QuestIntegrationPatches.PatchItemPickup(agent);
}
```

---

## Need Help?

1. Check the main `QUEST_SYSTEM_README.md`
2. Use the Setup Wizard: `TPSBR > Quest System Setup Wizard`
3. Check console for error messages with `[Quest` prefix
4. Verify setup with Wizard's "Validate Setup" button

**The integration is designed to be non-intrusive and should not affect existing game functionality!**
