# ⚡ Quick Start - Zone on Map

## 🎯 Goal
Show the shrinking zone circles on your full map (press M) so you can see where to go!

---

## 🚀 Fastest Setup (30 Seconds!)

### **Step 1: Open the Tool**
Unity Menu Bar → **Tools** → **Map Zone Setup Helper**

### **Step 2: Fill in 2 Fields**
1. **Map System** ← Drag your SimpleMapSystem component (from Game scene)
2. **Map Panel** ← Drag your Map Panel GameObject (the UI that appears when M is pressed)

### **Step 3: Click Button**
Click **"Auto-Setup Zone Circles"** → Done! ✅

---

## ✨ What You Get

Press **M** in Play mode to see:
- 🔴 **Red Circle** = Current safe zone
- ⚪ **White Circle** = Next zone (where it's shrinking to)

Both circles:
- Update in real-time
- Show correct position and size
- Match the actual zone in the game world

---

## 🎨 Optional Enhancement

Make circles look better:

1. Menu Bar → **Tools** → **Generate Circle Sprite for Map**
2. Click **"Generate Circle Sprite"**
3. Drag generated sprite onto:
   - CurrentZoneCircle → Source Image
   - NextZoneCircle → Source Image

---

## 🐛 If It Doesn't Work

**Check these:**
- ✅ SimpleMapSystem component exists in Game scene
- ✅ Map Panel GameObject exists
- ✅ ShrinkingArea component exists in Game scene
- ✅ "Show Zones" is checked in SimpleMapSystem Inspector
- ✅ Game has started (zone is active)

---

## 📚 Need More Help?

Read these files:
- **ZONE_VISUALIZATION_GUIDE.md** - Complete features guide
- **ZONE_ON_MAP_SETUP.md** - Detailed setup instructions

---

That's it! Super simple. Now you can survive the zone like a pro! 🎮
