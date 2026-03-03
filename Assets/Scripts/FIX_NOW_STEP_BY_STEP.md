# Fix Your Shop Icons & Characters - 3 Easy Steps

## The Problem Explained

You have TWO different character selection systems:

1. **Character Selection** (what you saw) = Shows OWNED characters you can select
2. **Shop** (Agent Shop) = Where you BUY new characters with CloudCoins

Right now, the Character Selection is empty because the IDs don't match!

### ID Mismatch Issue:

```
Your AgentSettings uses:          Your CharacterData uses:
- Agent.Soldier                   - characterID: "Soldier66" ❌
- Agent.Marine                    - characterID: "Marine" ❌
                                  - agentID: "" (empty!) ❌

ShopSystem checks ownership by ID, so they MUST match!
```

## The Fix (Takes 2 Minutes)

### Step 1: Exit Play Mode
```
Click the ⏸️ button to exit Play Mode
(Scripts can only be fixed when NOT playing)
```

### Step 2: Run the Auto-Fix
```
1. Unity Menu → TPSBR → Fix Shop Setup
2. Click "Fix All Issues" button
3. Wait for these messages in Console:
   ✅ "Fixed soldier66 - characterID: Agent.Soldier, agentID: Agent.Soldier"
   ✅ "Fixed marine - characterID: Agent.Marine, agentID: Agent.Marine"
   ✅ "Assigned UIShopItem prefab to UIList!"
   ✅ "Menu scene saved!"
```

### Step 3: Test It!
```
1. Press Play ▶️
2. From main menu, look for the character selector
3. You should now see Soldier 66 with icon!
```

## What the Fix Does

### Before Fix:
```
CharacterData/soldier66.asset:
  characterID: "Soldier66"  ❌ Wrong
  agentID: ""               ❌ Empty

AgentSettings:
  ID: "Agent.Soldier"       ✓ Correct

ShopSystem checks:
  OwnsAgent("Agent.Soldier") → looks for "Agent.Soldier"
  But has: "Soldier66"
  Result: No match = no character shows! ❌
```

### After Fix:
```
CharacterData/soldier66.asset:
  characterID: "Agent.Soldier"  ✓ Matches!
  agentID: "Agent.Soldier"      ✓ Matches!

AgentSettings:
  ID: "Agent.Soldier"           ✓ Matches!

ShopSystem checks:
  OwnsAgent("Agent.Soldier") → looks for "Agent.Soldier"
  Has: "Agent.Soldier"
  Result: MATCH = character shows! ✅
```

## Verification Checklist

After running the fix, verify:

### ✅ In Project Window:
```
1. Find: Assets/Scripts/CharacterData/soldier66.asset
2. Select it
3. In Inspector, verify:
   - Character ID: "Agent.Soldier" ✓
   - Display Name: "Soldier66" ✓
   - Agent ID: "Agent.Soldier" ✓
   - Icon: SoldierIcon.png ✓
   - Price: 0 ✓
   - Unlocked by Default: ✓ (checked)
```

### ✅ In Play Mode:
```
1. Press Play
2. Open Character Selection
3. Should see:
   - Soldier 66 with icon ✓
   - Description text ✓
   - "SELECT" button ✓
```

## Understanding the Two Systems

### System 1: Character Selection (UIAgentSelectionView)
```
Purpose: Select between OWNED characters
Location: Main menu → Character Selection
Shows: Only characters you own
Logic: ShopSystem.OwnsAgent(agentID)

┌────────────────────────────────┐
│   Character Selection          │
│                                │
│   [Icon] Soldier 66            │
│   Description here...          │
│   [SELECT]                     │
└────────────────────────────────┘
```

### System 2: Agent Shop (UIShopView)
```
Purpose: BUY new characters with CloudCoins
Location: Main menu → SHOP button
Shows: All characters (owned + locked)
Logic: Purchase with coins → unlock character

┌────────────────────────────────┐
│   AGENT SHOP                   │
│   CloudCoins: 750              │
│                                │
│   [Icon] Soldier 66  [OWNED]   │
│   FREE                         │
│                                │
│   [Icon] Marine      [BUY]     │
│   750 CloudCoins               │
└────────────────────────────────┘
```

## How They Work Together

```
1. Player starts game
   ↓
2. ShopSystem initializes
   ↓
3. Checks ShopDatabase for default unlocked characters
   ↓
4. Adds "Agent.Soldier" to owned list
   ↓
5. Character Selection opens
   ↓
6. Calls GetOwnedAgents()
   ↓
7. Filters AgentSettings by ShopSystem.OwnsAgent(id)
   ↓
8. Shows only owned characters = Soldier 66 appears! ✅
   ↓
9. Player goes to Shop
   ↓
10. Can buy Marine for 750 coins
    ↓
11. Marine added to owned list
    ↓
12. Character Selection now shows BOTH characters! ✅
```

## Common Questions

### Q: Why two systems?
**A:** 
- Character Selection = Choose your active character (free, no coins)
- Shop = Unlock new characters (costs coins)

Like most games:
- Locker = Choose skin
- Store = Buy new skins

### Q: What if I don't want a shop?
**A:** You can ignore the shop system and just use Character Selection. All characters will be owned by default.

### Q: Can I add more characters?
**A:** Yes!
```
1. TPSBR → Character & Shop Setup
2. Fill in:
   - Character ID: "Agent.Sniper"
   - Agent ID: "Agent.Sniper" (must match AgentSettings!)
   - Icon, price, etc.
3. Create asset
4. Done!
```

### Q: Icons still don't show?
**A:** Check these:
1. Icon file exists at path shown in CharacterData
2. Icon is actually a Sprite (not Texture2D)
3. Agent ID EXACTLY matches (case-sensitive!)

## Still Not Working?

### Debug Step 1: Check AgentSettings
```
1. Find: Assets/TPSBR/Resources/Settings/AgentSettings.asset
2. Open it
3. Note down the exact IDs:
   - Agent[0].ID = ?
   - Agent[1].ID = ?
```

### Debug Step 2: Check CharacterData
```
1. Find: Assets/Scripts/CharacterData/soldier66.asset
2. Compare:
   - characterID should match AgentSettings ID
   - agentID should match AgentSettings ID
```

### Debug Step 3: Check ShopSystem Owned List
```
Add this debug code somewhere in your UI:

var ownedAgents = Context.PlayerData.ShopSystem.OwnedSkins;
foreach (var id in ownedAgents)
{
    Debug.Log($"Owns: {id}");
}

Should see:
Owns: Agent.Soldier ✓
```

### Debug Step 4: Check Icon File
```
1. Find icon at: Assets/TPSBR/UI/AgentIcons/SoldierIcon.png
2. Select it
3. In Inspector:
   - Texture Type: Sprite (2D and UI) ✓
   - If not, change it and click Apply
```

## Final Notes

**After running the fix:**
- Soldier 66 should appear in Character Selection ✅
- Icons should display ✅
- Shop should show both characters ✅
- Marine should be purchasable for 750 coins ✅

**The key fix was:**
Making sure all IDs match across the three systems:
1. AgentSettings (spawning)
2. CharacterData (shop config)  
3. ShopSystem (ownership tracking)

---

**Still stuck?** Check `TROUBLESHOOTING_GUIDE.md` for more detailed debugging steps.
