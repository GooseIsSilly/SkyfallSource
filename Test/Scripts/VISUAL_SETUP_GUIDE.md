# 🎨 Visual Setup Guide - Zone Circles on Map

## 📐 UI Hierarchy Structure

Here's exactly how your Map UI should be structured:

```
Canvas
└── MapPanel (the panel that shows when you press M)
    ├── Background (Image - blue/black background)
    ├── MapDisplay (Image - your map sprite)
    │
    ├── POI Markers Container (Empty GameObject)
    │   ├── POI Marker 1 (Text/TMP)
    │   ├── POI Marker 2 (Text/TMP)
    │   └── POI Marker 3 (Text/TMP)
    │
    ├── CurrentZoneCircle ← NEW! (Image - Red circle)
    │   • Anchor: Center (0.5, 0.5)
    │   • Size: 200x200
    │   • Color: Red (255, 0, 0, 80)
    │   • Raycast Target: OFF
    │
    ├── NextZoneCircle ← NEW! (Image - White circle)
    │   • Anchor: Center (0.5, 0.5)
    │   • Size: 200x200
    │   • Color: White (255, 255, 255, 128)
    │   • Raycast Target: OFF
    │
    └── PlayerMarker (Image - Player icon)
        • This should be BELOW circles in hierarchy
        • (Lower = renders on top)
```

---

## 🎯 Visual Layout

### **What it looks like on the map:**

```
┌─────────────────────────────────────────────────┐
│                                                 │
│         Your Game Map Image                     │
│                                                 │
│                 ⚪ POI 1                        │
│                                                 │
│              ╭─────────╮  ← White circle       │
│         ╭────│─────────│────╮  (Next zone)     │
│         │    │         │    │                   │
│         │    ╰─────────╯    │  ← Red circle    │
│         │                   │   (Current zone)  │
│         │        📍         │                   │
│         │      (You)        │                   │
│         ╰───────────────────╯                   │
│                                                 │
│     ⚪ POI 2          ⚪ POI 3                  │
│                                                 │
└─────────────────────────────────────────────────┘

Legend:
📍 = Your player (always on top)
Red Circle = Current safe zone
White Circle = Next zone (where it's shrinking to)
⚪ = Points of Interest
```

---

## 🔧 Inspector Settings

### **CurrentZoneCircle (Red)**

```
┌─────────────────────────────────┐
│ Image Component                 │
├─────────────────────────────────┤
│ Source Image:    [None or Circle]│
│ Color:           █ Red          │
│                  R: 255         │
│                  G: 0           │
│                  B: 0           │
│                  A: 80          │
│ Material:        None           │
│ Raycast Target:  ☐ Unchecked   │
│ Maskable:        ☑ Checked      │
│ Image Type:      Simple         │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ Rect Transform                  │
├─────────────────────────────────┤
│ Pos X:           0              │
│ Pos Y:           0              │
│ Width:           200            │
│ Height:          200            │
│ Anchors:         ⊙ Center       │
│   Min: (0.5, 0.5)              │
│   Max: (0.5, 0.5)              │
│ Pivot:           (0.5, 0.5)    │
└─────────────────────────────────┘
```

### **NextZoneCircle (White)**

```
┌─────────────────────────────────┐
│ Image Component                 │
├─────────────────────────────────┤
│ Source Image:    [None or Circle]│
│ Color:           ▢ White         │
│                  R: 255         │
│                  G: 255         │
│                  B: 255         │
│                  A: 128         │
│ Material:        None           │
│ Raycast Target:  ☐ Unchecked   │
│ Maskable:        ☑ Checked      │
│ Image Type:      Simple         │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ Rect Transform                  │
├─────────────────────────────────┤
│ Pos X:           0              │
│ Pos Y:           0              │
│ Width:           200            │
│ Height:          200            │
│ Anchors:         ⊙ Center       │
│   Min: (0.5, 0.5)              │
│   Max: (0.5, 0.5)              │
│ Pivot:           (0.5, 0.5)    │
└─────────────────────────────────┘
```

---

## 🎮 SimpleMapSystem Inspector

### **Your SimpleMapSystem component should look like this:**

```
┌──────────────────────────────────────────┐
│ Simple Map System (Script)               │
├──────────────────────────────────────────┤
│ ▼ Map Settings                           │
│   Map Image:         [Your Map Sprite]   │
│   Map World Size:    (1000, 1000)        │
│   Map World Center:  (0, 0)              │
│                                          │
│ ▼ UI References                          │
│   Map Panel:         [MapPanel]          │
│   Map Display:       [MapDisplay Image]  │
│   Player Marker:     [PlayerMarker]      │
│   POI Marker Prefab: [POI Prefab]        │
│   POI Container:     [POI Container]     │
│                                          │
│ ▼ Zone Visualization            ← NEW!  │
│   ☑ Show Zones                           │
│   Current Zone Circle: [CurrentZone...] │
│   Next Zone Circle:    [NextZone...]    │
│   Current Zone Color:  █ Red (80 alpha) │
│   Next Zone Color:     ▢ White (128 α)  │
└──────────────────────────────────────────┘
```

---

## 🎨 Color Picker Guide

### **How to set colors in Unity:**

**For Current Zone (Red):**
1. Click the color box in Inspector
2. Set RGB values:
   - R: 255
   - G: 0
   - B: 0
