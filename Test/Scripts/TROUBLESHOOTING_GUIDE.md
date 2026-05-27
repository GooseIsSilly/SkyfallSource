# Shop System Troubleshooting Guide

## Current Issues & Fixes

### Issue 1: No Characters Showing in Shop ❌

**Symptoms:**
- Shop opens but is empty
- "AGENT SHOP" title shows but no character cards
- NullReferenceException in Console

**Root Cause:**
UIList component doesn't have the UIShopItem prefab assigned in the `_itemInstance` field.

**Fix:**
```
Option A - Use Auto-Fix Tool:
1. Unity Menu → TPSBR → Fix Shop Setup
2. Click "Fix All Issues"
3. Done!

Option B - Manual Fix:
1. Open Menu scene
2. Find: MenuUI/UIShopView/Content/ShopItemsList
3. Select it
4. In Inspector, find UIList component
5. Drag UIShopItem prefab to "_itemInstance" field
   Prefab location: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab
6. Save scene (Ctrl+S)
```

### Issue 2: Marine Icon Doesn't Show in Agent Locker ❌

**Symptoms:**
- Marine character has no icon in the agent selection
- Icon is missing or shows as blank/default

**Root Cause:**
CharacterData assets have empty `agentID` fields. They need to match AgentSettings IDs exactly.

**Current Agent IDs in AgentSettings:**
- `Agent.Soldier` (for Soldier 66)
- `Agent.Marine` (for Marine)

**Fix:**
```
Option A - Use Auto-Fix Tool:
1. Unity Menu → TPSBR → Fix Shop Setup
2. Click "Fix Character Agent IDs Only"
3. Done!

Option B - Manual Fix:
1. Find: Assets/Scripts/CharacterData/soldier66.asset
2. Select it
3. In Inspector, set "Agent ID" to: Agent.Soldier
4. Find: Assets/Scripts/CharacterData/marine.asset
5. Select it
6. In Inspector, set "Agent ID" to: Agent.Marine
7. Save (Ctrl+S)
```

### Issue 3: No Close Button in Shop UI ❌

**Symptoms:**
- Can't close the shop once opened
- No X or Close button visible
- Have to exit Play Mode to close

**Temporary Workaround:**
- Press ESC key (if back button handler is implemented)
- Click outside the shop area (if background close is enabled)

**Permanent Fix:**

You have two options:

**Option A - Add Close Button to UIShopView:**

1. Open Menu scene
2. Find another view with a close button, ex:
   - UISettingsView/PopUp/PopupFrame/Background/CloseButton
3. Copy the CloseButton GameObject
4. Paste it under UIShopView
5. Position it in top-right corner
6. Select UIShopView GameObject
7. In UIShopView component, assign the button to "_closeButton" field
8. Save scene

**Option B - Make UIShopView a Close View (Existing Setup):**

Your UIShopView already inherits from `UICloseView` which should support ESC/back button.

Check if it works:
1. Enter Play Mode
2. Open Shop
3. Press ESC
4. If it closes, you're done!

If ESC doesn't work:
1. Select UIShopView in scene
2. Check "_canHandleBackAction" is true
3. Make sure no other view is blocking it

## Quick Verification Checklist

After applying fixes, verify everything works:

### ✅ Checklist
- [ ] Run: TPSBR → Fix Shop Setup → "Fix All Issues"
- [ ] Open Menu scene
- [ ] Enter Play Mode
- [ ] Open Shop
- [ ] **Verify:** No Console errors
- [ ] **Verify:** Two characters appear (Soldier66, Marine)
- [ ] **Verify:** Icons show for both
- [ ] **Verify:** Soldier66 shows "OWNED" (unlocked by default)
- [ ] **Verify:** Marine shows price "750 CloudCoins"
- [ ] **Verify:** Can close shop (ESC or button)
- [ ] Exit Play Mode
- [ ] **Verify:** No errors when exiting

## Understanding the Fixes

### Why Agent IDs Matter

```
CharacterData (Shop System)        AgentSettings (Spawn System)
┌──────────────────────┐           ┌──────────────────────┐
│ soldier66.asset      │           │ Agent.Soldier        │
│ agentID: "Agent.Soldier" ───────→ │ Prefab: SoldierPrefab│
└──────────────────────┘           └──────────────────────┘

CharacterData (Shop System)        AgentSettings (Spawn System)
┌──────────────────────┐           ┌──────────────────────┐
│ marine.asset         │           │ Agent.Marine         │
│ agentID: "Agent.Marine" ────────→ │ Prefab: MarinePrefab │
└──────────────────────┘           └──────────────────────┘
```

