# Shop UI - Quick Start Guide

## 🎯 Goal
Get your shop working with characters, icons, buy buttons, and a close button.

## ⚡ 3-Step Fix (Takes 30 Seconds)

### Step 1: Run Auto-Fix
```
Unity Menu Bar → TPSBR → Apply Shop Fix Now
```

Wait for this message in Console:
```
✅ SHOP FIXED! UIShopItem prefab assigned to UIList!
```

### Step 2: Press Play
```
Click the Play ▶️ button in Unity
```

### Step 3: Open Shop
```
In-game → Click "SHOP" button from main menu
```

## ✅ What You Should See

```
╔════════════════════════════════════╗
║ [X]              AGENT SHOP        ║
║                                    ║
║ CloudCoins: 750                    ║
║                                    ║
║ ┌──────────────────────────────┐   ║
║ │ [🤖] Soldier66               │   ║
║ │ FREE           [OWNED]       │   ║
║ └──────────────────────────────┘   ║
║                                    ║
║ ┌──────────────────────────────┐   ║
║ │ [👨‍✈️] Marine                 │   ║
║ │ 750 CloudCoins    [BUY]      │   ║
║ └──────────────────────────────┘   ║
╚════════════════════════════════════╝
```

## 🎮 Test It Works

1. **See 2 characters** ✓
2. **See their icons** ✓  
3. **Soldier shows "OWNED"** ✓
4. **Marine shows "BUY"** ✓
5. **CloudCoins: 750 displayed** ✓
6. **Click Marine's BUY button** → Should purchase!
7. **CloudCoins drop to 0** ✓
8. **Marine changes to "SELECTED"** ✓
9. **Click [X] button** → Shop closes ✓

## 🐛 Still Not Working?

### Shop Opens But Empty (No Characters)
**Problem**: UIList missing prefab reference  
**Fix**: Run "TPSBR → Apply Shop Fix Now" again

### Icons Missing (Empty squares)
**Problem**: Icon sprites not assigned
**Fix**:
1. Open `Assets/Scripts/CharacterData/soldier66.asset`
2. Make sure "Icon" field shows SoldierIcon
3. If not, drag `Assets/TPSBR/UI/AgentIcons/SoldierIcon.png` into it

### Can't Buy Marine (Button Greyed Out)
**Problem**: Not enough CloudCoins  
**Fix**: You need 750 coins. Earn them in-game or check CloudCoinSystem

### Close Button Not Working
**Problem**: Button not connected  
**Fix**:
1. In scene, select `/MenuUI/UIShopView`
2. In Inspector, find `UIShopView` component
3. Assign `Close Button` field to the CloseButton object

## 📖 More Info

- **Full explanation**: `SHOP_FIXED_README.md`
- **Visual guide**: `VISUAL_FIX_GUIDE.md`
- **Troubleshooting**: `TROUBLESHOOTING_GUIDE.md`

## 🎉 That's It!

Your shop should now be fully functional with:
- ✅ 2 characters (Soldier66 & Marine)
- ✅ Character icons displaying
- ✅ Buy buttons working
- ✅ CloudCoins system
- ✅ Close button
- ✅ Connected to Character Selection

**Enjoy your shop system!**