3. Set Alpha (A): 80-100
4. Close color picker

**For Next Zone (White):**
1. Click the color box in Inspector
2. Set RGB values:
   - R: 255
   - G: 255
   - B: 255
3. Set Alpha (A): 128-150
4. Close color picker

**Alpha Channel Guide:**
- 0 = Fully transparent (invisible)
- 128 = 50% transparent
- 255 = Fully opaque (solid)

**Recommended Alphas:**
- Current Zone: 80-100 (subtle, danger)
- Next Zone: 128-150 (more visible, target)

---

## 📏 Sizing Guide

### **Initial Size:**
Both circles start at **200x200 pixels**

### **Runtime Size:**
The script automatically calculates size based on:
```
CircleSize = (ZoneRadius × 2 ÷ MapWorldSize) × MapDisplayWidth
```

Example:
- Map World Size = 1000 units
- Map Display Width = 800 pixels
- Zone Radius = 250 units

Calculation:
```
CircleSize = (250 × 2 ÷ 1000) × 800
          = (500 ÷ 1000) × 800
          = 0.5 × 800
          = 400 pixels
```

So the circle will be 400x400 pixels on screen.

---

## 🎯 Anchor Settings Explained

### **Why Center Anchor?**

```
Using Center (0.5, 0.5):
┌─────────────────┐
│                 │
│        ⊙        │  ← Circle positioned from center
│    ╭───────╮    │
│    │   ⊙   │    │
│    ╰───────╯    │
│                 │
└─────────────────┘

Using Top-Left (0, 1):
┌─────────────────┐
│⊙╭───────╮       │  ← Wrong! Position off-center
││   ⊙   │        │
│╰───────╯        │
│                 │
│                 │
└─────────────────┘
```

Center anchor ensures circles are positioned correctly relative to their world position!

---

## 🔄 Update Flow Diagram

```
Game Running
│
├─► Press M Key
│   │
│   └─► Map Opens
│       │
│       └─► Every Frame:
│           │
│           ├─► UpdateMarkers()
│           │   │
│           │   ├─► UpdatePlayerMarker()
│           │   ├─► UpdatePOIMarkers()
│           │   └─► UpdateZoneCircles() ← NEW!
│           │       │
│           │       ├─► Is Zone Active?
│           │       │   Yes → Continue
│           │       │   No → Hide circles
│           │       │
│           │       ├─► Update Current Circle
│           │       │   ├─► WorldToMapPosition()
│           │       │   └─► Calculate size
│           │       │
│           │       └─► Update Next Circle (if announced)
│           │           ├─► WorldToMapPosition()
│           │           └─► Calculate size
│           │
│           └─► Render on screen
│
└─► Press M Again → Map Closes
```

---

## 🎬 Animation Flow

### **Zone Lifecycle:**

```
1. Game Starts
   ├─ Zone Not Active
   └─ Circles Hidden
      ↓
2. Zone Activates
   ├─ Red circle appears
   └─ White circle hidden
      ↓
3. ~30 seconds later - Announcement
   ├─ Red circle still visible
   └─ White circle appears (target)
      ↓
4. Zone Shrinks
   ├─ Red circle moves & shrinks (animated)
   └─ White circle stays as target
      ↓
5. Shrink Complete
   ├─ Red circle = old white circle position
   └─ White circle hidden
      ↓
6. Repeat from step 3 (next shrink)
```

---

## 🎨 Visual States

### **State 1: Not Active**
```
┌─────────────────┐
│                 │
│   Just map      │
│   and player    │
│        📍       │
│                 │
└─────────────────┘
No circles shown
```

### **State 2: Active, Not Announced**
```
┌─────────────────┐
│                 │
│   ╭─────────╮   │
│   │         │   │  ← Red circle only
│   │    📍   │   │
│   ╰─────────╯   │
│                 │
└─────────────────┘
Stay inside red!
```

### **State 3: Announced**
```
┌─────────────────┐
│                 │
│  ╭─────────╮    │
│  │ ╭─────╮ │    │  ← Red (outer)
│  │ │     │ │    │     White (inner)
│  │ │ 📍  │ │    │
│  │ ╰─────╯ │    │
│  ╰─────────╯    │
└─────────────────┘
Move to white circle!
```

---

## ✨ Final Result

When everything is set up correctly, you'll have:

✅ **Visual Clarity:**
- See current danger zone (red)
- See safe target zone (white)
- Know exactly where to move

✅ **Real-Time Updates:**
- Circles move as zone shrinks
- Size changes dynamically
- Smooth animations

✅ **Professional Look:**
- Clean circle visualization
- Appropriate transparency
- Standard battle royale style

---

## 🎯 Quick Visual Checklist

Before testing, verify:

- [ ] Both circle images exist in Map Panel
- [ ] Both circles have Center anchors
- [ ] CurrentZoneCircle is Red with alpha ~80
- [ ] NextZoneCircle is White with alpha ~128
- [ ] Both circles have Raycast Target OFF
- [ ] PlayerMarker is BELOW circles in hierarchy
- [ ] Both circles are assigned in SimpleMapSystem
- [ ] "Show Zones" is checked

If all checked → You're ready to go! 🚀

---

Now you have a complete visual understanding of the system! 🎨