The `agentID` links the shop system to the character spawning system.

**Without correct Agent ID:**
- Character purchases but doesn't spawn ❌
- Wrong character spawns ❌
- Crashes or errors ❌

**With correct Agent ID:**
- Purchase → Spawn correct character ✅
- Icons display properly ✅
- Everything works ✅

### Why UIList Needs the Prefab

```
UIShopView (Script)
   ↓
Calls: _shopItemsList.Refresh(characterCount)
   ↓
UIList (Component)
   ↓
Spawns: _itemInstance prefab × characterCount
   ↓
UIShopItem instances appear in shop!
```

**Without prefab assigned:**
- UIList tries to spawn null ❌
- NullReferenceException ❌
- Empty shop ❌

**With prefab assigned:**
- UIList spawns UIShopItem for each character ✅
- Shop populates correctly ✅
- All characters visible ✅

## Common Errors & Solutions

### Error: "NullReferenceException: Object reference not set"
**Location:** UIList.cs line 129

**Cause:** UIList._itemInstance is null

**Solution:** Run "Fix UIList Prefab Only" from Fix Shop Setup window

---

### Error: "UIShopView: ShopDatabase is not assigned"

**Cause:** ShopDatabase field is empty in UIShopView

**Solution:**
1. Select UIShopView in scene
2. Drag ShopDatabase.asset to "_shopDatabase" field

---

### Error: Character spawns but is default/wrong model

**Cause:** Agent ID doesn't match AgentSettings

**Solution:** Run "Fix Character Agent IDs Only"

---

### Warning: "CharacterData has no icon assigned"

**Cause:** Icon sprite not set in CharacterData

**Solution:**
1. Open CharacterData asset
2. Assign icon sprite from Assets/TPSBR/UI/AgentIcons/

---

## Testing After Fixes

Complete test procedure:

```
1. Apply all fixes using Fix Shop Setup tool
2. Open Menu scene
3. Enter Play Mode
4. Click SHOP button from main menu
5. Expected result:
   ┌────────────────── AGENT SHOP ──────────────────┐
   │           CloudCoins: 750                      │
   ├────────────────────────────────────────────────┤
   │ [Icon] Soldier 66          ┌────────┐         │
   │        FREE                │ OWNED  │         │
   │                            └────────┘         │
   │                                                │
   │ [Icon] Marine              ┌────────┐         │
   │        750 CloudCoins      │  BUY   │         │
   │                            └────────┘         │
   └────────────────────────────────────────────────┘

6. Click "BUY" on Marine
7. Should deduct 750 coins → 0 remaining
8. Marine should now show "OWNED"
9. Press ESC or click close
10. Shop should close
11. No errors in Console
```

If this works, all issues are fixed! ✅

## Still Having Issues?

If problems persist after running the fixes:

### Check These:
1. **ShopDatabase exists and has characters**
   - Location: Assets/Scripts/ShopDatabase.asset
   - Should contain soldier66.asset and marine.asset

2. **UIShopItem prefab is valid**
   - Location: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab
   - Should have UIShopItem component
   - All UI references should be assigned

3. **Scene is saved**
   - After any changes, save scene (Ctrl+S)
   - Verify asterisk (*) disappears from scene name

4. **No script compilation errors**
   - Check Console for red errors
   - Fix any compilation errors first

### Nuclear Option (Last Resort):
If nothing works, check `FINAL_INTEGRATION_CHECKLIST.md` for complete setup from scratch.

## Prevention Tips

To avoid these issues in the future:

1. **Always use the Character Setup Tool**
   - TPSBR → Character & Shop Setup
   - It sets up everything correctly automatically

2. **Use validation before testing**
   - Open ShopDatabase asset
   - Click "Validate Database"
   - Fix any warnings

3. **Check agent IDs match**
   - AgentSettings IDs must match CharacterData.agentID exactly
   - Case-sensitive!

4. **Save scenes after UI changes**
   - Any GameObject/Component changes need scene save
   - Ctrl+S after modifications

---

**Need more help?** Check these files:
- QUICK_SETUP_CHECKLIST.md - Setup guide
- SHOP_SYSTEM_GUIDE.md - Complete documentation
- SYSTEM_OVERVIEW.md - Visual diagrams
