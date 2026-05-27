# 🎯 Circular Map Setup - Fortnite Style

## Problem
Your map is covered in red because the zone circle is filling the entire screen. You want a circular map like in the reference image!

---

## 🎨 Solution: Create Circular Map Display

### **Option 1: Quick Fix - Adjust Zone Colors (EASIEST)**

The red you're seeing is too opaque. Let's make it more transparent:

1. **Select** `/Canvas/MapPanel/CurrentZoneCircle` in Hierarchy
2. In **Inspector**, find **Image** component
3. Change **Color**:
   - Keep Red selected
   - Set **Alpha (A)** to **50-80** (currently 77, but lower it more)
   - Try: `RGB(255, 0, 0)` with `A = 30-40`

4. **Select** `/Canvas/MapPanel/NextZoneCircle`
5. Change **Color**:
   - Keep White selected
   - Set **Alpha (A)** to **80-100**
   - Try: `RGB(255, 255, 255)` with `A = 80`

This will make the circles more transparent so you can see the map through them!

---

### **Option 2: Circular Map with Mask (FORTNITE STYLE)**

To get the exact look from your reference image (circular map with purple background):

#### **Step 1: Restructure Your Map Panel**

Your hierarchy should look like this:

```
MapPanel (purple background)
├── MapContainer (circular mask)
│   ├── MapDisplay (your map image)
│   ├── PlayerMarker
│   ├── POIMarkers
│   ├── CurrentZoneCircle
│   └── NextZoneCircle
```

#### **Step 2: Manual Setup**

1. **Create MapContainer:**
   - Right-click on `MapPanel` → UI → Image
   - Rename to `MapContainer`
   - Set **Anchor** to Center
   - Set **Width** = 600, **Height** = 600 (adjust for your map size)
   - Set **Color** to White (full opacity)

2. **Add Mask Component:**
   - Select `MapContainer`
   - Click **Add Component** → UI → **Mask**
   - ✅ Check **Show Mask Graphic** = OFF

3. **Move Everything Inside MapContainer:**
   - Drag `MapDisplay` into `MapContainer`
   - Drag `PlayerMarker` into `MapContainer` (or keep inside MapDisplay)
   - Drag `POIMarkers` into `MapContainer`
   - Drag `CurrentZoneCircle` into `MapContainer`
   - Drag `NextZoneCircle` into `MapContainer`

4. **Set MapPanel Background:**
   - Select `MapPanel`
   - In **Image** component, set **Color** to purple/pink:
     - `RGB(180, 100, 200)` or match your desired background color

5. **Optional - Add Circle Sprite:**
   - Use the Circle Sprite Generator tool (Tools → Generate Circle Sprite)
   - Generate with **Fill Circle** = ON
   - Assign the sprite to `MapContainer`'s Image component

---

### **Option 3: Use Image Type Filled (Simpler)**

Instead of a mask, use Image Type Filled for circular shape:

1. **Select** `MapDisplay`
2. In **Image** component:
   - Change **Image Type** to **Filled**
   - Set **Fill Method** to **Radial 360**
   - Set **Fill Origin** to **Bottom**
   - Set **Fill Amount** to **1**
   - **Fill Clockwise** = ON

This creates a circular fill effect!

---

## 🎨 Recommended Settings for Your Look

Based on your reference image:

### **MapPanel (Background):**
- **Color**: Purple/Pink `RGB(160, 100, 180)` Alpha 255

### **MapDisplay (Center Circle):**
- **Size**: 600x600 (or 80% of screen height)
- **Position**: Center of MapPanel
- **Anchor**: Center

### **CurrentZoneCircle (Red):**
- **Color**: `RGB(200, 0, 50)` Alpha **40-60** (very transparent!)
- **Border**: Use circle sprite with thin border

### **NextZoneCircle (White):**
- **Color**: `RGB(255, 255, 255)` Alpha **100-120**
- **Border**: Use circle sprite with thin border

---

## 🎯 Quick Fix for Your Current Issue

**Right Now, do this:**

1. Select **CurrentZoneCircle**
2. Change Alpha to **40** (from 77)
3. Select **NextZoneCircle**  
4. Change Alpha to **80** (from 128)

This will make the red much less visible!

---

## 🖼️ Visual Layout

```
┌─────────────────────────────────────┐
│                                     │
│     Purple Background (MapPanel)    │
│                                     │
│           ╭─────────╮               │
│           │  Map    │               │
│           │ Circle  │               │
│           │    📍   │               │
│           │ (You)   │               │
│           ╰─────────╯               │
│                                     │
│     Game Info / UI Elements         │
│                                     │
└─────────────────────────────────────┘
```

With zone circles:
```
           ╭─────────╮
           │    ⭕   │  ← Thin white circle (next zone)
           │  ⭕     │  ← Very light red tint (current zone)
           │   📍    │
           ╰─────────╯
```

The circles should be **barely visible** as subtle overlays, not solid colors!

---

## 🔧 Alternative: Just Hide the Circles

If you don't want to see zone circles at all:

1. Find `SimpleMapSystem` component in your scene
2. In **Zone Visualization** section
3. **Uncheck** "Show Zones"

Done! No more zone circles.

---

## ✨ Pro Tip: Use Shader for Better Circles

For the best look, use a circle border shader:

1. Generate circle sprite with **Border Thickness** = 0.08
2. Set **Fill Circle** = OFF (hollow circle)
3. Assign to both zone circle images
4. Lower the alpha values significantly

This gives you clean circle borders instead of filled circles!

---

Choose the option that works best for you. **Option 1 (Quick Fix)** is the fastest - just lower the alpha values!
