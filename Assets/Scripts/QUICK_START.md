# Map System - Quick Start (5 Minutes!)

## What You Need

1. ✅ A screenshot of your map from above (any image works!)
2. ✅ 5 minutes

---

## Super Quick Setup

### 1. Take Map Screenshot
- Position Scene camera looking DOWN at your map
- Screenshot it
- Save as `MapImage.png` in `/Assets/Textures/`

### 2. Create UI (30 seconds)
```
Hierarchy:
  Canvas
    └─ MapPanel (Panel)
         └─ MapDisplay (Image) ← Put your map image here!
              ├─ PlayerMarker (Image, 20x20, Yellow)
              └─ POIMarkers (Empty GameObject)
```

### 3. Create POI Marker Prefab (15 seconds)
- Create UI Image, size 15x15, red color
- Name it "POIMarker"
- Drag to `/Assets/Prefabs/` folder
- Delete from Hierarchy

### 4. Add Map System (30 seconds)
- Create empty GameObject "MapSystem"
- Add `SimpleMapSystem` component
- Drag and drop all the UI elements into the fields

### 5. Add POIs (10 seconds each)
- Select any object in scene
- Add `MapPOI` component
- Done!

### 6. Play!
Press **M** to open/close map!

---

## Visual Setup Example

```
MapPanel Settings:
├─ Color: Black with 200 alpha (semi-transparent)
├─ Anchor: Stretch all
└─ Offset: All zeros

MapDisplay Settings:
├─ Source Image: Your MapImage.png
├─ Width: 600
├─ Height: 600
└─ Preserve Aspect: Checked

PlayerMarker Settings:
├─ Color: Yellow
├─ Width: 20
├─ Height: 20
└─ Anchor: Center
```

---

## Map World Size Guide

**Map World Size = How big is your playable area in Unity?**

Example measurements:
- Walk to the **far left** of your map, check X position: -50
- Walk to the **far right** of your map, check X position: 50
- **Map World Size X** = 50 - (-50) = **100**

Do the same for Z axis!

**Map World Center = Middle point of your map**
- Center X = (-50 + 50) / 2 = **0**
- Center Z = Same calculation

---

## That's It!

You now have a working Fortnite-style map! 🎉

**Controls:**
- Press **M** to toggle map

**To customize:**
- Change map image anytime
- Add more POIs by adding `MapPOI` component
- Resize the UI elements to your liking
