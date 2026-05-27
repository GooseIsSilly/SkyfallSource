# 🔧 How to Fix Your Shop - Quick Guide

## The Problem

You have 3 issues:
1. ❌ No characters showing in shop
2. ❌ Marine icon doesn't show in agent locker  
3. ❌ No close button in shop UI

## The Solution (2 Minutes) ⚡

### Step 1: Run the Auto-Fix Tool

```
1. In Unity, go to top menu bar
2. Click: TPSBR → Fix Shop Setup
3. Click the big button: "Fix All Issues"
4. Wait for success messages in Console
```

**What this does:**
- ✅ Sets correct Agent IDs (Agent.Soldier, Agent.Marine)
- ✅ Assigns UIShopItem prefab to UIList
- ✅ Saves the Menu scene

### Step 2: Test It

```
1. Press Play ▶️
2. Click SHOP button
3. You should now see:
   - Soldier 66 (FREE, OWNED)
   - Marine (750 CloudCoins, BUY button)
```

### Step 3: Close Button (Optional)

The shop should close with **ESC key**. Try it!

If ESC doesn't work, you can add a close button manually:
```
1. In Hierarchy, find: MenuUI/UISettingsView
2. Find its CloseButton
3. Copy it (Ctrl+C)
4. Select: MenuUI/UIShopView
5. Paste (Ctrl+V)
6. Position the button in top-right corner
7. Select UIShopView
8. Drag the CloseButton to "_closeButton" field
9. Save scene (Ctrl+S)
```

## Done! 🎉

Your shop should now work perfectly!

---

## What If It Doesn't Work?

### Issue: Auto-fix tool doesn't appear in menu

**Solution:**
Wait a few seconds for Unity to compile the new script, then try again.

### Issue: Still no characters showing

**Check these:**
```
1. Open: Assets/Scripts/ShopDatabase.asset
2. Verify it has 2 characters in the list:
   - soldier66.asset
   - marine.asset
3. If not, create them using: TPSBR → Character & Shop Setup
```

### Issue: Wrong Agent IDs

**Manually fix:**
```
1. Open: Assets/Scripts/CharacterData/soldier66.asset
2. Set "Agent ID" to: Agent.Soldier
3. Open: Assets/Scripts/CharacterData/marine.asset
4. Set "Agent ID" to: Agent.Marine
5. Save (Ctrl+S)
```

### Issue: UIShopItem prefab not assigned

**Manually fix:**
```
1. Open Menu scene
2. Find: MenuUI/UIShopView/Content/ShopItemsList
3. Select it
4. In Inspector, find "UIList" component
5. Look for "_itemInstance" field
6. Drag this prefab to it: Assets/TPSBR/UI/Prefabs/Widgets/UIShopItem.prefab
7. Save scene (Ctrl+S)
```

---

## Understanding What Was Wrong

### Problem 1: Empty Agent IDs
Your CharacterData files had empty `agentID` fields:
```
Before:
soldier66.asset → agentID: "" (empty) ❌
marine.asset → agentID: "" (empty) ❌

After:
soldier66.asset → agentID: "Agent.Soldier" ✅
marine.asset → agentID: "Agent.Marine" ✅
```

This links shop characters to their 3D models in AgentSettings.

### Problem 2: Missing Prefab Reference
UIList component needs to know what prefab to spawn:
```
Before:
UIList → _itemInstance: None ❌

After:
UIList → _itemInstance: UIShopItem.prefab ✅
```

Without this, the shop can't create character cards.

### Problem 3: Close Button
UIShopView inherits from UICloseView but needs either:
- Back button handling (ESC key) ✅ Already works!
- Or explicit close button reference

---

## Quick Verification

After fixes, you should see:

**In Shop:**
```
┌─────────────── AGENT SHOP ───────────────┐
│        CloudCoins: 750                    │
├───────────────────────────────────────────┤
│                                           │
│ [Soldier Icon] Soldier 66   [OWNED]      │
│               FREE                        │
│                                           │
│ [Marine Icon]  Marine       [BUY]        │
│               750 CloudCoins              │
│                                           │
└───────────────────────────────────────────┘
```

**Console:**
```
✅ No errors
✅ "Fixed soldier66 agentID to: Agent.Soldier"
✅ "Fixed marine agentID to: Agent.Marine"
✅ "Assigned UIShopItem prefab to UIList!"
✅ "Menu scene saved!"
```

---

## Need More Help?

Check these detailed guides:
- `TROUBLESHOOTING_GUIDE.md` - Detailed troubleshooting
- `QUICK_SETUP_CHECKLIST.md` - Step-by-step setup
- `SHOP_SYSTEM_GUIDE.md` - Complete documentation

---

**That's it! Your shop should now be working! 🎮✨**
