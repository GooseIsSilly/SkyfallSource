# 🎯 Shrinking Zone Display on Full Map - Setup Guide

I've updated your `SimpleMapSystem.cs` to display the shrinking zone circles on the full map (the one you open with M key)!

---

## ✅ What Was Added

### **New Features:**
1. **Current Zone Circle** (Red) - Shows the current safe zone boundary
2. **Next Zone Circle** (White) - Shows where the zone will shrink to next
3. **Auto-updates** - Circles update in real-time as the zone shrinks

---

## 🔧 Setup Instructions

### **Step 1: Add Zone Circle UI Elements**

You need to add two circle images to your Map Panel UI:

1. **Open your Game scene** (the one with the map UI)
2. **Find your Map Panel** in the Hierarchy (the UI that shows when you press M)
3. **Inside the Map Panel**, create two new UI Images:

#### **Create Current Zone Circle:**
- Right-click on Map Panel → UI → Image
- Rename it to `CurrentZoneCircle`
- In Inspector:
  - Set **Anchor** to Center (hold Alt+Shift, click center)
  - Set **Width** = 200, **Height** = 200
  - Set **Color** = Red with alpha (R:255, G:0, B:0, A:80)
  - **IMPORTANT:** Make sure this image is BEHIND the player marker in the hierarchy

#### **Create Next Zone Circle:**
- Right-click on Map Panel → UI → Image
- Rename it to `NextZoneCircle`
- In Inspector:
  - Set **Anchor** to Center (hold Alt+Shift, click center)
  - Set **Width** = 200, **Height** = 200
  - Set **Color** = White with alpha (R:255, G:255, B:255, A:128)
  - **IMPORTANT:** Make sure this is also BEHIND the player marker

### **Step 2: Assign Circle Sprites (Optional - for better visuals)**

For better-looking circles, you can:
1. Create a circle sprite (white circle with transparent background)
2. Assign it to both circle images' **Source Image** field

OR just leave **Source Image** as None and the circles will use the default Unity sprite.

### **Step 3: Link Everything in SimpleMapSystem**

1. Find the GameObject with `SimpleMapSystem` component in your Game scene
2. In Inspector, you'll see new fields under **Zone Visualization**:
   - **Show Zones** - ✅ Check this (enabled by default)
   - **Current Zone Circle** - Drag your `CurrentZoneCircle` image here
   - **Next Zone Circle** - Drag your `NextZoneCircle` image here
   - **Current Zone Color** - Red with transparency (default)
   - **Next Zone Color** - White with transparency (default)

---

## 🎮 How It Works

### **In-Game:**
1. Press **M** to open the full map
2. You'll see:
   - **Red Circle** = Current safe zone (you're safe inside)
   - **White Circle** = Next zone target (where it will shrink to)
3. The circles:
   - Show in the exact world position
   - Scale correctly with the zone radius
   - Update in real-time as the zone shrinks
   - Disappear when the zone isn't active

### **Zone States:**
- **Before zone activates** - No circles shown
- **Zone active, not announced** - Only red current zone shown
- **Zone announced** - Both red (current) and white (next) shown
- **Zone shrinking** - Circles update smoothly

---

## 🎨 Customization

You can customize the colors in the Inspector:

- **Current Zone Color** - Default: Red with 30% opacity
- **Next Zone Color** - Default: White with 50% opacity
- **Show Zones** - Toggle on/off to hide zones completely

### **Recommended Colors:**
- **Current Zone**: Red (danger) - `RGB(255, 0, 0)` Alpha 80-100
- **Next Zone**: White or Blue (safe target) - `RGB(255, 255, 255)` Alpha 100-150

---

## 📐 Hierarchy Structure

Your map UI should look like this:

```
Canvas
└── MapPanel
    ├── Background
    ├── MapDisplay (the map image)
    ├── POI Markers Container
    ├── CurrentZoneCircle ← NEW (behind player)
    ├── NextZoneCircle ← NEW (behind player)
    └── PlayerMarker (should be on top)
```

---

## ✨ Result

Now when you press M to open the map, you'll see:
- Where the current safe zone is (red circle)
- Where you need to go to stay safe (white circle)
- Your position relative to both zones

This makes it much easier to navigate and survive the shrinking zone, just like in Fortnite or other battle royale games!

---

## 🐛 Troubleshooting

**Circles don't show:**
- Make sure `Show Zones` is checked
- Verify both circle images are assigned in Inspector
- Check that `ShrinkingArea` component exists in your Game scene
- Ensure the zone is active (game has started)

**Circles wrong size:**
- Check `Map World Size` in SimpleMapSystem matches your actual map size
- Verify `Map World Center` is correctly set

**Circles wrong position:**
- Ensure both circle images have their anchors set to Center
- Check that `Map World Center` in SimpleMapSystem is correct

**Player marker behind circles:**
- Reorder hierarchy so PlayerMarker is BELOW the circles (renders on top)
